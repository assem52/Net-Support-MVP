# NetSupport School Clone — MVP Specification & Technical Plan

> **Project Duration:** ~10 days | **Team Size:** 9 members | **Language Support:** Arabic (RTL)

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [System Architecture](#2-system-architecture)
3. [Applications Breakdown](#3-applications-breakdown)
4. [Functional Requirements (FR)](#4-functional-requirements-fr)
5. [Non-Functional Requirements (NFR)](#5-non-functional-requirements-nfr)
6. [Technical Stack](#6-technical-stack)
7. [Data Flow & Communication Protocol](#7-data-flow--communication-protocol)
8. [Module-by-Module Implementation Plan](#8-module-by-module-implementation-plan)
9. [Database / File Schema](#9-database--file-schema)
10. [UI/UX Screens Inventory](#10-uiux-screens-inventory)
11. [PDF Report Specification](#11-pdf-report-specification)
12. [Team Task Distribution](#12-team-task-distribution)
13. [Submission Checklist](#13-submission-checklist)

---

## 1. Project Overview

Build a **minimal viable clone** of NetSupport School — a classroom management system — consisting of **three separate desktop applications** that communicate with each other over a local network.

| Application | Who Uses It | Platform |
|---|---|---|
| **Tutor App** | Instructor / Teacher | Windows Desktop |
| **Student App** | Student (each PC) | Windows Desktop (runs as service) |
| **Designer App** | Exam creator / Admin | Windows Desktop |

### Core Value Proposition
- Tutor can remotely lock/unlock student PCs
- Tutor can assign and monitor exams in real-time
- Students can take exams with a login flow
- Exams are designed via a dedicated MCQ Designer
- All UI must support Arabic (RTL layout)

---

## 2. System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        LOCAL NETWORK (LAN)                   │
│                                                             │
│  ┌──────────────┐        TCP/UDP        ┌────────────────┐  │
│  │  Tutor App   │◄─────────────────────►│  Student App   │  │
│  │  (Desktop)   │                       │  (Win Service) │  │
│  └──────┬───────┘                       └────────────────┘  │
│         │                                                   │
│         │  reads .csv exam file                             │
│         ▼                                                   │
│  ┌──────────────┐                                           │
│  │ Designer App │  ──exports──► exam.csv                    │
│  │  (Desktop)   │                                           │
│  └──────────────┘                                           │
└─────────────────────────────────────────────────────────────┘
```

### Communication Model
- **Protocol:** TCP Sockets (reliable, ordered delivery)
- **Discovery:** UDP Broadcast for auto-discovery of students on the LAN
- **Message Format:** JSON payloads over TCP
- **Room Concept:** Students register themselves under a "Room Name" on startup; Tutor filters by room name

---

## 3. Applications Breakdown

### 3.1 Student App
- Installed on every student PC
- **Runs as a Windows Service** — starts automatically on boot
- Listens for commands from the Tutor App
- Hosts a small local TCP server
- Renders an exam UI when exam is pushed by Tutor

### 3.2 Tutor App
- Runs on the instructor's machine
- Discovers students via UDP broadcast + room filter
- Sends lock/unlock commands
- Manages exam flow: assign → start → monitor → stop → report
- Generates PDF report at the end

### 3.3 Designer App
- Standalone tool (no network needed)
- Creates MCQ exams
- Exports exam to `.csv` file
- CSV is imported by Tutor App when assigning an exam

---

## 4. Functional Requirements (FR)

### FR-01 — Student Auto-Detection
| ID | Requirement |
|---|---|
| FR-01.1 | Student app broadcasts its presence (PC name, IP, room name) via UDP on startup |
| FR-01.2 | Tutor app listens on a known UDP port and adds students to the list automatically |
| FR-01.3 | Students are grouped and filtered by **Room Name** |
| FR-01.4 | Tutor UI refreshes the student list in real-time (add/remove on connect/disconnect) |
| FR-01.5 | Each student entry shows: Name, IP, Status (Online / Locked / In Exam) |

---

### FR-02 — Lock / Unlock Student Computers
| ID | Requirement |
|---|---|
| FR-02.1 | Tutor can select one, multiple, or all students |
| FR-02.2 | Tutor sends a **LOCK** command → student PC becomes input-locked (keyboard + mouse disabled, screen overlay shown) |
| FR-02.3 | Tutor sends an **UNLOCK** command → student PC restores normal operation |
| FR-02.4 | Lock state is shown visually in the Tutor student list (e.g., red icon) |
| FR-02.5 | Lock/Unlock must respond within **< 2 seconds** on a normal LAN |
| FR-02.6 | If a student disconnects while locked, state is preserved and shown as "Offline/Locked" |

---

### FR-03 — Exam Assignment & Configuration (Tutor Side)
| ID | Requirement |
|---|---|
| FR-03.1 | Tutor opens a "Testing Console" panel |
| FR-03.2 | Tutor browses and selects a `.csv` exam file created by Designer App |
| FR-03.3 | Tutor selects students (checkboxes) who will take the exam |
| FR-03.4 | Tutor sets a **time limit** (minutes) for the exam |
| FR-03.5 | Tutor clicks **"Start Exam"** to push the exam to selected students |
| FR-03.6 | Tutor can click **"Stop Exam"** at any time to end the exam for all or selected students |

---

### FR-04 — Test Login (Student Side)
| ID | Requirement |
|---|---|
| FR-04.1 | When exam is pushed, Student App displays a login screen |
| FR-04.2 | Student enters their **Full Name** to begin |
| FR-04.3 | After submitting name, student status changes to **"Ready"** on Tutor's dashboard |
| FR-04.4 | Exam questions are NOT shown until Tutor clicks "Start Exam" (or immediately on push — define with team) |

> **Design Decision:** Recommended flow → Push exam (student logs in) → Tutor confirms all ready → Tutor clicks Start → timer begins.

---

### FR-05 — Student Exam Navigation
| ID | Requirement |
|---|---|
| FR-05.1 | Student sees one question at a time with 4 MCQ options (A, B, C, D) |
| FR-05.2 | Student can navigate between questions using **Next / Previous** buttons |
| FR-05.3 | A question navigation bar/panel shows answered vs. unanswered questions |
| FR-05.4 | A countdown timer is shown prominently |
| FR-05.5 | Student **cannot end the exam** themselves — it ends on timeout or Tutor stop |
| FR-05.6 | When exam ends (timeout or stop), the student's answers are locked and sent to Tutor |

---

### FR-06 — Live Tracking (Tutor Side)
| ID | Requirement |
|---|---|
| FR-06.1 | Tutor sees a real-time dashboard showing each student's progress |
| FR-06.2 | Dashboard shows per-student: Name, Questions Answered / Total, Correct Count |
| FR-06.3 | Tutor can see answer correctness per question per student (green = correct, red = wrong) |
| FR-06.4 | Student app sends answer updates to Tutor each time a student submits an answer |
| FR-06.5 | Dashboard auto-refreshes without manual action |

---

### FR-07 — Exam Completion & PDF Report
| ID | Requirement |
|---|---|
| FR-07.1 | Exam ends when: timer expires OR Tutor manually stops it |
| FR-07.2 | Tutor clicks "Generate Report" (or it's auto-generated on stop) |
| FR-07.3 | PDF report includes: Student Name, Score (X/Total), Number of Correct Answers, Number of Questions |
| FR-07.4 | Report is saved to a Tutor-specified path or a default `reports/` folder |
| FR-07.5 | Report supports Arabic student names (UTF-8 rendering in PDF) |

---

### FR-08 — Designer App (MCQ Exam Creation)
| ID | Requirement |
|---|---|
| FR-08.1 | Designer shows a form to add a new question |
| FR-08.2 | Each question has: Question Text, 4 options (A/B/C/D), and a marked Correct Answer |
| FR-08.3 | Designer shows a list of all added questions |
| FR-08.4 | Designer can edit or delete any question |
| FR-08.5 | Designer exports the exam as a `.csv` file |
| FR-08.6 | CSV format: `QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption` |
| FR-08.7 | Designer supports Arabic text input for questions and options |

---

## 5. Non-Functional Requirements (NFR)

| ID | Requirement | Detail |
|---|---|---|
| NFR-01 | **Arabic Language Support** | All UI must support Arabic text and RTL layout switching |
| NFR-02 | **Low Latency** | Lock/Unlock commands must execute within 2 seconds on LAN |
| NFR-03 | **Windows Service** | Student app must be installable as a Windows Service (auto-start on boot) |
| NFR-04 | **Local Network Only** | No internet required; all communication is LAN-based |
| NFR-05 | **Graceful Disconnect** | If student disconnects, Tutor shows offline status; no crash |
| NFR-06 | **PDF Output** | Report must be a properly formatted PDF file |
| NFR-07 | **CSV Compatibility** | Exam files must be UTF-8 encoded `.csv` to support Arabic |
| NFR-08 | **Installer** | Each app should have a simple installer or a "How to Run" guide |

---

## 6. Technical Stack

> **Recommended Stack** (team may adapt):

### Option A — Python (Recommended for rapid dev)
```
Language:     Python 3.10+
UI:           PyQt6 or tkinter (PyQt6 preferred for RTL support)
Networking:   socket (stdlib) — TCP + UDP
PDF Gen:      reportlab or fpdf2
CSV:          csv (stdlib)
Service:      pywin32 (win32serviceutil) for Windows Service
Packaging:    PyInstaller for .exe builds
```

### Option B — C# / .NET (If team prefers)
```
Language:     C# .NET 6+
UI:           WinForms or WPF
Networking:   System.Net.Sockets
PDF Gen:      iTextSharp or PdfSharp
CSV:          CsvHelper
Service:      Windows Service project template (built-in)
```

### Shared Regardless of Stack
```
Version Control:  Git + GitHub
Communication:    JSON over TCP sockets
Exam Format:      UTF-8 CSV
Report Format:    PDF
```

---

## 7. Data Flow & Communication Protocol

### 7.1 Network Ports
| Port | Protocol | Purpose |
|---|---|---|
| `9000` | UDP | Student broadcast (heartbeat/discovery) |
| `9001` | TCP | Tutor → Student commands |
| `9002` | TCP | Student → Tutor updates (answers, status) |

> Ports are configurable via a config file.

### 7.2 Message Format (JSON)

All messages follow this envelope:

```json
{
  "type": "MESSAGE_TYPE",
  "payload": { ... },
  "timestamp": "2025-01-21T10:00:00Z"
}
```

#### Message Types

| Message Type | Direction | Payload |
|---|---|---|
| `STUDENT_HELLO` | Student → Tutor (UDP) | `{ "name": "PC-01", "ip": "192.168.1.5", "room": "Eval" }` |
| `LOCK` | Tutor → Student | `{}` |
| `UNLOCK` | Tutor → Student | `{}` |
| `PUSH_EXAM` | Tutor → Student | `{ "exam": [...questions...], "duration_minutes": 30 }` |
| `STUDENT_READY` | Student → Tutor | `{ "student_name": "Mohamed" }` |
| `START_EXAM` | Tutor → Student | `{}` |
| `STOP_EXAM` | Tutor → Student | `{}` |
| `ANSWER_UPDATE` | Student → Tutor | `{ "question_index": 2, "selected_option": "B", "is_correct": true }` |
| `EXAM_RESULT` | Student → Tutor | `{ "student_name": "Mohamed", "answers": [...] }` |
| `ACK` | Any → Any | `{ "ref_type": "LOCK" }` |

### 7.3 Exam Question Object (in PUSH_EXAM)

```json
{
  "index": 0,
  "question": "ما هي عاصمة مصر؟",
  "options": {
    "A": "الإسكندرية",
    "B": "القاهرة",
    "C": "الجيزة",
    "D": "أسوان"
  },
  "correct": "B"
}
```

---

## 8. Module-by-Module Implementation Plan

### Phase 1 — Foundation (Days 1–2)
**Goal:** Network layer + Student service skeleton

- [ ] Set up Git repository with branch structure (`main`, `dev`, `feature/*`)
- [ ] Implement UDP broadcast from Student App (heartbeat every 5s)
- [ ] Implement UDP listener in Tutor App + student list population
- [ ] Implement TCP server in Student App
- [ ] Implement TCP client in Tutor App
- [ ] Define and implement all JSON message types
- [ ] Install Student App as Windows Service (test on one machine)

**Deliverable:** Tutor detects Student on the same network; student appears in list.

---

### Phase 2 — Lock/Unlock (Day 3)
**Goal:** Core control feature

- [ ] Tutor UI: Student list with checkboxes + Lock/Unlock buttons
- [ ] Send `LOCK` command over TCP → Student overlays lock screen (fullscreen, input blocked)
- [ ] Send `UNLOCK` command → Student removes overlay
- [ ] Update student status icon in Tutor list (🔴 Locked / 🟢 Online)
- [ ] Test round-trip latency (target < 2s)

**Deliverable:** Tutor can lock/unlock student PCs remotely.

---

### Phase 3 — Designer App (Days 3–4)
**Goal:** MCQ exam creation + CSV export

- [ ] Build Designer UI: question form + question list panel
- [ ] Implement Add / Edit / Delete question
- [ ] Implement Export to CSV (UTF-8, correct delimiter)
- [ ] Validate: no empty questions, no missing correct answer
- [ ] Test with Arabic text input + verify CSV encoding

**Deliverable:** Designer exports a valid `.csv` exam file.

---

### Phase 4 — Exam Flow (Days 4–6)
**Goal:** Full exam pipeline — assign → login → start → take → submit

#### Tutor Side
- [ ] Testing Console panel: file picker for CSV, student selector, time limit input
- [ ] Parse CSV exam file and display question count
- [ ] Send `PUSH_EXAM` to selected students
- [ ] Display student "Ready" status after login
- [ ] Start Exam button → send `START_EXAM`
- [ ] Stop Exam button → send `STOP_EXAM` + collect results

#### Student Side
- [ ] Receive `PUSH_EXAM` → show login screen (name input)
- [ ] On name submit → send `STUDENT_READY` to Tutor
- [ ] Receive `START_EXAM` → show question 1, start timer
- [ ] Implement question navigation (Next/Prev + nav panel)
- [ ] On answer select → send `ANSWER_UPDATE` to Tutor
- [ ] On timeout or `STOP_EXAM` → send `EXAM_RESULT` → show "Exam Ended" screen

**Deliverable:** End-to-end exam flow working between Tutor and at least one Student.

---

### Phase 5 — Live Tracking (Day 6–7)
**Goal:** Tutor monitors exam in real-time

- [ ] Tutor live dashboard: table with one row per student
- [ ] Columns: Student Name | Answered | Correct | Progress Bar
- [ ] Color-code per-question answer status (green/red grid)
- [ ] Auto-refresh on incoming `ANSWER_UPDATE` messages

**Deliverable:** Tutor sees live exam progress per student.

---

### Phase 6 — PDF Report (Day 7–8)
**Goal:** Generate exam report

- [ ] Collect all `EXAM_RESULT` messages
- [ ] Aggregate: student name, correct answers, total questions, score %
- [ ] Generate PDF with table layout
- [ ] Support Arabic names in PDF (use Arabic-compatible font, e.g., Amiri or Cairo)
- [ ] Save to `reports/` directory with timestamp filename

**Deliverable:** PDF report generated after exam ends.

---

### Phase 7 — Arabic RTL Support (Days 8–9)
**Goal:** Full Arabic UI across all 3 apps

- [ ] Add language toggle (Arabic / English) in all apps
- [ ] Apply RTL layout when Arabic is selected (flip UI elements)
- [ ] Translate all labels, buttons, messages to Arabic
- [ ] Verify Arabic text in PDF report renders correctly
- [ ] Verify CSV with Arabic questions is parsed correctly

**Deliverable:** All 3 apps fully operable in Arabic.

---

### Phase 8 — Polish & Testing (Day 9–10)
- [ ] Integration testing: all 3 apps on separate machines/VMs
- [ ] Edge cases: student disconnects mid-exam, exam timeout, no students selected
- [ ] Create installer or clear "How to Run" instructions
- [ ] Record demo video
- [ ] Final GitHub cleanup: meaningful commit messages, README

---

## 9. Database / File Schema

### 9.1 Exam CSV Format

**File:** `exam_name.csv`
**Encoding:** UTF-8 with BOM (for Arabic compatibility)
**Delimiter:** Comma (`,`)

```
QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption
"What is 2+2?","1","2","4","5","C"
"ما عاصمة مصر؟","الإسكندرية","القاهرة","الجيزة","أسوان","B"
```

### 9.2 In-Memory Exam Result Structure (per student)

```json
{
  "student_name": "Mohamed Ali",
  "pc_name": "PC-Lab-03",
  "ip": "192.168.1.10",
  "total_questions": 10,
  "answers": [
    { "question_index": 0, "selected": "C", "correct": "C", "is_correct": true },
    { "question_index": 1, "selected": "A", "correct": "B", "is_correct": false }
  ],
  "score": 7,
  "finished_at": "2025-01-21T11:30:00Z"
}
```

### 9.3 Config File (per app)

**File:** `config.json`

```json
{
  "room_name": "Eval",
  "tutor_ip": "auto",
  "udp_broadcast_port": 9000,
  "tcp_command_port": 9001,
  "tcp_update_port": 9002,
  "language": "ar",
  "reports_directory": "./reports"
}
```

---

## 10. UI/UX Screens Inventory

### Tutor App Screens

| Screen | Key Elements |
|---|---|
| **Main Dashboard** | Student list table, Room selector, Lock All / Unlock All buttons, Open Testing Console button |
| **Student List Row** | PC Name, Student Name (if in exam), IP, Status badge, Individual Lock/Unlock |
| **Testing Console** | Exam file picker, Student selector (checkboxes), Time limit input, Start/Stop Exam buttons |
| **Live Tracking Panel** | Per-student table with live answer correctness, progress bar |
| **Report Preview** | Summary table before PDF save, Save button |

### Student App Screens

| Screen | Key Elements |
|---|---|
| **Idle Screen** | Shows "Connected" status, Room name, PC name (minimal UI — runs as service) |
| **Lock Screen** | Fullscreen overlay, message "Screen Locked by Instructor" (in Arabic + English) |
| **Exam Login** | Name input field, Submit button |
| **Exam Screen** | Question text, 4 option buttons, Navigation panel (question dots), Timer, Next/Prev |
| **Exam Ended Screen** | "Exam Submitted" message, Student's own score (optional) |

### Designer App Screens

| Screen | Key Elements |
|---|---|
| **Main Screen** | Split: Question Form (left) + Question List (right) |
| **Question Form** | Question text area, 4 option inputs, Correct answer radio, Add/Update button |
| **Question List** | Scrollable list of questions, Edit / Delete per row |
| **Toolbar** | New Exam, Open Exam (.csv), Save Exam, Export button |

---

## 11. PDF Report Specification

### Report Layout

```
┌─────────────────────────────────────────────┐
│         [Logo / Title]                      │
│    Exam Report — [Exam Name]                │
│    Date: [Date]   Room: [Room]              │
├──────┬────────────┬─────────┬───────────────┤
│  #   │  Student   │  Score  │  Correct / Q  │
├──────┼────────────┼─────────┼───────────────┤
│  1   │  Mohamed   │  70%    │   7 / 10      │
│  2   │  Ali Sami  │  80%    │   8 / 10      │
│  ... │  ...       │  ...    │   ...         │
└──────┴────────────┴─────────┴───────────────┘
│  Class Average: 75%                         │
└─────────────────────────────────────────────┘
```

### PDF Technical Requirements
- Font: **Amiri** or **Cairo** (Arabic-compatible, open-source)
- Direction: RTL when Arabic names/content present
- Library: `reportlab` (Python) or `iTextSharp` (C#)
- Filename: `report_YYYYMMDD_HHMMSS.pdf`
- Paper: A4

---

## 12. Team Task Distribution

> Suggested split for 9 members over 10 days:

| Member Role | Responsibility |
|---|---|
| **Team Lead / PM** | Architecture decisions, integration, GitHub management, demo video |
| **Network Engineer (×2)** | TCP/UDP communication layer, message protocol, service installer |
| **Tutor App Dev (×2)** | Tutor UI, student list, lock/unlock, testing console, live tracking |
| **Student App Dev (×1)** | Student service, exam UI, lock screen, answer sending |
| **Designer App Dev (×1)** | MCQ designer UI, CSV export |
| **PDF & Report Dev (×1)** | PDF generation, report formatting |
| **Arabic / i18n Dev (×1)** | Arabic translations, RTL layout, Arabic font in PDF |

---

## 13. Submission Checklist

- [ ] GitHub repository with meaningful commits per contributor
- [ ] `README.md` with project description and setup instructions
- [ ] "How to Install" video or written guide for all 3 apps
- [ ] Demo video showing: student detection, lock/unlock, exam flow, live tracking, PDF report
- [ ] Team leader name + all 9 member names in README
- [ ] Each member has commits linked to their GitHub account

---

## Appendix A — Quick Start for LLM Agents

If you are an AI assistant helping implement this project, here is the summary of what needs to be built:

1. **Three separate desktop apps:** Tutor, Student, Designer
2. **Student app is a Windows Service** (auto-starts, runs TCP server, broadcasts UDP)
3. **Communication:** JSON over TCP sockets; student discovery via UDP broadcast
4. **Core commands:** LOCK, UNLOCK, PUSH_EXAM, START_EXAM, STOP_EXAM, ANSWER_UPDATE, EXAM_RESULT
5. **Exam format:** CSV file with columns `QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption`
6. **PDF report** generated at exam end with student names + scores
7. **Arabic RTL support** required in all UIs
8. **No internet needed** — everything runs on LAN

Implement in order: Network layer → Lock/Unlock → Designer CSV → Exam flow → Live tracking → PDF → Arabic

---

*Specification version 1.0 — Generated for university lab project based on NetSupport School clone requirements.*
