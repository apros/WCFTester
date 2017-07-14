using System.Windows;
using UNWcfTester.ViewModel;

namespace UNWcfTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       public MainWindow()
        {
           InitializeComponent();
            var mvm = (MainViewModel)DataContext;
            mvm.PnlPropertyContainer = PnlPropertyContainer;
            mvm.RichMessage = RichMessage;
            this.DataContext = mvm;//"{Binding Main, Source={StaticResource Locator}}";           
        }
    }    
}