import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

export interface Player {
  name: string;
  score: number;
}

export interface GameState {
  gameId: string;
  ball: { x: number; y: number };
  paddle1: { y: number };
  paddle2: { y: number };
  player1: Player | null;
  player2: Player | null;
  state: string;
}

export interface LobbyGame {
  gameId: string;
  hostName: string;
}

export type AppState = 'lobby' | 'waiting' | 'playing' | 'ended';

export function useGameHub() {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [connected, setConnected] = useState(false);
  const [lobby, setLobby] = useState<LobbyGame[]>([]);
  const [gameState, setGameState] = useState<GameState | null>(null);
  const [appState, setAppState] = useState<AppState>('lobby');
  const [winner, setWinner] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isPlayer1, setIsPlayer1] = useState(true);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('/gamehub')
      .withAutomaticReconnect()
      .build();

    newConnection.on('LobbyUpdated', (games: LobbyGame[]) => {
      setLobby(games);
    });

    newConnection.on('GameStarted', (state: GameState) => {
      setGameState(state);
      setAppState('playing');
    });

    newConnection.on('GameStateUpdated', (state: GameState) => {
      setGameState(state);
    });

    newConnection.on('GameEnded', (winnerName: string) => {
      setWinner(winnerName);
      setAppState('ended');
    });

    newConnection.on('JoinFailed', (message: string) => {
      setError(message);
    });

    newConnection.start()
      .then(() => {
        setConnected(true);
        newConnection.invoke('GetLobby');
      })
      .catch(err => console.error('Connection failed:', err));

    setConnection(newConnection);
    connectionRef.current = newConnection;

    return () => {
      newConnection.stop();
    };
  }, []);

  const createGame = useCallback(async (playerName: string) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      setIsPlayer1(true);
      await connectionRef.current.invoke('CreateGame', playerName);
      setAppState('waiting');
    }
  }, []);

  const joinGame = useCallback(async (gameId: string, playerName: string) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      setIsPlayer1(false);
      await connectionRef.current.invoke('JoinGame', gameId, playerName);
    }
  }, []);

  const movePaddle = useCallback((y: number) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      connectionRef.current.invoke('MovePaddle', y);
    }
  }, []);

  const refreshLobby = useCallback(async () => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke('GetLobby');
    }
  }, []);

  const returnToLobby = useCallback(() => {
    setAppState('lobby');
    setGameState(null);
    setWinner(null);
    setError(null);
    refreshLobby();
  }, [refreshLobby]);

  return {
    connected,
    lobby,
    gameState,
    appState,
    winner,
    error,
    isPlayer1,
    createGame,
    joinGame,
    movePaddle,
    refreshLobby,
    returnToLobby,
  };
}
