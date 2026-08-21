import { Loader2 } from 'lucide-react';

export function Spinner({ className = 'w-5 h-5' }) {
  return <Loader2 className={`animate-spin text-primary ${className}`} />;
}

export function PageLoader({ label }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 gap-3 text-muted">
      <Spinner className="w-7 h-7" />
      {label && <p className="text-sm">{label}</p>}
    </div>
  );
}

export function Skeleton({ className = '' }) {
  return <div className={`animate-pulse bg-elevated rounded-lg ${className}`} />;
}

export function ProgressBar({ value, className = '' }) {
  const pct = Math.max(0, Math.min(100, value));
  return (
    <div className={`h-2 w-full bg-elevated rounded-full overflow-hidden ${className}`}>
      <div
        className="h-full bg-primary rounded-full transition-all duration-300"
        style={{ width: `${pct}%` }}
      />
    </div>
  );
}

export function EmptyState({ icon: Icon, title, description, action }) {
  return (
    <div className="flex flex-col items-center justify-center text-center py-16 px-6">
      {Icon && (
        <div className="w-14 h-14 rounded-2xl bg-primary-tint text-primary flex items-center justify-center mb-4">
          <Icon className="w-7 h-7" />
        </div>
      )}
      <h3 className="text-base font-semibold text-text">{title}</h3>
      {description && <p className="text-sm text-muted mt-1 max-w-xs">{description}</p>}
      {action && <div className="mt-5">{action}</div>}
    </div>
  );
}
