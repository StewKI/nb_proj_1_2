import type { LeaderboardEntry } from '../types/leaderboard';
import './LeaderboardTable.css';

interface LeaderboardTableProps {
  entries: LeaderboardEntry[];
  title: string;
  scoreLabel?: string;
}

export function LeaderboardTable({ entries, title, scoreLabel = 'Score' }: LeaderboardTableProps) {
  const getMedalEmoji = (rank: number) => {
    switch (rank) {
      case 1:
        return '🥇';
      case 2:
        return '🥈';
      case 3:
        return '🥉';
      default:
        return null;
    }
  };

  return (
    <div className="leaderboard-table">
      <h2 className="leaderboard-title">{title}</h2>
      {entries.length === 0 ? (
        <div className="no-data">No data available</div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Rank</th>
              <th>Player</th>
              <th>{scoreLabel}</th>
            </tr>
          </thead>
          <tbody>
            {entries.map((entry) => (
              <tr
                key={`${entry.playerId}-${entry.rank}`}
                className={entry.rank <= 3 ? `rank-${entry.rank}` : ''}
              >
                <td className="rank-cell">
                  {getMedalEmoji(entry.rank) || `#${entry.rank}`}
                </td>
                <td className="username-cell">{entry.username}</td>
                <td className="score-cell">{entry.score.toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
