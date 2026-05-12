# Automation Exercise Platform

A complete e-commerce platform with user authentication, product catalog, shopping cart, and checkout functionality built with ASP.NET Core 8 and Angular.

## Prerequisites (Install These First)

Before running this project, you need the following installed on your computer:

### 1. .NET 8 SDK

Download: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

After install, verify with:
dotnet --version

### 2. SQL Server

SQL Server LocalDB (Lightweight)
Download: https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver17

### 3. SQL Server Management Studio (SSMS)

Purpose: To restore the database backup file
Download: https://learn.microsoft.com/en-us/ssms/install/install

### 4. Node.js

Download: https://nodejs.org/en
Version: 18.x or later

Verify with:
node --version
npm --version

### 5. Angular CLI

Install with:
npm install -g @angular/cli

Verify with:
ng version

### 6. Git (for cloning the repository)

Download: https://git-scm.com/

---

## Getting the Project

### Clone with Git

git clone https://github.com/AlexandraMarcu13/Assignment.git
cd YOUR_REPOSITORY_NAME

## Database Setup

### Step 1: Locate the Database Backup File

Find the .bak file in your project folder:
Database/AutomationExercise.bak

### Step 2: Open SQL Server Management Studio (SSMS)

1. Launch SSMS from Start Menu
2. Connect to your SQL Server:
   - Server type: Database Engine
   - Server name options:
     - (localdb)\MSSQLLocalDB (for LocalDB)
     - .\SQLEXPRESS (for SQL Express)
     - localhost (for full SQL Server)
   - Authentication: Windows Authentication
3. Click Connect

### Step 3: Restore the Database

Using T-SQL Query

1. Click New Query button
2. Paste this command:

RESTORE DATABASE AutomationExercise
FROM DISK = 'C:\Full\Path\To\Your\Project\Database\automationexercise.bak'
WITH REPLACE;

3. Replace the path with your actual project path
4. Click Execute

### Step 4: Verify Database Restored

1. Expand Databases folder in SSMS
2. You should see AutomationExercise database
3. Expand it - Tables - You should see:
   - Users
   - Products
   - Orders
   - OrderItems

### Step 5: Test Data Exists

Open a new query and run:

USE AutomationExercise;
SELECT COUNT(*) as ProductCount FROM Products;

You should see a number greater than 0.

### Step 6: Update Connection String

Open AutomationExercise.API/appsettings.json in any text editor.

Change the connection string based on YOUR SQL Server:

For LocalDB use:
Server=(localdb)\\MSSQLLocalDB;Database=AutomationExercise;Trusted_Connection=True;

For SQL Express use:
Server=.\\SQLEXPRESS;Database=AutomationExercise;Trusted_Connection=True;

For Full SQL Server use:
Server=localhost;Database=AutomationExercise;Trusted_Connection=True;

### Step 7: Save the File

Save appsettings.json after making changes.

---

## Backend Setup (.NET API)

### Step 1: Open Command Prompt / Terminal

### Step 2: Navigate to API Folder

cd AutomationExercise.API

### Step 3: Restore NuGet Packages

dotnet restore

### Step 4: Build the Project

dotnet build

You should see: Build succeeded.

### Step 5: Run the API

dotnet run

### Step 6: Keep API Running

IMPORTANT: Leave this terminal window open. Do not close it.

---

## Frontend Setup (Angular)

### Step 1: Open a NEW Terminal Window

Keep the backend terminal running. Open a second terminal.

### Step 2: Navigate to Frontend Folder

cd AutomationExercise.Frontend

### Step 3: Install Dependencies

npm install

### Step 4: Start and Open Angular Application

ng serve --open

---

## Running Everything Together

You Need TWO Terminal Windows Open:

Terminal 1 - Backend (leave running):
cd AutomationExercise.API
dotnet run

Terminal 2 - Frontend (leave running):
cd AutomationExercise.Frontend
ng serve

---

### Run All Tests (Backend + Frontend)


Windows PowerShell (navigate to project root first):

cd C:\path\to\your\project
dotnet test; cd AutomationExercise.Frontend; ng test --watch=false
