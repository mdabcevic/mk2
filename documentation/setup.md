
# Development setup (EN)

Follow along these steps to replicate development environment for entire project:
1.	Install Docker Desktop based on your OS: https://www.docker.com/products/docker-desktop/
2.	Clone repository (access needed beforehand): https://github.com/mdabcevic/mk2
3.	Install .NET 9 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
4.	Update Visual Studio.
5.	Install Node.js: https://nodejs.org/en/download
6.	Open cmd and navigate to BartenderBackend – you should see a docker-compose.yml there
7.	Run following command: docker compose up
    - Postgres container appears on Docker Desktop, and you can inspect its logs to verify whether database was created, seeded and if it's operational
8.	Open cmd and navigate to /frontend/mk2.
9.	Run following command: npm install

At this point, database, backend and frontend are configured and ready for launching. You can do so with following steps:
-	Open Backend.sln located in backend folder and run it within Visual Studio. Scalar UI should launch in browser. 
-	In frontend/mk2 run following command that starts up frontend: npm run dev

---------------------

#	Postavljanje razvojnog okruženja (Development setup)

Potrebno je slijediti navedene korake za potpuni setup:
1.	Instalacija Docker Desktop alata: https://www.docker.com/products/docker-desktop/
2.	Kloniranje repozitorija (potrebno zatražiti pristup): https://github.com/mdabcevic/mk2
3.	Instalacija NET 9 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
4.	Ažuriranje Visual Studio u skladu s novim SDK.
5.	Instalacija Node.js okruženja: https://nodejs.org/en/download
6.	Otvoriti direktorij BartenderBackend u konzoli – tamo se nalazi docker-compose.yml datoteka
7.	Pokrenuti naredbu: docker compose up
8.	Provjera na Docker Desktop sučelju je li Postgres kontejner pokrenut. Dodatno, pogledati logove kontejnera za provjeru konkretnih koraka: stvaranje, popunjavanje i operativni status baze
9.	Otvoriti direktorij /frontend/mk2 u konzoli.
10.	Pokrenuti naredbu: npm install

Nakon izvršavanja prethodnih koraka, uspješno je postavljeno okruženje te je spremno za pokretanje poslužitelsjke aplikacije i / ili korisničkog sučelja pomoću navedenih naredbi:
-	Otvoriti Backend.sln (Visual Studio) unutar backend direktorija; pokretanjem se otvara Scalar UI putem google Chrome koji omogućava pregled i testiranje kontrolera tokom razvoja 
-	U direktoriju frontend/mk2 pokrenuti naredbu: npm run dev
