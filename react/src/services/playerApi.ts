export interface PlayerStats {
  totalPoints: number;
  wins: number;
  losses: number;
  winRate: string;
}

export async function getMyStats(): Promise<PlayerStats> {
  const token = localStorage.getItem('jwt_token');

  const response = await fetch('/player/me/stats', {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
  });

  if (!response.ok) {
    throw new Error('Failed to fetch player stats');
  }

  return response.json();
}
