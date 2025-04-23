# Commands
Collection of useful shortcuts and commands for development.

### Backend 
dotnet run --urls "http://0.0.0.0:7214"
dotnet test ./backend/BartenderTests/BartenderTests.csproj --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
dotnet add package <Name>

### EF Migrations
dotnet ef migrations add <Name>
dotnet ef database update

### Frontend
npm install
npm run dev
npm run build

### Docker
docker build -no-cache
docker compose up
docker compose ps -a

### Git
git reflog
git reflog show <87-backend-notifications-follow-up>
git reset --hard <Commithash>
git fsck --lost-found
git checkout -b <restore-notifications> <3c2f553>

### Other
curl -f http://localhost:7214/health