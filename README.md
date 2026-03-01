# Pharmacy Benefit Manager Dashboard

This project is a standalone Pharmacy Benefit Manager (PBM) dashboard built for hospital and healthcare administrators who need a quick, visual way to understand pharmacy operations. The goal is simple: make it easier to spot trends, monitor performance, and act on issues before they become expensive or disruptive.

Instead of digging through spreadsheets or waiting on static reports, this dashboard brings together the key metrics that matter day to day. It highlights claim activity, month-to-date pharmacy spend, prior authorization pressure, generic dispensing trends, facility-level performance, and operational alerts in one place.

The application is split into a lightweight backend API and an Angular frontend. The API provides aggregated PBM KPI data, and the frontend presents that data in an interactive dashboard designed for fast decision-making.

## What This Project Includes

- `PBMDashboard.Api`: an ASP.NET Core Web API that generates and serves PBM dashboard metrics
- `pbm-dashboard`: an Angular 14 frontend that renders the dashboard UI
- `database`: a SQL Server schema and seed script for PBM reporting scenarios

## Core Dashboard Focus

This dashboard is built around the kinds of questions operations teams ask every day:

- How many claims are being processed right now?
- What is the current pharmacy spend trend?
- Are generic dispensing targets being met?
- Which facilities are seeing slower turnaround or lower approval rates?
- Are urgent prior authorizations starting to pile up?

The included demo dataset helps the dashboard feel realistic out of the box, so the application is immediately useful for presentations, demos, or as a base for a real implementation.

## Technology Stack

- C#
- ASP.NET Core (`.NET 7`)
- Angular 14
- TypeScript
- Bootstrap 5
- SQLite for local development
- SQL Server script for relational deployment or reporting

## Running The Project

Start the backend API:

```bash
dotnet run --project PBMDashboard.Api/PBMDashboard.Api.csproj --launch-profile http
```

Then start the Angular frontend:

```bash
cd pbm-dashboard
npm install
npm start
```

The Angular app is configured to proxy API requests to the local backend during development.

## Main API Endpoint

- `GET /api/dashboard/pbm`

This endpoint returns the full dashboard payload, including summary KPIs, monthly spend trends, department utilization, facility performance comparisons, and alert data.

## Who This Is For

This project is a good fit for:

- healthcare administrators
- pharmacy operations teams
- internal analytics teams
- developers building PBM or hospital operations tools

It works well as a starter template, a prototype for stakeholder review, or a foundation for a more advanced production dashboard.
