\# Habit Tracker



A web-based Habit Tracker developed using ASP.NET Core MVC, Entity Framework Core, ASP.NET Core Identity, Razor Views, and SQL Server.



\## Features



\- User registration and login

\- Role-based access using ASP.NET Core Identity

\- Create and manage habits

\- Track daily habit completion

\- Habit logs and progress tracking

\- Calendar-based habit tracking

\- Admin functionality

\- Demo data seeding



\## Technology Stack



\- ASP.NET Core MVC

\- C#

\- Entity Framework Core

\- ASP.NET Core Identity

\- Razor Views

\- SQL Server

\- HTML, CSS and JavaScript



\## Project Architecture



The application follows an MVC-based layered architecture:



User Interface  

↓  

Razor Views  

↓  

Controllers  

↓  

Services / Business Logic  

↓  

ApplicationDbContext  

↓  

Entity Framework Core  

↓  

SQL Server



\## Database



The main entities include:



\- ApplicationUser

\- Habit

\- HabitLog



The relationships follow:



ApplicationUser → Habits → HabitLogs



Entity Framework Core is used to communicate with the SQL Server database.



\## Data Layer



The Data layer contains:



\- `ApplicationDbContext.cs` - Configures the EF Core database context, entities and relationships.

\- `DbInitializer.cs` - Initializes roles and the default administrator account.

\- `DemoDataSeeder.cs` - Generates demo users, habits and habit logs.



\## Running the Project



1\. Clone or download the repository.

2\. Open the solution in Visual Studio or JetBrains Rider.

3\. Configure the SQL Server connection string.

4\. Restore the required NuGet packages.

5\. Build the project.

6\. Run the application.



\## Team Project



Group 17 - Habit Tracker

