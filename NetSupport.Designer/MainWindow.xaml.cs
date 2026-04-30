using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NetSupport.Shared.Models;
using NetSupport.Shared.Services;

namespace NetSupport.Designer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ObservableCollection<ExamQuestion> _questions = new();

    public MainWindow()
    {
        InitializeComponent();
        QuestionsGrid.ItemsSource = _questions;
        TranslationService.IsArabic = false;
    }

    private void AddQuestionBtn_Click(object sender, RoutedEventArgs e)
    {
        bool isArabic = TranslationService.IsArabic;
        
        if (string.IsNullOrWhiteSpace(TxtQuestion.Text) ||
            string.IsNullOrWhiteSpace(TxtOptionA.Text) ||
            string.IsNullOrWhiteSpace(TxtOptionB.Text) ||
            string.IsNullOrWhiteSpace(TxtOptionC.Text) ||
            string.IsNullOrWhiteSpace(TxtOptionD.Text))
        {
            MessageBox.Show(TranslationService.Translate("Please fill in all fields before adding a question.", isArabic), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string correct = RbA.IsChecked == true ? "A" :
                         RbB.IsChecked == true ? "B" :
                         RbC.IsChecked == true ? "C" : "D";

        var question = new ExamQuestion
        {
            Index = _questions.Count + 1,
            QuestionText = TxtQuestion.Text.Trim(),
            OptionA = TxtOptionA.Text.Trim(),
            OptionB = TxtOptionB.Text.Trim(),
            OptionC = TxtOptionC.Text.Trim(),
            OptionD = TxtOptionD.Text.Trim(),
            CorrectOption = correct
        };

        _questions.Add(question);

        // Clear Form
        TxtQuestion.Clear();
        TxtOptionA.Clear();
        TxtOptionB.Clear();
        TxtOptionC.Clear();
        TxtOptionD.Clear();
        RbA.IsChecked = true;
    }

    private void DeleteQuestionBtn_Click(object sender, RoutedEventArgs e)
    {
        if (QuestionsGrid.SelectedItem is ExamQuestion selected)
        {
            _questions.Remove(selected);
            // Re-index
            for (int i = 0; i < _questions.Count; i++)
            {
                _questions[i].Index = i + 1;
            }
            QuestionsGrid.Items.Refresh();
        }
    }

    private void ExportBtn_Click(object sender, RoutedEventArgs e)
    {
        bool isArabic = TranslationService.IsArabic;

        if (_questions.Count == 0)
        {
            MessageBox.Show(TranslationService.Translate("Please add at least one question before exporting.", isArabic), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            DefaultExt = ".csv",
            FileName = "New_Exam.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                using var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)); // UTF-8 with BOM
                
                // Write Header
                writer.WriteLine("QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption");

                // Write Data
                foreach (var q in _questions)
                {
                    string SafeCsv(string input) => $"\"{input.Replace("\"", "\"\"")}\"";
                    
                    var line = $"{SafeCsv(q.QuestionText)},{SafeCsv(q.OptionA)},{SafeCsv(q.OptionB)},{SafeCsv(q.OptionC)},{SafeCsv(q.OptionD)},{q.CorrectOption}";
                    writer.WriteLine(line);
                }

                MessageBox.Show(TranslationService.Translate("Export Successful", isArabic), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void LanguageToggle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        
        bool isArabic = ((ComboBoxItem)LanguageToggle.SelectedItem).Tag.ToString() == "ar";
        TranslationService.IsArabic = isArabic;
        
        this.FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        
        Title = TranslationService.Translate("NetSupport MVP - MCQ Designer", isArabic);
        ExportBtn.Content = TranslationService.Translate("Export to CSV", isArabic);
        LblLanguage.Content = TranslationService.Translate("Language:", isArabic);
        LblAddTitle.Text = TranslationService.Translate("Add New Question", isArabic);
        LblQuestion.Content = TranslationService.Translate("Question Text:", isArabic);
        LblOptions.Content = TranslationService.Translate("Options:", isArabic);
        AddQuestionBtn.Content = TranslationService.Translate("Add to Exam", isArabic);
        LblListTitle.Text = TranslationService.Translate("Questions in Exam:", isArabic);
        DeleteBtn.Content = TranslationService.Translate("Delete Selected", isArabic);
    }
}