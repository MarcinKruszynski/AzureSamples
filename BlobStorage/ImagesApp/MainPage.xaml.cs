using ImagesStorage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImagesApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        IImageStorage _imageStorage;

        private ObservableCollection<ImageBlob> _images;

        public ObservableCollection<ImageBlob> Images
        {
            get { return _images; }
            set
            {
                _images = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Images"));
            }
        }

        private ImageBlob _selectedImage;

        public ImageBlob SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                _selectedImage = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedImage"));
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            DataContext = this;

            Images = new ObservableCollection<ImageBlob>();

            _imageStorage = new ImageStorage();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var bytes = await GetBytesAsync(file);

                var cloudBlockBlob = await _imageStorage.UploadImageAsync(bytes, file.Name);

                var item = new ImageBlob { BlobName = cloudBlockBlob.Name, BlobUri = cloudBlockBlob.Uri.ToString() };

                Images.Add(item);

                SelectedImage = item;
            }
        }

        private async Task<byte[]> GetBytesAsync(StorageFile file)
        {
            byte[] fileBytes = null;
            if (file == null) return null;
            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }
    }
}
