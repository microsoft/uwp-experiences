namespace Yoga.Models
{
    public class Video
    {
        public TrainingVideo[] Suggested { get; set; }
        public TrainingVideo[] Completed { get; set; }
        public TrainingVideo[] Browse { get; set; }
    }
}