# Pull Request: Phase 6 - PDF Report Generation

## Overview
This PR implements **Phase 6: PDF Report**, which gives the Tutor the ability to permanently save the live tracking scores into a highly professional, stylized PDF file using the `QuestPDF` library. 

## Logic & Implementation

### 1. `QuestPDF` Integration
- I opted for **QuestPDF** over iTextSharp because it's completely free for Community/MVP projects, has an incredibly modern Fluent API, and produces incredibly beautiful tables out of the box with `Fonts.Arial` which works perfectly on Windows.

### 2. `PdfReportGenerator` Service
- Located at `NetSupport.Tutor/Services/PdfReportGenerator.cs`.
- Takes the live `ObservableCollection` of students directly from the dashboard and parses it into a clean table structure.
- **Auto-directory creation**: It automatically ensures a `/Reports/` folder exists alongside the executable.
- **Timestamped Naming**: Saves files in the format `ExamReport_20260430_1030.pdf`.

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
