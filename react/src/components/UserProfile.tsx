import './UserProfile.css'; // Ne zaboravi da importuješ CSS

const UserProfile = () => {
  // OVO SU MOCK PODACI (kasnije ćeš ih dovlačiti sa bekenda)
  const userData = {
    nickname: "nmlad",
    avatarUrl: "https://api.dicebear.com/7.x/avataaars/svg?seed=nmlad", // Generiše avatar na osnovu imena
    stats: {
      wins: 12,
      losses: 3,
      winRate: "80%"
    },
    // Poslednjih 5 mečeva
    lastMatches: [
      { id: 1, result: "WIN", score: "16-14", map: "Mirage" },
      { id: 2, result: "LOSS", score: "10-13", map: "Inferno" },
      { id: 3, result: "WIN", score: "13-5", map: "Dust 2" },
      { id: 4, result: "WIN", score: "13-9", map: "Nuke" },
      { id: 5, result: "WIN", score: "13-11", map: "Vertigo" },
    ]
  };

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
        <ul className="match-list">
          {userData.lastMatches.map((match) => (
            <li key={match.id} className={`match-item ${match.result.toLowerCase()}`}>
              <span className="match-result">{match.result}</span>
              <span className="match-map">{match.map}</span>
              <span className="match-score">{match.score}</span>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};

export default UserProfile;