using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VideoLibrary;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Foundation;
using System.Threading.Tasks;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolBox.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConverterView : Page
    {
        public ConverterView()
        {
            this.InitializeComponent();
        }

        private async void DownloadMp3File(object sender, RoutedEventArgs e)
        {
            string url = youtubeUrl.Text;

            if (!string.IsNullOrEmpty(url) && url.Contains("youtube"))
            {

                string savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

                StorageFolder storageFolder = KnownFolders.MusicLibrary;
            
                var youTube = YouTube.Default; // starting point for YouTube actions
                var video = await youTube.GetVideoAsync(url).ConfigureAwait(false); // gets a Video object with info about the video
                Debug.WriteLine("Video Downloaded");

                string videoPath = Path.Combine(savePath, video.FullName);
                string mp3Path = Path.Combine(savePath, videoPath.Replace(".mp4", ".mp3"));

                StorageFile videoFile = await storageFolder.CreateFileAsync(Path.GetFileName(videoPath), CreationCollisionOption.ReplaceExisting);
                StorageFile mp3File = await storageFolder.CreateFileAsync(Path.GetFileName(mp3Path), CreationCollisionOption.ReplaceExisting);

                await WriteBytesIntoVideoFile(videoFile, video);
                await ConvertMp4ToMp3(videoFile, mp3File);

                await videoFile.DeleteAsync();
                await ResetProgressBar();

                //https://www.youtube.com/watch?v=t1YHv1wHAxo

            }
            else
            {
                showPopup("Please insert a valid youtube url!");
            }
        }

        public async Task WriteBytesIntoVideoFile(StorageFile videoFile, YouTubeVideo downloadedVideo)
        {
            //await FileIO.WriteBytesAsync(videoFile, downloadedVideo.GetBytes());

            var outputStream = await videoFile.OpenAsync(FileAccessMode.ReadWrite);
            using (var dataWriter = new DataWriter(outputStream))
            {

                byte[] videoBytes = downloadedVideo.GetBytes();
                int videoBytesArrayLength = videoBytes.Length;

                int byteArrayFourth = videoBytesArrayLength / 4;
                int toAddValue = videoBytesArrayLength / 4;


                for (int i = 0; i < videoBytesArrayLength; i++)
                {
                    //Debug.WriteLine(byteArrayFourth);
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

        public async Task ConvertMp4ToMp3(IStorageFile videoFile, IStorageFile mp3File)
        {
            MediaEncodingProfile profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
            MediaTranscoder transcoder = new MediaTranscoder();

            PrepareTranscodeResult prepareOp = await transcoder.PrepareFileTranscodeAsync(videoFile, mp3File, profile);

            if (prepareOp.CanTranscode)
            {
                await prepareOp.TranscodeAsync();

            }
            else
            {
                switch (prepareOp.FailureReason)
                {
                    case TranscodeFailureReason.CodecNotFound:
                        System.Diagnostics.Debug.WriteLine("Codec not found.");
                        break;
                    case TranscodeFailureReason.InvalidProfile:
                        System.Diagnostics.Debug.WriteLine("Invalid profile.");
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unknown failure.");
                        break;
                }
            }
        }

        public void showPopup(string message)
        {
            successMessage.Text = message;
            successPopup.IsOpen = true;
        }
    }
}
