# NPP - Network Ping Pong

Multiplayer ping pong igra u realnom vremenu sa .NET backend-om i React frontend-om.

## Tehnologije

- **Backend**: .NET 10, ASP.NET Core, SignalR
- **Frontend**: React 19, TypeScript, Vite
- **Baza podataka**: Cassandra (za perzistenciju - planirano)
- **Cache**: Redis (za stanje igre u realnom vremenu - planirano)
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

2. Pokrenite aplikaciju:
   ```bash
   cd docker
   docker compose up --build
   ```

3. Otvorite pregledac:
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

Oba servisa podrzavaju hot reload u development modu:

- **Backend**: `dotnet watch` automatski restartuje pri promeni .cs fajlova
- **Frontend**: Vite HMR automatski osvezava browser pri promeni

### Pokretanje bez Docker-a

**Backend:**
```bash
cd dotnet/NppApi
dotnet watch run
```

**Frontend:**
```bash
cd react
npm install
npm run dev
```

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
