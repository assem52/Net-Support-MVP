# Pull Request: Phase 3 - Designer App

## Overview
This PR fully implements **Phase 3: Designer App (MCQ Exam Creation)**. The `NetSupport.Designer` project is now a fully functional standalone desktop tool that allows teachers to create Multiple Choice exams and export them as properly formatted CSV files. 

## Key Features

### 1. Split-Screen User Interface
- **Left Panel (Form)**: A clean, intuitive form to input the Question text and all 4 options (A, B, C, D). Radio buttons allow the user to quickly select which option is the correct answer.
- **Right Panel (List)**: A live `DataGrid` connected to an `ObservableCollection<ExamQuestion>` that displays all questions currently added to the exam.
- **Delete Support**: Teachers can select any question from the list and click "Delete Selected" to remove it and re-index the list.

### 2. CSV Export Engine
- Safely exports the exam to a `.csv` file.
- **Data Safety**: Questions or options containing commas (`,`) are automatically wrapped in quotes (`"..."`), and internal quotes are properly escaped (`""`) to prevent the CSV structure from breaking.
- **UTF-8 Support**: The CSV writer is explicitly configured to use `new UTF8Encoding(true)` (UTF-8 with BOM), ensuring that Excel and the Tutor app can read Arabic text seamlessly!

### 3. Native Arabic (RTL) Support
- Inherits the `TranslationService` from `NetSupport.Shared`.
- The top toolbar features a Language Toggle dropdown. When switched to Arabic, the entire application immediately mirrors itself (Form moves to the right, List to the left), and all labels and buttons are translated instantly!

## How to Test
1. Run the `NetSupport.Designer` project.
2. Type in a sample question, fill out options A through D, select a radio button for the correct answer, and click **Add to Exam**.
3. Toggle the Language to **العربية** at the top right to verify the RTL layout and translations work smoothly.
4. Click **Export to CSV**, save the file, and then open the CSV to verify the formatting!
