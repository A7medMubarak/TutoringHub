import { useEffect, useState } from 'react';
import { CalendarDays } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { meApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import { Card, PageLoader, EmptyState, Badge } from '../../components/ui';
import { formatDateOnly, reportError } from '../../lib/format';

export default function MyAttendance() {
  const { t } = useTranslation();
  const toast = useToast();
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        setRows(await meApi.attendance());
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
      <h1 className="text-xl font-bold text-text">{t('myAttendance.title')}</h1>
      {rows.length === 0 ? (
        <Card className="p-2">
          <EmptyState icon={CalendarDays} title={t('myAttendance.noAttendance')} />
        </Card>
      ) : (
        <Card className="divide-y divide-border">
          {rows.map((r, i) => (
            <div key={`${r.classGroupId}-${r.date}-${i}`} className="flex items-center justify-between gap-3 px-4 py-3">
              <div className="min-w-0">
                <p className="font-medium text-text truncate">{r.className}</p>
                <p className="text-xs text-muted">{formatDateOnly(r.date)}</p>
              </div>
              {r.isMakeUp && <Badge tone="warning">{t('myAttendance.makeup')}</Badge>}
            </div>
          ))}
        </Card>
      )}
    </div>
  );
}
