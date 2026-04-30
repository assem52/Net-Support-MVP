# Pull Request: Phase 3 - Designer App (MCQ Creation)

## Overview
This PR introduces the standalone **Designer App** required in Phase 3. It allows instructors to rapidly build multiple-choice exams and export them into a correctly formatted, UTF-8 encoded `.csv` file. 

## Logic & Implementation

### 1. Data Model (`NetSupport.Shared`)
- **`ExamQuestion` Model**: Added a new model representing a single MCQ. It holds the question text, the four options (A, B, C, D), and the correct option. We put this in `NetSupport.Shared` because the Tutor app will need to read these same objects when importing the CSV later.

### 2. CSV Export Engine (`NetSupport.Designer/Services`)
- **`CsvExporter`**: A custom service that streams the list of `ExamQuestion` objects into a file. 
  - **Encoding**: It explicitly uses `UTF8Encoding(true)` (which includes a Byte Order Mark / BOM). This is critical for the MVP because it guarantees that Excel and Windows native text editors will properly display **Arabic text** (RTL support).
  - **Escaping**: It includes a safety mechanism (`EscapeForCsv`) so that if a teacher accidentally types a comma `,` or quotation mark `"` inside their question text, it won't break the CSV columns.

### 3. Designer UI (`NetSupport.Designer/UI`)
- **Split-Screen Layout**: Implemented a responsive two-column grid. 
  - **Left Column**: The input form. Contains TextBoxes for the question and options, and a ComboBox to select the correct answer.
  - **Right Column**: A real-time `DataGrid` bound to an `ObservableCollection<ExamQuestion>`. As soon as a question is added on the left, it instantly appears on the right.
- **Validation**: The "Add Question" button verifies that no text fields are left blank before allowing the question into the list.
- **Exporting**: Uses the standard Windows `SaveFileDialog` to let the instructor choose exactly where to save their `exam.csv` file.

## How to Test This PR
1. Build the solution and run the `NetSupport.Designer` project.
2. The window will open with a form on the left.
3. Try adding a test question (e.g., *Question: "What is 2+2?", Options: 1, 2, 4, 5, Correct: C*).
4. Try adding an **Arabic** question (e.g., *Question: "ما عاصمة مصر؟", Options: القاهرة, الجيزة, الإسكندرية, أسوان, Correct: A*).
5. Notice them populating the table on the right.
6. Click **Export to CSV**, save the file to your desktop.
7. Open the `.csv` file in Notepad or Excel. Verify the formatting and that the Arabic text is legible.
