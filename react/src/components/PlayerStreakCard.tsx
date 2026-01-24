import type { PlayerStreakDto } from '../types/leaderboard';
import './PlayerStreakCard.css';

interface PlayerStreakCardProps {
  streak: PlayerStreakDto | null;
  loading: boolean;
  error: string | null;
}

export function PlayerStreakCard({ streak, loading, error }: PlayerStreakCardProps) {
  if (loading) {
    return (
      <div className="streak-card">
        <div className="streak-loading">Loading your streak...</div>
      </div>
    );
  }

  if (error || !streak) {
    return (
      <div className="streak-card">
        <div className="streak-error">
          {error || 'No streak data available. Play some games first!'}
        </div>
      </div>
    );
  }

  const isOnWinStreak = streak.currentStreak > 0;

  return (
    <div className="streak-card">
      <h2 className="streak-title">
        {isOnWinStreak ? '🔥' : '💪'} Your Streak
      </h2>
      
      <div className="streak-stats">
        <div className={`streak-stat ${isOnWinStreak ? 'active' : ''}`}>
          <div className="streak-label">Current Streak</div>
          <div className="streak-value">
            {streak.currentStreak}
            {isOnWinStreak && <span className="streak-icon">🔥</span>}
          </div>
        </div>

        <div className="streak-divider"></div>

        <div className="streak-stat">
          <div className="streak-label">Longest Streak</div>
          <div className="streak-value trophy">
            {streak.longestStreak}
            <span className="streak-icon">🏆</span>
          </div>
        </div>
      </div>

      <div className="streak-footer">
        <span className={`last-result ${streak.lastResult.toLowerCase()}`}>
          Last result: {streak.lastResult === 'WIN' ? '✓ Win' : '✗ Loss'}
        </span>
      </div>
    </div>
  );
}
