-- =====================================================
-- KEYSPACE
-- =====================================================
CREATE KEYSPACE IF NOT EXISTS pong_game
WITH replication = {
    'class': 'SimpleStrategy',
    'replication_factor': 1
};

USE pong_game;

-- =====================================================
-- PLAYERS (osnovni podaci o igraču)
-- =====================================================
CREATE TABLE IF NOT EXISTS players (
    player_id uuid PRIMARY KEY,
    username text,
    email text,
    avatar_url text,
    created_at timestamp
);

-- =====================================================
-- PLAYERS BY EMAIL (login / autentifikacija)
-- =====================================================
CREATE TABLE IF NOT EXISTS players_by_email (
    email text PRIMARY KEY,
    password_hash text,
    player_id uuid
);

-- =====================================================
-- MATCH HISTORY (mečevi po igraču i godini)
-- =====================================================
CREATE TABLE IF NOT EXISTS player_matches (
    player_id uuid,
    year text,                    -- npr. "2026"
    match_time timestamp,
    opponent_id uuid,
    opponent_username text,       -- denormalizacija radi brzine
    score text,                   -- npr. "11:7"
    result text,                  -- WIN / LOSS
    PRIMARY KEY ((player_id, year), match_time)
) WITH CLUSTERING ORDER BY (match_time DESC);

-- =====================================================
-- PLAYER STATS (ukupna statistika)
-- =====================================================
CREATE TABLE IF NOT EXISTS player_stats (
    player_id uuid PRIMARY KEY,
    total_points counter,
    games_won counter,
    games_lost counter
);

-- =====================================================
-- MONTHLY LEADERBOARD (statistika po mesecu)
-- =====================================================
CREATE TABLE IF NOT EXISTS monthly_leaderboard (
    year_month text,              -- npr. "2026-01"
    player_id uuid,
    total_points counter,
    PRIMARY KEY (year_month, player_id)
);

-- =====================================================
-- REDIS STRUCTURES (DOKUMENTACIJA)
-- =====================================================
-- Redis nema CQL tabele, ali ovo je dokumentacija
-- ključeva koji se koriste u sistemu
--
-- 1) GLOBAL LEADERBOARD (Sorted Set)
--    Key: leaderboard:global
--    ZADD leaderboard:global 0 player_id        (registracija)
--    ZINCRBY leaderboard:global 50 player_id    (tokom meča)
--    ZREVRANGE leaderboard:global 0 9 WITHSCORES
--
-- 2) MONTHLY LEADERBOARD (Sorted Set)
--    Key: leaderboard:{YYYY-MM}
--    ZINCRBY leaderboard:2026-01 50 player_id
--    ZREVRANGE leaderboard:2026-01 0 9 WITHSCORES
--
-- 3) ACTIVE GAME STATE (Hash)
--    Key: game:{gameId}
--    Fields:
--      ball_x
--      ball_y
--      p1_y
--      p2_y
--      p1_score
--      p2_score
--      state        -- WAITING | RUNNING | FINISHED
--
-- Primer:
-- HSET game:123 ball_x 120 ball_y 300 p1_y 200 p2_y 210 p1_score 3 p2_score 5 state RUNNING
