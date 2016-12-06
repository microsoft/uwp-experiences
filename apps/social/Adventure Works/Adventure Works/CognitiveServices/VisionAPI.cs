using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Adventure_Works.CognitiveServices
{
    public class VisionAPI
    {
        private VisionServiceClient _client;

        private static VisionAPI _instance;

        public static VisionAPI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VisionAPI();
                }

                return _instance;
            }
        }

        private VisionAPI()
        {
            _client = new VisionServiceClient(Keys.VisionServiceKey);
        }

        public async Task<string> GetThumbnail(IRandomAccessStream stream, string filename, int width = 250, int height = 250)
        {
            byte[] thumbnailData = null;
            var success = false;

            while (!success)
            {
                try
                {
                    thumbnailData = await _client.GetThumbnailAsync(stream.AsStream(), width, height);
                    success = true;
                }
                catch (Exception ex)
                {
                    // wait out the limitation in the API
                    await Task.Delay(TimeSpan.FromSeconds(60));
                }
            }
            var file = await (await AdventureObjectStorageHelper.GetDataSaveFolder()).CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);

            using (var writeStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await writeStream.WriteAsync(thumbnailData.AsBuffer());
            }

            return file.Path;
        }

        public async Task<AnalysisResult> AnalyzePhoto(IRandomAccessStream stream)
        {
            try
            {
                var result = await _client.AnalyzeImageAsync(stream.AsStream(),
                        new[] { VisualFeature.Tags, VisualFeature.Description,
                        VisualFeature.Faces, VisualFeature.ImageType });
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MAKE SURE TO POPULATE Keys.cs");
                return null;
            }
        }

    }
}
