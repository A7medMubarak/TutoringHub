import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

export function NavRail({ items, footer }) {
  const { t } = useTranslation();
  return (
    <aside className="hidden lg:flex w-60 shrink-0 flex-col border-e border-border bg-surface p-3 sticky top-16 h-[calc(100vh-4rem)]">
      <nav className="flex flex-col gap-1">
        {items.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-primary-tint text-primary'
                    : 'text-muted hover:bg-elevated hover:text-text'
                }`
              }
            >
              <Icon className="w-5 h-5 shrink-0" />
              {t(item.label)}
            </NavLink>
          );
        })}
      </nav>
      {footer && <div className="mt-auto pt-3 border-t border-border">{footer}</div>}
    </aside>
  );
}

export function BottomNav({ items }) {
  const { t } = useTranslation();
  return (
    <nav className="lg:hidden fixed bottom-0 inset-x-0 z-30 bg-surface border-t border-border grid grid-cols-5 py-1.5 px-1">
      {items.map((item) => {
        const Icon = item.icon;
        return (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex flex-col items-center gap-0.5 py-1 rounded-lg text-[11px] transition-colors ${
                isActive ? 'text-primary' : 'text-muted'
              }`
            }
          >
            <Icon className="w-5 h-5" />
            <span className="truncate px-0.5">{t(item.label)}</span>
          </NavLink>
        );
      })}
    </nav>
  );
}
