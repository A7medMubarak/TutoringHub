import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { CalendarCheck, Users, Clock, CalendarDays } from 'lucide-react';
import { classesApi, sessionsApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import { Card, Button, PageLoader, EmptyState, Badge } from '../../components/ui';
import {
  dayLabel,
  freqLabel,
  formatTime,
  formatDateOnly,
  isoDateMonthsAgo,
  todayISO,
  reportError,
} from '../../lib/format';

export default function ClassDetail() {
  const { id } = useParams();
  const { t } = useTranslation();
  const toast = useToast();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [cls, setCls] = useState(null);
  const [sessions, setSessions] = useState([]);

  useEffect(() => {
    let active = true;
    (async () => {
      try {
        const list = await classesApi.list();
        const found = list.find((c) => String(c.id) === String(id));
        if (!found) {
          toast.error(t('common.fetchError'));
          navigate('/classes');
          return;
        }
        setCls(found);
        setSessions(
          await sessionsApi.list(found.id, isoDateMonthsAgo(3), todayISO())
        );
      } catch (err) {
        reportError(toast, err, t('common.fetchError'));
      } finally {
        if (active) setLoading(false);
      }
    })();
    return () => {
      active = false;
    };
  }, [id, navigate, toast, t]);

  if (loading) return <PageLoader label={t('app.loading')} />;
  if (!cls) return null;

  return (
    <div className="space-y-5">
      <Link to="/classes" className="text-sm text-muted hover:text-text inline-flex items-center gap-1">
        <span>←</span> {t('app.back')}
      </Link>

      <div className="flex items-start justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-text">{cls.name}</h1>
          <p className="text-sm text-muted">{cls.centerName}</p>
        </div>
        <Button size="sm" onClick={() => navigate(`/attendance?classId=${cls.id}`)}>
          <CalendarCheck className="w-4 h-4" />
          {t('classDetail.takeAttendance')}
        </Button>
      </div>

      <Card className="p-4 grid gap-3 sm:grid-cols-3">
        <div className="flex items-center gap-3">
          <span className="w-9 h-9 rounded-xl bg-primary-tint text-primary flex items-center justify-center">
            <CalendarDays className="w-4 h-4" />
          </span>
          <div>
            <p className="text-xs text-muted">{t('classDetail.day')}</p>
            <p className="font-medium text-text">{dayLabel(t, cls.dayOfWeek)}</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <span className="w-9 h-9 rounded-xl bg-primary-tint text-primary flex items-center justify-center">
            <Clock className="w-4 h-4" />
          </span>
          <div>
            <p className="text-xs text-muted">{t('classDetail.time')}</p>
            <p className="font-medium text-text">{formatTime(cls.startTime)}</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <span className="w-9 h-9 rounded-xl bg-primary-tint text-primary flex items-center justify-center">
            <Users className="w-4 h-4" />
          </span>
          <div>
            <p className="text-xs text-muted">{t('classes.studentCount')}</p>
            <p className="font-medium text-text">{cls.studentCount}</p>
          </div>
        </div>
      </Card>

      <Card className="p-4">
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-semibold text-text">{t('classDetail.sessions')}</h2>
          <Badge tone="neutral">{freqLabel(t, cls.frequency)}</Badge>
        </div>
        {sessions.length === 0 ? (
          <EmptyState icon={CalendarCheck} title={t('classDetail.noSessions')} />
        ) : (
          <ul className="divide-y divide-border">
            {sessions.map((s) => (
              <li key={s.id} className="flex items-center justify-between py-3 px-1">
                <span className="text-text">{formatDateOnly(s.date)}</span>
                <span className="text-xs text-muted">
                  {t('attendance.presentCount')}: {s.presentCount}
                </span>
              </li>
            ))}
          </ul>
        )}
      </Card>
    </div>
  );
}
