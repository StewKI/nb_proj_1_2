import './UserProfile.css';
import { getMatchesByYear } from '../services/playerMatchesApi';
import { getMyStats, type PlayerStats } from '../services/playerApi';
import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import type { PlayerMatchesResponse } from '../types/playerMatches';

const UserProfile = () => {
  const { user } = useAuth();
  const [matches, setMatches] = useState<PlayerMatchesResponse[]>([]);
  const [stats, setStats] = useState<PlayerStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [statsLoading, setStatsLoading] = useState(true);

  const username = user?.username ?? 'Player';
  const avatarUrl = `https://api.dicebear.com/7.x/avataaars/svg?seed=${username}`;

  useEffect(() => {
    // Fetch player stats
    getMyStats()
      .then((data) => {
        setStats(data);
        setStatsLoading(false);
      })
      .catch((error) => {
        console.error("Error loading stats:", error);
        setStatsLoading(false);
      });

    // Fetch recent matches
    getMatchesByYear(new Date().getFullYear().toString(), 1, 5)
      .then((data) => {
        setMatches(data);
        setLoading(false);
      })
      .catch((error) => {
        console.error("Error loading matches:", error);
        setLoading(false);
      });
  }, []);

  return (
    <div className="profile-widget">
      {/* Header: Avatar and Name */}
      <div className="profile-header">
        <img
          src={avatarUrl}
          alt="User Avatar"
          className="profile-avatar"
        />
        <div className="profile-info">
          <h3 className="profile-name">{username}</h3>
          <span className="profile-status">Online</span>
        </div>
      </div>

      {/* Stats section */}
      <div className="profile-stats">
        {statsLoading ? (
          <p style={{textAlign: 'center', color: '#888', width: '100%'}}>Loading stats...</p>
        ) : (
          <>
            <div className="stat-item">
              <span className="stat-value">{stats?.wins ?? 0}</span>
              <span className="stat-label">Wins</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{stats?.losses ?? 0}</span>
              <span className="stat-label">Losses</span>
            </div>
            <div className="stat-item">
              <span className="stat-value highlight">{stats?.winRate ?? '0%'}</span>
              <span className="stat-label">Win Rate</span>
            </div>
          </>
        )}
      </div>

      {/* Match history section */}
      <div className="matches-history">
        <h4>Last 5 Matches</h4>

        {loading ? (
          <p style={{textAlign: 'center', color: '#888'}}>Loading matches...</p>
        ) : (
          <ul className="match-list">
            {matches.map((match, index) => (
              <li key={`${match.playerId}-${index}`} className={`match-item ${match.result ? match.result.toLowerCase() : ''}`}>
                <span className="match-result">{match.result}</span>
                <span className="match-score">{match.score}</span>
                <span className="match-map">{match.opponentUsername}</span>
              </li>
            ))}

            {matches.length === 0 && <p style={{fontSize: '0.8rem', textAlign: 'center'}}>No matches found.</p>}
          </ul>
        )}
      </div>
    </div>
  );
};

export default UserProfile;