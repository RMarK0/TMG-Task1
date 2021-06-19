using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using static Task1.ViewModelHelper;

namespace Task1
{
    public partial class MainWindow : Window
    {
        private readonly MainWindow _activeWindow; // Variable, that contains reference to current window
        public MainWindow()
        {
            InitializeComponent();
            _activeWindow = this;
        }

        /// <summary>
        /// Method that processes click for "Calculate" button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousResults(_activeWindow);                // First of all, we need to clear previous table
            string input = StringsIdTextBox.Text;               //
            List<int> cleanInput = CleanInputString(input);     // Then we need to clean our input 

            
            foreach (int clInput in cleanInput)
            {
                string text = null;
                try
                {
                    text = GetTextFromJson(ObtainJObject(clInput, _activeWindow), _activeWindow);   // Obtain text from json
                    TextData temp = CalculateTextData(text);                                        // Calculate vowels and words count
                    CreateTextNode(temp, _activeWindow);                                            // Create text nodes in table 
                }
                catch (Exception exception)
                {
                    CreateNotification(_activeWindow, exception.Message);   // If error occurs, create notification
                    break;
                }
                if (badStringsId.Count > 0) // If we have bad IDs, create notification
                {
                    string error = "Неверные идентификаторы: ";
                    foreach (int item in badStringsId)
                        error = String.Concat(error, item.ToString(), " ");
                    CreateNotification(_activeWindow, error);
                }
            }
        }

        private static readonly Regex _regex = new Regex("[^0-9,;]+"); // Regex for preventing entering wrong symbols
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        /// <summary>
        /// Method for detecting wrong symbols in StringsIdTextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StringsIdTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}
