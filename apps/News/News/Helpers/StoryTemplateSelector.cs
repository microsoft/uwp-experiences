using News.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace News.Helpers
{
    public class StoryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HeroStory { get; set; }
        public DataTemplate Story { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var story = item as NewsItem;

            return story.IsHero ? HeroStory : Story;
        }
    }
}
