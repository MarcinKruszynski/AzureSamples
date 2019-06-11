using ServiceBusLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;
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
        public ObservableCollection<string> ReadQueueItems { get; set; }

        public MainPage()
        {
            this.DataContext = this;

            this.InitializeComponent();

            ReadQueueItems = new ObservableCollection<string>();
        }        

        private async void SendQueue_Click(object sender, RoutedEventArgs e)
        {
            await ServiceBusUtils.SendMessage(sendQueueTxt.Text);

            sendQueueTxt.Text = "";
        }

        private async void ReadQueue_Click(object sender, RoutedEventArgs e)
        {
            await ServiceBusUtils.ReceiveMessage(UpdateUI);
        }

        private void UpdateUI(string text)
        {
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
               () =>
               {
                   ReadQueueItems.Add(text);
               });
        }        
    }
}
