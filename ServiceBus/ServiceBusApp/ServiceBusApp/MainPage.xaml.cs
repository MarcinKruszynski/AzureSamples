using ServiceBusLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ServiceBusApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }        

        private async void SendQueue_Click(object sender, RoutedEventArgs e)
        {
            await ServiceBusUtils.SendMessage(sendQueueTxt.Text);
        }

        private async void ReadQueue_Click(object sender, RoutedEventArgs e)
        {
            await ServiceBusUtils.ReceiveMessage(text => readQueueTxt.Text = text);
        }
    }
}
