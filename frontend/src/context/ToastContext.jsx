import { createContext, useContext, useState, useCallback, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { CheckCircle2, AlertCircle, Info, X } from 'lucide-react';

const ToastContext = createContext(null);
let idCounter = 0;

const ICONS = {
  success: CheckCircle2,
  error: AlertCircle,
  info: Info,
};

const TONE = {
  success: 'text-success',
  error: 'text-danger',
  info: 'text-primary',
};

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const dismiss = useCallback((id) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const push = useCallback(
    (type, message) => {
      const id = ++idCounter;
      setToasts((prev) => [...prev, { id, type, message }]);
      setTimeout(() => dismiss(id), 3800);
    },
    [dismiss]
  );

  const api = useMemo(() => ({
    success: (m) => push('success', m),
    error: (m) => push('error', m),
    info: (m) => push('info', m),
  }), [push]);

  return (
    <ToastContext.Provider value={api}>
      {children}
      {createPortal(
        <div className="fixed top-3 inset-x-0 z-[100] flex flex-col items-center gap-2 px-3 pointer-events-none">
          {toasts.map((t) => {
            const Icon = ICONS[t.type] || Info;
            return (
              <div
                key={t.id}
                className="pointer-events-auto flex items-center gap-2.5 w-full max-w-sm bg-surface border border-border shadow-lg rounded-xl px-4 py-3 text-sm animate-[fadeIn_.2s_ease]"
                role="status"
              >
                <Icon className={`w-5 h-5 shrink-0 ${TONE[t.type]}`} />
                <span className="flex-1 text-text">{t.message}</span>
                <button
                  onClick={() => dismiss(t.id)}
                  className="text-muted hover:text-text transition-colors"
                  aria-label="Dismiss"
                >
                  <X className="w-4 h-4" />
                </button>
              </div>
            );
          })}
        </div>,
        document.body
      )}
    </ToastContext.Provider>
  );
}

export const useToast = () => useContext(ToastContext);
