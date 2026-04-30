# Pull Request: Phase 4 (Part 2) - Exam Taking Interface

## Overview
This PR completes **Phase 4: Exam Flow**. Instructors can now click "Start Exam" to launch the exam on the student's PC. The student sees a countdown timer and can navigate through questions. As they select answers, their live score is updated on the Tutor's dashboard.

## Logic & Implementation

### 1. New Payloads (`NetSupport.Shared`)
- **`AnswerUpdatePayload`**: Includes the IP and `ScoreString` (e.g., "2/5"). Sent on every radio button click.
- **`ExamResultPayload`**: Includes the IP and `FinalScore`. Sent when the timer hits zero, or the student clicks "Submit Exam".
- **`StudentHelloPayload`**: Added a `Score` property to the DataGrid.

### 2. Tutor Controls (`NetSupport.Tutor`)
- **Dashboard Updates**: Added "Start Exam" (LightGreen) and "Stop Exam" (LightCoral) buttons.
- **Live Scoring**: The `TcpUpdateListener` was expanded to listen for `ANSWER_UPDATE` and `EXAM_RESULT` messages. When received, it uses the UI Dispatcher to instantly update the student's score in the `DataGrid`.

### 3. Student Interface (`NetSupport.Student`)
- **`ExamWindow`**: A full-screen, top-most window identical to the Lock screen, but containing the exam UI. 
  - Uses `DispatcherTimer` to decrement the time remaining.
  - Dynamically loads question text and choices from the `PushExamPayload`.
  - When an option is clicked, it recalculates the current correct answers and fires an `ANSWER_UPDATE` over TCP.
- **`App.xaml.cs`**: Deserializes the `PushExamPayload` to memory when `PUSH_EXAM` is received. When `START_EXAM` is received, it closes the `ExamLoginWindow` and launches the `ExamWindow`.

## How to Test This PR
1. Run both Tutor and Student apps.
2. In the Tutor Dashboard, select the student and push `test_exam.csv`.
3. In the Student app, enter your name and hit "I am Ready".
4. Back in the Tutor Dashboard, select the student and click **Start Exam**.
5. The student's screen will change to the Exam interface!
6. Click an answer on the Student PC.
7. Look at the Tutor Dashboard: The **Score** column will instantly update to show your live progress!
8. Navigate through the questions and click **Submit Exam**. The window will close, and the Tutor Dashboard will show "FINAL: 3/3".
