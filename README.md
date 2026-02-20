# NPP - Network Ping Pong

Multiplayer ping pong igra u realnom vremenu sa .NET backend-om i React frontend-om.

## Tehnologije

- **Backend**: .NET 10, ASP.NET Core, SignalR
- **Frontend**: React 19, TypeScript, Vite
- **Baza podataka**: Cassandra (za perzistenciju - planirano)
- **Cache**: Redis (za redovno čuvanje snapshot-a stanja igre)
- **Kontejnerizacija**: Docker, Docker Compose

## Struktura projekta

```
npp/
├── docker/
│   └── docker-compose.yml    # Docker Compose konfiguracija
├── dotnet/
│   ├── NppApi/               # ASP.NET Core Web API
│   │   ├── Hubs/             # SignalR hub-ovi
│   │   └── Services/         # Servisi (GameManager)
│   └── NppCore/              # Deljeni modeli i logika
│       └── Models/           # Game, Ball, Paddle, Player
└── react/
    └── src/
        ├── components/       # React komponente (Lobby, GameCanvas)
        └── hooks/            # Custom hook-ovi (useGameHub)
```

## Pokretanje

### Preduslovi

- Docker i Docker Compose

### Koraci

1. Klonirajte repozitorijum:
   ```bash
   git clone <repo-url>
   cd npp
   ```

2. Popunite .env
   - u direktorijumu _./docker_
   - na osnovu _./docker/.env.example_


3. Pokrenite aplikaciju:
   ```bash
   cd docker
   docker compose up --build
   ```

4. Dodajte tabele u Cassandra-u:
   - konektovati se na cassandradb (localhost:9042)
   - izvršiti sve komande iz fajla _./dotnet/NppCore/Db/schema.cql_

5. Otvorite pregledac:
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000

## Kako igrati

1. Otvorite http://localhost:3000 u dva browser taba (ili na dva racunara)

2. **Tab 1 - Kreiranje igre:**
   - Unesite vase ime
   - Kliknite "Create Game"
   - Sacekajte protivnika

3. **Tab 2 - Pridruzivanje:**
   - Unesite vase ime
   - U listi otvorenih igara kliknite "Join"

4. **Kontrole:**
   - `W` ili `Strelica Gore` - pomeranje palice gore
   - `S` ili `Strelica Dole` - pomeranje palice dole

5. **Cilj:**
   - Prvi igrac koji postigne 5 poena pobedjuje

## Razvoj

### Hot Reload

Oba servisa podrzavaju hot reload:

- **Backend**: `dotnet watch` automatski restartuje pri promeni .cs fajlova
- **Frontend**: Vite HMR automatski osvezava browser pri promeni


## Arhitektura

```
┌─────────────┐     SignalR      ┌─────────────┐
│   React     │◄────WebSocket────►│   .NET      │
│   Client    │                   │   Backend   │
├─────────────┤                   ├─────────────┤
│ - Canvas    │                   │ - GameHub   │
│ - Lobby UI  │                   │ - GameState │
│ - SignalR   │                   │ - In-memory │
└─────────────┘                   └─────────────┘
```

- **SignalR** se koristi za real-time komunikaciju izmedju klijenata i servera
- **GameManager** servis upravlja stanjem svih aktivnih igara u memoriji
- **Game loop** radi na 60 FPS i racuna fiziku lopte i kolizije
- Stanje igre se salje svim igracima 60 puta u sekundi
- Stanje igre se takođe pamti kao **snapshot** i omogućava oporavak i vraćanje u prethodno u slučaju prekida igre
