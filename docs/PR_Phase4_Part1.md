# Pull Request: Phase 4 (Part 1) - Exam Push & Student Login

## Overview
This PR implements the first half of **Phase 4: Exam Flow**. The Tutor can now load the `.csv` exam created by the Designer, select a student, and push the exam over the network. The Student app instantly responds by displaying a locked Login screen, asks for the student's name, and sends a "Ready" signal back to the Tutor.

## Architecture & Logic

### 1. Two-Way TCP Communication
Previously, the Tutor acted only as a Client (sending commands), and the Student acted only as a Server (listening for commands). 
Now, we have implemented **Two-Way TCP**:
- The Student continues to listen on `Port 9001` for incoming commands (`PUSH_EXAM`).
- The Tutor now starts a **`TcpUpdateListener`** on `Port 9002` to receive updates.
- The Student uses a new **`TcpUpdateSender`** to connect to the Tutor's IP and send updates (`STUDENT_READY`).

### 2. Tutor Updates
- **`CsvParser`**: A new service reads the CSV file, respecting quotes to safely handle commas inside question text, and converts it into `List<ExamQuestion>`.
- **`TestingConsoleWindow`**: A new dialog window where the tutor selects the `.csv` file, sets a time limit, and fires the `PUSH_EXAM` command.
- **Status Tracking**: The Tutor's `StudentHelloPayload` now includes an `IsReady` flag. When the student logs in, this flag is set to true, instantly updating the dashboard.

### 3. Student Updates
- **`ExamLoginWindow`**: When the `PUSH_EXAM` command is received, the background listener extracts the Tutor's IP address and passes it to the UI thread. The UI opens `ExamLoginWindow`, an un-closable full-screen overlay (blocking Alt-F4). 
- Once the student enters their name and clicks "I am Ready", the app fires the `STUDENT_READY` payload back to the Tutor's IP on port `9002` and shows a "Waiting for Instructor..." message.

## How to Test This PR
1. **Preparation**: Use the Designer app to create a short exam and export it to a `.csv` file.
2. Run both the `NetSupport.Tutor` and `NetSupport.Student` apps.
3. In the Tutor dashboard, wait for the student to appear, then click to select them.
4. Click **Open Testing Console**.
5. Browse for your `.csv` file, leave the duration at 30, and click **Push Exam to Selected Student**.
6. Switch to the Student PC/App. The **Exam Login Screen** should have taken over the screen!
7. Enter a name (e.g., "John Doe") and click **I am Ready**.
8. The screen will change to say "Waiting for Instructor...", and if you look at the Tutor Dashboard, the `IsReady` column should now be checked, and their name should be updated to "John Doe"!
