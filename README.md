# ClassLedger — Student Result & Attendance System

A full-stack ASP.NET Core MVC web application built during my IT internship at **IFFCO Paradeep Limited Corporation** (May 2025) to digitize student academic record management — attendance tracking, result entry, and role-based dashboards backed by Oracle DB.


## Project Overview

Manual management of student attendance and results is error-prone, scattered, and hard to audit. This system provides a centralized, role-based platform for:

- ✅ Tracking daily student attendance (Present / Absent / Leave)
- ✅ Recording exam-wise marks per subject per student
- ✅ Managing multi-class, multi-section school structures
- ✅ Role-based login for Admin, Teacher, and Student
- ✅ Oracle DB-backed relational schema with full referential integrity

### Architecture

```
[Login / Auth]
      ↓
[Role Check] → Admin / Teacher / Student
      ↓
[MVC Controller] → Business Logic (C#)
      ↓
[Razor Views] → HTML + CSS Frontend
      ↓
[ADO.NET / ODP.NET] → Oracle DB Queries
      ↓
[Oracle Database] → SR_USERS, SR_STUDENTS, SR_CLASSES,
                    SR_SUBJECTS, SR_MARKS, SR_ATTENDANCE
```

---

## Quick Setup (5 minutes)

### Step 1: Clone the Repository

```bash
git clone https://github.com/Ishita-195/student-attendance-system.git
cd student-attendance-system
```

### Step 2: Set Up Oracle Database

```bash
# Run schema script in your Oracle SQL client
@schema.sql
```

This creates 6 tables and seeds sample data:
- 4 users (Admin, Teacher, 2 Students)
- 2 classes (10th A, 9th A)
- 4 subjects mapped to classes
- Sample marks and attendance records

### Step 3: Configure Connection String

Update `StudentResult/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=<your_user>;Password=<your_pass>;Data Source=<datasource>"
  }
}
```

### Step 4: Build and Run

```bash
cd StudentResult
dotnet build
dotnet run
```

App runs at `https://localhost:5001` by default.

---

## File Structure

```
student-attendance-system/
├── StudentResult/                  # ASP.NET Core MVC application
│   ├── Controllers/                # Route handlers per module
│   ├── Models/                     # Entity + ViewModel classes
│   ├── Views/                      # Razor HTML templates
│   │   ├── Attendance/             # Attendance views
│   │   ├── Results/                # Marks entry & view
│   │   └── Shared/                 # Layout, nav, partials
│   ├── appsettings.json            # DB config
│   └── Program.cs                  # App entry point
│
├── schema.sql                      # Oracle DB schema + seed data
├── StudentResult.sln               # Visual Studio solution file
└── .gitignore
```

---

## Key Components

### 1. Database Schema (`schema.sql`)

- 6 normalized Oracle tables with foreign key constraints
- Role-based user model: `Admin`, `Teacher`, `Student`
- Exam types supported: Unit Test, Mid Term, Final
- Attendance status: `Present`, `Absent`, `Leave`

**Usage:**
```sql
-- Load in Oracle SQL*Plus or SQL Developer
@schema.sql

-- Verify tables created
SELECT table_name FROM user_tables WHERE table_name LIKE 'SR_%';
```

### 2. MVC Controllers (`StudentResult/Controllers/`)

- Handle HTTP requests and route to appropriate views
- Enforce role-based access before serving data
- Execute Oracle queries via ADO.NET / ODP.NET

**Example flow:**
```csharp
// Teacher marks attendance for a class
[Authorize(Roles = "Teacher,Admin")]
public IActionResult MarkAttendance(int classId, DateTime date)
{
    // Fetch students → render form → save to SR_ATTENDANCE
}
```

### 3. Razor Views (`StudentResult/Views/`)

- Server-side rendered HTML with C# logic
- Attendance form: date picker + student list + status dropdown
- Results view: subject-wise marks table per exam type
- Admin dashboard: user and class management

### 4. Oracle DB Layer

- Oracle Data Provider for .NET (ODP.NET) handles all queries
- Parameterized queries for safe data access
- Referential integrity enforced at DB level via FK constraints

