namespace NetSupport.Shared.Services;

/// <summary>
/// A lightweight translation service for the MVP to switch between English and Arabic.
/// </summary>
public static class TranslationService
{
    public static bool IsArabic { get; set; } = false;

    public static string Translate(string key, bool isArabic)
    {
        if (!isArabic) return key;

        return key switch
        {
            // Tutor Dashboard
            "Lock Selected" => "قفل المحدد",
            "Unlock Selected" => "إلغاء القفل",
            "Open Testing Console" => "فتح وحدة التحكم",
            "Start Exam" => "بدء الامتحان",
            "Stop Exam" => "إنهاء الامتحان",
            "Generate Report" => "إنشاء تقرير",
            "Discovered Students:" => "الطلاب المكتشفون:",
            "Language:" => "اللغة:",
            "Student Name" => "اسم الطالب",
            "IP Address" => "عنوان IP",
            "Status" => "الحالة",
            "Score" => "الدرجة",
            
            // Testing Console
            "Push Exam to Selected" => "إرسال الامتحان للمحددين",
            "Browse CSV" => "تصفح ملف CSV",
            "Time Limit (Minutes):" => "الوقت المحدد (بالدقائق):",
            "No file selected" => "لم يتم تحديد ملف",
            
            // Student Login UI
            "I am Ready" => "أنا جاهز",
            "Welcome to the Exam!" => "مرحباً بك في الامتحان!",
            "Please enter your full name to begin:" => "يرجى إدخال اسمك الكامل للبدء:",
            
            // Student Exam UI
            "Next Question" => "السؤال التالي",
            "Previous Question" => "السؤال السابق",
            "Submit Exam" => "تسليم الامتحان",
            "Time Remaining:" => "الوقت المتبقي:",
            "Waiting for instructor to start the exam..." => "في انتظار بدء المعلم للامتحان...",
            
            // PDF Report
            "Final Exam Report" => "التقرير النهائي للامتحان",
            "Generated on:" => "تاريخ الإنشاء:",
            "Student:" => "الطالب:",
            "Final Score:" => "الدرجة النهائية:",
            "Question" => "السؤال",
            "Answer Given" => "الإجابة المقدمة",
            "Correct Answer" => "الإجابة الصحيحة",
            "Correct" => "صحيح",
            "Incorrect" => "خاطئ",
            
            _ => key // Fallback to English
        };
    }
}
