import { useState } from 'react';
import type { LobbyGame } from '../hooks/useGameHub';

interface LobbyProps {
  lobby: LobbyGame[];
  connected: boolean;
  onCreateGame: (playerName: string) => void;
  onJoinGame: (gameId: string, playerName: string) => void;
  onRefresh: () => void;
}

export function Lobby({ lobby, connected, onCreateGame, onJoinGame, onRefresh }: LobbyProps) {
  const [playerName, setPlayerName] = useState('');

  const handleCreateGame = () => {
    if (playerName.trim()) {
      onCreateGame(playerName.trim());
    }
  };

  const handleJoinGame = (gameId: string) => {
    if (playerName.trim()) {
      onJoinGame(gameId, playerName.trim());
    }
  };

  return (
    <div className="lobby">
      <h1>NPP Ping Pong</h1>

      <div className="connection-status">
        Status: {connected ? 'Connected' : 'Connecting...'}
      </div>

      <div className="player-setup">
        <input
          type="text"
          placeholder="Enter your name"
          value={playerName}
          onChange={(e) => setPlayerName(e.target.value)}
          maxLength={20}
        />
        <button onClick={handleCreateGame} disabled={!connected || !playerName.trim()}>
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
                  onClick={() => handleJoinGame(game.gameId)}
                  disabled={!connected || !playerName.trim()}
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
