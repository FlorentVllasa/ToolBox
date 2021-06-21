using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using ToolBox.Models;
using VideoLibrary;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolBox.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoDownloader : Page
    {

        //private StorageFolder pickedFolder;
        private VideoDownloaderUserInput userInput;
        public VideoDownloader()
        {
            this.InitializeComponent();
            userInput = new VideoDownloaderUserInput();
        }


        private async void SetDownloadLocation(object sender, RoutedEventArgs e)
        {

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder pickedFolder = await folderPicker.PickSingleFolderAsync();

            if (pickedFolder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", pickedFolder);
                userInput.pickedFolder = pickedFolder;
                Debug.WriteLine(pickedFolder.Path);
            }
        }


        //ToDo Videos above 480p have no sound: Investigate more... 
        private async void GetAvailableQualities(object sender, RoutedEventArgs e)
        {
            string url = youtubeUrl.Text;

            if (!string.IsNullOrEmpty(url) && url.Contains("youtube"))
            {
                var youTube = YouTube.Default;
                var videos = await youTube.GetAllVideosAsync(url).ConfigureAwait(false);
                userInput.downloadedVideos = videos;
                userInput.youtubeUrl = url;



                //var highestQualityVideo = YouTube.Default
                //                        .GetAllVideos(url)
                //                        .OrderByDescending(v => v.Resolution)
                //                        .Where(v => v.FullName.Contains(".mp4"))
                //                        .Select(j => j.Resolution)
                //                        .ToHashSet();

                var resolutions = videos
                    .Where(j => j.AdaptiveKind == AdaptiveKind.Video)
                    .Select(j => j.Resolution)
                    .OrderByDescending(v => v)
                    .ToHashSet();

                Debug.WriteLine(resolutions);

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    foreach (int resolution in resolutions)
                    {
                        Debug.WriteLine(resolution);
                        qualitySelection.Items.Add(resolution);
                        
                    }
                    qualitySelection.SelectedIndex = 0;
                });

                //https://www.youtube.com/watch?v=t1YHv1wHAxo
            }
            else
            {
                //Show error
            }


        }

        private async void DownloadVideo(object sender, RoutedEventArgs e)
        {
            string url = userInput.youtubeUrl;
            StorageFolder pickedFolder = userInput.pickedFolder;
            var qualities = qualitySelection;
            var downloadedVideos = userInput.downloadedVideos;
            YouTubeVideo selectedVideo = null;
            StorageFile systemFile = null;

            if (!string.IsNullOrEmpty(url) && pickedFolder != null && qualities.Items.Count != 0 && downloadedVideos.Count() != 0)
            {
                string selectedQuality = qualities.SelectedItem.ToString();

                var selectedVideoToDownloadList = downloadedVideos.Where(v => v.Resolution == Int32.Parse(selectedQuality)).ToList();
                if(selectedVideoToDownloadList.Count != 0)
                {
                    selectedVideo = selectedVideoToDownloadList.ElementAt(0);
                }
                else
                {
                    Debug.WriteLine("Selected Video List is empty");
                }

                if (selectedVideo != null)
                {
                    Debug.WriteLine(selectedVideo.FullName);
                    string fullPath = Path.Combine(pickedFolder.Path, selectedVideo.FullName);
                    Debug.WriteLine(fullPath);
                    systemFile = await pickedFolder.CreateFileAsync(Path.GetFileName(fullPath), CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    Debug.WriteLine("Selected Video is null");
                }

                if (systemFile != null)
                {
                    await Task.Run(() => WriteBytesIntoVideoFile(systemFile, selectedVideo));
                }
                else
                {
                    Debug.WriteLine("System File was not created");
                }

                await Task.Delay(1000);
                await ResetProgressBar();

            }
            else
            {
                Debug.WriteLine("Wrong");
            }

        }

        public async Task WriteBytesIntoVideoFile(StorageFile videoFile, YouTubeVideo downloadedVideo)
        {
            var outputStream = await videoFile.OpenAsync(FileAccessMode.ReadWrite);
            using (var dataWriter = new DataWriter(outputStream))
            {

                byte[] videoBytes = downloadedVideo.GetBytes();
                int videoBytesArrayLength = videoBytes.Length;

                int byteArrayFourth = videoBytesArrayLength / 4;
                int toAddValue = videoBytesArrayLength / 4;


                for (int i = 0; i < videoBytesArrayLength; i++)
                {
                    if (i == byteArrayFourth)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            progessBar.Value += 25;
                            byteArrayFourth += toAddValue;
                        });
                    }
                    dataWriter.WriteByte(videoBytes[i]);
                }
                await dataWriter.StoreAsync();
                await outputStream.FlushAsync();
            }
        }

        public async Task ResetProgressBar()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                progessBar.Value = 0;
            });
        }
    }
}
