using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.FaceAnalysis;
using Windows.Storage.Streams;

namespace Adventure_Works.CognitiveServices
{
    public class EmotionAPI
    {
        private EmotionServiceClient _client;

        private static EmotionAPI _instance;

        public static EmotionAPI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EmotionAPI();
                }

                return _instance;
            }
        }

        private EmotionAPI()
        {
            _client = new EmotionServiceClient(Keys.EmotionServiceKey);
        }

        /// <summary>
        /// Returns true if all faces are smiling in the frame
        /// </summary>
        /// <param name="stream">stream of frame</param>
        /// <param name="faces">list of faces from the builtin FaceRecognition</param>
        /// <param name="scale">factor by which frame has been scaled compared to recognized faces</param>
        /// <returns></returns>
        public async Task<bool> CheckIfEveryoneIsSmiling(IRandomAccessStream stream, IEnumerable<DetectedFace> faces, double scale)
        {
            List<Rectangle> rectangles = new List<Rectangle>();

            foreach (var face in faces)
            {
                var box = face.FaceBox;
                rectangles.Add(new Rectangle()
                {
                    Top = (int)((double)box.Y * scale),
                    Left = (int)((double)box.X * scale),
                    Height = (int)((double)box.Height * scale),
                    Width = (int)((double)box.Width * scale)
                });
            }

            try
            {
                var emotions = await _client.RecognizeAsync(stream.AsStream(), rectangles.ToArray());
                return emotions.Where(emotion => GetEmotionType(emotion) == EmotionType.Happiness).Count() == emotions.Count();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MAKE SURE TO POPULATE Keys.cs");
                return false;
            }
        }

        /// <summary>
        /// Converts <see cref="Emotion"/> to <see cref="EmotionType"/>
        /// </summary>
        /// <param name="emotion">Emotion to convert</param>
        /// <returns></returns>
        public static EmotionType GetEmotionType(Emotion emotion)
        {
            EmotionType type = default(EmotionType);

            var scores = emotion.Scores;
            float max = 0;

            if (scores.Contempt > max)
            {
                max = scores.Contempt;
                type = EmotionType.Contempt;
            }
            if (scores.Disgust > max)
            {
                max = scores.Disgust;
                type = EmotionType.Disgust;
            }
            if (scores.Fear > max)
            {
                max = scores.Fear;
                type = EmotionType.Fear;
            }
            if (scores.Happiness > max)
            {
                max = scores.Happiness;
                type = EmotionType.Happiness;
            }
            if (scores.Neutral > max)
            {
                max = scores.Neutral;
                type = EmotionType.Neutral;
            }
            if (scores.Sadness > max)
            {
                max = scores.Sadness;
                type = EmotionType.Sadness;
            }
            if (scores.Surprise > max)
            {
                max = scores.Surprise;
                type = EmotionType.Surprise;
            }

            return type;
        }
    }
}
