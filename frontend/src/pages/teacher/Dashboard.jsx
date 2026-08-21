import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Users, BookOpen, ClipboardList, Plus, CalendarCheck } from 'lucide-react';
import { studentsApi, classesApi, quizzesApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import { Card, StatCard, PageLoader, EmptyState, Badge } from '../../components/ui';
import { formatDateTime, typeLabel } from '../../lib/format';

export default function Dashboard() {
  const { t } = useTranslation();
  const toast = useToast();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [counts, setCounts] = useState({ students: 0, classes: 0, quizzes: 0 });
  const [recent, setRecent] = useState([]);

  useEffect(() => {
    let active = true;
    (async () => {
      try {
        const [students, classes, quizzes] = await Promise.all([
          studentsApi.list(),
          classesApi.list(),
          quizzesApi.list(),
        ]);
        if (!active) return;
        setCounts({ students: students.length, classes: classes.length, quizzes: quizzes.length });
        setRecent(
          [...quizzes]
            .sort((a, b) => new Date(b.createdAtUtc) - new Date(a.createdAtUtc))
            .slice(0, 5)
        );
      } catch (err) {
        toast.error(t('common.fetchError'));
      } finally {
        if (active) setLoading(false);
      }
    })();
    return () => {
      active = false;
    };
  }, [toast, t]);

  if (loading) return <PageLoader label={t('app.loading')} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-text">{t('dashboard.title')}</h1>
      </div>

      <div className="grid grid-cols-3 gap-3">
        <StatCard icon={Users} label={t('dashboard.totalStudents')} value={counts.students} />
        <StatCard icon={BookOpen} label={t('dashboard.totalClasses')} value={counts.classes} />
        <StatCard icon={ClipboardList} label={t('dashboard.totalQuizzes')} value={counts.quizzes} />
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        <button
          onClick={() => navigate('/quizzes/new')}
          className="flex items-center gap-3 p-4 rounded-2xl bg-surface border border-border hover:border-primary/40 transition-colors text-start"
        >
          <span className="w-10 h-10 rounded-xl bg-primary-tint text-primary flex items-center justify-center">
            <Plus className="w-5 h-5" />
          </span>
          <div>
            <p className="font-medium text-text">{t('dashboard.createQuiz')}</p>
            <p className="text-xs text-muted">{t('common.mcq')} / {t('common.tf')}</p>
          </div>
        </button>
        <button
          onClick={() => navigate('/attendance')}
          className="flex items-center gap-3 p-4 rounded-2xl bg-surface border border-border hover:border-primary/40 transition-colors text-start"
        >
          <span className="w-10 h-10 rounded-xl bg-primary-tint text-primary flex items-center justify-center">
            <CalendarCheck className="w-5 h-5" />
          </span>
          <div>
            <p className="font-medium text-text">{t('dashboard.takeAttendance')}</p>
            <p className="text-xs text-muted">{t('attendance.markHint')}</p>
          </div>
        </button>
      </div>

      <Card className="p-4">
        <h2 className="font-semibold text-text mb-3">{t('dashboard.recentQuizzes')}</h2>
        {recent.length === 0 ? (
          <EmptyState title={t('dashboard.noQuizzes')} />
        ) : (
          <ul className="divide-y divide-border">
            {recent.map((q) => (
              <li key={q.id}>
                <Link
                  to={`/quizzes/${q.id}/results`}
                  className="flex items-center justify-between gap-3 py-3 hover:bg-elevated/50 rounded-lg px-2 -mx-2 transition-colors"
                >
                  <div className="min-w-0">
                    <p className="font-medium text-text truncate">{q.title}</p>
                    <p className="text-xs text-muted">{formatDateTime(q.createdAtUtc)}</p>
                  </div>
                  <div className="flex items-center gap-2 shrink-0">
                    <Badge tone="neutral">{typeLabel(t, q.questionType)}</Badge>
                    <Badge tone={q.publishedAtUtc ? 'success' : 'warning'}>
                      {q.publishedAtUtc ? t('quizzes.published') : t('quizzes.draft')}
                    </Badge>
                  </div>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </Card>
    </div>
  );
}
