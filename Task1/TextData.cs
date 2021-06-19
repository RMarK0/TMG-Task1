using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Task1
{
    class TextData : INotifyPropertyChanged
    {
        private int _vowelsCount;
        private int _wordsCount;
        private string _text;

        public int VowelsCount
        {
            get => _vowelsCount;
            set
            {
                _vowelsCount = value;
                OnPropertyChanged("VowelsCount");
            }
        }

        public int WordsCount
        {
            get => _wordsCount;
            set
            {
                _wordsCount = value;
                OnPropertyChanged("WordsCount");
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public TextData(int vowelsCount, int wordsCount, string text)
        {
            _wordsCount = wordsCount;
            _vowelsCount = vowelsCount;
            _text = text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}