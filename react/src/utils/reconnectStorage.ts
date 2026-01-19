const RECONNECT_KEY = 'npp_reconnect_token';

export interface ReconnectToken {
  token: string;
  gameId: string;
  playerNumber: number;
  savedAt: number;
}

export function saveReconnectToken(data: { token: string; gameId: string; playerNumber: number }): void {
  const tokenData: ReconnectToken = {
    ...data,
    savedAt: Date.now()
  };
  localStorage.setItem(RECONNECT_KEY, JSON.stringify(tokenData));
}

export function getReconnectToken(): ReconnectToken | null {
  const stored = localStorage.getItem(RECONNECT_KEY);
  if (!stored) {
    return null;
  }

  try {
    const data = JSON.parse(stored) as ReconnectToken;

    // Check if token is expired (10 minutes)
    const expiryMs = 10 * 60 * 1000;
    if (Date.now() - data.savedAt > expiryMs) {
      clearReconnectToken();
      return null;
    }

    return data;
  } catch {
    clearReconnectToken();
    return null;
  }
}

export function clearReconnectToken(): void {
  localStorage.removeItem(RECONNECT_KEY);
}
