using MahApps.Metro.Controls;

namespace EbonySnapsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static string StatusBarText { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            StatusBarText = "App launched!";
        }
    }
}