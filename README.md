ACID Banking Simulation (Transaction Audit System)
A professional C# Windows Forms application that demonstrates the practical implementation of ACID Properties in a database environment using SQLite.

🚀 Key Features
Atomic Transactions: Ensures that money is either fully transferred or not at all, preventing data loss during crashes.

System Crash Simulation: A dedicated feature to test and prove the Rollback mechanism.

Real-time Audit Log: A dynamic DataGridView that records every transaction's status (SUCCESS/FAILED) with timestamps.

Professional Dashboard: Organized UI with Account Overview and Transaction History panels.

🛠 Tech Stack
Language: C# (.NET Framework)

Database: SQLite (using Microsoft.Data.Sqlite)

Library: SQLitePCLRaw for native SQLite initialization.

📸 Screenshots
Tip: Yahan apne project ke screenshots ka link dein (maslan Audit Log wala screenshot).

💡 What I Learned
Managing database connections and transactions in a Desktop environment.

Handling unexpected system failures using try-catch and transaction.Rollback().

Designing a user-friendly dashboard for financial monitoring.

⚙️ How to Run
Clone the repository.

Open ACID.slnx in Visual Studio 2022.

Restore NuGet packages (specifically Microsoft.Data.Sqlite).

Build and Run the project.
