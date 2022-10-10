namespace BlazorApp1
{
    public class DrawPointList
    {
        public TimeSpan Tick { get; set; }

        public IList<DrawPoint> DrawPoints { get; set; }

        public string Tool { get; set; }
    }
}
