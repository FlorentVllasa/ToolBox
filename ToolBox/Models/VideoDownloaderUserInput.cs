using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;
using Windows.Storage;

namespace ToolBox.Models
{
    class VideoDownloaderUserInput
    {
        public StorageFolder pickedFolder{ get; set; }
        public string youtubeUrl { get; set; }

        public IEnumerable<YouTubeVideo> downloadedVideos { get; set; }

    }
}
