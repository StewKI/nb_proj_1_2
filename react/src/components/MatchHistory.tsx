import React, { useState, useEffect } from 'react';
import { GetHistory } from '../services/playerMatchesApi';
import type { MatchHistory } from '../types/playerMatches';
import './MatchHistory.css'; // <--- OBAVEZNO: Importujemo CSS fajl

export const HistoryWindow: React.FC = () => {
    const [matches, setMatches] = useState<MatchHistory[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const ITEMS_PER_PAGE = 10;
    const FETCH_LIMIT = 50;

    const loadMatches = async () => {
        try {
            const data = await GetHistory(1, FETCH_LIMIT); // Vuče 50 poslednjih završenih
            setMatches(data);
            setError('');
        } catch (err) {
            console.error(err);
            setError('Greska pri ucitavanju');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadMatches();
        // Osvežavamo na 60 sekundi jer se istorija sporije menja nego live rezultati
        const interval = setInterval(loadMatches, 60000);
        return () => clearInterval(interval);
    }, []);

    // Paginacija
    const totalPages = Math.ceil(matches.length / ITEMS_PER_PAGE);
    const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
    const currentMatches = matches.slice(startIndex, startIndex + ITEMS_PER_PAGE);

    const handleNext = () => {
        if (currentPage < totalPages) setCurrentPage(prev => prev + 1);
    };

    const handlePrev = () => {
        if (currentPage > 1) setCurrentPage(prev => prev - 1);
    };

    if (loading && matches.length === 0) return <div className="recent-feed-container loading-msg">Učitavanje...</div>;
    if (error && matches.length === 0) return null;

    return (
        <div className="recent-feed-container">
            <div className="feed-header">
                {/* IZMENA: Nema više "Live" i pulsiranja */}
                <h3 className="header-title">Poslednji Mečevi 🏁</h3>
            </div>

            <ul className="match-list1">
                {currentMatches.map((match) => {
                    const isP1Winner = match.p1Username === match.score;
                    const isP2Winner = match.p2Username === match.score;

                    return (
                        <li  className="match-item">
                            <div className="match-row">
                                <span
                                    className={`player-name ${isP1Winner ? 'winner-text' : ''}`}
                                    title={match.p1Username}
                                >
                                    {match.p1Username}
                                </span>

                                <span className="match-score final-score">
                                    {match.result}
                                </span>

                                <span
                                    className={`player-name ${isP2Winner ? 'winner-text' : ''}`}
                                    title={match.p2Username}
                                >
                                    {match.p2Username}
                                </span>
                            </div>

                        
                        </li>
                    );
                })}
            </ul>

            {totalPages > 1 && (
                <div className="pagination">
                    <button
                        onClick={handlePrev}
                        disabled={currentPage === 1}
                        className="nav-btn"
                    >
                        ◀
                    </button>
                    <span className="page-info">{currentPage} / {totalPages}</span>
                    <button
                        onClick={handleNext}
                        disabled={currentPage === totalPages}
                        className="nav-btn"
                    >
                        ▶
                    </button>
                </div>
            )}
        </div>
    );
};