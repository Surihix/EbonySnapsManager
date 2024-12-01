using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace EbonySnapsManager
{
    public class AppViewModel : INotifyPropertyChanged
    {
        private bool _isUIenabled;
        public bool IsUIenabled
        {
            get { return _isUIenabled; }
            set
            {
                if (_isUIenabled != value)
                {
                    _isUIenabled = value;
                    OnPropertyChanged(nameof(IsUIenabled));
                }
            }
        }

        private string _statusBarTxt;
        public string StatusBarTxt
        {
            get { return _statusBarTxt; }
            set
            {
                if (_statusBarTxt != value)
                {
                    _statusBarTxt = value;
                    OnPropertyChanged(nameof(StatusBarTxt));
                }
            }
        }

        private BitmapImage _bitmapSrc0;
        public BitmapImage BitmapSrc0
        {
            get { return _bitmapSrc0; }
            set 
            {
                if (_bitmapSrc0 != value)
                {
                    _bitmapSrc0 = value;
                    OnPropertyChanged(nameof(BitmapSrc0));
                }
            }
        }

        private BitmapImage _bitmapSrc1;
        public BitmapImage BitmapSrc1
        {
            get { return _bitmapSrc1; }
            set
            {
                if (_bitmapSrc1 != value)
                {
                    _bitmapSrc1 = value;
                    OnPropertyChanged(nameof(BitmapSrc1));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}