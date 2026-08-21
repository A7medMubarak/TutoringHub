import { Outlet } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  BookOpen,
  CalendarCheck,
  Wallet,
  ClipboardList,
} from 'lucide-react';
import TopBar from './TopBar';
import { NavRail, BottomNav } from './Nav';

const NAV = [
  { to: '/dashboard', label: 'nav.dashboard', icon: LayoutDashboard },
  { to: '/students', label: 'nav.students', icon: Users },
  { to: '/classes', label: 'nav.classes', icon: BookOpen },
  { to: '/attendance', label: 'nav.attendance', icon: CalendarCheck },
  { to: '/quotas', label: 'nav.quotas', icon: Wallet },
  { to: '/quizzes', label: 'nav.quizzes', icon: ClipboardList },
];

export default function TeacherLayout() {
  return (
    <div className="min-h-screen flex flex-col bg-bg">
      <TopBar />
      <div className="flex flex-1">
        <NavRail items={NAV} />
        <main className="flex-1 min-w-0 pb-24 lg:pb-10">
          <div className="mx-auto max-w-5xl px-4 py-5">
            <Outlet />
          </div>
        </main>
      </div>
      <BottomNav items={NAV} />
    </div>
  );
}
