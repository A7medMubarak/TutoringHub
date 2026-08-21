export function Card({ className = '', children, ...props }) {
  return (
    <div
      className={`bg-surface border border-border rounded-2xl shadow-sm ${className}`}
      {...props}
    >
      {children}
    </div>
  );
}

export function StatCard({ icon: Icon, label, value, tone = 'primary', hint }) {
  const TONES = {
    primary: 'text-primary bg-primary-tint',
    success: 'text-success bg-success/10',
    warning: 'text-warning bg-warning/10',
    danger: 'text-danger bg-danger/10',
  };
  return (
    <Card className="p-4 flex items-center gap-4">
      {Icon && (
        <div className={`w-11 h-11 rounded-xl flex items-center justify-center ${TONES[tone]}`}>
          <Icon className="w-5 h-5" />
        </div>
      )}
      <div className="min-w-0">
        <p className="text-2xl font-bold text-text tabular leading-none">{value}</p>
        <p className="text-xs text-muted mt-1 truncate">{label}</p>
        {hint && <p className="text-[11px] text-muted/80 mt-0.5">{hint}</p>}
      </div>
    </Card>
  );
}
