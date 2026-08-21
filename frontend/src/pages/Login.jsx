import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { GraduationCap, LogIn } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import RoleToggle from '../components/RoleToggle';
import { Button, TextField } from '../components/ui';
import LangSwitcher from '../components/LangSwitcher';
import ThemeToggle from '../components/ThemeToggle';

export default function Login() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { loginTeacher, loginStudent } = useAuth();

  const [role, setRole] = useState('Teacher');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [phone, setPhone] = useState('');
  const [pin, setPin] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      if (role === 'Teacher') {
        await loginTeacher(username.trim(), password);
        navigate('/dashboard');
      } else {
        await loginStudent(phone.trim(), pin);
        navigate('/my-classes');
      }
    } catch (err) {
      setError(err.response?.data?.detail || t('login.failed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex flex-col bg-bg">
      <div className="flex items-center justify-between p-4">
        <div className="flex items-center gap-2">
          <span className="w-9 h-9 rounded-xl bg-primary text-white flex items-center justify-center shadow-sm shadow-primary/30">
            <GraduationCap className="w-5 h-5" />
          </span>
          <span className="font-extrabold text-lg text-text">TutoringHub</span>
        </div>
        <div className="flex items-center gap-2">
          <LangSwitcher />
          <ThemeToggle />
        </div>
      </div>

      <div className="flex-1 flex items-center justify-center px-4 py-6">
        <div className="w-full max-w-sm">
          <div className="text-center mb-6">
            <h1 className="text-2xl font-extrabold text-text">{t('login.welcome')}</h1>
            <p className="text-sm text-muted mt-1">{t('login.subtitle')}</p>
          </div>

          <form
            onSubmit={submit}
            className="bg-surface border border-border rounded-2xl shadow-sm p-5 space-y-4"
          >
            <RoleToggle value={role} onChange={setRole} />

            {error && (
              <div className="text-sm text-danger bg-danger/10 rounded-xl p-3">{error}</div>
            )}

            {role === 'Teacher' ? (
              <>
                <TextField
                  label={t('login.username')}
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  autoFocus
                  required
                  placeholder={t('login.usernamePlaceholder')}
                />
                <TextField
                  label={t('login.password')}
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  placeholder={t('login.passwordPlaceholder')}
                />
              </>
            ) : (
              <>
                <TextField
                  label={t('login.phone')}
                  type="tel"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  autoFocus
                  required
                  placeholder={t('login.phonePlaceholder')}
                />
                <TextField
                  label={t('login.pin')}
                  type="password"
                  inputMode="numeric"
                  maxLength={4}
                  value={pin}
                  onChange={(e) => setPin(e.target.value.replace(/\D/g, ''))}
                  required
                  placeholder="••••"
                />
              </>
            )}

            <Button type="submit" fullWidth size="lg" loading={loading}>
              <LogIn className="w-4 h-4" />
              {t('login.signIn')}
            </Button>
          </form>

          <p className="text-center text-xs text-muted mt-5 px-4">
            {role === 'Teacher' ? t('login.teacherHint') : t('login.studentHint')}
          </p>
        </div>
      </div>
    </div>
  );
}
