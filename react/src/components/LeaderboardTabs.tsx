import { useState } from 'react';
import './LeaderboardTabs.css';

export type TabType = 'global' | 'wins' | 'streaks';

interface LeaderboardTabsProps {
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
}

export function LeaderboardTabs({ activeTab, onTabChange }: LeaderboardTabsProps) {
  return (
    <div className="leaderboard-tabs">
      <button
        className={`tab-button ${activeTab === 'global' ? 'active' : ''}`}
        onClick={() => onTabChange('global')}
      >
        <span className="tab-icon">🌍</span>
        <span className="tab-label">Global Rankings</span>
      </button>
      
      <button
        className={`tab-button ${activeTab === 'wins' ? 'active' : ''}`}
        onClick={() => onTabChange('wins')}
      >
        <span className="tab-icon">🏆</span>
        <span className="tab-label">Most Wins</span>
      </button>
      
      <button
        className={`tab-button ${activeTab === 'streaks' ? 'active' : ''}`}
        onClick={() => onTabChange('streaks')}
      >
        <span className="tab-icon">🔥</span>
        <span className="tab-label">Longest Streaks</span>
      </button>
    </div>
  );
}

interface PeriodSelectorProps {
  selectedPeriod: string;
  onPeriodChange: (period: string) => void;
}

export function PeriodSelector({ selectedPeriod, onPeriodChange }: PeriodSelectorProps) {
  const getCurrentMonth = () => {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    return `${year}-${month}`;
  };

  const getPreviousMonth = () => {
    const now = new Date();
    const year = now.getMonth() === 0 ? now.getFullYear() - 1 : now.getFullYear();
    const month = now.getMonth() === 0 ? 12 : now.getMonth();
    return `${year}-${String(month).padStart(2, '0')}`;
  };

  const getCurrentYear = () => {
    return String(new Date().getFullYear());
  };

  const periods = [
    { value: `MONTHLY|${getCurrentMonth()}`, label: 'This Month' },
    { value: `MONTHLY|${getPreviousMonth()}`, label: 'Last Month' },
    { value: `YEARLY|${getCurrentYear()}`, label: 'This Year' },
    { value: 'ALL_TIME|all', label: 'All Time' },
  ];

  return (
    <div className="period-selector">
      <label htmlFor="period">Time Period:</label>
      <select
        id="period"
        value={selectedPeriod}
        onChange={(e) => onPeriodChange(e.target.value)}
        className="period-select"
      >
        {periods.map((period) => (
          <option key={period.value} value={period.value}>
            {period.label}
          </option>
        ))}
      </select>
    </div>
  );
}
