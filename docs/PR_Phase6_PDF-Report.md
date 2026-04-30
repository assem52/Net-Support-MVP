# Pull Request: Phase 6 - PDF Report Generation

## Overview
This PR implements **Phase 6: PDF Report**, which gives the Tutor the ability to permanently save the live tracking scores into a highly professional, stylized PDF file using the `QuestPDF` library. 

## Logic & Implementation

### 1. `QuestPDF` Integration
- I opted for **QuestPDF** over iTextSharp because it's completely free for Community/MVP projects, has an incredibly modern Fluent API, and produces incredibly beautiful tables out of the box with `Fonts.Arial` which works perfectly on Windows.

### 2. Detailed `PdfReportGenerator` Service & Analytics
- Located at `NetSupport.Tutor/Services/PdfReportGenerator.cs`.
- **Enhanced Payloads**: Modified `ExamResultPayload` to carry a full breakdown of every question the student answered via the new `StudentAnswerInfo` model.
- **Detailed Layout**: Generates a dedicated section for *each student*. Instead of a single column, it prints out an entire grid mapping out the Question Text, the Answer Given, the Correct Answer, and a color-coded Status (Green for Correct, Red for Incorrect).
- **Auto-directory creation**: It automatically ensures a `/Reports/` folder exists alongside the executable.
- **Timestamped Naming**: Saves files in the format `ExamReport_20260430_1030.pdf`.
- **Validation Guards**: The Tutor is now **blocked** from clicking "Generate Report" if no students have fully submitted their exams, preventing empty/useless PDFs from being created.

### 3. Tutor Dashboard Updates
- Added a `LightBlue` **Generate Report** button to the main toolbar.
- When clicked, it builds the PDF. If successful, it pops a message box asking: *"Do you want to open the folder containing the report?"*
- If the user clicks "Yes", it fires an OS-level command (`explorer.exe /select`) to pop open Windows Explorer directly highlighting the newly created PDF file!

## How to Test
**CRITICAL SETUP STEP:**
Before testing, you **must** run this command in your terminal to install the PDF library:
```bash
cd "NetSupport.Tutor"
dotnet add package QuestPDF
```

1. Run the Tutor and Student applications.
2. Push an exam to the student and start it.
3. Have the student answer some questions to populate the `Score` column.
4. Click the **Generate Report** button on the Tutor Dashboard.
5. Click **"Yes"** on the success dialog.
6. Your File Explorer should open automatically, highlighting your brand new PDF. Open the PDF and admire your professional grading sheet!
