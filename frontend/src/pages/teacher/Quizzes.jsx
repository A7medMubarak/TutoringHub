import { useEffect, useState, useCallback } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, ClipboardList, BarChart3, Trash2, Send, EyeOff, Check } from 'lucide-react';
import { quizzesApi, classesApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import {
  Card,
  Button,
  PageLoader,
  EmptyState,
  Badge,
  Modal,
  ConfirmDialog,
} from '../../components/ui';
import { typeLabel, formatDateTime, reportError } from '../../lib/format';

export default function Quizzes() {
  const { t } = useTranslation();
  const toast = useToast();
  const navigate = useNavigate();

  const [quizzes, setQuizzes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [classes, setClasses] = useState([]);

  const [publishId, setPublishId] = useState(null);
  const [selected, setSelected] = useState([]);
  const [publishing, setPublishing] = useState(false);

  const [deleteId, setDeleteId] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [qs, cs] = await Promise.all([quizzesApi.list(), classesApi.list()]);
      setQuizzes(qs);
      setClasses(cs);
    } catch (err) {
      reportError(toast, err, t('common.fetchError'));
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    load();
  }, [load]);

  const openPublish = (q) => {
    setPublishId(q.id);
    setSelected(q.publishedClassNames || []);
  };

  const toggleClass = (name) => {
    setSelected((prev) =>
      prev.includes(name) ? prev.filter((n) => n !== name) : [...prev, name]
    );
  };

  const doPublish = async () => {
    setPublishing(true);
    try {
      const ids = classes
        .filter((c) => selected.includes(c.name))
        .map((c) => c.id);
      await quizzesApi.publish(publishId, ids);
      toast.success(t('common.publishSuccess'));
      setPublishId(null);
      await load();
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setPublishing(false);
    }
  };

  const unpublish = async (q, name) => {
    const cls = classes.find((c) => c.name === name);
    if (!cls) return;
    try {
      await quizzesApi.unpublish(q.id, cls.id);
      toast.success(t('common.unpublishSuccess'));
      await load();
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    }
  };

  const confirmDelete = async () => {
    setDeleting(true);
    try {
      await quizzesApi.remove(deleteId);
      toast.success(t('common.deleteSuccess'));
      setDeleteId(null);
      await load();
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-text">{t('quizzes.title')}</h1>
        <Button size="sm" onClick={() => navigate('/quizzes/new')}>
          <Plus className="w-4 h-4" />
          {t('quizzes.new')}
        </Button>
      </div>

      {loading ? (
        <PageLoader label={t('app.loading')} />
      ) : quizzes.length === 0 ? (
        <Card className="p-2">
          <EmptyState
            icon={ClipboardList}
            title={t('quizzes.noQuizzes')}
            description={t('quizzes.emptyHint')}
            action={
              <Button size="sm" onClick={() => navigate('/quizzes/new')}>
                <Plus className="w-4 h-4" />
                {t('quizzes.new')}
              </Button>
            }
          />
        </Card>
      ) : (
        <div className="space-y-3">
          {quizzes.map((q) => (
            <Card key={q.id} className="p-4">
              <div className="flex items-start justify-between gap-3">
                <div className="min-w-0">
                  <p className="font-semibold text-text truncate">{q.title}</p>
                  <div className="flex flex-wrap items-center gap-2 mt-1 text-xs text-muted">
                    <Badge tone="neutral">{typeLabel(t, q.questionType)}</Badge>
                    <span>{t('quizzes.questionsCount')}: {q.questionCount}</span>
                    <span>{t('quizzes.attempts')}: {q.attemptCount}</span>
                    <span>{formatDateTime(q.createdAtUtc)}</span>
                  </div>
                </div>
                <Badge tone={q.publishedAtUtc ? 'success' : 'warning'}>
                  {q.publishedAtUtc ? t('quizzes.published') : t('quizzes.draft')}
                </Badge>
              </div>

              {q.publishedClassNames?.length > 0 && (
                <div className="flex flex-wrap gap-1.5 mt-3">
                  {q.publishedClassNames.map((n) => (
                    <button
                      key={n}
                      onClick={() => unpublish(q, n)}
                      className="inline-flex items-center gap-1 px-2 py-1 rounded-full bg-success/10 text-success text-xs hover:bg-danger/10 hover:text-danger transition-colors"
                      title={t('quizzes.unpublish')}
                    >
                      {n}
                      <EyeOff className="w-3 h-3" />
                    </button>
                  ))}
                </div>
              )}

              <div className="flex flex-wrap gap-2 mt-3">
                <Button size="sm" variant="outline" onClick={() => openPublish(q)}>
                  <Send className="w-3.5 h-3.5" />
                  {t('quizzes.publish')}
                </Button>
                <Link to={`/quizzes/${q.id}/results`}>
                  <Button size="sm" variant="secondary">
                    <BarChart3 className="w-3.5 h-3.5" />
                    {t('quizzes.results')}
                  </Button>
                </Link>
                <Button size="sm" variant="ghost" onClick={() => setDeleteId(q.id)} className="text-danger hover:bg-danger/10">
                  <Trash2 className="w-3.5 h-3.5" />
                  {t('quizzes.delete')}
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}

      <Modal
        open={publishId !== null}
        onClose={() => setPublishId(null)}
        title={t('quizzes.publish')}
        footer={
          <>
            <Button variant="secondary" onClick={() => setPublishId(null)} disabled={publishing}>
              {t('app.cancel')}
            </Button>
            <Button onClick={doPublish} loading={publishing}>
              {t('quizzes.publish')}
            </Button>
          </>
        }
      >
        <p className="text-sm text-muted mb-3">{t('quizzes.publishedClasses')}</p>
        {classes.length === 0 ? (
          <p className="text-sm text-muted">{t('app.noResults')}</p>
        ) : (
          <div className="space-y-2">
            {classes.map((c) => {
              const on = selected.includes(c.name);
              return (
                <button
                  key={c.id}
                  onClick={() => toggleClass(c.name)}
                  className={`w-full flex items-center justify-between gap-2 px-3 py-2.5 rounded-xl border transition-colors ${
                    on ? 'border-primary bg-primary-tint text-primary' : 'border-border text-text'
                  }`}
                >
                  <span className="font-medium">{c.name}</span>
                  <span
                    className={`w-5 h-5 rounded-md flex items-center justify-center ${
                      on ? 'bg-primary text-white' : 'bg-elevated'
                    }`}
                  >
                    {on && <Check className="w-3.5 h-3.5" />}
                  </span>
                </button>
              );
            })}
          </div>
        )}
      </Modal>

      <ConfirmDialog
        open={deleteId !== null}
        title={t('app.delete')}
        message={t('quizzes.deleteConfirm')}
        confirmLabel={t('app.delete')}
        cancelLabel={t('app.cancel')}
        danger
        loading={deleting}
        onConfirm={confirmDelete}
        onCancel={() => setDeleteId(null)}
      />
    </div>
  );
}
