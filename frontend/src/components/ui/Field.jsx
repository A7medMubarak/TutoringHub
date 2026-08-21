export function TextField({
  label,
  error,
  hint,
  className = '',
  required,
  ...props
}) {
  return (
    <div className={className}>
      {label && (
        <label className="block text-sm font-medium text-text mb-1.5">
          {label}
          {required && <span className="text-danger"> *</span>}
        </label>
      )}
      <input
        className={`w-full bg-surface border border-border rounded-xl px-3.5 py-2.5 text-sm text-text placeholder:text-muted/70 transition-colors focus:border-primary focus:ring-2 focus:ring-primary/30 ${
          error ? 'border-danger focus:border-danger focus:ring-danger/30' : ''
        }`}
        {...props}
      />
      {error ? (
        <p className="text-xs text-danger mt-1">{error}</p>
      ) : hint ? (
        <p className="text-xs text-muted mt-1">{hint}</p>
      ) : null}
    </div>
  );
}

export function TextArea({ label, error, className = '', required, ...props }) {
  return (
    <div className={className}>
      {label && (
        <span className="block text-sm font-medium text-text mb-1.5">
          {label}
          {required && <span className="text-danger"> *</span>}
        </span>
      )}
      <textarea
        className={`w-full bg-surface border border-border rounded-xl px-3.5 py-2.5 text-sm text-text placeholder:text-muted/70 transition-colors focus:border-primary focus:ring-2 focus:ring-primary/30 ${
          error ? 'border-danger' : ''
        }`}
        {...props}
      />
    </div>
  );
}

export function Select({ label, error, className = '', children, ...props }) {
  return (
    <div className={className}>
      {label && (
        <label className="block text-sm font-medium text-text mb-1.5">
          {label}
        </label>
      )}
      <select
        className={`w-full bg-surface border border-border rounded-xl px-3.5 py-2.5 text-sm text-text transition-colors focus:border-primary focus:ring-2 focus:ring-primary/30 ${
          error ? 'border-danger' : ''
        }`}
        {...props}
      >
        {children}
      </select>
      {error && <p className="text-xs text-danger mt-1">{error}</p>}
    </div>
  );
}
