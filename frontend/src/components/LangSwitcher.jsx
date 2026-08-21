import { useTranslation } from 'react-i18next';
import { Languages } from 'lucide-react';

export default function LangSwitcher() {
  const { i18n } = useTranslation();
  const isAr = i18n.language === 'ar';
  return (
    <button
      onClick={() => i18n.changeLanguage(isAr ? 'en' : 'ar')}
      className="inline-flex items-center gap-1.5 text-xs px-2.5 py-1.5 rounded-lg border border-border text-muted hover:bg-elevated transition-colors"
      title={isAr ? 'English' : 'العربية'}
    >
      <Languages className="w-3.5 h-3.5" />
      {isAr ? 'EN' : 'ع'}
    </button>
  );
}
