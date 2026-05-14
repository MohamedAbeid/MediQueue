# MediQueue

MediQueue is a comprehensive healthcare appointment and clinic management system built with ASP.NET Core MVC. It is designed to streamline the administration of medical facilities, providing an intuitive, RTL-compliant interface for managing users, clinics, appointments, and patient queues.

## 🚀 Features

- **Role-Based Access Control (RBAC):** Secure authentication and authorization using ASP.NET Core Identity with specific roles for Admin, Doctor, Receptionist, and Patient.
- **Appointment Management:** Patients can book appointments, and staff can track and manage them efficiently.
- **Doctor & Clinic Interfaces:** Dedicated portals for doctors to manage their available slots and patient queues.
- **Admin Dashboard:** System-wide monitoring and management of users, roles, and clinics.
- **RTL-Compliant UI:** Designed with Right-to-Left (RTL) support for a seamless experience in Arabic and other RTL languages.
- **User Management:** Robust functionalities for password resets, profile editing, and user tracking.

## 🛠️ Technology Stack

- **Framework:** ASP.NET Core MVC (.NET)
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Frontend:** HTML5, CSS3, JavaScript (RTL-compliant design)

## 🏗️ Project Structure

- `Controllers/`: Handles incoming HTTP requests and routing.
- `Models/`: Data models and Entity Framework schema definitions.
- `Views/`: Razor pages (`.cshtml`) for the user interface.
- `BL/` (Business Logic): Services for clinics, users, appointments, doctor slots, and queues.
- `ViewModel/`: Data transfer objects for views.
- `Data/` & `Migrations/`: Database context and migration files.

## ⚙️ Setup and Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   ```
2. **Navigate to the project directory:**
   ```bash
   cd MediQueue
   ```
3. **Configure Database Connection:**
   Update the `DefaultConnection` string in `appsettings.json` to point to your local SQL Server instance.
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=MediQueueDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```
4. **Apply Migrations:**
   Open the Package Manager Console or use the .NET CLI to apply pending migrations and create the database.
   ```bash
   dotnet ef database update
   ```
   *Note: The application is configured to automatically seed basic roles and an Admin user on the first run.*

5. **Run the Application:**
   ```bash
   dotnet run
   ```
   Or open the solution in Visual Studio and run it.

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the issues page.
