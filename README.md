# Bartender MK2

![License](https://img.shields.io/github/license/your-org/bartender-mk2)
![Build](https://img.shields.io/github/actions/workflow/status/your-org/bartender-mk2/ci.yml?label=build)
![Coverage](https://img.shields.io/codecov/c/github/your-org/bartender-mk2)

Bartender MK2 is a **modern, modular hospitality platform** for cafés, bars, and restaurants.  
It helps owners manage businesses, places, staff, and – most importantly – **turn every table into a smart, QR‑powered touch‑point** for guests.

> *From first scan to paid cheque — Bartender keeps the shift flowing.*

---

## Table of Contents
1. [Features](#features)  
2. [Tech Stack](#tech-stack)  
3. [Architecture](#architecture)  
4. [Getting Started](#getting-started)  
5. [Usage Examples](#usage-examples)  
6. [Project Structure](#project-structure)  
7. [API Reference](#api-reference)  
8. [Database Schema](#database-schema)  
9. [Testing & Quality](#testing--quality)  
10. [Deployment](#deployment)  
11. [Roadmap](#roadmap)  
12. [Contributing](#contributing)  
13. [License](#license)  
14. [Contact & Acknowledgements](#contact--acknowledgements)

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
