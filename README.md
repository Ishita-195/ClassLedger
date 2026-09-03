# ClassLedger — Student Result & Attendance System

A role-based web application for schools to digitize student academic records — attendance tracking, exam marks, class/subject management, and an analytics dashboard — with separate experiences for Admins, Teachers, and Students.

<p align="center">
  <a href="https://classledger.onrender.com">
    <img src="https://img.shields.io/badge/Try_Live_Demo-Open_App-17516b?style=for-the-badge&logo=render&logoColor=white" alt="Try the live demo" />
  </a>
</p>

---

## About

**ClassLedger** replaces error-prone, scattered manual record-keeping with a single, role-based platform where a school can:

- Track daily student attendance (Present / Absent / Leave)
- Record exam-wise marks per subject per student
- Organize multi-class, multi-section school structures with a shared subject curriculum
- Give Admins, Teachers, and Students their own role-appropriate views
- See academic trends on an admin analytics dashboard

The project began as an ASP.NET Core MVC application backed by **Oracle DB**, built during a one-month IT internship at **IFFCO Paradeep Limited Corporation** (May 2025). It has since been modernized to **.NET 8** with an embedded **SQLite** database, so it now runs and deploys anywhere with **zero database setup** — the schema is created and seeded automatically on first run.

**Live demo:** `https://classledger.onrender.com` — hosted free on Render (the first request after idle may take ~30–50s to wake). Sign in with a demo account from the table below.

---

## Features

- **Dashboard** — at-a-glance counts (students, classes, attendance records) and a recent-students list
- **Students** — searchable roster with add/delete and per-class filtering
- **Marks** — pick a student, then view their marks across all 6 subjects with total, average, grade, and pass counts
- **Attendance** — pick a student, then view a monthly breakdown (present / absent / leave / working days / percentage) with a doughnut chart and a month selector
- **Classes & Subjects** — class cards with student counts plus a shared curriculum panel
- **Analytics (Admin)** — four Chart.js visualizations backed by SQL aggregation queries
- **Auth** — session-based login with role-aware navigation and access control
- **Seeded demo data** — 22 students, a 6-subject curriculum, marks for every student, and two months of daily attendance so the app looks actively used

---

## Architecture

```
[Login / Auth]
      ↓
[Role Check] → Admin / Teacher / Student   (session-based)
      ↓
[MVC Controllers] → Business Logic (C#)
      ↓
[Razor Views + Bootstrap] → Server-rendered UI
      ↓
[Entity Framework Core + raw aggregate SQL]
      ↓
[SQLite database] → SR_USERS, SR_STUDENTS, SR_CLASSES,
                    SR_SUBJECTS, SR_MARKS, SR_ATTENDANCE
```

---

## Quick Setup

### Prerequisites
- **.NET 8 SDK** (no database server required — SQLite is embedded)

### Run locally

```bash
git clone https://github.com/Ishita-195/ClassLedger.git
cd ClassLedger/StudentResult
dotnet run
```

Then open the URL shown in the console (defaults to `http://localhost:5000`).

On first run the app **creates and seeds** a local `studentresult.db` SQLite file automatically — there is no schema script to run and no connection string to configure. To reset all data back to the seed, stop the app, delete `studentresult.db`, and run again.

### Run with Docker

```bash
docker build -t classledger .
docker run -p 8080:8080 -e PORT=8080 classledger
# open http://localhost:8080
```

### Deploy to Render (free)

The repo includes a `render.yaml` Blueprint and a `Dockerfile`. In the Render dashboard: **New → Blueprint → select this repo → Apply**. Render builds the container and hosts it on the free tier; the SQLite data re-seeds on each deploy.

---

## Demo Accounts

| Role    | Email                | Password    |
|---------|----------------------|-------------|
| Admin   | `admin@school.com`   | `admin123`  |
| Teacher | `sharma@school.com`  | `teacher123`|
| Student | `ravi@student.com`   | `ravi123`   |

> These are seeded demo credentials for a public showcase. They are intentionally simple and are stored in plain text — do not reuse them for anything holding real data.

---

## File Structure

```
ClassLedger/
├── StudentResult/                  # ASP.NET Core MVC application (.NET 8)
│   ├── Controllers/                # HomeController, ReportsController
│   ├── Models/                     # Entities, ModelContext (EF Core + seeding), ViewModels
│   ├── Views/
│   │   ├── Home/                   # Login, Dashboard, Students, Marks, StudentMarks,
│   │   │                           #   Attendance, StudentAttendance, Classes, ReportCard
│   │   ├── Reports/                # Admin analytics dashboard
│   │   └── Shared/                 # _Layout, _Icon, _StudentGrid, Error
│   ├── wwwroot/                    # site.css, Bootstrap + Chart.js (vendored locally)
│   ├── appsettings.json
│   ├── Startup.cs
│   └── Program.cs                  # App entry point + first-run DB seeding
│
├── Dockerfile                      # Container build (Render / any host)
├── render.yaml                     # Render Blueprint (free web service)
├── schema.sql                      # Original Oracle schema (historical reference)
├── StudentResult.sln               # Visual Studio solution file
└── .gitignore
```

---

## Key Components

### 1. Data layer (`Models/ModelContext.cs`)

- **Entity Framework Core** with the **SQLite** provider — one self-contained `.db` file, no server.
- Six mapped tables: `SR_USERS`, `SR_CLASSES`, `SR_SUBJECTS`, `SR_STUDENTS`, `SR_MARKS`, `SR_ATTENDANCE`.
- `EnsureSeeded()` creates the schema and seeds sample data on startup (only when empty).

