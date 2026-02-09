using System.Windows.Controls;

namespace DNSveil
{
    /// <summary>
    /// Interaction logic for PageTest.xaml
    /// </summary>
    public partial class PageTest : Page
    {
        public PageTest()
        {
            InitializeComponent();
        }

        public string GetButtonText()
        {
            return ButtonTest.Content as string;
        }
    }
}
