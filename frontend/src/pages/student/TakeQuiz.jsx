import { useEffect, useState, useCallback } from 'react';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Check, X, ArrowRight, ArrowLeft, RotateCcw, Trophy } from 'lucide-react';
import { meApi } from '../../api';
import { useToast } from '../../context/ToastContext';
import {
  Card,
  Button,
  PageLoader,
  ProgressBar,
  ConfirmDialog,
} from '../../components/ui';
import { isTrueFalse, reportError } from '../../lib/format';

function boolLabel(t, v) {
  return v ? t('quizCreate.trueLabel') : t('quizCreate.falseLabel');
}

export default function TakeQuiz() {
  const { id } = useParams();
  const { t } = useTranslation();
  const toast = useToast();

  const [quiz, setQuiz] = useState(null);
  const [loading, setLoading] = useState(true);
  const [index, setIndex] = useState(0);
  const [answers, setAnswers] = useState([]);
  const [phase, setPhase] = useState('quiz');
  const [result, setResult] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const q = await meApi.quizDetail(id);
      setQuiz(q);
      setAnswers(new Array(q.questions.length).fill(null));
      setIndex(0);
      setPhase('quiz');
      setResult(null);
    } catch (err) {
      reportError(toast, err, t('common.fetchError'));
    } finally {
      setLoading(false);
    }
  }, [id, toast, t]);

  useEffect(() => {
    load();
  }, [load]);

  if (loading) return <PageLoader label={t('app.loading')} />;
  if (!quiz) return null;

  const type = quiz.questionType;
  const tf = isTrueFalse(type);
  const current = quiz.questions[index];
  const selected = answers[index];

  const choose = (val) => {
    setAnswers((prev) => prev.map((a, i) => (i === index ? val : a)));
  };

  const answeredCount = answers.filter((a) => a !== null && a !== undefined).length;
  const isLast = index === quiz.questions.length - 1;

  const doSubmit = async () => {
    setSubmitting(true);
    try {
      const payload = {
        answers: answers.map((a) =>
          tf ? { optionIndex: null, isTrue: a } : { optionIndex: a, isTrue: null }
        ),
      };
      const res = await meApi.submitAttempt(id, payload);
      setResult(res);
      setPhase('result');
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setSubmitting(false);
      setConfirmOpen(false);
    }
  };

  if (phase === 'result' && result) {
    return (
      <div className="space-y-5">
        <Card className="p-6 text-center">
          <div className="w-14 h-14 rounded-2xl bg-primary-tint text-primary flex items-center justify-center mx-auto mb-3">
            <Trophy className="w-7 h-7" />
          </div>
          <h1 className="text-xl font-bold text-text">{t('takeQuiz.score')}</h1>
          <p className="text-3xl font-extrabold text-primary mt-1 tabular">
            {result.percentage}%
          </p>
          <p className="text-sm text-muted">
            {result.correctCount}/{result.totalCount} · {t('common.correct')}
          </p>
          {!result.isFirstAttempt && (
            <p className="text-xs text-muted mt-2">{t('takeQuiz.retakeNote')}</p>
          )}
        </Card>

        <div className="space-y-3">
          {result.results.map((r, i) => {
            const opts = quiz.questions[i]?.options || [];
            const yourText = tf
              ? r.yourAnswer === null || r.yourAnswer === undefined
                ? '—'
                : boolLabel(t, r.yourAnswer)
              : r.yourIndex != null
              ? opts[r.yourIndex] || '—'
              : '—';
            const correctText = tf
              ? boolLabel(t, r.correctAnswer)
              : r.correctIndex != null
              ? opts[r.correctIndex] || '—'
              : '—';
            return (
              <Card key={i} className="p-4">
                <div className="flex items-start justify-between gap-3">
                  <p className="font-medium text-text">
                    {i + 1}. {r.question}
                  </p>
                  {r.isCorrect ? (
                    <span className="w-6 h-6 shrink-0 rounded-full bg-success text-white flex items-center justify-center">
                      <Check className="w-4 h-4" />
                    </span>
                  ) : (
                    <span className="w-6 h-6 shrink-0 rounded-full bg-danger text-white flex items-center justify-center">
                      <X className="w-4 h-4" />
                    </span>
                  )}
                </div>
                <div className="mt-2 text-sm space-y-0.5">
                  <p className="text-muted">
                    {t('takeQuiz.yourAnswer')}: <span className="text-text">{yourText}</span>
                  </p>
                  {!r.isCorrect && (
                    <p className="text-muted">
                      {t('takeQuiz.correctAnswer')}:{' '}
                      <span className="text-success">{correctText}</span>
                    </p>
                  )}
                </div>
              </Card>
            );
          })}
        </div>

        <Button variant="outline" fullWidth onClick={load}>
          <RotateCcw className="w-4 h-4" />
          {t('takeQuiz.retake')}
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-5">
      <div>
        <p className="text-sm text-muted">{quiz.title}</p>
        <div className="flex items-center justify-between mt-2 text-xs text-muted">
          <span>
            {t('takeQuiz.questionOf', { current: index + 1, total: quiz.questions.length })}
          </span>
          <span>
            {t('takeQuiz.answered')}: {answeredCount}/{quiz.questions.length}
          </span>
        </div>
        <ProgressBar value={(answeredCount / quiz.questions.length) * 100} className="mt-2" />
      </div>

      <Card className="p-5 space-y-4">
        <h2 className="text-lg font-semibold text-text">{current.question}</h2>

        {tf ? (
          <div className="grid grid-cols-2 gap-2">
            {[true, false].map((val) => (
              <button
                key={String(val)}
                type="button"
                onClick={() => choose(val)}
                className={`flex items-center justify-center gap-2 py-3 rounded-xl border text-base font-medium transition-colors ${
                  selected === val
                    ? 'border-primary bg-primary-tint text-primary'
                    : 'border-border text-text'
                }`}
              >
                {selected === val && <Check className="w-4 h-4" />}
                {boolLabel(t, val)}
              </button>
            ))}
          </div>
        ) : (
          <div className="space-y-2">
            {current.options.map((opt, oIdx) => (
              <button
                key={oIdx}
                type="button"
                onClick={() => choose(oIdx)}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl border text-start transition-colors ${
                  selected === oIdx
                    ? 'border-primary bg-primary-tint text-primary'
                    : 'border-border text-text'
                }`}
              >
                <span
                  className={`w-5 h-5 shrink-0 rounded-full border flex items-center justify-center text-xs ${
                    selected === oIdx ? 'bg-primary text-white border-primary' : 'border-border'
                  }`}
                >
                  {String.fromCharCode(65 + oIdx)}
                </span>
                {opt}
              </button>
            ))}
          </div>
        )}
      </Card>

      <div className="flex items-center justify-between gap-2">
        <Button variant="secondary" onClick={() => setIndex((i) => Math.max(0, i - 1))} disabled={index === 0}>
          <ArrowLeft className="w-4 h-4" />
          {t('takeQuiz.back')}
        </Button>
        {isLast ? (
          <Button onClick={() => setConfirmOpen(true)} loading={submitting}>
            <Check className="w-4 h-4" />
            {t('takeQuiz.submit')}
          </Button>
        ) : (
          <Button onClick={() => setIndex((i) => Math.min(quiz.questions.length - 1, i + 1))}>
            {t('takeQuiz.next')}
            <ArrowRight className="w-4 h-4" />
          </Button>
        )}
      </div>

      <ConfirmDialog
        open={confirmOpen}
        title={t('takeQuiz.submit')}
        message={t('takeQuiz.submitConfirm')}
        confirmLabel={t('takeQuiz.submit')}
        cancelLabel={t('app.cancel')}
        loading={submitting}
        onConfirm={doSubmit}
        onCancel={() => setConfirmOpen(false)}
      />
    </div>
  );
}
