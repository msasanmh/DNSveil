using MsmhToolsWpfClass;
using System.Windows;

namespace DNSveil
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : WpfWindow
    {
        public TestWindow()
        {
            InitializeComponent();
            Loaded += TestWindow_Loaded;
        }

        private void TestWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PART_Button1.Visibility = Visibility.Visible;
        }
    }
}
