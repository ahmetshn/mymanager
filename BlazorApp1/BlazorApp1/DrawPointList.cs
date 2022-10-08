namespace BlazorApp1
{
    public class DrawPointList
    {
        public long Tick { get; set; }

        public IList<DrawPoint> DrawPoints { get; set; }

        public string Tool { get; set; }
    }
}
