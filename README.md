# NetSupport MVP

## Overview

NetSupport MVP is a distributed classroom management and assessment software suite engineered using .NET 9 and Windows Presentation Foundation (WPF). The platform facilitates real-time workstation monitoring, synchronous remote execution (kiosk locking), and live-tracked examination deployment across a Local Area Network (LAN).

The system relies on asynchronous UDP broadcasting for zero-configuration client discovery and established TCP streams for reliable payload transmission and telemetry tracking.

## System Architecture

The solution is organized into four discrete modules:

### 1. NetSupport.Shared

The foundational library containing common interfaces, serialization objects, and networking protocols utilized by the ecosystem.

- **Payload Models:** `PushExamPayload`, `AnswerUpdatePayload`, `ExamResultPayload`, `StudentHelloPayload`.
- **Networking:** Extensible `UdpDiscoveryListener` and `UdpDiscoveryBroadcaster` classes.
- **Localization:** A central `TranslationService` managing dynamic UI localization (English/Arabic RTL).

### 2. NetSupport.Tutor

The command-and-control server designed for instructor deployment.

## Screenshots

![student waiting for instructor](lib/assests/waiting_for_instructor.png)

![Tutor Dashboard Screenshot](lib/assests/tutor_dashboard.png)

![Tutor Dashboard Arabic Screenshot](lib/assests/tutor_dashboard_ar.png)

![Testing Console Screenshot](lib/assests/exam_console.png)

![PDF Report Screenshot](lib/assests/report_pdf.png)

- **Auto-Discovery:** Actively listens on UDP port 8000 for client heartbeats.
- **Command Transmission:** Initiates TCP connections to remote hosts to dispatch `LOCK`, `UNLOCK`, and `PUSH_EXAM` directives.
- **Live Telemetry:** Processes asynchronous `ANSWER_UPDATE` payloads from connected clients, dynamically updating the DataGrid interface via the UI Dispatcher.
- **Reporting:** Utilizes QuestPDF to generate standard-compliant UTF-8 PDF documentation of student assessment scores.

### 3. NetSupport.Student

The client daemon installed on target workstations.

![Student Exam Interface Screenshot](lib/assests/student_exam.png)

![Student Lock Screen Screenshot](lib/assests/screen_locked.png)

- **Background Execution:** Initiates an isolated background thread broadcasting system metadata (IP, Hostname) over UDP.
- **TCP Listener:** Binds to TCP port 9000 to await commands from the Tutor node.
- **Kiosk Mode:** Upon receiving a `LOCK` or `START_EXAM` command, instantiates a TopMost, Maximized WPF overlay intercepting OS-level interrupt signals (e.g., `Alt+F4`) to enforce a restricted environment.

### 4. NetSupport.Designer

A standalone desktop application dedicated to assessment authoring.

![Designer Application Screenshot](lib/assests/designer_app.png)

- **Authoring Interface:** Split-pane design allowing localized creation of multiple-choice questions (MCQ).
- **Data Serialization:** Exports validated examination objects into standardized `.csv` files utilizing `UTF-8 with BOM` encoding to guarantee data integrity for non-Latin character sets.

## Deployment Instructions

The application is distributed via automated compilation scripts ensuring environment independence for the end user.

### Prerequisites

- .NET 9.0 SDK (For compilation only)
- Windows 10/11 x64 architecture

### Compilation Workflow

1. Navigate to the repository root directory.
2. Execute the included batch script:
   ```cmd
   Build_And_Deploy.bat
   ```
3. The script utilizes `dotnet publish` leveraging the `--self-contained true` flag. This bundles the Common Language Runtime (CLR) directly into the executable, removing any external dependencies.
4. The output binaries are routed to the `NetSupport_Release` directory.

### Execution Policy

- **Authoring:** Execute `NetSupport.Designer.exe` to generate an assessment `.csv` dataset.
- **Client Deployment:** Execute `NetSupport.Student.exe` on target workstations. The process runs without an initial GUI.
- **Server Deployment:** Execute `NetSupport.Tutor.exe` on the instructor workstation. Discovered nodes will populate the dashboard, allowing command transmission and remote control operations.