**Core tables:**
```sql
SR_USERS       → Login accounts with roles
SR_CLASSES     → Classes + sections (e.g., 10th A)
SR_SUBJECTS    → Subjects mapped per class
SR_STUDENTS    → Student profiles linked to class + user
SR_MARKS       → Exam-wise marks per student per subject
SR_ATTENDANCE  → Daily attendance per student per class
```

---

## Role Capabilities

| Feature                  | Admin | Teacher | Student |
|--------------------------|-------|---------|---------|
| Manage users & classes   | ✅    | ❌      | ❌      |
| Mark attendance          | ✅    | ✅      | ❌      |
| Enter exam marks         | ✅    | ✅      | ❌      |
| View own results         | ✅    | ✅      | ✅      |
| View own attendance      | ✅    | ✅      | ✅      |
| Analytics dashboard      | ✅    | ❌      | ❌      |

---

## Analytics Dashboard (Admin)

An admin-only analytics dashboard visualizes academic trends across the school.
It lives at **`/Reports`** (sidebar → *Analytics → Reports*) and is guarded by the
same session-based role check used elsewhere — non-admins are redirected away.

Charts are rendered with **[Chart.js](https://www.chartjs.org/)**, served **locally**
from `wwwroot/js/chartjs/chart.umd.min.js` (no external CDN). Each chart is backed by
a single **`GROUP BY` aggregation query** run directly against the Oracle schema, so the
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

## Performance Notes

| Operation                   | Notes                              |
|-----------------------------|------------------------------------|
| Attendance save (bulk)      | Single transaction per class/date  |
| Marks retrieval (per student) | Indexed on STUDENTID + SUBJECTID |
| Login auth                  | Role resolved from SR_USERS table  |
| Schema setup                | One-time, ~30 seconds              |

---

## Troubleshooting

### Oracle connection fails
```bash
# Verify ODP.NET is installed
dotnet list package | grep Oracle

# Check connection string in appsettings.json
# Ensure Oracle service is running on your system
```

### Schema tables not found
```sql
-- Confirm you ran schema.sql in the correct schema/user
SELECT table_name FROM user_tables WHERE table_name LIKE 'SR_%';

-- If empty, re-run:
@schema.sql
```

### Build errors
```bash
# Restore NuGet packages
dotnet restore

# Check .NET SDK version (requires 6.0+)
dotnet --version
```

### Login not working
```sql
-- Verify seed users were inserted
SELECT USERNAME, USERROLE FROM SR_USERS;

-- Default credentials from schema:
-- Admin: admin@school.com / admin123
-- Teacher: sharma@school.com / teacher123
-- Student: ravi@student.com / ravi123
```

---

## Technology Stack

- **Backend**: ASP.NET Core MVC (C#)
- **Frontend**: HTML, CSS (Razor Views), Chart.js (vendored locally) for analytics charts
- **Database**: Oracle DB
- **DB Access**: ADO.NET + Oracle Data Provider for .NET (ODP.NET)
- **IDE**: Visual Studio 2022
- **Schema**: SQL with Oracle-specific syntax (VARCHAR2, NUMBER, TO_DATE)

---

## Internship Context

Built as part of a one-month IT internship at **IFFCO Paradeep Limited Corporation** in May 2025. The project provided hands-on experience with enterprise .NET development, Oracle database schema design, and MVC architecture in a real-world organizational setting — mirroring the kind of internal tooling commonly used in corporate IT environments.

---

## Summary

You now have a **complete, role-based academic management system** that:

✅ **Tracks** daily student attendance per class  
✅ **Records** exam-wise marks across subjects  
✅ **Enforces** role-based access (Admin / Teacher / Student)  
✅ **Backed** by a normalized Oracle DB schema  
✅ **Built** with ASP.NET Core MVC and Razor Views  
✅ **Ready** to extend with reporting or notifications

---
  
**Framework**: ASP.NET Core MVC + Oracle DB  
**Status**: Complete — Internship Deliverable

---

## Author

**Ishita Anand**  
B.Tech CSE, KIIT University (2023–2027)  
[GitHub](https://github.com/Ishita-195) · [LinkedIn](https://linkedin.com/in/ishita-anand-791770343)
