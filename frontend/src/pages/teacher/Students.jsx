import { useEffect, useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Users, Plus, Search, UserRound } from 'lucide-react';
import { studentsApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import {
  Card,
  Button,
  TextField,
  PageLoader,
  EmptyState,
  Modal,
  Avatar,
} from '../../components/ui';
import { reportError } from '../../lib/format';

export default function Students() {
  const { t } = useTranslation();
  const toast = useToast();
  const navigate = useNavigate();

  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState('');

  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({ fullName: '', phone: '', pin: '' });
  const [errors, setErrors] = useState({});

  const load = async () => {
    setLoading(true);
    try {
      setStudents(await studentsApi.list());
    } catch (err) {
      reportError(toast, err, t('common.fetchError'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return students;
    return students.filter(
      (s) => s.fullName.toLowerCase().includes(q) || s.phone.toLowerCase().includes(q)
    );
  }, [students, query]);

  const openModal = () => {
    setForm({ fullName: '', phone: '', pin: '' });
    setErrors({});
    setModalOpen(true);
  };

  const validate = () => {
    const e = {};
    if (!form.fullName.trim()) e.fullName = t('app.required');
    if (!/^\d{4}$/.test(form.pin)) e.pin = t('app.required');
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const submit = async (ev) => {
    ev.preventDefault();
    if (!validate()) return;
    setSaving(true);
    try {
      await studentsApi.create({
        fullName: form.fullName.trim(),
        phone: form.phone.trim(),
        pin: form.pin,
      });
      toast.success(t('students.createSuccess'));
      setModalOpen(false);
      await load();
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-text">{t('students.title')}</h1>
        <Button onClick={openModal} size="sm">
          <Plus className="w-4 h-4" />
          {t('students.new')}
        </Button>
      </div>

      <div className="relative">
        <Search className="w-4 h-4 absolute top-1/2 -translate-y-1/2 start-3 text-muted" />
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder={t('students.searchPlaceholder')}
          className="w-full bg-surface border border-border rounded-xl ps-9 pe-3 py-2.5 text-sm text-text placeholder:text-muted/70 focus:border-primary focus:ring-2 focus:ring-primary/30"
        />
      </div>

      {loading ? (
        <PageLoader label={t('app.loading')} />
      ) : filtered.length === 0 ? (
        <Card className="p-2">
          <EmptyState
            icon={Users}
            title={students.length === 0 ? t('students.noStudents') : t('app.noResults')}
            description={students.length === 0 ? t('students.emptyHint') : undefined}
            action={
              students.length === 0 ? (
                <Button onClick={openModal} size="sm">
                  <Plus className="w-4 h-4" />
                  {t('students.new')}
                </Button>
              ) : undefined
            }
          />
        </Card>
      ) : (
        <Card className="divide-y divide-border">
          {filtered.map((s) => (
            <button
              key={s.id}
              onClick={() => navigate(`/students/${s.id}`)}
              className="w-full flex items-center gap-3 px-4 py-3 hover:bg-elevated/50 transition-colors text-start"
            >
              <Avatar name={s.fullName} />
              <div className="min-w-0">
                <p className="font-medium text-text truncate">{s.fullName}</p>
                <p className="text-xs text-muted">{s.phone}</p>
              </div>
              <UserRound className="w-4 h-4 text-muted ms-auto" />
            </button>
          ))}
        </Card>
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={t('students.newTitle')}
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
          <TextField
            label={t('students.name')}
            required
            value={form.fullName}
            error={errors.fullName}
            onChange={(e) => setForm({ ...form, fullName: e.target.value })}
          />
          <TextField
            label={t('students.phone')}
            value={form.phone}
            error={errors.phone}
            onChange={(e) => setForm({ ...form, phone: e.target.value })}
          />
          <TextField
            label={t('students.pin')}
            required
            inputMode="numeric"
            maxLength={4}
            placeholder="••••"
            value={form.pin}
            error={errors.pin}
            onChange={(e) => setForm({ ...form, pin: e.target.value.replace(/\D/g, '') })}
          />
        </form>
      </Modal>
    </div>
  );
}
