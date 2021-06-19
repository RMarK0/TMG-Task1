using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace Task1
{
    /*
     * Class, which contains most of the logic
     * of the app. 
     */

    static class ViewModelHelper
    {
        // List that contains incorrect IDs (< 1 && > 20)
        internal static List<int> badStringsId = new List<int>(); 


        /// <summary>
        /// Method that cleans up selection of input IDs
        /// </summary>
        /// <param name="input"> Input unclean string </param>
        /// <returns> String with clean input </returns>
        public static List<int> CleanInputString(string input)
        {
            char[] stringSeparators = { ',',';' };
            string[] inputSubstrings = input.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            List<int> cleanInput = new List<int>();

            foreach (var substring in inputSubstrings)
            {
                bool isClean;
                int temp = 0;
                try 
                {
                    temp = Convert.ToInt32(substring.Trim()); // If substring can be converted to int and (0 < input < 21), it is considered as "clean"
                    if (temp > 0 && temp < 21)
                        isClean = true;
                    else
                    {
                        badStringsId.Add(temp);               // If not, it is considered "unclean" and will be put at special list
                        isClean = false;
                    }
                }
                catch (Exception)
                {
                    isClean = false;                           // Same pattern here
                }
                if (isClean)
                {
                    cleanInput.Add(temp);
                }
            }

            cleanInput = cleanInput.Distinct().ToList();     // Removing all duplicates in lists
            badStringsId = badStringsId.Distinct().ToList(); //
            return cleanInput;
        }

        /// <summary>
        /// Method that gives a preset for new TextBlock
        /// </summary>
        /// <returns> New TextBlock instance with certain parameters </returns>
        private static TextBlock CreateTextBlock()
        {
            TextBlock output = new TextBlock() 
            {
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(15)
            };
            return output;
        }

        /// <summary>
        /// Method that creates new data node in TextViewer of active window
        /// </summary>
        /// <param name="item"> Input unclean string </param>
        /// <param name="window"> Active window instance </param>
        public static void CreateTextNode(TextData item, MainWindow window)
        {
            window.TextViewerGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = GridLength.Auto
            });

            for (int i = 0; i < window.TextViewerGrid.ColumnDefinitions.Count; i++) // For each grid.column we create 3 TextBlocks,
                                                                                    // which contain text, words count and vowels count
            {
                TextBlock dataNode = CreateTextBlock();
                window.TextViewerGrid.Children.Add(dataNode);
                Grid.SetRow(dataNode, window.TextViewerGrid.RowDefinitions.Count - 1);
                Grid.SetColumn(dataNode, i);

                switch (i) // For each column we assign, whether it will be text,words count or vowels count TextBlock
                {
                    case (0):
                        dataNode.Text = item.Text;
                        break;
                    case (1):
                        dataNode.Text = item.WordsCount.ToString();
                        break;
                    case (2):
                        dataNode.Text = item.VowelsCount.ToString();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        /// <summary>
        /// Method that clears the data nodes' table
        /// </summary>
        /// <param name="window"> Active window instance </param>
        public static void ClearPreviousResults(MainWindow window) 
        {
            window.TextViewerGrid.Children.Clear();
            window.TextViewerGrid.RowDefinitions.Clear();
        }

        /// <summary>
        /// Method that gets JSON object from the server
        /// </summary>
        /// <param name="index"> Index of desired JSON text object </param>
        /// <param name="window"> Active window instance </param>
        /// <returns> JSON object, retrieved from the server </returns>
        public static JObject ObtainJObject(int index, MainWindow window) 
        {
            string uri = $"https://tmgwebtest.azurewebsites.net/api/textstrings/{index}"; // Lovely TMG link <3

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("TMG-Api-Key", "0J/RgNC40LLQtdGC0LjQutC4IQ==");
            HttpWebResponse response;

            try // Trying to get response on our request
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            Stream receiveStream = response.GetResponseStream();
            Encoding encoding = Encoding.UTF8;
            StreamReader readStream = new StreamReader(receiveStream ?? throw new InvalidOperationException("Cannot open StreamReader"), encoding);

            // Opening stream to read incoming files from server

            char[] bytes = new char[256]; // Buffer array
            int count = readStream.Read(bytes, 0, 256); // Collects data from incoming stream 
            StringBuilder sb = new StringBuilder();

            while (count > 0)
            {
                sb.Append(new String(bytes, 0, count)); // Brings data from bytes to StringBuilder
                count = readStream.Read(bytes, 0, 256);
            }
            response.Close();
            readStream.Close();

            JObject json;

            try // Trying to convert StringBuilder to JSON object
            {
                json = JObject.Parse(sb.ToString()); 
            }
            catch (Exception e)
            {
                CreateNotification(window, e.Message);
                throw;
            }

            return json;
        }

        /// <summary>
        /// Method that creates notification TextBlock
        /// </summary>
        /// <param name="window"> Active window instance </param>
        /// <param name="text"> Notification text </param>
        public static void CreateNotification(MainWindow window, string text)
        {
            TextBlock exceptionTextBlock = new TextBlock() 
            {
                Text = text,
                Margin = new Thickness(10)
            };
            window.mainGrid.Children.Add(exceptionTextBlock);
            Grid.SetRow(exceptionTextBlock, 0);
            Grid.SetColumn(exceptionTextBlock, 0);
            Grid.SetColumnSpan(exceptionTextBlock, 3);
        }

        /// <summary>
        /// Method that gets content of the token named "text"
        /// </summary>
        /// <param name="json"> Input JSON object </param>
        /// <param name="window"> Active window instance </param>
        /// <returns> String variable with content of "text" token </returns>
        public static string GetTextFromJson(JObject json, MainWindow window)
        {
            bool containsText = false;

            foreach (var pair in json) // if code finds token named "text", it switches flag containsText to true
                if (pair.Key == "text")
                    containsText = true;

            if (!containsText)
                throw new ArgumentException("Current JSON don't have any tokens named 'text'");

            try
            {
                string text = json.SelectToken("text").Value<string>(); // if it finds text, it returns it
                return text;
            }
            catch (Exception e)
            {
                CreateNotification(window, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Method, which is called to calculate words and vowels count for certain data node
        /// </summary>
        /// <param name="text"> Input text to calculate vowels and words in </param>
        /// <returns></returns>
        public static TextData CalculateTextData(string text)
        {
            int vowelsCount = 0;
            int wordsCount = 0;

            var vowels = new HashSet<char>() // It is possible that it contains not all vowel letters
            {
                'a', 'e', 'i', 'o', 'u', 'æ', 'ø', 'ǫ', 'ö', 'ü', 'ä', 'y', 'а', 'е', 'ё', 'и', 'о', 'у', 'ы', 'э', 'ю',
                'я', 'è', 'ê', 'é', 'à', 'á', 'â', 'ã', 'ä', 'å', 'ë', 'ё', 'ì', 'í', 'î', 'ï', 'ò', 'ó', 'ô', 'õ', 'ù',
                'ú', 'û', 'ý', 'ÿ', 'ı', 'ō', 'ŏ', 'ő', 'œ', 'ũ', 'ū', 'ŭ', 'ů', 'ű', 'ų', 'ŷ', 'ƣ', 'ơ', 'ư', 'ƴ', 'ƹ',
                'ƺ', 'ӧ', 'і'
            };
            
            text = text.ToLower();
            for (int i = 0; i < text.Length; i++) // Checks if current letter is vowel
            {
                if (vowels.Contains(text[i]))
                {
                    vowelsCount++;
                }
            }

            var matches = Regex.Matches(text,                                   // 
                @"[\w']+", RegexOptions.CultureInvariant | RegexOptions.Multiline   // Counts words in input string with regex [\w']+
                                                         | RegexOptions.IgnoreCase);            //
            wordsCount = matches.Count;

            return new TextData(vowelsCount, wordsCount, text); 
        }
    }
}
