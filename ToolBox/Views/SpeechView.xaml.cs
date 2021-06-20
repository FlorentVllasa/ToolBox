using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using ToolBox.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolBox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpeechView : Page
    {
        public SpeechView()
        {
            this.InitializeComponent();
        }

        public SpeechModel SpeechModel = new SpeechModel();
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ToSay.Text))
            {
                SpeechModel.ToSay = ToSay.Text;
                await PlayAudio(SpeechModel.ToSay);
            }
            else
            {
                SpeechModel.ToSay = null;
            }
        }

        private async void SaveAsAudio(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SpeechModel.ToSay))
            {
                StorageFolder storageFolder = KnownFolders.MusicLibrary;
                StorageFile storageFile = await storageFolder.CreateFileAsync("audio.mp3", CreationCollisionOption.GenerateUniqueName);

                if(storageFile != null)
                {
                    try
                    {
                        var synth = new SpeechSynthesizer();
                        SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(SpeechModel.ToSay);

                        using (var reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            IBuffer buffer = reader.ReadBuffer((uint)stream.Size);
                            await FileIO.WriteBufferAsync(storageFile, buffer);
                        }
                        showPopup("Your file has been saved under the Music Folder");
                        SpeechModel.ToSay = null;
                        
                    }
                    catch { }
                }
            }
            else
            {
                showPopup("You should say something before you want to save");
            }
        }

        public async Task PlayAudio(string ToPlay)
        {
            MediaElement mediaElement = new MediaElement();
            var synth = new SpeechSynthesizer();
            SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(ToPlay);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }
        
        public void showPopup(string message)
        {
            successMessage.Text = message;
            successPopup.IsOpen = true;
        }
    }
}
