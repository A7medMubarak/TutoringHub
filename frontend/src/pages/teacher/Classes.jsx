import { useEffect, useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { BookOpen, Plus, Search } from 'lucide-react';
import { classesApi, centersApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import {
  Card,
  Button,
  TextField,
  Select,
  PageLoader,
  EmptyState,
  Modal,
} from '../../components/ui';
import { dayLabel, freqLabel, formatTime, reportError } from '../../lib/format';

export default function Classes() {
  const { t } = useTranslation();
  const toast = useToast();
  const navigate = useNavigate();

  const [classes, setClasses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState('');

  const [modalOpen, setModalOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [centers, setCenters] = useState([]);
  const [form, setForm] = useState({
    name: '',
    centerId: '',
    dayOfWeek: '0',
    startTime: '08:30',
    frequency: '0',
  });
  const [errors, setErrors] = useState({});

  const load = async () => {
    setLoading(true);
    try {
      setClasses(await classesApi.list());
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
    if (!q) return classes;
    return classes.filter((c) => c.name.toLowerCase().includes(q));
  }, [classes, query]);

  const openModal = async () => {
    setErrors({});
    setForm({ name: '', centerId: '', dayOfWeek: '0', startTime: '08:30', frequency: '0' });
    setModalOpen(true);
    try {
      setCenters(await centersApi.list());
    } catch {
      setCenters([]);
    }
  };

  const submit = async (ev) => {
    ev.preventDefault();
    const e = {};
    if (!form.name.trim()) e.name = t('app.required');
    if (!form.centerId) e.centerId = t('app.required');
    setErrors(e);
    if (Object.keys(e).length) return;
    setSaving(true);
    try {
      await classesApi.create({
        name: form.name.trim(),
        centerId: Number(form.centerId),
        dayOfWeek: Number(form.dayOfWeek),
        startTime: `${form.startTime}:00`,
        frequency: Number(form.frequency),
      });
      toast.success(t('common.createSuccess'));
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
        <h1 className="text-xl font-bold text-text">{t('classes.title')}</h1>
        <Button onClick={openModal} size="sm">
          <Plus className="w-4 h-4" />
          {t('classes.new')}
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
            icon={BookOpen}
            title={classes.length === 0 ? t('classes.noClasses') : t('app.noResults')}
            description={classes.length === 0 ? t('classes.emptyHint') : undefined}
            action={
              classes.length === 0 ? (
                <Button onClick={openModal} size="sm">
                  <Plus className="w-4 h-4" />
                  {t('classes.new')}
                </Button>
              ) : undefined
            }
          />
        </Card>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2">
          {filtered.map((c) => (
            <button
              key={c.id}
              onClick={() => navigate(`/classes/${c.id}`)}
              className="text-start bg-surface border border-border rounded-2xl p-4 hover:border-primary/40 transition-colors"
            >
              <div className="flex items-start justify-between gap-2">
                <div className="min-w-0">
                  <p className="font-semibold text-text truncate">{c.name}</p>
                  <p className="text-xs text-muted">{c.centerName}</p>
                </div>
                <span className="text-xs px-2 py-0.5 rounded-full bg-elevated text-muted shrink-0">
                  {t('classes.studentCount')}: {c.studentCount}
                </span>
              </div>
              <div className="mt-3 flex flex-wrap gap-2 text-xs">
                <span className="px-2 py-1 rounded-lg bg-primary-tint text-primary">
                  {dayLabel(t, c.dayOfWeek)}
                </span>
                <span className="px-2 py-1 rounded-lg bg-elevated text-muted">
                  {formatTime(c.startTime)}
                </span>
                <span className="px-2 py-1 rounded-lg bg-elevated text-muted">
                  {freqLabel(t, c.frequency)}
                </span>
              </div>
            </button>
          ))}
        </div>
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={t('classes.newTitle')}
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
            label={t('classes.name')}
            required
            value={form.name}
            error={errors.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
          />
          <Select
            label={t('classes.center')}
            value={form.centerId}
            error={errors.centerId}
            onChange={(e) => setForm({ ...form, centerId: e.target.value })}
          >
            <option value="">{t('app.select')}</option>
            {centers.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
          <div className="grid grid-cols-2 gap-3">
            <Select
              label={t('classes.day')}
              value={form.dayOfWeek}
              onChange={(e) => setForm({ ...form, dayOfWeek: e.target.value })}
            >
              {[0, 1, 2, 3, 4, 5, 6].map((d) => (
                <option key={d} value={d}>
                  {dayLabel(t, d)}
                </option>
              ))}
            </Select>
            <Select
              label={t('classes.frequency')}
              value={form.frequency}
              onChange={(e) => setForm({ ...form, frequency: e.target.value })}
            >
              {[0, 1, 2].map((f) => (
                <option key={f} value={f}>
                  {freqLabel(t, f)}
                </option>
              ))}
            </Select>
          </div>
          <TextField
            label={t('classes.time')}
            type="time"
            value={form.startTime}
            onChange={(e) => setForm({ ...form, startTime: e.target.value })}
          />
        </form>
      </Modal>
    </div>
  );
}
