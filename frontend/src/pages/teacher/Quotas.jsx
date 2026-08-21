import { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Wallet, Plus, Check, Search } from 'lucide-react';
import { studentsApi, quotasApi, classesApi } from '../../api';
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
  ConfirmDialog,
} from '../../components/ui';
import { formatDateOnly, reportError } from '../../lib/format';

export default function Quotas() {
  const { t } = useTranslation();
  const toast = useToast();

  const [students, setStudents] = useState([]);
  const [studentId, setStudentId] = useState('');
  const [studentName, setStudentName] = useState('');
  const [quotas, setQuotas] = useState([]);
  const [classes, setClasses] = useState([]);
  const [loading, setLoading] = useState(false);
  const [query, setQuery] = useState('');

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

  const [payId, setPayId] = useState(null);
  const [payLoading, setPayLoading] = useState(false);

  useEffect(() => {
    studentsApi
      .list()
      .then(setStudents)
      .catch(() => setStudents([]));
    classesApi
      .list()
      .then(setClasses)
      .catch(() => setClasses([]));
  }, []);

  const loadQuotas = useCallback(
    async (sid) => {
      if (!sid) {
        setQuotas([]);
        return;
      }
      setLoading(true);
      try {
        setQuotas(await quotasApi.forStudent(sid));
      } catch (err) {
        reportError(toast, err, t('common.fetchError'));
        setQuotas([]);
      } finally {
        setLoading(false);
      }
    },
    [toast, t]
  );

  const filteredStudents = students.filter((s) =>
    s.fullName.toLowerCase().includes(query.trim().toLowerCase())
  );

  const pick = (s) => {
    setStudentId(s.id);
    setStudentName(s.fullName);
    loadQuotas(s.id);
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
      await quotasApi.create(studentId, {
        classGroupId: Number(form.classGroupId),
        totalSessions: Number(form.totalSessions),
        price: Number(form.price),
        periodStart: form.periodStart,
        periodEnd: form.periodEnd,
      });
      toast.success(t('common.createSuccess'));
      setModalOpen(false);
      await loadQuotas(studentId);
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setSaving(false);
    }
  };

  const confirmPay = async () => {
    setPayLoading(true);
    try {
      await quotasApi.pay(studentId, payId);
      toast.success(t('common.paySuccess'));
      await loadQuotas(studentId);
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setPayLoading(false);
      setPayId(null);
    }
  };

  return (
    <div className="space-y-5">
      <h1 className="text-xl font-bold text-text">{t('quotas.title')}</h1>

      <div className="grid gap-3 sm:grid-cols-[260px_1fr]">
        <Card className="p-2 self-start max-h-[70vh] overflow-y-auto">
          <div className="relative mb-2">
            <Search className="w-4 h-4 absolute top-1/2 -translate-y-1/2 start-3 text-muted" />
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder={t('students.searchPlaceholder')}
              className="w-full bg-surface border border-border rounded-xl ps-9 pe-3 py-2 text-sm text-text placeholder:text-muted/70 focus:border-primary focus:ring-2 focus:ring-primary/30"
            />
          </div>
          {filteredStudents.length === 0 ? (
            <p className="text-xs text-muted text-center py-4">{t('app.noResults')}</p>
          ) : (
            <ul className="space-y-1">
              {filteredStudents.map((s) => (
                <li key={s.id}>
                  <button
                    onClick={() => pick(s)}
                    className={`w-full text-start px-3 py-2 rounded-lg text-sm transition-colors ${
                      String(s.id) === String(studentId)
                        ? 'bg-primary-tint text-primary font-medium'
                        : 'hover:bg-elevated text-text'
                    }`}
                  >
                    {s.fullName}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </Card>

        <div>
          {!studentId ? (
            <Card className="p-2">
              <EmptyState icon={Wallet} title={t('quotas.noQuotas')} description={t('quotas.emptyHint')} />
            </Card>
          ) : loading ? (
            <PageLoader label={t('app.loading')} />
          ) : (
            <Card className="divide-y divide-border">
              <div className="flex items-center justify-between px-4 py-3">
                <h2 className="font-semibold text-text">{studentName}</h2>
                <Button size="sm" onClick={openModal}>
                  <Plus className="w-4 h-4" />
                  {t('studentDetail.addQuota')}
                </Button>
              </div>
              {quotas.length === 0 ? (
                <EmptyState icon={Wallet} title={t('quotas.noQuotas')} />
              ) : (
                quotas.map((q) => {
                  const paid = !!q.paidAt;
                  return (
                    <div key={q.id} className="flex items-center justify-between gap-3 px-4 py-3">
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
                          <Button size="sm" variant="outline" onClick={() => setPayId(q.id)}>
                            <Check className="w-3.5 h-3.5" />
                            {t('quotas.pay')}
                          </Button>
                        )}
                      </div>
                    </div>
                  );
                })
              )}
            </Card>
          )}
        </div>
      </div>

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

      <ConfirmDialog
        open={payId !== null}
        title={t('quotas.pay')}
        message={t('quotas.payConfirm')}
        confirmLabel={t('quotas.pay')}
        cancelLabel={t('app.cancel')}
        loading={payLoading}
        onConfirm={confirmPay}
        onCancel={() => setPayId(null)}
      />
    </div>
  );
}
