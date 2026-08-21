const TONES = {
  primary: 'bg-primary-tint text-primary',
  success: 'bg-success/10 text-success',
  warning: 'bg-warning/10 text-warning',
  danger: 'bg-danger/10 text-danger',
  neutral: 'bg-elevated text-muted',
};

export function Badge({ children, tone = 'neutral', dot = false, className = '' }) {
  return (
    <span
      className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium ${TONES[tone]} ${className}`}
    >
      {dot && <span className="w-1.5 h-1.5 rounded-full bg-current" />}
      {children}
    </span>
  );
}

export function Avatar({ name = '', size = 'md' }) {
  const initials =
    name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((w) => w[0]?.toUpperCase())
      .join('') || '?';
  const SIZES = { sm: 'w-8 h-8 text-xs', md: 'w-10 h-10 text-sm', lg: 'w-12 h-12 text-base' };
  return (
    <span
      className={`inline-flex items-center justify-center rounded-full bg-primary-tint text-primary font-semibold shrink-0 ${SIZES[size]}`}
    >
      {initials}
    </span>
  );
}
