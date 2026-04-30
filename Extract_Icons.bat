@echo off
echo ========================================================
echo   Extracting AI Generated Logos
echo ========================================================

mkdir "App_Icons" 2>nul

echo Copying Tutor Logo...
copy "C:\Users\Assem_CS\.gemini\antigravity\brain\1b56644b-a412-4142-a388-433e9e8b8a73\netsupport_tutor_logo_1777514418491.png" "App_Icons\tutor_logo.png"

echo Copying Student Logo...
copy "C:\Users\Assem_CS\.gemini\antigravity\brain\1b56644b-a412-4142-a388-433e9e8b8a73\netsupport_student_logo_1777517089057.png" "App_Icons\student_logo.png"

echo Copying Designer Logo...
copy "C:\Users\Assem_CS\.gemini\antigravity\brain\1b56644b-a412-4142-a388-433e9e8b8a73\netsupport_designer_logo_1777517106122.png" "App_Icons\designer_logo.png"

echo.
echo SUCCESS! All 3 images have been copied to the 'App_Icons' folder in your project!
pause
