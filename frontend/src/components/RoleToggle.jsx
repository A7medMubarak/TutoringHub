import { useTranslation } from 'react-i18next';
import { GraduationCap, UserRound } from 'lucide-react';

export default function RoleToggle({ value, onChange }) {
  const { t } = useTranslation();
  const options = [
    { key: 'Teacher', label: t('login.teacher'), icon: GraduationCap },
    { key: 'Student', label: t('login.student'), icon: UserRound },
  ];
  return (
    <div className="grid grid-cols-2 gap-1 p-1 bg-elevated rounded-xl">
      {options.map((o) => {
        const active = value === o.key;
        const Icon = o.icon;
        return (
          <button
            key={o.key}
            type="button"
            onClick={() => onChange(o.key)}
            className={`flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-medium transition-all ${
              active
                ? 'bg-surface text-primary shadow-sm'
                : 'text-muted hover:text-text'
            }`}
          >
            <Icon className="w-4 h-4" />
            {o.label}
          </button>
        );
      })}
    </div>
  );
}
