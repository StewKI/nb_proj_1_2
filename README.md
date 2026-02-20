# NPP - (No)Ping Pong

Multiplayer ping pong igra u realnom vremenu sa .NET backend-om i React frontend-om.

## Tehnologije

- **Backend**: .NET 10, ASP.NET Core, SignalR
- **Frontend**: React 19, TypeScript, Vite
- **Baza podataka**: Cassandra (za trajno čuvanje podataka o igračima, mečevima i leaderboard-u)
- **Cache**: Redis (za čuvanje snapshot-a aktivnih igara)
- **Kontejnerizacija**: Docker, Docker Compose

## Struktura projekta

```
nb_proj_1_2/
├── docker/
│   └── docker-compose.yml    # Docker Compose konfiguracija
├── dotnet/
│   ├── NppApi/               # ASP.NET Core Web API
│   │   ├── Controllers/      # REST endpointi (Auth, Player, Leaderboard...)
│   │   ├── Hubs/             # SignalR hub (GameHub)
│   │   └── Services/         # Servisi (GameManager, LeaderboardSnapshot)
│   └── NppCore/              # Deljeni modeli i logika
│       ├── Models/           # Game, Ball, Paddle, Player, Leaderboard...
│       ├── Services/         # Business logika (Auth, Player, Match, Leaderboard)
│       └── Db/               # Cassandra schema i primeri upita
└── react/
    └── src/
        ├── pages/            # Stranice (Login, Register, Game, Leaderboard)
        ├── components/       # React komponente (Lobby, GameCanvas, MatchHistory...)
        ├── hooks/            # Custom hook-ovi (useGameHub)
        ├── contexts/         # Auth kontekst
        └── services/         # API klijenti
```

## Pokretanje

### Preduslovi

- Docker i Docker Compose

### Koraci

1. Klonirajte repozitorijum:
   ```bash
   git clone <repo-url>
   cd nb_proj_1_2
   ```

2. Popunite `.env` fajl:
   - u direktorijumu `./docker/`
   - na osnovu `./docker/.env.example`

3. Pokrenite aplikaciju:
   ```bash
   cd docker
   docker compose up --build
   ```

4. Dodajte tabele u Cassandra-u (samo prvi put):
   - Konektujte se na Cassandra (localhost:9042)
   - Izvršite sve komande iz fajla `./dotnet/NppCore/Db/schema.cql`

5. Otvorite pregledač:
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000

## Kako igrati

1. Otvorite http://localhost:3000 u dva browser taba (ili na dva računara)

2. **Tab 1 - Kreiranje igre:**
   - Registrujte se / Ulogujte se
   - Kliknite "Create Game"
   - Sačekajte protivnika

3. **Tab 2 - Pridruživanje:**
   - Registrujte se / Ulogujte se
   - U listi otvorenih igara kliknite "Join"

4. **Kontrole:**
   - `W` ili `Strelica Gore` — pomeranje palice gore
   - `S` ili `Strelica Dole` — pomeranje palice dole

5. **Cilj:**
   - Prvi igrač koji postigne 5 poena pobeđuje

## Trajnost podataka i nastavak igre

Projekat je dizajniran tako da se može bezbedno ugasiti i ponovo pokrenuti bez gubitka podataka.

- **Podaci o igračima** (profili, statistike, istorija mečeva, leaderboard) trajno se čuvaju u Cassandra bazi i ostaju sačuvani bez obzira na restart.
- **Aktivne igre** se periodično snimaju kao snapshot u Redis. Kada se projekat ponovo pokrene, igrači mogu da se rekonektuju i nastave igru od mesta gde su stali.

### Gašenje i pokretanje

```bash
# Gašenje (podaci ostaju sačuvani)
docker compose down

# Ponovno pokretanje
docker compose up
```

## Arhitektura

```
┌─────────────┐     SignalR       ┌──────────────────────┐
│   React     │◄────WebSocket────►│      .NET Backend    │
│   Client    │                   ├──────────────────────┤
├─────────────┤                   │ GameHub (SignalR)    │
│ - Canvas    │     REST API      │ GameManager (in-mem) │
│ - Lobby UI  │◄────HTTP──────────│ Controllers (REST)   │
│ - Auth      │                   └──────────┬───────────┘
└─────────────┘                              │
                                   ┌─────────┴──────────┐
                                   │                    │
                             ┌─────▼──────┐    ┌────────▼───────┐
                             │  Cassandra │    │     Redis      │
                             │ (trajni    │    │ (snapshot-i    │
                             │  podaci)   │    │  aktivnih      │
                             └────────────┘    │  igara)        │
                                               └────────────────┘
```

- **SignalR** se koristi za real-time komunikaciju između klijenata i servera
- **GameManager** servis upravlja stanjem svih aktivnih igara u memoriji i šalje update-e na 60 FPS
- **Redis** čuva snapshot-e aktivnih igara — pri restartu server učitava poslednji snapshot i igrači mogu da se rekonektuju i nastave
- **Cassandra** trajno čuva sve podatke: profile igrača, statistike, istoriju mečeva i leaderboard
