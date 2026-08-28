import { useEffect, useState, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { CalendarCheck, Check, X } from 'lucide-react';
import { classesApi, attendanceApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import { Card, Select, PageLoader, EmptyState, Spinner } from '../../components/ui';
import { formatDateOnly, todayISO, reportError } from '../../lib/format';

export default function Attendance() {
  const { t } = useTranslation();
  const toast = useToast();
  const [searchParams] = useSearchParams();

  const [classes, setClasses] = useState([]);
  const [classId, setClassId] = useState(searchParams.get('classId') || '');
  const [date, setDate] = useState(todayISO());
  const [entries, setEntries] = useState([]);
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState(null);

  useEffect(() => {
    classesApi
      .list()
      .then(setClasses)
      .catch(() => setClasses([]));
  }, []);

  const refresh = useCallback(async () => {
    if (!classId || !date) {
      setEntries([]);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const roster = await attendanceApi.roster(classId, date);
      setEntries(roster.entries || []);
    } catch (err) {
      reportError(toast, err, t('common.fetchError'));
      setEntries([]);
    } finally {
      setLoading(false);
    }
  }, [classId, date, t]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const toggle = async (entry) => {
    if (!classId || !date) return;
    setBusyId(entry.studentId);
    try {
      if (entry.isPresent) {
        await attendanceApi.untick(classId, entry.studentId, date);
      } else {
        await attendanceApi.tick(classId, entry.studentId, date);
      }
      await refresh();
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setBusyId(null);
    }
  };

  const presentCount = entries.filter((e) => e.isPresent).length;

  return (
    <div className="space-y-5">
      <h1 className="text-xl font-bold text-text">{t('attendance.title')}</h1>

      <Card className="p-4 space-y-3">
        <div className="grid gap-3 sm:grid-cols-2">
          <Select
            label={t('attendance.selectClass')}
            value={classId}
            onChange={(e) => setClassId(e.target.value)}
          >
            <option value="">{t('app.select')}</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
          <div>
            <label className="block text-sm font-medium text-text mb-1.5">
              {t('attendance.selectDate')}
            </label>
            <input
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="w-full bg-surface border border-border rounded-xl px-3.5 py-2.5 text-sm text-text focus:border-primary focus:ring-2 focus:ring-primary/30"
            />
          </div>
        </div>
      </Card>

      {!classId ? (
        <Card className="p-2">
          <EmptyState icon={CalendarCheck} title={t('attendance.noRoster')} description={t('attendance.markHint')} />
        </Card>
      ) : loading ? (
        <PageLoader label={t('app.loading')} />
      ) : entries.length === 0 ? (
        <Card className="p-2">
          <EmptyState icon={CalendarCheck} title={t('attendance.noRoster')} description={t('attendance.markHint')} />
        </Card>
      ) : (
        <Card className="divide-y divide-border">
          <div className="flex items-center justify-between px-4 py-3 bg-elevated/40">
            <span className="text-sm text-muted">{formatDateOnly(date)}</span>
            <span className="text-sm font-medium text-text">
              {t('attendance.presentCount')}: {presentCount}/{entries.length}
            </span>
          </div>
          {entries.map((e) => (
            <button
              key={e.studentId}
              onClick={() => toggle(e)}
              disabled={busyId === e.studentId}
              className={`w-full flex items-center gap-3 px-4 py-3 transition-colors text-start ${
                e.isPresent ? 'bg-success/5 hover:bg-success/10' : 'hover:bg-elevated/50'
              }`}
            >
              <span
                className={`w-7 h-7 rounded-full flex items-center justify-center shrink-0 ${
                  e.isPresent
                    ? 'bg-success text-white'
                    : 'bg-elevated text-muted'
                }`}
              >
                {busyId === e.studentId ? (
                  <Spinner className="w-4 h-4" />
                ) : e.isPresent ? (
                  <Check className="w-4 h-4" />
                ) : (
                  <X className="w-4 h-4" />
                )}
              </span>
              <span className="flex-1 min-w-0">
                <span className="block font-medium text-text truncate">{e.studentName}</span>
                {e.isMakeUp && (
                  <span className="text-[11px] text-accent font-medium">{t('attendance.makeup')}</span>
                )}
              </span>
              <span
                className={`text-xs font-medium ${
                  e.isPresent ? 'text-success' : 'text-muted'
                }`}
              >
                {e.isPresent ? t('attendance.present') : t('attendance.absent')}
              </span>
            </button>
          ))}
        </Card>
      )}
    </div>
  );
}
