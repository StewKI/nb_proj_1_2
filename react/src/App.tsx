import './App.css'
import { useGameHub } from './hooks/useGameHub'
import { Lobby } from './components/Lobby'
import { GameCanvas } from './components/GameCanvas'

function App() {
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

  if (appState === 'lobby') {
    return (
      <Lobby
        lobby={lobby}
        connected={connected}
        onCreateGame={createGame}
        onJoinGame={joinGame}
        onRefresh={refreshLobby}
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

export default App
