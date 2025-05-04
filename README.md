# Bartender MK2

Bartender MK2 is a **modern, modular hospitality platform** for cafés, bars, and restaurants.  
It helps owners manage businesses, places, staff, and – most importantly – **turn every table into a smart, QR‑powered touch‑point** for guests.

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
![C#](https://img.shields.io/badge/C♯-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![PostgreSQL 17+](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=azure-devops&logoColor=white)


## Architecture

Clean Onion layout:

- **Domain** — pure business logic & DTOs  
- **Data** — EF Core models & repositories  
- **API** — thin controllers

## Getting Started
### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/)  
- PostgreSQL 17+  
- (Optional) **Docker & Compose**

### Local setup
```bash
# clone & restore
git clone https://github.com/your-org/bartender-mk2.git
cd bartender-mk2

# set connection string
echo "ConnectionStrings__Default=Host=localhost;Port=5432;Database=bartender;Username=postgres;Password=postgres" >> .env

# run DB migrations & seed
dotnet ef database update
psql -f db/seed.sql

# launch API
dotnet run --project Bartender.API

# open a second terminal
cd frontend

# --- prerequisites ---
# Node 20 LTS or newer (tested on 22.13.1)
node -v   # make sure it’s ≥20.x

# --- install deps ---
npm install

# --- start Vite dev server ---
npm run dev     # default http://localhost:5173
```

### Project Structure
```
mk2/
├── backend/ # .NET solution folder
│ ├── Bartender.API/ # Controllers, Exception Handlers, Program.cs
│ ├── Bartender.Domain/ # Core services, DTOs
│ ├── Bartender.Data/ # EF Core models, migrations
│ └── tests/ # NUnit
├── frontend/
├── db/ # Seed scripts, backups
├── documentation/ # Architecture diagrams, ADRs, ERD
├── docker-compose.yml # Postgres + backend + Redis + frontend for local dev
└── .github/ # Workflows
```

### API Reference
Swagger UI is available at https://localhost:7281/swagger when the backend is running locally.
Scalar UI available at https://localhost:7281/scalar when the backend is running locally.
OpenAPI JSON spec is auto-generated at https://localhost:7281/openapi/v1.json.

### Database Schema
The application uses PostgreSQL with a code-first schema managed by EF Core.
Frequently updated ER Diagram lives in documentation/DevUtils/database-schema.dbml
Migrations are stored in backend/Bartender.Data/Migrations.
Seed data is applied via db/seed.sql.

```
# Add a new migration
dotnet ef migrations add AddSomethingImportant -p Bartender.Data -s Bartender.API

# Apply migrations
dotnet ef database update
```

### CI/CD and Testing
The app uses NUnit and NSubstitute for test coverage and service mocking.
CI/CD consists of few pipelines.
These include: build, running tests, generating coverage, creating images, pushing images to repo, running the compose stack, healthchecks, OWASP Zap, SonarQube Analysis, running Migrations and seeding test data (if test env) and lastly, deploy to Azure as 2x container applications.

### Roadmap
- Guest session system (QR, JWT, table lockout)
- Place setup (employees, products, menu)
- Staff & place-based authorization policies
- Guest interaction - scan, enter session, order, request bill, call staff
- Main overview screen - notifications, manager interactions, UI updates via SignalR
- Redis Cache notifications
- Save tables based on place blueprint

- Admin dashboard analytics & filters
- Stripe payments & digital checkout
- Better mobile support
