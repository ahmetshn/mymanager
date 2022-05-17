using Excubo.Blazor.Canvas;

namespace BlazorApp1
{
    public class DrawPoint
    {
        public string Tool { get; set; }

        public double StartX { get; set; }
        public double StartY { get; set; }

        public double EndX { get; set; }
        public double EndY { get; set; }

        public double LineWidth { get; set; }

        public string LineColor { get; set; }

        public string FillColor { get; set; }

        public double GlobalAlpha { get; set; }

        public LineCap LineCap { get; set; }

        public LineJoin LineJoin { get; set; }
    }
}
