using EventHubLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EventHubApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<string> ReadMessages { get; set; }

        public MainPage()
        {
            this.DataContext = this;

            this.InitializeComponent();

            ReadMessages = new ObservableCollection<string>();
        }

        private async void SendMessages_Click(object sender, RoutedEventArgs e)
        {
            await EventHubUtils.SendMessages();
        }

        private void ReadMessages_Click(object sender, RoutedEventArgs e)
        {
            EventHubUtils.ReadMessages(UpdateUI);
        }

        private void UpdateUI(string text)
        {
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
               () =>
               {
                   ReadMessages.Add(text);
               });
        }
    }
}
