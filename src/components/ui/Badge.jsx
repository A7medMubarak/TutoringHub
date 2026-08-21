const TONES = {
  primary: 'bg-primary-tint text-primary',
  success: 'bg-success/10 text-success',
  warning: 'bg-warning/10 text-warning',
  danger: 'bg-danger/10 text-danger',
  neutral: 'bg-elevated text-muted',
  accent: 'bg-accent/10 text-accent',
};

export function Badge({ tone = 'neutral', className = '', children }) {
  return (
    <span
      className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium ${TONES[tone]} ${className}`}
    >
      {children}
    </span>
  );
}

export function Avatar({ name = '', size = 'md' }) {
  const initials = name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0])
    .join('')
    .toUpperCase();
  const SIZES = { sm: 'w-8 h-8 text-xs', md: 'w-10 h-10 text-sm', lg: 'w-14 h-14 text-lg' };
  return (
    <div
      className={`${SIZES[size]} rounded-full bg-primary-tint text-primary font-semibold flex items-center justify-center shrink-0`}
    >
      {initials || '?'}
    </div>
  );
}
