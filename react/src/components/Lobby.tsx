import { Link } from 'react-router-dom';
import type { LobbyGame } from '../hooks/useGameHub';
import UserProfile from './UserProfile';
import { HistoryWindow } from './MatchHistory';
interface LobbyProps {
  lobby: LobbyGame[];
  connected: boolean;
  playerName: string;
  onCreateGame: () => void;
  onJoinGame: (gameId: string) => void;
  onRefresh: () => void;
  onLogout: () => void;
}

export function Lobby({ lobby, connected, playerName, onCreateGame, onJoinGame, onRefresh, onLogout }: LobbyProps) {
  return (

    <div className="lobby">
      <HistoryWindow />
      <UserProfile />
      <div className="lobby-header">
        <h1>NPP Ping Pong</h1>
        <div className="user-info">
          <span>Playing as <strong>{playerName}</strong></span>
          <Link to="/leaderboard" className="leaderboard-link">
            🏆 Leaderboard
          </Link>
          <button onClick={onLogout} className="logout-btn">Logout</button>
        </div>
      </div>

      <div className="connection-status">
        Status: {connected ? 'Connected' : 'Connecting...'}
      </div>

      <div className="player-setup">
        <button onClick={onCreateGame} disabled={!connected}>
          Create Game
        </button>
      </div>

      <div className="games-list">
        <div className="games-header">
          <h2>Open Games</h2>
          <button onClick={onRefresh} disabled={!connected}>
            Refresh
          </button>
        </div>

        {lobby.length === 0 ? (
          <p className="no-games">No open games. Create one!</p>
        ) : (
          <ul>
            {lobby.map((game) => (
              <li key={game.gameId}>
                <span>{game.hostName}'s game</span>
                <button
                  onClick={() => onJoinGame(game.gameId)}
                  disabled={!connected}
                >
                  Join
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
