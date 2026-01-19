-- Tabela: players
INSERT INTO players (player_id, username, email, avatar_url, created_at) 
VALUES (550e8400-e29b-41d4-a716-446655440000, 'Nikola_Ping', 'nikola@email.com', 'http://avatar.com/n1', toTimestamp(now()));

INSERT INTO players (player_id, username, email, avatar_url, created_at) 
VALUES (660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong', 'maja@email.com', 'http://avatar.com/m2', toTimestamp(now()));

-- Tabela: players_by_email (Lookup tabela za login)
INSERT INTO players_by_email (email, password_hash, player_id)
VALUES ('nikola@email.com', 'hash_lozinka_123', 550e8400-e29b-41d4-a716-446655440000);

INSERT INTO players_by_email (email, password_hash, player_id)
VALUES ('maja@email.com', 'hash_lozinka_456', 660f9511-f30c-52e5-b827-557766551111);

-- Tabela: player_matches (Nikolina istorija)
INSERT INTO player_matches (player_id, year, match_time, opponent_id, opponent_username, score, result)
VALUES (550e8400-e29b-41d4-a716-446655440000, '2024', '2024-05-20 14:30:00', 660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong', '11:9, 11:7', 'WIN');

INSERT INTO player_matches (player_id, year, match_time, opponent_id, opponent_username, score, result)
VALUES (550e8400-e29b-41d4-a716-446655440000, '2024', '2024-05-20 15:00:00', 660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong', '5:11, 8:11', 'LOSS');

-- Tabela: player_stats (Counter tabele se ažuriraju sa UPDATE, ne INSERT)
UPDATE player_stats SET total_points = total_points + 35, games_won = games_won + 1, games_lost = games_lost + 1 
WHERE player_id = 550e8400-e29b-41d4-a716-446655440000;

UPDATE player_stats SET total_points = total_points + 38, games_won = games_won + 1, games_lost = games_lost + 1 
WHERE player_id = 660f9511-f30c-52e5-b827-557766551111;

-- Tabela: global_leaderboard (Mesečni snapshot za maj 2024)
INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2024-05', 1250, 550e8400-e29b-41d4-a716-446655440000, 'Nikola_Ping');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2024-05', 1180, 660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong');

-- Tabela: player_current_streak
INSERT INTO player_current_streak (player_id, username, current_streak, longest_streak, last_result)
VALUES (550e8400-e29b-41d4-a716-446655440000, 'Nikola_Ping', 3, 7, 'WIN');

-- Tabela: leaderboard_by_longest_streak
INSERT INTO leaderboard_by_longest_streak (category, longest_streak, player_id, username)
VALUES ('global_all_time', 7, 550e8400-e29b-41d4-a716-446655440000, 'Nikola_Ping');

INSERT INTO leaderboard_by_longest_streak (category, longest_streak, player_id, username)
VALUES ('global_all_time', 12, 660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong');

-- =====================================================
-- DODATNI TEST PODACI ZA LEADERBOARDS
-- =====================================================

-- Više igrača u global leaderboard (Maj 2024)
INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2024-05', 1400, 770e8400-e29b-41d4-a716-446655440001, 'ProGamer_2024');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2024-05', 1350, 880e8400-e29b-41d4-a716-446655440002, 'PingMaster');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2024-05', 1200, 990e8400-e29b-41d4-a716-446655440003, 'TableTennis_Pro');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2024-05', 1150, aa0e8400-e29b-41d4-a716-446655440004, 'SpinDoctor');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2024-05', 1050, bb0e8400-e29b-41d4-a716-446655440005, 'PaddleChamp');

-- Trenutni mesec (Januar 2026)
INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2026-01', 850, 550e8400-e29b-41d4-a716-446655440000, 'Nikola_Ping');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2026-01', 920, 660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('MONTHLY', '2026-01', 1050, 770e8400-e29b-41d4-a716-446655440001, 'ProGamer_2024');

-- All-time leaderboard
INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('ALL_TIME', 'all', 5600, 770e8400-e29b-41d4-a716-446655440001, 'ProGamer_2024');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('ALL_TIME', 'all', 4800, 660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('ALL_TIME', 'all', 4200, 550e8400-e29b-41d4-a716-446655440000, 'Nikola_Ping');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('ALL_TIME', 'all', 3900, 880e8400-e29b-41d4-a716-446655440002, 'PingMaster');

INSERT INTO global_leaderboard (period_type, period_id, rank_score, player_id, username)
VALUES ('ALL_TIME', 'all', 3500, 990e8400-e29b-41d4-a716-446655440003, 'TableTennis_Pro');

-- Leaderboard po broju pobeda
INSERT INTO leaderboard_by_wins (category, games_won, player_id, username)
VALUES ('most_wins', 145, 770e8400-e29b-41d4-a716-446655440001, 'ProGamer_2024');

INSERT INTO leaderboard_by_wins (category, games_won, player_id, username)
VALUES ('most_wins', 132, 660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong');

INSERT INTO leaderboard_by_wins (category, games_won, player_id, username)
VALUES ('most_wins', 118, 550e8400-e29b-41d4-a716-446655440000, 'Nikola_Ping');

INSERT INTO leaderboard_by_wins (category, games_won, player_id, username)
VALUES ('most_wins', 105, 880e8400-e29b-41d4-a716-446655440002, 'PingMaster');

INSERT INTO leaderboard_by_wins (category, games_won, player_id, username)
VALUES ('most_wins', 98, 990e8400-e29b-41d4-a716-446655440003, 'TableTennis_Pro');

INSERT INTO leaderboard_by_wins (category, games_won, player_id, username)
VALUES ('most_wins', 87, aa0e8400-e29b-41d4-a716-446655440004, 'SpinDoctor');

INSERT INTO leaderboard_by_wins (category, games_won, player_id, username)
VALUES ('most_wins', 73, bb0e8400-e29b-41d4-a716-446655440005, 'PaddleChamp');

-- Više igrača u longest streak leaderboard
INSERT INTO leaderboard_by_longest_streak (category, longest_streak, player_id, username)
VALUES ('global_all_time', 18, 770e8400-e29b-41d4-a716-446655440001, 'ProGamer_2024');

INSERT INTO leaderboard_by_longest_streak (category, longest_streak, player_id, username)
VALUES ('global_all_time', 15, 880e8400-e29b-41d4-a716-446655440002, 'PingMaster');

INSERT INTO leaderboard_by_longest_streak (category, longest_streak, player_id, username)
VALUES ('global_all_time', 10, 990e8400-e29b-41d4-a716-446655440003, 'TableTennis_Pro');

INSERT INTO leaderboard_by_longest_streak (category, longest_streak, player_id, username)
VALUES ('global_all_time', 9, aa0e8400-e29b-41d4-a716-446655440004, 'SpinDoctor');

INSERT INTO leaderboard_by_longest_streak (category, longest_streak, player_id, username)
VALUES ('global_all_time', 6, bb0e8400-e29b-41d4-a716-446655440005, 'PaddleChamp');

-- Dodatni streakovi za test igrače
INSERT INTO player_current_streak (player_id, username, current_streak, longest_streak, last_result)
VALUES (660f9511-f30c-52e5-b827-557766551111, 'Maja_Pong', 5, 12, 'WIN');

INSERT INTO player_current_streak (player_id, username, current_streak, longest_streak, last_result)
VALUES (770e8400-e29b-41d4-a716-446655440001, 'ProGamer_2024', 8, 18, 'WIN');

INSERT INTO player_current_streak (player_id, username, current_streak, longest_streak, last_result)
VALUES (880e8400-e29b-41d4-a716-446655440002, 'PingMaster', 0, 15, 'LOSS');