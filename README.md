# Mini-NetSupport School (MVP)

A collaborative classroom management tool built for the GitHub Lab.

## 👥 The Team
- **Team Lead:** [Name]
- **Tutor Team:** [Names]
- **Student Team:** [Names]
- **Backend/Networking:** [Names]

## 🛠 Tech Stack
- **Language:** C# (.NET)
- **Communication:** SignalR (Recommended for real-time) or TCP Sockets.
- **Reporting:** iTextSharp or QuestPDF (for PDF generation).

## 🚩 Milestone 1: The "Starter" MVP (48-Hour Goal)
- [ ] **Networking:** Student can "Ping" the Tutor and appear in a list.
- [ ] **Lockdown:** Tutor sends a message; Student UI shows a "Locked" screen.
- [ ] **Data:** A sample CSV exam is successfully parsed by the Designer.

## 📜 Workflow Rules
1. **Commits:** Every member must commit using their own GitHub account (Instructor requirement).
2. **Branches:** Use feature branches (e.g., `feature/lock-screen`) and Merge Requests.
3. **Demo:** Ensure an `.exe` or an installer script is ready for the final submission.

## Project Structure

```

NetSupport-MVP/

├── NetSupport.Tutor/       # WPF/WinForms App

├── NetSupport.Student/     # Windows Service + Hidden UI

├── NetSupport.Designer/    # Desktop App for MCQ creation

├── NetSupport.Shared/      # Class Library (Models, Networking Logic, DTOs)

├── docs/

│   └── Protocols/          # Documentation on how Tutor/Student talk

├── exams/                  # Sample CSV/JSON exam files

├── README.md

└── .gitignore

```