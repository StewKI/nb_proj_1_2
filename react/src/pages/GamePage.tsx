import { useGameHub } from '../hooks/useGameHub'
import { useAuth } from '../contexts/AuthContext'
import { Lobby } from '../components/Lobby'
import { GameCanvas } from '../components/GameCanvas'
import UserProfile from '../components/UserProfile'

export function GamePage() {
  const { user, logout } = useAuth();
  const {
    connected,
    lobby,
    gameState,
    appState,
    winner,
    isPlayer1,
    createGame,
    joinGame,
    movePaddle,
    refreshLobby,
    returnToLobby,
  } = useGameHub();

  const playerName = user?.username ?? 'Player';

  if (appState === 'lobby') {
    return (
      <Lobby
        lobby={lobby}
        connected={connected}
        playerName={playerName}
        onCreateGame={() => createGame(playerName)}
        onJoinGame={(gameId) => joinGame(gameId, playerName)}
        onRefresh={refreshLobby}
        onLogout={logout}
      />
    );
  }

  if (appState === 'waiting') {
    return (
      <div className="waiting">
        <h1>Waiting for opponent...</h1>
        <p>Share this page with a friend to play!</p>
        <div className="loader"></div>
      </div>
    );
  }

  if (appState === 'ended') {
    return (
      <div className="game-over">
        <h1>Game Over!</h1>
        <p className="winner">{winner} wins!</p>
        <button onClick={returnToLobby}>Back to Lobby</button>
      </div>
    );
  }

  if (appState === 'playing' && gameState) {
    return (
      <GameCanvas
        gameState={gameState}
        onMovePaddle={movePaddle}
        isPlayer1={isPlayer1}
      />
    );
  }

  return <div>Loading...</div>;
}
