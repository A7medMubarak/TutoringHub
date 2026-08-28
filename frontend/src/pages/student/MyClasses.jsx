import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { BookOpen, Clock, Users } from 'lucide-react';
import { meApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import { Card, PageLoader, EmptyState } from '../../components/ui';
import { dayLabel, freqLabel, formatTime, reportError } from '../../lib/format';

export default function MyClasses() {
  const { t } = useTranslation();
  const toast = useToast();
  const [classes, setClasses] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        setClasses(await meApi.classes());
      } catch (err) {
        reportError(toast, err, t('common.fetchError'));
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  if (loading) return <PageLoader label={t('app.loading')} />;

  return (
    <div className="space-y-5">
      <h1 className="text-xl font-bold text-text">{t('myClasses.title')}</h1>
      {classes.length === 0 ? (
        <Card className="p-2">
          <EmptyState icon={BookOpen} title={t('myClasses.noClasses')} />
        </Card>
      ) : (
        <div className="grid gap-3">
          {classes.map((c) => (
            <Card key={c.id} className="p-4 space-y-3">
              <div>
                <p className="font-semibold text-text">{c.name}</p>
                <p className="text-xs text-muted">{c.centerName}</p>
              </div>
              <div className="flex flex-wrap gap-2 text-xs">
                <span className="px-2 py-1 rounded-lg bg-primary-tint text-primary">
                  {dayLabel(t, c.dayOfWeek)}
                </span>
                <span className="inline-flex items-center gap-1 px-2 py-1 rounded-lg bg-elevated text-muted">
                  <Clock className="w-3 h-3" />
                  {formatTime(c.startTime)}
                </span>
                <span className="px-2 py-1 rounded-lg bg-elevated text-muted">
                  {freqLabel(t, c.frequency)}
                </span>
                <span className="inline-flex items-center gap-1 px-2 py-1 rounded-lg bg-elevated text-muted">
                  <Users className="w-3 h-3" />
                  {c.studentCount}
                </span>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
