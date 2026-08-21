import { Outlet } from 'react-router-dom';
import { BookOpen, ClipboardList, CalendarCheck, Wallet, UserRound } from 'lucide-react';
import TopBar from './TopBar';
import { NavRail, BottomNav } from './Nav';

const NAV = [
  { to: '/my-classes', label: 'nav.myClasses', icon: BookOpen },
  { to: '/my-quizzes', label: 'nav.myQuizzes', icon: ClipboardList },
  { to: '/my-attendance', label: 'nav.myAttendance', icon: CalendarCheck },
  { to: '/my-quotas', label: 'nav.myQuotas', icon: Wallet },
  { to: '/profile', label: 'nav.profile', icon: UserRound },
];

export default function StudentLayout() {
  return (
    <div className="min-h-screen flex flex-col bg-bg">
      <TopBar />
      <div className="flex flex-1">
        <NavRail items={NAV} />
        <main className="flex-1 min-w-0 pb-24 lg:pb-10">
          <div className="mx-auto max-w-3xl px-4 py-5">
            <Outlet />
          </div>
        </main>
      </div>
      <BottomNav items={NAV} />
    </div>
  );
}
