# Pull Request: Phase 7 - Arabic RTL Support

## Overview
This PR implements **Phase 7: Arabic RTL Support**, perfectly fulfilling the final major project specification. All applications can now seamlessly toggle between English and Arabic layouts without needing a restart!

## Logic & Implementation

### 1. `TranslationService` Engine
- Created a lightweight, centralized `TranslationService` in the `NetSupport.Shared.Services` namespace.
- It acts as a fast, in-memory dictionary to translate standard English strings into Arabic without the heavy overhead of WPF `ResourceDictionaries`.
- Holds a global `IsArabic` state flag so that pop-up windows instantly inherit the language choice of their parent application.

### 2. Tutor & Student App Integration
- **Toggle Control**: Added a clean `ComboBox` to the Tutor Dashboard and Student Login Screen to let users flip languages anytime.
- **RTL Magic**: Thanks to WPF's powerful layout engine, when Arabic is selected, the application simply runs `this.FlowDirection = FlowDirection.RightToLeft`. This flawlessly mirrors all grids, data tables, and input boxes automatically!
- **Text Update**: A fast dispatch routine updates the `Content` and `Text` of all visible buttons and labels.

### 3. PDF Generator RTL Compliance
- Updated `PdfReportGenerator.cs` to accept the `IsArabic` flag.
- When generating reports in Arabic, `QuestPDF` is configured to use `TextDirection.RightToLeft` for the document, completely mirroring the table layout (so the Question number column is on the far right!).
- All PDF headers and table headers are dynamically translated via the new `TranslationService`.
- The default Arial font natively supports Arabic characters in QuestPDF.

## How to Test
1. Run both the **Tutor App** and **Student App**.
2. On both apps, click the "Language" dropdown in the top-right corner and select **العربية**.
3. Watch the UI instantly flip to an RTL layout with fully translated Arabic buttons!
4. Push a CSV exam (like `test_exam.csv` which has Arabic questions) to the Student.
5. Take the exam on the Student UI (notice the 'Next' and 'Previous' buttons are translated and swapped).
6. Finish the exam, then click "إنشاء تقرير" (Generate Report) on the Tutor side.
7. Open the generated PDF and admire the fully RTL, Arabic-translated final grade sheet!
