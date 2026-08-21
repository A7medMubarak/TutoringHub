import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { GraduationCap, LogOut } from 'lucide-react';
import LangSwitcher from '../LangSwitcher';
import ThemeToggle from '../ThemeToggle';
import { Avatar } from '../ui';
import { useAuth } from '../../context/AuthContext';

export default function TopBar() {
  const { t } = useTranslation();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    window.location.href = '/login';
  };

  return (
    <header className="sticky top-0 z-40 bg-surface/90 backdrop-blur border-b border-border">
      <div className="h-16 px-4 flex items-center justify-between gap-3">
        <Link to={user?.role === 'Teacher' ? '/dashboard' : '/my-classes'} className="flex items-center gap-2">
          <span className="w-9 h-9 rounded-xl bg-primary text-white flex items-center justify-center shadow-sm shadow-primary/30">
            <GraduationCap className="w-5 h-5" />
          </span>
          <span className="font-extrabold text-lg tracking-tight text-text">TutoringHub</span>
        </Link>

        <div className="flex items-center gap-2">
          <LangSwitcher />
          <ThemeToggle />
          {user && (
            <>
              <div className="hidden sm:flex items-center gap-2 ps-1">
                <Avatar name={user.name || (user.role === 'Teacher' ? t('role.teacher') : t('role.student'))} size="sm" />
                <span className="text-sm font-medium text-text">{user.name || ''}</span>
              </div>
              <button
                onClick={handleLogout}
                className="inline-flex items-center justify-center w-9 h-9 rounded-lg border border-border text-muted hover:text-danger hover:border-danger/40 transition-colors"
                title={t('app.logout')}
                aria-label={t('app.logout')}
              >
                <LogOut className="w-4 h-4" />
              </button>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
