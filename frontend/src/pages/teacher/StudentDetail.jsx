import { useEffect, useState, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, Wallet, BookOpen, Check } from 'lucide-react';
import { studentsApi, quotasApi, enrollmentsApi, classesApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import {
  Card,
  Button,
  TextField,
  Select,
  PageLoader,
  EmptyState,
  Modal,
  Badge,
  Avatar,
} from '../../components/ui';
import { formatDateOnly, reportError } from '../../lib/format';

export default function StudentDetail() {
  const { id } = useParams();
  const { t } = useTranslation();
  const toast = useToast();

  const [student, setStudent] = useState(null);
  const [quotas, setQuotas] = useState([]);
  const [classes, setClasses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [enrollId, setEnrollId] = useState('');
  const [enrolling, setEnrolling] = useState(false);

  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({
    classGroupId: '',
    totalSessions: '10',
    price: '',
    periodStart: '',
    periodEnd: '',
  });
  const [errors, setErrors] = useState({});

  const loadQuotas = useCallback(async () => {
    try {
      setQuotas(await quotasApi.forStudent(id));
    } catch (err) {
      reportError(toast, err, t('common.fetchError'));
    }
  }, [id, toast, t]);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const [s, cs] = await Promise.all([studentsApi.get(id), classesApi.list()]);
        setStudent(s);
        setClasses(cs);
        setQuotas(await quotasApi.forStudent(id));
      } catch (err) {
        reportError(toast, err, t('common.fetchError'));
      } finally {
        setLoading(false);
      }
    })();
  }, [id, toast, t]);

  const doEnroll = async () => {
    if (!enrollId) return;
    setEnrolling(true);
    try {
      await enrollmentsApi.enroll(Number(enrollId), Number(id));
      toast.success(t('common.updateSuccess'));
      setEnrollId('');
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setEnrolling(false);
    }
  };

  const pay = async (quotaId) => {
    try {
      await quotasApi.pay(id, quotaId);
      toast.success(t('common.paySuccess'));
      await loadQuotas();
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    }
  };

  const openModal = () => {
    setErrors({});
    const today = new Date().toISOString().slice(0, 10);
    const end = new Date();
    end.setMonth(end.getMonth() + 1);
    setForm({
      classGroupId: classes[0]?.id ? String(classes[0].id) : '',
      totalSessions: '10',
      price: '',
      periodStart: today,
      periodEnd: end.toISOString().slice(0, 10),
    });
    setModalOpen(true);
  };

  const submit = async (ev) => {
    ev.preventDefault();
    const e = {};
    if (!form.classGroupId) e.classGroupId = t('app.required');
    if (!form.totalSessions || Number(form.totalSessions) <= 0) e.totalSessions = t('app.required');
    if (!form.price || Number(form.price) < 0) e.price = t('app.required');
    setErrors(e);
    if (Object.keys(e).length) return;
    setSaving(true);
    try {
      await quotasApi.create(id, {
        classGroupId: Number(form.classGroupId),
        totalSessions: Number(form.totalSessions),
        price: Number(form.price),
        periodStart: form.periodStart,
        periodEnd: form.periodEnd,
      });
      toast.success(t('common.createSuccess'));
      setModalOpen(false);
      await loadQuotas();
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <PageLoader label={t('app.loading')} />;
  if (!student) return null;

  return (
    <div className="space-y-5">
      <Link to="/students" className="text-sm text-muted hover:text-text inline-flex items-center gap-1">
        <span>←</span> {t('app.back')}
      </Link>

      <Card className="p-4 flex items-center gap-4">
        <Avatar name={student.fullName} size="lg" />
        <div>
          <h1 className="text-xl font-bold text-text">{student.fullName}</h1>
          <p className="text-sm text-muted">{student.phone}</p>
        </div>
      </Card>

      <Card className="p-4">
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-semibold text-text">{t('studentDetail.quotas')}</h2>
          <Button size="sm" onClick={openModal}>
            <Plus className="w-4 h-4" />
            {t('studentDetail.addQuota')}
          </Button>
        </div>
        {quotas.length === 0 ? (
          <EmptyState icon={Wallet} title={t('studentDetail.noQuotas')} />
        ) : (
          <div className="space-y-2">
            {quotas.map((q) => {
              const paid = !!q.paidAt;
              return (
                <div
                  key={q.id}
                  className="flex items-center justify-between gap-3 p-3 rounded-xl border border-border"
                >
                  <div className="min-w-0">
                    <p className="font-medium text-text truncate">{q.className}</p>
                    <p className="text-xs text-muted">
                      {t('quotas.remaining')}: {q.remainingSessions}/{q.totalSessions} · {q.price}
                    </p>
                    <p className="text-[11px] text-muted">
                      {formatDateOnly(q.periodStart)} – {formatDateOnly(q.periodEnd)}
                    </p>
                  </div>
                  <div className="flex items-center gap-2 shrink-0">
                    <Badge tone={paid ? 'success' : 'warning'}>
                      {paid ? t('quotas.paid') : t('quotas.unpaid')}
                    </Badge>
                    {!paid && (
                      <Button size="sm" variant="outline" onClick={() => pay(q.id)}>
                        <Check className="w-3.5 h-3.5" />
                        {t('quotas.pay')}
                      </Button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </Card>

      <Card className="p-4">
        <h2 className="font-semibold text-text mb-3">{t('studentDetail.enroll')}</h2>
        <div className="flex flex-col sm:flex-row gap-2">
          <Select value={enrollId} onChange={(e) => setEnrollId(e.target.value)} className="flex-1">
            <option value="">{t('app.select')}</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
          <Button onClick={doEnroll} loading={enrolling} disabled={!enrollId}>
            <BookOpen className="w-4 h-4" />
            {t('studentDetail.enroll')}
          </Button>
        </div>
      </Card>

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={t('studentDetail.quotaTitle')}
        footer={
          <>
            <Button variant="secondary" onClick={() => setModalOpen(false)} disabled={saving}>
              {t('app.cancel')}
            </Button>
            <Button onClick={submit} loading={saving}>
              {t('app.save')}
            </Button>
          </>
        }
      >
        <form className="space-y-4" onSubmit={submit}>
          <Select
            label={t('students.enrolledIn')}
            value={form.classGroupId}
            error={errors.classGroupId}
            onChange={(e) => setForm({ ...form, classGroupId: e.target.value })}
          >
            <option value="">{t('app.select')}</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
          <TextField
            label={t('studentDetail.totalSessions')}
            type="number"
            min="1"
            value={form.totalSessions}
            error={errors.totalSessions}
            onChange={(e) => setForm({ ...form, totalSessions: e.target.value })}
          />
          <TextField
            label={t('studentDetail.price')}
            type="number"
            min="0"
            step="0.01"
            value={form.price}
            error={errors.price}
            onChange={(e) => setForm({ ...form, price: e.target.value })}
          />
          <div className="grid grid-cols-2 gap-3">
            <TextField
              label={t('quotas.period') + ' · ' + t('app.start')}
              type="date"
              value={form.periodStart}
              onChange={(e) => setForm({ ...form, periodStart: e.target.value })}
            />
            <TextField
              label={t('quotas.period') + ' · ' + t('app.end')}
              type="date"
              value={form.periodEnd}
              onChange={(e) => setForm({ ...form, periodEnd: e.target.value })}
            />
          </div>
        </form>
      </Modal>
    </div>
  );
}
