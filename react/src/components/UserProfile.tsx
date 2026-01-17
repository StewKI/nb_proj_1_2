import './UserProfile.css'; // Ne zaboravi da importuješ CSS
import { getMatchesByYear } from '../services/playerMatchesApi';
import { useState, useEffect } from 'react';
import type { PlayerMatchesResponse } from '../types/playerMatches';

const UserProfile = () => {
  // OVO SU MOCK PODACI (kasnije ćeš ih dovlačiti sa bekenda)

  const [matches, setMatches] = useState<PlayerMatchesResponse[]>([]);
  const [loading, setLoading] = useState(true)

  const userData = {
    nickname: "nmlad",
    avatarUrl: "https://api.dicebear.com/7.x/avataaars/svg?seed=nmlad", // Generiše avatar na osnovu imena
    stats: {
      wins: 12,
      losses: 3,
      winRate: "80%"
    },
  };
  useEffect(() => {
    // Pozivamo funkciju: godina "2024", strana 1, limit 5
    getMatchesByYear("2026", 1, 5)
      .then((data) => {
        // Ovde pretpostavljamo da je 'data' niz mečeva ili objekat koji sadrži niz
        // Ako backend vraća { content: [...] }, onda stavi data.content
        setMatches(data); 
        setLoading(false);
      })
      .catch((error) => {
        console.error("Greška pri učitavanju mečeva:", error);
        setLoading(false);
      });
  }, []);

  return (
    <div className="profile-widget">
      {/* Gornji deo: Slika i Ime */}
      <div className="profile-header">
        <img
          src={userData.avatarUrl}
          alt="User Avatar"
          className="profile-avatar"
        />
        <div className="profile-info">
          <h3 className="profile-name">{userData.nickname}</h3>
          <span className="profile-status">Online</span>
        </div>
      </div>

      {/* Srednji deo: Statistika */}
      <div className="profile-stats">
        <div className="stat-item">
          <span className="stat-value">{userData.stats.wins}</span>
          <span className="stat-label">Wins</span>
        </div>
        <div className="stat-item">
          <span className="stat-value">{userData.stats.losses}</span>
          <span className="stat-label">Losses</span>
        </div>
        <div className="stat-item">
          <span className="stat-value highlight">{userData.stats.winRate}</span>
          <span className="stat-label">Win Rate</span>
        </div>
      </div>

      {/* Donji deo: Istorija mečeva */}
      <div className="matches-history">
        <h4>Last 5 Matches</h4>
        
        {loading ? (
          <p style={{textAlign: 'center', color: '#888'}}>Loading matches...</p>
        ) : (
          <ul className="match-list">
            {/* Ovde sada vrtimo 'matches' iz state-a, a ne userData.lastMatches */}
            {matches.map((match) => (
              <li key={match.playerId} className={`match-item ${match.result ? match.result.toLowerCase() : ''}`}>
                <span className="match-result">{match.result}</span>
                <span className="match-score">{match.score}</span>
                <span className="match-map">{match.opponentUsername}</span>
              </li>
            ))}
            
            {/* Ako nema mečeva */}
            {matches.length === 0 && <p style={{fontSize: '0.8rem', textAlign: 'center'}}>No matches found.</p>}
          </ul>
        )}
      </div>
    </div>
  );
};

export default UserProfile;