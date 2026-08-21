import { useTheme } from '../context/ThemeContext';
import { Sun, Moon } from 'lucide-react';

export default function ThemeToggle() {
  const { dark, toggle } = useTheme();
  return (
    <button
      onClick={toggle}
      className="inline-flex items-center justify-center w-8 h-8 rounded-lg border border-border text-muted hover:bg-elevated transition-colors"
      title={dark ? 'Light mode' : 'Dark mode'}
      aria-label="Toggle theme"
    >
      {dark ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
    </button>
  );
}
