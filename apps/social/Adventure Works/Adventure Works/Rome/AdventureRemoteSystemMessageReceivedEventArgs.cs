using Windows.Foundation.Collections;

namespace Adventure_Works
{
    public class AdventureRemoteSystemMessageReceivedEventArgs
    {
        public ValueSet Message { get; set; }
        public ValueSet ResponseMessage { get; set; }
    }
}