import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Wallet } from 'lucide-react';
import { meApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import { Card, PageLoader, EmptyState, Badge } from '../../components/ui';
import { formatDateOnly, reportError } from '../../lib/format';

export default function MyQuotas() {
  const { t } = useTranslation();
  const toast = useToast();
  const [quotas, setQuotas] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        setQuotas(await meApi.quotas());
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
      <h1 className="text-xl font-bold text-text">{t('myQuotas.title')}</h1>
      {quotas.length === 0 ? (
        <Card className="p-2">
          <EmptyState icon={Wallet} title={t('myQuotas.noQuotas')} />
        </Card>
      ) : (
        <div className="space-y-3">
          {quotas.map((q) => {
            const paid = !!q.paidAt;
            return (
              <Card key={q.id} className="p-4">
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <p className="font-semibold text-text truncate">{q.className}</p>
                    <p className="text-xs text-muted">{formatDateOnly(q.periodStart)} – {formatDateOnly(q.periodEnd)}</p>
                  </div>
                  <Badge tone={paid ? 'success' : 'warning'}>
                    {paid ? t('myQuotas.paid') : t('myQuotas.unpaid')}
                  </Badge>
                </div>
                <div className="mt-3 grid grid-cols-3 gap-2 text-center">
                  <div className="rounded-xl bg-elevated py-2">
                    <p className="text-lg font-bold text-text tabular">{q.remainingSessions}</p>
                    <p className="text-[11px] text-muted">{t('myQuotas.remaining')}</p>
                  </div>
                  <div className="rounded-xl bg-elevated py-2">
                    <p className="text-lg font-bold text-text tabular">{q.totalSessions}</p>
                    <p className="text-[11px] text-muted">{t('myQuotas.total')}</p>
                  </div>
                  <div className="rounded-xl bg-elevated py-2">
                    <p className="text-lg font-bold text-text tabular">{q.price}</p>
                    <p className="text-[11px] text-muted">{t('myQuotas.price')}</p>
                  </div>
                </div>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
