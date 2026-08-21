import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { LogOut } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import LangSwitcher from '../../components/LangSwitcher';
import ThemeToggle from '../../components/ThemeToggle';
import { Card, Button, Avatar, ConfirmDialog } from '../../components/ui';

export default function Profile() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [confirmOpen, setConfirmOpen] = useState(false);

  const handleLogout = async () => {
    setConfirmOpen(false);
    await logout();
    navigate('/login');
  };

  return (
    <div className="space-y-5">
      <h1 className="text-xl font-bold text-text">{t('profile.title')}</h1>

      <Card className="p-5 flex items-center gap-4">
        <Avatar name={user?.name || ''} size="lg" />
        <div>
          <p className="text-lg font-semibold text-text">{user?.name || '—'}</p>
          <p className="text-sm text-muted">
            {t('profile.role')}: {t(`role.${user?.role?.toLowerCase()}`)}
          </p>
        </div>
      </Card>

      <Card className="divide-y divide-border">
        <div className="flex items-center justify-between px-4 py-3">
          <span className="text-sm font-medium text-text">{t('profile.language')}</span>
          <LangSwitcher />
        </div>
        <div className="flex items-center justify-between px-4 py-3">
          <span className="text-sm font-medium text-text">{t('profile.theme')}</span>
          <ThemeToggle />
        </div>
      </Card>

      <Button variant="danger" fullWidth onClick={() => setConfirmOpen(true)}>
        <LogOut className="w-4 h-4" />
        {t('profile.logout')}
      </Button>

      <ConfirmDialog
        open={confirmOpen}
        title={t('profile.logout')}
        message={t('profile.logoutConfirm')}
        confirmLabel={t('profile.logout')}
        cancelLabel={t('app.cancel')}
        danger
        onConfirm={handleLogout}
        onCancel={() => setConfirmOpen(false)}
      />
    </div>
  );
}
