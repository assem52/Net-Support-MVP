# Phase 5: Live Exam Tracking

## Overview
Phase 5 focuses on providing the Tutor with real-time insight into the students' progress while they are actively taking an exam. Instead of waiting until the end of the test to see the grades, the Tutor can monitor them live!

*Note: The core functionality of Phase 5 was technically built and integrated during **Phase 4 (Part 2)**, but this document explains how the Live Tracking architecture specifically works.*

## How Live Tracking Works

### 1. Data Model (`AnswerUpdatePayload.cs`)
We created a lightweight payload specifically designed for rapid, real-time updates. It contains:
- `Ip`: The student's IP address to identify them in the grid.
- `ScoreString`: A pre-formatted string (e.g., "3/5") representing how many correct answers they currently have.

### 2. The Trigger (`NetSupport.Student`)
In the `ExamWindow.xaml.cs`, we hooked into the `Checked` event of every RadioButton (Options A, B, C, D). 
- **Every single time** a student clicks an option, the app instantly recalculates their total correct answers.
- It then immediately builds an `AnswerUpdatePayload` and sends an `ANSWER_UPDATE` TCP network message back to the Tutor on Port `9002`.

### 3. The Live Dashboard (`NetSupport.Tutor`)
- The Tutor's `TcpUpdateListener` runs a background thread constantly listening for these `ANSWER_UPDATE` messages.
- When it receives one, it invokes the main UI thread via `Application.Current.Dispatcher.Invoke()`.
- It finds the student in the `ObservableCollection` by their IP address and updates their `Score` property.
- Because the `Score` property uses `INotifyPropertyChanged`, the WPF `DataGrid` on the main dashboard instantly refreshes the score cell without the Tutor needing to click a refresh button!

## Why This Architecture?
By having the Student compute their own score and just sending a lightweight string to the Tutor, we significantly reduce the computational load on the Tutor's machine. The Tutor simply updates the UI, allowing it to easily scale to monitor dozens of students simultaneously without freezing.
