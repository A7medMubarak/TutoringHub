export const QUESTION_TYPE = { MultipleChoice: 0, TrueFalse: 1 };

export function isTrueFalse(type) {
  return Number(type) === QUESTION_TYPE.TrueFalse;
}

export function typeLabel(t, type) {
  return isTrueFalse(type) ? t('common.tf') : t('common.mcq');
}

export function dayLabel(t, day) {
  return t(`day.${Number(day)}`);
}

export function freqLabel(t, freq) {
  return t(`freq.${Number(freq)}`);
}

export function formatDateOnly(value) {
  if (!value) return '';
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) {
    const [y, m, d] = value.split('-').map(Number);
    const dt = new Date(y, m - 1, d);
    return dt.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
  }
  const dt = new Date(value);
  return Number.isNaN(dt.getTime()) ? '' : dt.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

export function formatDateTime(value) {
  if (!value) return '';
  const dt = new Date(value);
  return Number.isNaN(dt.getTime())
    ? ''
    : dt.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
}

export function formatTime(value) {
  if (!value) return '';
  return String(value).slice(0, 5);
}

export function todayISO() {
  const d = new Date();
  const off = d.getTimezoneOffset();
  return new Date(d.getTime() - off * 60000).toISOString().slice(0, 10);
}

export function isoDateMonthsAgo(months) {
  const d = new Date();
  d.setMonth(d.getMonth() - months);
  const off = d.getTimezoneOffset();
  return new Date(d.getTime() - off * 60000).toISOString().slice(0, 10);
}

export function reportError(toast, err, fallbackKey) {
  const detail = err?.response?.data?.detail;
  toast.error(detail || fallbackKey);
}
