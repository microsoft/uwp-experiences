using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Data
{
    public class NewsItem
    {
        private static string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam imperdiet iaculis porta. Morbi tortor sem, lobortis sed vulputate vitae, pulvinar at tortor. Vestibulum faucibus consectetur augue, sit amet congue ante commodo vitae. ";
        private static string imageHome = "http://adx.azureedge.net/Images/Watermark";
        public string Title { get; set; }
        public string Summary { get; set; } = lorem;
        public string Author { get; set; } = "JILL SMITH";
        public DateTime Timestamp { get; set; }
        public int Likes { get; set; }
        public string HeroImage { get; set; }
        public bool IsHero { get; set; } = false;

        public static List<NewsItem> GetData()
        {
            var items = new List<NewsItem>();
            items.Add(new NewsItem()
            {
                Title = "WITH LOVE FROM SEATTLE",
                Timestamp = DateTime.Now,
                Likes = 5,
                HeroImage = $"{imageHome}/Large/FeaturedImage_2x1_Image47.jpg",
                IsHero = true
            });
            items.Add(new NewsItem()
            {
                Title = "TAKING IT BACK",
                Timestamp = DateTime.Now,
                Likes = 5,
                HeroImage = $"{imageHome}/Large/FeaturedImage_2x1_Image60.jpg"
            });
            items.Add(new NewsItem()
            {
                Title = "ALL FOR THE GAME",
                Timestamp = items.Last().Timestamp - TimeSpan.FromMinutes(new Random().Next(9, 36) * 10),
                Likes = 4,
                HeroImage = $"{imageHome}/Large/FeaturedImage_2x1_Image13.jpg"
            });
            items.Add(new NewsItem()
            {
                Title = "FUTURE OF TOMORROW",
                Timestamp = items.Last().Timestamp - TimeSpan.FromMinutes(new Random().Next(9, 36) * 10),
                Likes = 7,
                HeroImage = $"{imageHome}/Large/FeaturedImage_2x1_Image41.jpg"
            });
            items.Add(new NewsItem()
            {
                Title = "THE VOICE OF TOMORROW",
                Timestamp = items.Last().Timestamp - TimeSpan.FromMinutes(new Random().Next(9, 36) * 10),
                Likes = 3,
                HeroImage = $"{imageHome}/Large/FeaturedImage_2x1_Image11.jpg"
            });
            items.Add(new NewsItem()
            {
                Title = "TAKING IT ALL ON",
                Timestamp = items.Last().Timestamp - TimeSpan.FromMinutes(new Random().Next(9, 36) * 10),
                Likes = 4,
                HeroImage = $"{imageHome}/Large/FeaturedImage_2x1_Image12.jpg"
            });
            items.Add(new NewsItem()
            {
                Title = "BEYOND THE OLYMPICS",
                Timestamp = items.Last().Timestamp - TimeSpan.FromMinutes(new Random().Next(9, 36) * 10),
                Likes = 7,
                HeroImage = $"{imageHome}/Large/FeaturedImage_2x1_Image30.jpg"
            });
            items.Add(new NewsItem()
            {
                Title = "A ROAD LESS TRAVELED",
                Timestamp = items.Last().Timestamp - TimeSpan.FromMinutes(new Random().Next(9, 36) * 10),
                Likes = 3,
                HeroImage = $"{imageHome}/Large/FeaturedImage_2x1_Image23.jpg"
            });

            return items;
        }

        public static List<string> GetListOfTopics()
        {
            var topics = new List<string>();
            topics.Add("LATEST");
            topics.Add("WORLD");
            topics.Add("ENTERTAINMENT");
            topics.Add("SOCIAL");
            topics.Add("TRAVEL");
            topics.Add("VIDEOS");

            return topics;
        }
    }
}
