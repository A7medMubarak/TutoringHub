import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ClipboardList, Play, Eye } from 'lucide-react';
import { meApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import { Card, Button, PageLoader, EmptyState, Badge } from '../../components/ui';
import { typeLabel, formatDateTime, reportError } from '../../lib/format';

export default function MyQuizzes() {
  const { t } = useTranslation();
  const toast = useToast();
  const navigate = useNavigate();
  const [quizzes, setQuizzes] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        setQuizzes(await meApi.quizzes());
      } catch (err) {
        reportError(toast, err, t('common.fetchError'));
      } finally {
        setLoading(false);
      }
    })();
  }, [toast, t]);

  if (loading) return <PageLoader label={t('app.loading')} />;

  return (
    <div className="space-y-5">
      <h1 className="text-xl font-bold text-text">{t('myQuizzes.title')}</h1>
      {quizzes.length === 0 ? (
        <Card className="p-2">
          <EmptyState
            icon={ClipboardList}
            title={t('myQuizzes.noQuizzes')}
            description={t('myQuizzes.emptyHint')}
          />
        </Card>
      ) : (
        <div className="space-y-3">
          {quizzes.map((q) => (
            <Card key={q.quizId} className="p-4">
              <div className="flex items-start justify-between gap-3">
                <div className="min-w-0">
                  <p className="font-semibold text-text truncate">{q.title}</p>
                  <div className="flex flex-wrap items-center gap-2 mt-1 text-xs text-muted">
                    <Badge tone="neutral">{typeLabel(t, q.questionType)}</Badge>
                    <span>{t('quizzes.questionsCount')}: {q.questionCount}</span>
                    <span>{formatDateTime(q.publishedAtUtc)}</span>
                  </div>
                </div>
                {q.taken ? (
                  <Badge tone="success">{t('myQuizzes.taken')}</Badge>
                ) : (
                  <Badge tone="warning">{t('myQuizzes.new')}</Badge>
                )}
              </div>

              {q.taken && q.lastTotalCount ? (
                <p className="text-xs text-muted mt-2">
                  {t('myQuizzes.score')}: {q.lastCorrectCount}/{q.lastTotalCount} ·{' '}
                  {t('quizzes.attempts')}: {q.attemptCount}
                </p>
              ) : null}

              <div className="mt-3">
                <Button size="sm" onClick={() => navigate(`/my-quizzes/${q.quizId}`)}>
                  {q.taken ? <Eye className="w-3.5 h-3.5" /> : <Play className="w-3.5 h-3.5" />}
                  {q.taken ? t('myQuizzes.review') : t('myQuizzes.take')}
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
