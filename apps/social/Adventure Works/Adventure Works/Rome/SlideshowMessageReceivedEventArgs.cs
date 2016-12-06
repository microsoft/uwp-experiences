using Windows.Foundation.Collections;

namespace Adventure_Works.Rome
{
    public class SlideshowMessageReceivedEventArgs
    {
        public ValueSet Message { get; set; }
        public ValueSet ResponseMessage { get; set; }
        public SlideshowMessageTypeEnum QueryType { get; set; }
    }
}