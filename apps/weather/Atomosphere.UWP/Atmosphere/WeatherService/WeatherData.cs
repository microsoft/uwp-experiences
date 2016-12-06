namespace Atmosphere
{
    public sealed class WeatherData 
    {
        public string[] Locations { get; set; }
        public Today[] Today { get; set; }
        public WeekDay[] WeekDay { get; set; }
    }
}