

# Solution architecture (EN)

Web application designed as RESTful communication between .NET backend and React frontend. In future, might get upgraded into using WebSockets.

## Database

For development purposes, we are using Postgres latest that is setup in docker container with initialization script that contains structure and initial seed data required for testing workflow of solution. Whenever the script is 'merged' from main, local database should be reinitialized in order to maintain same state locally.
We opted for using multi-tenant database while cross-entity ownership constraints will be handled on backend.
Custom „types“ are defined as postgres enums in order to reduce number of infrastructure tables (table status, payment type, employee roles, subscription tiers, etc.)
 
See Figure 1: Database Schema for visual design.
 
## Backend

For backend, we are using .NET 9 that is structured as multiple projects: Data, Domain, „Backend“, „Tests“, etc for better code reusability and easier maintenance. 

### Data 
Class library which contains models, enums and DBContext required for communication with database via EntityFramework.

### Domain 
Class library which contains designed to handle business logic. 
Currently contains following items:
-	generic repository
-	services
-	utilities
-	interfaces
-	DTOs
It was decided that each service will have access only to necessary repository instances, rather than entire database context, especially for Create, Update and Delete. Any „custom“ querying is achievable via exposed IQueryable on repository, but such implementations should be done carefully.
Services call upon repositories to fetch data (via models) which are then transformed into dedicated DTO based on request from controller. Transformation is done via AutoMapper tool that uses defined maps for each DTO <-> Entity pair.
Authorization and Cross-entity constraints are handled with JWT from request. It includes verifying the role of user before performing a request, and extracting the placeid used for filtering entities based on facility/business where user is stationed at.

### „Backend“ 
ASP Net Core project that contains controllers, DI and necessary configuration. On development startup, launches Scalar UI that uses OpenApi documentation. We use it to test endpoints manually.

### „Tests“ 
Test project which currently contains only service-based unit tests based on NUnit and NSubstitute.

## Frontend

Uses Node.js latest version and React, currently set up with Tailwind and no external libraries for prebuilt components.
It was decided to use typescript and tsx based files.

----------------------------------------------------------

# Arhitektura rješenja (HR)

Web-aplikacija dizajnirana na temelju RESTful komunikacije između .NET pozadinskog sustava i React korisničkog sučelja. U budućnosti je plan unaprijediti komunikaciju korištenjem WebSocket-a gdje ima potrebe za time.

## Baza podataka

Za potrebe razvoja koristimo najnoviju verziju PostgreSQL-a postavljenu unutar Docker kontejnera s inicijalizacijskim skriptama koje sadrže strukturu baze (Slika 1) i početne podatke potrebne za testiranje tijeka rada rješenja. Svaki put kada se skripta „spoji“ s glavnom granom, lokalna baza podataka treba se ponovno inicijalizirati kako bi se održalo konzistentno stanje baze na svakom računalu.
Odlučili smo se za korištenje višeorganizacijske (multi-tenant) baze podataka, dok će se ograničenja vlasništva među entitetima rješavati s poslužiteljske strane (backendu).
Prilagođeni „tipovi“ definirani su kao PostgreSQL enumeracije (enumi) kako bi se smanjio broj pomoćnih tablica (statusi narudžbi, načini plaćanja, uloge zaposlenika, razine pretplate i sl.).
 
Pogledati Slika 1: Struktura baze podataka
 
## Poslužiteljska aplikacija (backend)

Za pozadinski dio koristimo .NET 9, strukturiran kao više odvojenih projekata: Data, Domain, Backend, Tests, itd., kako bi se postigla bolja iskoristivost koda i lakše održavanje.

### Data 
Biblioteka klasa (class library) koja sadrži modele, enum vrijednosti i DbContext potreban za komunikaciju s bazom podataka putem Entity Frameworka.

### Domain 
Biblioteka klasa namijenjena za implementaciju poslovne logike.
Trenutno uključuje:
-	Generički repozitorij
-	Servise
-	Pomoćne klase (utilities)
-	Sučelja (interfaces)
-	DTO-ove
Odlučeno je da svaki servis ima pristup samo potrebnim repozitorijima, umjesto čitavom kontekstu baze podataka, posebno za operacije stvaranja, ažuriranja i brisanja. Bilo kakve prilagođene upite moguće je provoditi pomoću izložene IQueryable metode unutar repozitorija, no takve implementacije treba pažljivo izvesti.
Servisi dohvaćaju podatke pomoću repozitorija (preko modela), a zatim ih transformiraju u odgovarajuće DTO-ove ovisno o zahtjevu kontrolera. Transformacija se obavlja pomoću AutoMapper alata, koristeći unaprijed definirane mape za svaki par DTO <-> entitet.
Autorizacija i međuentitetska ograničenja rješavaju se pomoću JWT tokena iz zahtjeva. To uključuje provjeru uloge korisnika prije izvođenja zahtjeva, te izdvajanje placeid parametra koji se koristi za filtriranje entiteta ovisno o objektu u kojem je korisnik zaposlen.

### „Backend“ 

ASP.NET Core projekt koji sadrži kontrolere, Dependency Injection i potrebne konfiguracije. Pri pokretanju u razvojnom okruženju, pokreće Scalar UI sučelje temeljeno na OpenAPI dokumentaciji – koristi se za ručno testiranje API endpointa.


### „Tests“ 

Test projekt koji trenutno sadrži samo jedinicne testove servisa temeljene na NUnit i NSubstitute.

## Korisničko sučelje (Frontend)

Frontend koristi najnoviju verziju Node.js i React, trenutno konfiguriran s Tailwind CSS-om bez korištenja vanjskih knjižnica za gotove komponente.
Odlučeno je koristiti TypeScript i .tsx datoteke.
