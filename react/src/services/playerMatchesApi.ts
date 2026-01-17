import type { PlayerMatches, PlayerMatchesRequest, PlayerMatchesResponse } from "../types/playerMatches";

const API_BASE = "api/playermatches"


export async function getMatchesByYear(year: string, page: number = 1, limit: number = 5) :Promise<PlayerMatchesResponse[]> {

    const storedUser = localStorage.getItem('npp_user');
    let token = '';

    
    if (storedUser) {
        const userObject = JSON.parse(storedUser);
        token = userObject.token; 
    }
//?page=${page}&limit=${limit}
    const response = await fetch(`${API_BASE}/${year}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    });
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'PlayerMatchesByYear get failed');
    }

    return response.json();
}