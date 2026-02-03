using System.Windows;
using WeatherAppWpf.ViewModels;

namespace WeatherAppWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
