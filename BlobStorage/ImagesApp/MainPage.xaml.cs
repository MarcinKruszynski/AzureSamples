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
using Windows.Storage.Provider;
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

        public string Prefix { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            DataContext = this;

            Images = new ObservableCollection<ImageBlob>();

            _imageStorage = new ImageStorage();

            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadImageListAsync();
        }

        private async Task LoadImageListAsync()
        {
            var items = await _imageStorage.ListImageBlobsAsync(Prefix);

            Images = new ObservableCollection<ImageBlob>(items.Select(i => new ImageBlob { BlobName = i.Name, BlobUri = i.Uri.ToString(), Blob = i }));

            SelectedImage = Images.FirstOrDefault();
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
                var blobExists = await _imageStorage.CheckIfBlobExistsAsync(file.Name);

                if (!blobExists)
                {
                    var bytes = await GetBytesAsync(file);

                    var cloudBlockBlob = await _imageStorage.UploadImageAsync(bytes, file.Name);

                    var item = new ImageBlob { BlobName = cloudBlockBlob.Name, BlobUri = cloudBlockBlob.Uri.ToString(), Blob = cloudBlockBlob };

                    Images.Add(item);

                    SelectedImage = item;
                }
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

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadImageListAsync();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedImage == null)
                return;

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();            
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("Images", new List<string>() { ".jpg", ".jpeg", ".png" });
            savePicker.SuggestedFileName = SelectedImage.BlobName;

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                using (var streamToWrite = await file.OpenStreamForWriteAsync())
                {
                    await _imageStorage.DownloadImageAsync(SelectedImage.Blob, streamToWrite);
                }
            }
        }
    }
}
