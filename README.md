# Bartender MK2

Bartender MK2 is a **modern, modular hospitality platform** for cafés, bars, and restaurants.  
It enables guests to scan, order, and request help, while giving staff and managers full control of the venue floor, menus, and service flow.  
No stress. Bartender makes the communication easy.

> *From first scan to paid cheque — Bartender keeps the shift flowing.*

---

## Table of Contents
1. [Features](#features)  
2. [Tech Stack](#tech-stack)  
3. [Architecture](#architecture)  
4. [Getting Started](#getting-started)  
5. [Project Structure](#project-structure)  
6. [API Reference](#api-reference)  
7. [Database Schema](#database-schema)  
8. [Testing & Quality](#testing--quality)  
9. [Roadmap](#roadmap)  

---

## Features
- **Multi‑business hierarchy** — Businesses → Places → Staff  
- **Role‑aware auth** — shared JWT pipeline for Staff & Guests  
- **QR‑based table workflow**  
  - Unique rotating salt per table, printed on a QR  
  - Guest scans → session starts/resumes  
  - Staff scans auto‑mark *occupied* and can change status  
- **Table management suite** — create, enable/disable, regenerate QR, adjust seats  
- **Staff management** — onboard, update, revoke staff within a place  
- **Places & menus** — rich place details with eager‑loaded menu items  
- **Subscription tiers** — upgrade/downgrade business plans on the fly  
- **Plug‑in architecture (planned)** — payments, loyalty, analytics, etc.

## Tech Stack

![.NET 9](https://img.shields.io/badge/.NET%209-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![PostgreSQL 17+](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=azure-devops&logoColor=white)
![React](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)
![Vite](https://img.shields.io/badge/Vite-646CFF?style=for-the-badge&logo=vite&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![SonarCloud](https://img.shields.io/badge/SonarCloud-F3702A?style=for-the-badge&logo=sonarcloud&logoColor=white)
![NUnit](https://img.shields.io/badge/NUnit-823089?style=for-the-badge&logo=nunit&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-2088FF?style=for-the-badge&logo=githubactions&logoColor=white)

## Architecture

Full Architecture explained in the [Wiki](https://github.com/mdabcevic/mk2/wiki/Architecture).  
Clean Onion layout on backend:

- **Domain** — pure business logic & DTOs  
- **Data** — EF Core models & repositories  
- **API** — thin controllers

![image](https://github.com/user-attachments/assets/ef283fb6-6cdb-4dd1-8e7d-69845987660d)

### Core frontend dependencies

**Tailwind**: css stilization of components and screens
**i18n**: language translations
**Nivo** components: analytic components (heatmaps, barcharts, line graphs...)
**qrcode**: generation of QR codes (remark: likely to be enhanced with QR stilization from here.
**jspdf**: printing QR to PDF.

### Core backend dependencies
- **Microsoft.AspNetCore**: Base for Web API & middleware
- **Microsoft.EntityFrameworkCore**: ORM for PostgreSQL
- **Npgsql.EntityFrameworkCore.PostgreSQL**: PostgreSQL provider for EF Core
- **BCrypt.Net**: Password hashing for staff auth
- **AutoMapper**: DTO ↔ Entity conversions
- **NUnit, NUnit3TestAdapter, NUnit.Analyzers**: Test framework
- **NSubstitute**: Lightweight mocking
- **Coverlet.collector**: Code coverage tool for SonarCloud
- **Testcontainers.PostgreSql**: Spins up disposable Postgres containers for integration tests
- **StackExchange.Redis**: Used for pub/sub with SignalR backplane
- **Serilog.AspNetCore**: Structured loggin


## Getting Started

### Developer Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/)  
- Visual Studio 2022+ (with .NET 9 support)
- PgAdmin 4 (or other Postgres client)
- Docker
- Node.js (v20+) & npm

### Local setup

1. Clone repository: https://github.com/your-org/bartender-mk2.git
2. move to mk2\frontend
3. Install frontend dependencies via command line: npm install
4. Start local infrastructure via Docker Compose: docker-compose up -d
5. Connect to Dockerized postgres cluster (via PgAdmin)
6. Create database bartenderdb, with user 'admin' and password 'adminpass'
7. Open the mk2\backend\Backend.sln with Visual Studio
8. Install Global EF Tools if not yet installed: dotnet tool install --global dotnet-ef
9. Run following command: dotnet ef database update --project Bartender.Data --startup-project BartenderBackend
10. Open Developer PowerShell and navigate to mk2\backend, then apply migrations: dotnet ef database update --project Bartender.Data --startup-project BartenderBackend
11. In PgAdmin, apply the queries from mk2\db\initseed.sql to the database
12. Run local "https" project via Visual Studio
13. Open command line and navigate to mk2\frontend
14. Run npm run dev
15. Visit http://localhost:5174/

### Project Structure
```
mk2/
├── backend/ # .NET solution folder
│ ├── Bartender.API/ # Controllers, Exception Handlers, Program.cs
│ ├── Bartender.Domain/ # Core services, DTOs
│ ├── Bartender.Data/ # EF Core models, migrations
│ └── tests/ # Unit and Integration tests
├── frontend/
├── db/ # Seed scripts, backups
├── documentation/ # Architecture diagrams, ADRs, ERD
├── docker-compose.yml # Postgres + backend + Redis + frontend for local dev
└── .github/ # Workflows
```

### Use Cases
Detailed use cases are explained in [Wiki Use Cases](https://github.com/mdabcevic/mk2/wiki/Use-Cases).   
Quick Figma prototype is available [here](https://www.figma.com/proto/9skQgT6qOISLZYrR51RInZ/Bartender?node-id=63-170&starting-point-node-id=211%3A4346).
Final demo video can be found on [this url](URL!!).

### API Reference
Swagger UI is available at https://localhost:7281/swagger when the backend is running locally.  
Scalar UI available at https://localhost:7281/scalar when the backend is running locally.  
OpenAPI JSON spec is auto-generated at https://localhost:7281/openapi/v1.json.  

### Database Schema
The application uses PostgreSQL with a code-first schema managed by EF Core.
Frequently updated ER Diagram lives in [Wiki pages](https://github.com/mdabcevic/mk2/wiki/Architecture#database).
Migrations are stored in backend/Bartender.Data/Migrations.
Seed data is applied via db/initseed.sql.

![image](https://github.com/user-attachments/assets/e6221057-7756-414d-9b44-1d407af1e4ed)

### CI/CD and Testing
The app uses NUnit and NSubstitute for test coverage and service mocking.
CI/CD consists of few pipelines.
These include: build, running tests, generating coverage, creating images, pushing images to repo, running the compose stack, healthchecks, OWASP Zap, SonarQube Analysis, running Migrations and seeding test data (if test env) and lastly, deploy to Azure as 2x container applications.

![image](https://github.com/user-attachments/assets/4f0ba97d-af72-4f01-8ae8-13091f122192)

![image](https://github.com/user-attachments/assets/1e768f9d-81bf-46ca-80da-75f03319c028)

Full [Deployment details](https://github.com/mdabcevic/mk2/wiki/Architecture#backend) are in Architecture and External Tools Wiki pages.

### Roadmap
- Guest session system (QR, JWT, table lockout)
- Place setup (employees, products, menu)
- Staff & place-based authorization policies
- Guest interaction - scan, enter session, order, request bill, call staff
- Main overview screen - notifications, manager interactions, UI updates via SignalR
- Redis Cache notifications
- Save tables based on place blueprint
- Admin dashboard analytics & place overview

- Stripe payments & digital checkout
- Guest login and features
