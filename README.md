# NetSupport School Clone (MVP) - 2026 Edition

Welcome to the NetSupport School MVP! This classroom management system consists of three distinct applications that communicate over a Local Area Network (LAN) using TCP and UDP sockets.

## 🏛️ The Three Applications
1. **NetSupport Designer**: A standalone tool for teachers to create Multiple Choice Questions (MCQ) and export them safely to a `.csv` file.
2. **NetSupport Student**: The client application. It runs silently in the background, broadcasts its presence to the network, and waits for exams or lock commands.
3. **NetSupport Tutor**: The teacher's dashboard. It discovers students automatically, can lock their screens, assign exams, track live scores, and generate PDF reports.

---

## 🛠️ Deployment & Local Testing

### Local Safe Mode (Anti-Lockout)
If you are testing the **Tutor App** and the **Student App** on the exact same laptop, clicking "Lock" from the Tutor app would normally lock your entire screen, preventing you from ever clicking "Unlock"!

**Solution:** 
We have implemented a safety flag. Open `NetSupport.Student/UI/LockScreenWindow.xaml.cs` and look at line 13:
```csharp
public static bool IsLocalDebugMode = true;
```
As long as this is `true`, the Lock Screen will only open as a small 800x600 floating window. **Important:** Before you deploy this to actual university computers, change this to `false` so the lock screen is truly inescapable!

### Adding Custom App Icons (.ico)
To give your `.exe` files professional icons before submission:
1. Go to a free site like [CloudConvert](https://cloudconvert.com/png-to-ico) to convert your logo PNGs into `.ico` files.
2. Place the `.ico` file into your project folder (e.g., `NetSupport.Tutor/tutor_icon.ico`).
3. Open the `.csproj` file for that app and add this line inside the `<PropertyGroup>`:
   ```xml
   <ApplicationIcon>tutor_icon.ico</ApplicationIcon>
   ```

### Publishing (Creating Standalone `.exe` Files)
You don't want to run the app from Visual Studio during your demo. You want real, standalone `.exe` files.
Open a terminal in the root of the repository and run these three commands to compile each app into a standalone file for Windows 64-bit:

```powershell
dotnet publish NetSupport.Designer/NetSupport.Designer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish NetSupport.Student/NetSupport.Student.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish NetSupport.Tutor/NetSupport.Tutor.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
*You will find the final `.exe` files buried in the `bin/Release/net9.0-windows/win-x64/publish/` folders of each project.*

---

## 🚀 Execution Guide (How to run a full test)

For your final university demo, follow this exact order of operations:

### Step 1: Create the Exam
1. Open **NetSupport.Designer.exe**.
2. Type in a few questions (try an Arabic one to show off the RTL support!).
3. Click **Export to CSV** and save it to your desktop.

### Step 2: Start the Students
1. Open **NetSupport.Student.exe**.
2. *Note: The app runs in the background. You won't see a window appear! It is silently broadcasting its IP address to the network.*
3. (Optional) You can run multiple instances of the Student app if you want to simulate multiple PCs on your local machine.

### Step 3: Launch the Tutor Dashboard
1. Open **NetSupport.Tutor.exe**.
2. You will instantly see the Student PC appear in the "Discovered Students" DataGrid.
3. Test the **Lock Selected** button. (Thanks to Debug Mode, it will just pop up a small lock window). Hit **Unlock Selected**.
4. Click **Open Testing Console**.
5. Browse for the CSV file you made in Step 1.
6. Click **Push Exam**.

### Step 4: Take the Exam
1. The Student PC will suddenly pop up an Exam Login window!
2. Enter a student name and click "I am Ready".
3. On the Tutor app, click the green **Start Exam** button.
4. The student takes the test. Watch the Tutor Dashboard's **Score** column update in real-time as the student clicks answers!

### Step 5: Generate the Report
1. Once the student submits the exam, click **Generate Report** on the Tutor app.
2. A beautiful PDF will be generated in the `Reports/` folder detailing the student's exact answers.

---
*Developed by the NetSupport MVP Team - 2026*
