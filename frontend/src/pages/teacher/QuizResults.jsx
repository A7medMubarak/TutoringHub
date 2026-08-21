import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { BarChart3 } from 'lucide-react';
import { quizzesApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import { Card, PageLoader, EmptyState, Badge, ProgressBar } from '../../components/ui';
import { formatDateTime, reportError } from '../../lib/format';

export default function QuizResults() {
  const { id } = useParams();
  const { t } = useTranslation();
  const toast = useToast();

  const [title, setTitle] = useState('');
  const [scores, setScores] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const [detail, results] = await Promise.all([
          quizzesApi.detail(id),
          quizzesApi.results(id),
        ]);
        setTitle(detail.title);
        setScores(results || []);
      } catch (err) {
        reportError(toast, err, t('common.fetchError'));
      } finally {
        setLoading(false);
      }
    })();
  }, [id, toast, t]);

  if (loading) return <PageLoader label={t('app.loading')} />;

  return (
    <div className="space-y-5">
      <Link to="/quizzes" className="text-sm text-muted hover:text-text inline-flex items-center gap-1">
        <span>←</span> {t('quizResults.back')}
      </Link>

      <div>
        <h1 className="text-xl font-bold text-text">{t('quizResults.title')}</h1>
        <p className="text-sm text-muted">{title}</p>
      </div>

      {scores.length === 0 ? (
        <Card className="p-2">
          <EmptyState icon={BarChart3} title={t('quizResults.noResults')} />
        </Card>
      ) : (
        <Card className="divide-y divide-border">
          {scores.map((s) => {
            const pct =
              s.hasTaken && s.firstAttemptTotal
                ? Math.round((s.firstAttemptCorrect / s.firstAttemptTotal) * 100)
                : 0;
            return (
              <div key={s.studentId} className="px-4 py-3">
                <div className="flex items-center justify-between gap-3">
                  <p className="font-medium text-text truncate">{s.studentName}</p>
                  {s.hasTaken ? (
                    <Badge tone="success">{t('quizResults.taken')}</Badge>
                  ) : (
                    <Badge tone="neutral">{t('quizResults.notTaken')}</Badge>
                  )}
                </div>
                {s.hasTaken ? (
                  <div className="mt-2 space-y-1.5">
                    <div className="flex items-center justify-between text-xs text-muted">
                      <span>
                        {t('quizResults.firstAttempt')}: {s.firstAttemptCorrect}/{s.firstAttemptTotal}
                      </span>
                      <span>
                        {t('quizResults.attempts')}: {s.attemptCount}
                      </span>
                      <span>{pct}%</span>
                    </div>
                    <ProgressBar value={pct} />
                    <p className="text-[11px] text-muted">
                      {formatDateTime(s.firstAttemptAtUtc)}
                    </p>
                  </div>
                ) : (
                  <p className="text-xs text-muted mt-0.5">{t('quizResults.notTaken')}</p>
                )}
              </div>
            );
          })}
        </Card>
      )}
    </div>
  );
}
