import { Loader2 } from 'lucide-react';

const VARIANTS = {
  primary:
    'bg-primary text-white hover:bg-primary-dark active:scale-[.98] shadow-sm shadow-primary/30',
  secondary:
    'bg-elevated text-text hover:bg-border active:scale-[.98] border border-border',
  ghost: 'text-primary hover:bg-primary-tint active:scale-[.98]',
  danger: 'bg-danger text-white hover:opacity-90 active:scale-[.98] shadow-sm shadow-danger/30',
  outline:
    'border border-primary text-primary hover:bg-primary-tint active:scale-[.98]',
};

const SIZES = {
  sm: 'px-3 py-1.5 text-xs gap-1.5 rounded-lg',
  md: 'px-4 py-2.5 text-sm gap-2 rounded-xl',
  lg: 'px-5 py-3 text-base gap-2 rounded-xl',
};

export default function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  fullWidth = false,
  className = '',
  children,
  disabled,
  ...props
}) {
  return (
    <button
      className={`inline-flex items-center justify-center font-medium transition-all duration-150 disabled:opacity-50 disabled:pointer-events-none ${VARIANTS[variant]} ${SIZES[size]} ${fullWidth ? 'w-full' : ''} ${className}`}
      disabled={disabled || loading}
      {...props}
    >
      {loading && <Loader2 className="w-4 h-4 animate-spin" />}
      {children}
    </button>
  );
}
