import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Sparkles, Plus, Trash2, Save, Check, X, Wand2 } from 'lucide-react';
import { quizzesApi, aiApi } from '../api';
import { useToast } from '../context/ToastContext';
import {
  Card,
  Button,
  TextField,
  TextArea,
  Select,
  PageLoader,
  EmptyState,
} from '../components/ui';
import { QUESTION_TYPE, isTrueFalse, reportError } from '../lib/format';

function blankQuestion(type = QUESTION_TYPE.MultipleChoice) {
  return {
    question: '',
    type,
    options: type === QUESTION_TYPE.MultipleChoice ? ['', ''] : [],
    correctIndex: 0,
    isTrue: true,
  };
}

export default function QuizCreate() {
  const { t } = useTranslation();
  const toast = useToast();
  const navigate = useNavigate();

  const [tab, setTab] = useState('ai');

  const [title, setTitle] = useState('');
  const [questions, setQuestions] = useState([blankQuestion()]);

  // AI form
  const [aiTopic, setAiTopic] = useState('');
  const [aiCount, setAiCount] = useState(10);
  const [aiType, setAiType] = useState(QUESTION_TYPE.MultipleChoice);
  const [generating, setGenerating] = useState(false);

  const [saving, setSaving] = useState(false);

  const updateQ = (idx, patch) =>
    setQuestions((prev) => prev.map((q, i) => (i === idx ? { ...q, ...patch } : q)));

  const addQuestion = () =>
    setQuestions((prev) => [...prev, blankQuestion(QUESTION_TYPE.MultipleChoice)]);

  const removeQuestion = (idx) =>
    setQuestions((prev) => prev.filter((_, i) => i !== idx));

  const setType = (idx, type) => {
    if (type === QUESTION_TYPE.TrueFalse) {
      updateQ(idx, { type, options: [], correctIndex: null, isTrue: true });
    } else {
      updateQ(idx, {
        type,
        options: ['', ''],
        correctIndex: 0,
        isTrue: null,
      });
    }
  };

  const setOption = (qIdx, oIdx, value) =>
    setQuestions((prev) =>
      prev.map((q, i) =>
        i === qIdx
          ? { ...q, options: q.options.map((o, j) => (j === oIdx ? value : o)) }
          : q
      )
    );

  const addOption = (idx) =>
    setQuestions((prev) => prev.map((q, i) => (i === idx ? { ...q, options: [...q.options, ''] } : q)));

  const removeOption = (qIdx, oIdx) =>
    setQuestions((prev) =>
      prev.map((q, i) =>
        i === qIdx && q.options.length > 2
          ? {
              ...q,
              options: q.options.filter((_, j) => j !== oIdx),
              correctIndex:
                q.correctIndex === oIdx ? 0 : q.correctIndex > oIdx ? q.correctIndex - 1 : q.correctIndex,
            }
          : q
      )
    );

  const generate = async () => {
    if (!aiTopic.trim()) {
      toast.error(t('app.required'));
      return;
    }
    setGenerating(true);
    try {
      const fd = new FormData();
      fd.append('Topic', aiTopic.trim());
      fd.append('QuestionCount', String(aiCount));
      fd.append('QuestionType', String(aiType));
      const res = await aiApi.generateQuiz(fd);
      const mapped = (res.questions || []).map((gq) => {
        if (gq.options && gq.options.length) {
          return {
            question: gq.question,
            type: QUESTION_TYPE.MultipleChoice,
            options: gq.options,
            correctIndex: gq.correctIndex ?? 0,
            isTrue: null,
          };
        }
        return {
          question: gq.question,
          type: QUESTION_TYPE.TrueFalse,
          options: [],
          correctIndex: null,
          isTrue: gq.isTrue ?? true,
        };
      });
      if (mapped.length === 0) {
        toast.error(t('quizCreate.aiError'));
        return;
      }
      if (!title.trim()) setTitle(aiTopic.trim());
      setQuestions(mapped);
      setTab('manual');
      toast.success(t('quizCreate.generateSuccess'));
    } catch (err) {
      reportError(toast, err, t('quizCreate.aiError'));
    } finally {
      setGenerating(false);
    }
  };

  const validate = () => {
    if (!title.trim()) {
      toast.error(t('app.required'));
      return false;
    }
    for (const q of questions) {
      if (!q.question.trim()) {
        toast.error(t('app.required'));
        return false;
      }
      if (isTrueFalse(q.type)) {
        if (q.isTrue === null || q.isTrue === undefined) {
          toast.error(t('app.required'));
          return false;
        }
      } else {
        const filled = q.options.filter((o) => o.trim());
        if (filled.length < 2) {
          toast.error(t('app.required'));
          return false;
        }
        if (q.correctIndex == null || !q.options[q.correctIndex]?.trim()) {
          toast.error(t('app.required'));
          return false;
        }
      }
    }
    return true;
  };

  const save = async () => {
    if (!validate()) return;
    setSaving(true);
    try {
      const payload = {
        title: title.trim(),
        questions: questions.map((q) =>
          isTrueFalse(q.type)
            ? { question: q.question.trim(), options: null, correctIndex: null, isTrue: q.isTrue }
            : {
                question: q.question.trim(),
                options: q.options.map((o) => o.trim()).filter(Boolean),
                correctIndex: q.correctIndex,
                isTrue: null,
              }
        ),
      };
      await quizzesApi.create(payload);
      toast.success(t('quizCreate.saveSuccess'));
      navigate('/quizzes');
    } catch (err) {
      reportError(toast, err, t('common.genericError'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-bold text-text">{t('quizCreate.title')}</h1>
        <Button onClick={save} loading={saving}>
          <Save className="w-4 h-4" />
          {t('quizCreate.save')}
        </Button>
      </div>

      <TextField
        label={t('quizCreate.titlePlaceholder')}
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        placeholder={t('quizCreate.titlePlaceholder')}
      />

      <div className="grid grid-cols-2 gap-1 p-1 bg-elevated rounded-xl">
        <button
          type="button"
          onClick={() => setTab('ai')}
          className={`flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-medium transition-all ${
            tab === 'ai' ? 'bg-surface text-primary shadow-sm' : 'text-muted hover:text-text'
          }`}
        >
          <Sparkles className="w-4 h-4" />
          {t('quizCreate.aiTab')}
        </button>
        <button
          type="button"
          onClick={() => setTab('manual')}
          className={`flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-medium transition-all ${
            tab === 'manual' ? 'bg-surface text-primary shadow-sm' : 'text-muted hover:text-text'
          }`}
        >
          <Wand2 className="w-4 h-4" />
          {t('quizCreate.manualTab')}
        </button>
      </div>

      {tab === 'ai' ? (
        <Card className="p-4 space-y-4">
          <TextField
            label={t('quizCreate.topic')}
            value={aiTopic}
            onChange={(e) => setAiTopic(e.target.value)}
            placeholder={t('quizCreate.topicPlaceholder')}
          />
          <div className="grid grid-cols-2 gap-3">
            <TextField
              label={t('quizCreate.count')}
              type="number"
              min="1"
              max="30"
              value={aiCount}
              onChange={(e) => setAiCount(Number(e.target.value))}
            />
            <Select
              label={t('quizCreate.type')}
              value={aiType}
              onChange={(e) => setAiType(Number(e.target.value))}
            >
              <option value={QUESTION_TYPE.MultipleChoice}>{t('quizCreate.mcq')}</option>
              <option value={QUESTION_TYPE.TrueFalse}>{t('quizCreate.tf')}</option>
            </Select>
          </div>
          <Button onClick={generate} loading={generating} fullWidth>
            <Sparkles className="w-4 h-4" />
            {generating ? t('quizCreate.generating') : t('quizCreate.generate')}
          </Button>
          <p className="text-xs text-muted">{t('quizCreate.aiError')}</p>
        </Card>
      ) : (
        <div className="space-y-3">
          {questions.length === 0 ? (
            <Card className="p-2">
              <EmptyState title={t('quizzes.empty')} />
            </Card>
          ) : (
            questions.map((q, idx) => (
              <Card key={idx} className="p-4 space-y-3">
                <div className="flex items-start gap-2">
                  <span className="text-sm font-semibold text-muted pt-2">
                    {idx + 1}.
                  </span>
                  <TextArea
                    label={t('quizCreate.question')}
                    className="flex-1"
                    value={q.question}
                    onChange={(e) => updateQ(idx, { question: e.target.value })}
                  />
                  <button
                    onClick={() => removeQuestion(idx)}
                    className="mt-7 w-9 h-9 shrink-0 rounded-lg border border-border text-muted hover:text-danger hover:border-danger/40 transition-colors"
                    title={t('quizCreate.removeQuestion')}
                  >
                    <Trash2 className="w-4 h-4 mx-auto" />
                  </button>
                </div>

                <Select
                  label={t('quizCreate.type')}
                  value={q.type}
                  onChange={(e) => setType(idx, Number(e.target.value))}
                >
                  <option value={QUESTION_TYPE.MultipleChoice}>{t('quizCreate.mcq')}</option>
                  <option value={QUESTION_TYPE.TrueFalse}>{t('quizCreate.tf')}</option>
                </Select>

                {isTrueFalse(q.type) ? (
                  <div>
                    <p className="text-xs font-medium text-text mb-1.5">{t('quizCreate.correctAnswer')}</p>
                    <div className="grid grid-cols-2 gap-2">
                      {[true, false].map((val) => (
                        <button
                          key={String(val)}
                          type="button"
                          onClick={() => updateQ(idx, { isTrue: val })}
                          className={`flex items-center justify-center gap-2 py-2 rounded-xl border transition-colors ${
                            q.isTrue === val
                              ? 'border-primary bg-primary-tint text-primary'
                              : 'border-border text-text'
                          }`}
                        >
                          {q.isTrue === val && <Check className="w-4 h-4" />}
                          {val ? t('quizCreate.trueLabel') : t('quizCreate.falseLabel')}
                        </button>
                      ))}
                    </div>
                  </div>
                ) : (
                  <div className="space-y-2">
                    <p className="text-xs font-medium text-text">{t('quizCreate.options')}</p>
                    {q.options.map((opt, oIdx) => (
                      <div key={oIdx} className="flex items-center gap-2">
                        <button
                          type="button"
                          onClick={() => updateQ(idx, { correctIndex: oIdx })}
                          className={`w-6 h-6 shrink-0 rounded-full flex items-center justify-center border transition-colors ${
                            q.correctIndex === oIdx
                              ? 'bg-primary text-white border-primary'
                              : 'border-border text-muted'
                          }`}
                          title={t('quizCreate.correctAnswer')}
                        >
                          {q.correctIndex === oIdx && <Check className="w-3.5 h-3.5" />}
                        </button>
                        <input
                          value={opt}
                          onChange={(e) => setOption(idx, oIdx, e.target.value)}
                          placeholder={t('quizCreate.optionPlaceholder')}
                          className="flex-1 bg-surface border border-border rounded-xl px-3 py-2 text-sm text-text placeholder:text-muted/70 focus:border-primary focus:ring-2 focus:ring-primary/30"
                        />
                        {q.options.length > 2 && (
                          <button
                            type="button"
                            onClick={() => removeOption(idx, oIdx)}
                            className="w-8 h-8 shrink-0 rounded-lg text-muted hover:text-danger transition-colors"
                          >
                            <X className="w-4 h-4 mx-auto" />
                          </button>
                        )}
                      </div>
                    ))}
                    <Button size="sm" variant="ghost" onClick={() => addOption(idx)}>
                      <Plus className="w-3.5 h-3.5" />
                      {t('quizCreate.options')}
                    </Button>
                  </div>
                )}
              </Card>
            ))
          )}

          <Button variant="outline" onClick={addQuestion} fullWidth>
            <Plus className="w-4 h-4" />
            {t('quizCreate.addQuestion')}
          </Button>
        </div>
      )}
    </div>
  );
}