**Core tables**
```
SR_USERS       → Login accounts with roles (Admin / Teacher / Student)
SR_CLASSES     → Classes + sections (e.g., 10th A)
SR_SUBJECTS    → Curriculum subjects (Physics, Chemistry, Hindi, Math, Biology, History)
SR_STUDENTS    → Student profiles linked to class + (optional) login account
SR_MARKS       → Exam-wise marks per student per subject
SR_ATTENDANCE  → Daily attendance per student per class
```

### 2. Controllers (`Controllers/`)

- Handle HTTP requests and route to Razor views.
- **Session-based role checks** guard each action (redirecting unauthenticated or unauthorized users); mutating actions are `POST` + anti-forgery token.
- `ReportsController` runs raw `GROUP BY` aggregate queries for the analytics charts.

### 3. Views (`Views/`)

- Server-rendered Razor + Bootstrap, styled from a single design-token stylesheet (`wwwroot/css/site.css`).
- **Marks** and **Attendance** use a consistent master → detail flow: a student card grid, then a per-student detail page.
- UI icons are inline SVG (no emoji, no icon-font dependency) via the `_Icon` partial.

### 4. Analytics (`Views/Reports/Index.cshtml`)

- Four **[Chart.js](https://www.chartjs.org/)** charts, served **locally** from `wwwroot/js/chartjs/chart.umd.min.js` (no external CDN).

---

## Role Capabilities

| Feature                      | Admin | Teacher | Student |
|------------------------------|:-----:|:-------:|:-------:|
| Manage classes & subjects    |  Yes  |   No    |   No    |
| Manage students              |  Yes  |   Yes   |   No    |
| Mark attendance              |  Yes  |   Yes   |   No    |
| Enter exam marks             |  Yes  |   Yes   |   No    |
| View own report card         |  Yes  |   Yes   |   Yes   |
| View own attendance          |  Yes  |   Yes   |   Yes   |
| Analytics dashboard          |  Yes  |   No    |   No    |

---

## Analytics Dashboard (Admin)

An admin-only dashboard visualizes academic trends across the school. It lives at
**`/Reports`** (sidebar → *Analytics → Reports*) and is guarded by the same session-based
role check used elsewhere — non-admins are redirected away.

Charts render with **Chart.js** (vendored locally). Each chart is backed by a single
**`GROUP BY` aggregation query** run directly against the **SQLite** database, so the
database does the aggregation and only compact result sets reach the app (no in-memory
LINQ over full tables). Every chart has its own ViewModel (see `Models/ReportsVM.cs`).

| Chart | Question it answers | Aggregation source |
|-------|---------------------|--------------------|
| Attendance % distribution | How are students spread across attendance bands? | Per-student present/total %, bucketed, over `SR_ATTENDANCE` |
| Pass / fail per course | Which subjects have the most failures? | Pass = scored ≥ 40% of total, grouped by subject over `SR_MARKS` |
| Grade / marks distribution | What's the overall grade spread? | Mark % bucketed into A–F bands over `SR_MARKS` |
| Avg attendance vs avg result | Does attendance correlate with results? | Per-student avg attendance % vs avg result % (scatter) |

### Screenshot

<!-- Replace the placeholder below with an actual screenshot of the /Reports dashboard -->
![Analytics dashboard screenshot placeholder](docs/screenshots/reports-dashboard.png)

> _Screenshot placeholder — capture the `/Reports` page while logged in as Admin and save it to `docs/screenshots/reports-dashboard.png`._

---

## Troubleshooting

**Build errors**
```bash
dotnet restore          # restore NuGet packages
dotnet --version        # requires the .NET 8 SDK
```

**Port already in use** — set a port explicitly:
```bash
dotnet run --urls http://localhost:5055
```

**Reset the database** — the SQLite file is disposable:
```bash
# from StudentResult/
rm studentresult.db     # it is recreated and re-seeded on the next run
```

**Login not working** — use a seeded account from the Demo Accounts table above. (Login is `POST`-only; navigating directly to the login handler by URL returns 405 by design.)

---

## Technology Stack

- **Runtime**: .NET 8
- **Backend**: ASP.NET Core MVC (C#)
- **Data access**: Entity Framework Core (SQLite provider) + raw aggregate SQL for analytics
- **Database**: SQLite (embedded, auto-seeded)
- **Frontend**: Razor Views, Bootstrap, custom CSS design tokens, inline SVG icons
- **Charts**: Chart.js (vendored locally, no CDN)
- **Deployment**: Docker + Render (free tier)

---

## Project History

ClassLedger started as an internship deliverable at **IFFCO Paradeep Limited Corporation**
(May 2025), built with ASP.NET Core MVC and **Oracle DB** to mirror the internal tooling
common in corporate IT environments. It has since been modernized for portability and
public hosting:

- **.NET Core 2.1 → .NET 8**
- **Oracle (ODP.NET) → SQLite (EF Core)** — no database server or connection string needed
- Login and all mutations moved to `POST` + anti-forgery tokens
- Student-centric Marks & Attendance flows, an analytics dashboard, a responsive UI, and a containerized deployment

The original Oracle schema is preserved as `schema.sql` for reference.

---

## Author

**Ishita Anand**
B.Tech CSE, KIIT University (2023–2027)
[GitHub](https://github.com/Ishita-195) · [LinkedIn](https://linkedin.com/in/ishita-anand-791770343)
