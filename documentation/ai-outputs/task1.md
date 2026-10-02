# Task 1 - Architecture (RCTC prompt)

Tool: Google Gemini (gemini.google.com), 2026-10-02

## Prompt

```text
Role: Act as a lead systems architect.
Context: A team of 3 BSIT students has 180 minutes to build an Online Campus Event Management System. Students view upcoming events and register; administrators view registered attendees.
Task: Propose the architecture, database entities, API endpoints, folder structure, and division of work. Use semantic HTML, CSS, vanilla JavaScript, ASP.NET Core, and SQL Server. Include a short scope assessment.
Constraints: Do not use third-party state management libraries like Redux. Do not use microservices, payment processing, or email delivery. Keep database passwords out of source control. Keep it small enough to finish in 3 hours.
```

## Gemini output

## Scope Assessment: The 180-Minute MVP

Three hours is a sprint. To guarantee a working deliverable, we must aggressively cut scope. We will omit a formal login system. Students will register by simply entering their Student ID and Name into a form. The admin dashboard will be a standalone, unlinked HTML page (`/admin.html`) rather than protected by role-based authorization. We will use a single monolithic project to avoid cross-origin (CORS) setup time, serving the frontend directly from the backend.

## Architecture Proposal

- **Pattern:** Monolithic Web API with static file hosting.
- **Backend:** ASP.NET Core Web API handling HTTP requests and business logic.
- **Database Access:** Entity Framework (EF) Core connecting to SQL Server.
- **Frontend:** Semantic HTML5, standard CSS, and vanilla JavaScript hosted in the `wwwroot` folder. State will be managed entirely in the DOM using standard event listeners and the Fetch API.
- **Security:** Database connection strings must be stored using the `dotnet user-secrets` tool or kept in a local `appsettings.Development.json` that is explicitly added to `.gitignore`. Never commit credentials.

## Database Entities

To save time, we will denormalize the `Student` into the `Registration` table. This eliminates the need to manage a separate Student CRUD process.

| Entity | Primary Key | Attributes | Navigation Properties |
| --- | --- | --- | --- |
| Event | Id (Guid) | Title (string), Description (string), EventDate (datetime), Location (string) | ICollection<Registration> Registrations |
| Registration | Id (Guid) | StudentId (string), StudentName (string), RegisteredAt (datetime) | EventId (Guid), Event Event |

## API Endpoints

Keep controllers thin. Use simple Data Transfer Objects (DTOs) to prevent over-posting and circular reference loops during JSON serialization.

| Method | Endpoint | Purpose | Request Body | Response |
| --- | --- | --- | --- | --- |
| GET | /api/events | Fetch all upcoming events | None | 200 OK (Array of Events) |
| POST | /api/events/{id}/register | Register a student | { studentId, studentName } | 201 Created |
| GET | /api/events/{id}/attendees | Admin view of registrations | None | 200 OK (Array of Registrations) |

## Folder Structure

Keep everything in a single ASP.NET Core solution to eliminate CORS configuration and simplify execution.

```text
CampusEventManager/
├── Controllers/
│   └── EventsController.cs
├── Data/
│   └── AppDbContext.cs
├── Models/
│   ├── Event.cs
│   └── Registration.cs
├── DTOs/
│   └── RegistrationDto.cs
├── wwwroot/
│   ├── index.html        (Student View: Semantic tags <header>, <main>, <article>)
│   ├── admin.html        (Admin View)
│   ├── css/
│   │   └── style.css
│   └── js/
│       ├── app.js        (DOM manipulation, Event Listeners)
│       └── api.js        (Fetch calls)
├── appsettings.json
├── Program.cs            (Configure services, AddControllers, UseStaticFiles)
└── CampusEventManager.csproj
```

## Division of Work

To maximize the 180 minutes, the team must work in parallel.

**Developer 1: Database & Infrastructure (Minutes 0 - 90)**

- Initialize the ASP.NET Core Web API project.
- Create the Event and Registration model classes.
- Configure AppDbContext and wire up EF Core.
- Use `dotnet user-secrets` to configure the SQL Server connection string.
- Run Entity Framework migrations and seed the database with 3 dummy events so the frontend has data to fetch.

**Developer 2: API & Backend Logic (Minutes 30 - 120)**

- Prerequisite: Dev 1 finishes the Models.
- Build the EventsController.
- Implement the GET endpoints for events and attendees.
- Implement the POST endpoint for registration, ensuring basic validation (e.g., checking if the EventId exists before saving).
- Test all endpoints locally using Swagger/Postman to ensure clean JSON responses.

**Developer 3: Frontend UI & Integration (Minutes 0 - 150)**

- Write semantic HTML (index.html and admin.html) and basic CSS grid/flexbox layouts. Do not waste time on pixel-perfect design.
- Write the vanilla JavaScript fetch() calls in api.js.
- Prerequisite: Dev 2 finishes endpoints (or use mock JSON files temporarily).
- Wire the DOM: dynamically render the event list into HTML `<article>` cards, attach submit listeners to the registration form, and render the attendee table on the admin page.

**The Final 30 Minutes (Minutes 150 - 180): Team Integration**

All three developers stop building. Pull code together, resolve merge conflicts, ensure the static files are being served correctly by Program.cs via `app.UseStaticFiles()`, and conduct an end-to-end test of registering a student and viewing them on the admin page.
