# PBM Dashboard

This is a standalone PBM-only project. It does not depend on the insurance solution.

## Projects

- `PBMDashboard.Api`: ASP.NET Core API for PBM KPIs
- `pbm-dashboard`: Angular 14 frontend
- `database`: SQL Server schema and seed script

## Run API

```bash
dotnet run --project PBMDashboard.Api/PBMDashboard.Api.csproj --launch-profile http
```

## Run Frontend

```bash
cd pbm-dashboard
npm install
npm start
```

## Endpoint

- `GET /api/dashboard/pbm`
