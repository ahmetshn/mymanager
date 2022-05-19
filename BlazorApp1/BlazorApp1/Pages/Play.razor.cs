using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace BlazorApp1.Pages
{
    public partial class Play
    {
        [Inject]
        public IJSRuntime JS { get; set; }

        private class Position
        {
            public double Left { get; set; }
            public double Top { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }

        public string JsonStr { get; set; }

        private IList<DrawPointList> _currentPlayAllPoints;
        private IList<DrawPointList> _playAllPoints;
        private IList<DrawPointList> _reDrawAllPoints;

        private IList<DrawPoint> _points;
        private ElementReference _container;
        private Canvas _mainCanvas;

        private Context2D _mainContext;

        private double canvasx;
        private double canvasy;
        private double last_mousex;
        private double last_mousey;
        private double start_mousex;
        private double start_mousey;
        private double mousex;
        private double mousey;
        private bool mousedown = false;
        private string clr = "black";

        private string _width;
        private string _height;

        private string _tool;

        private Position _position;

        private bool render_required = true;

        private System.Timers.Timer _timer;
        private long _tick;

        private bool _play;

        protected override void OnInitialized()
        {

            _reDrawAllPoints = new List<DrawPointList>();
            _playAllPoints = new List<DrawPointList>();

            _timer = new System.Timers.Timer();
            _timer.Interval = 1;
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;

            _tick = 0;
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            _tick++;

            if (_play)
            {
                //Console.WriteLine("_tick = {0}", _tick);

                var drawPoints = _currentPlayAllPoints.Where(p => p.DrawPoints.Any(x => x.Tick == _tick)).ToList();

                var removerPoints = drawPoints.Where(p => p.DrawPoints.Any(x => x.Tick == _tick && x.Tool == "undo")).ToList();

                if (removerPoints.Count > 0)
                {
                    foreach (var drawPoint in removerPoints)
                    {
                        foreach (var item in drawPoint.DrawPoints)
                        {
                            Console.WriteLine("Tool =" + item.Tool);
                        }
                    }

                    var index = _currentPlayAllPoints.IndexOf(removerPoints[0]);

                    _currentPlayAllPoints.RemoveAt(index);

                    _currentPlayAllPoints.RemoveAt(index - 1);

                    _mainContext.ClearRectAsync(0, 0, _position.Width, _position.Height);
                    drawPoints = _currentPlayAllPoints.Where(p => p.DrawPoints.Any(x => x.Tick < _tick)).ToList();
                }

                Console.WriteLine(drawPoints.Count);
                ReDraw(drawPoints);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                object[] objects = new object[1];
                object obj = $"let e = document.querySelector('[_bl_{_container.Id}=\"\"]'); e = e.getBoundingClientRect(); e = {{ 'Left': e.x, 'Top': e.y, 'Width':e.width,'Height':e.height  }}; e";
                objects[0] = obj;

                _position = await JS.InvokeAsync<Position>("eval", objects);
                _width = _position.Width.ToString("N0").Replace(",", string.Empty).Replace(".", string.Empty);
                _height = _position.Height.ToString("N0").Replace(",", string.Empty).Replace(".", string.Empty);

                Console.WriteLine("_width = {0} _height = {1}", _width, _height);

                (canvasx, canvasy) = (_position.Left, _position.Top);

                _mainContext = await _mainCanvas.GetContext2DAsync(true);

                //var scale = Math.Min((_position.Width / 2300), (_position.Height / 1800));
                //await _tempContext.JS.scale(scale, scale);
                //await _mainContext.JS.scale(scale, scale);
                //Console.WriteLine("_position.Width = {0} _position.Height = {1} scale = {2}", _position.Width, _position.Height, scale);

                // initialize settings

                await _mainContext.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
                
                //DrawPoint dp = new DrawPoint { StartX = 634, EndX = 757, StartY = 161, EndY = 344, LineColor = "red", FillColor = "green", GlobalAlpha = 1, LineCap = LineCap.Round, LineJoin = LineJoin.Miter, LineWidth = 5 };
                DrawPoint dp = new DrawPoint { StartX = 50, EndX = 250, StartY = 50, EndY = 250, LineColor = "red", FillColor = "green", GlobalAlpha = 1, LineCap = LineCap.Round, LineJoin = LineJoin.Miter, LineWidth = 5 };
                await DrawRectangle(_mainContext, dp, false);

                render_required = false;
                // this retrieves the top left corner of the canvas container (which is equivalent to the top left corner of the canvas, as we don't have any margins / padding)
            }
        }

        protected override bool ShouldRender()
        {
            if (!render_required)
            {
                render_required = true;
                return false;
            }
            return base.ShouldRender();
        }

        private async Task PlayVid()
        {
            _playAllPoints = JsonConvert.DeserializeObject<List<DrawPointList>>(JsonStr);
            DrawPointList[] array = new DrawPointList[_playAllPoints.Count];
            _playAllPoints.CopyTo(array, 0);

            _currentPlayAllPoints = new List<DrawPointList>();
            _currentPlayAllPoints = array.ToList();

            foreach (var points in _currentPlayAllPoints)
            {
                foreach (var point in points.DrawPoints)
                {
                    Console.WriteLine(string.Format("Tick = {0} Tool = {1}", point.Tick, point.Tool));
                }
            }

            await _mainContext.ClearRectAsync(0, 0, _position.Width, _position.Height);

            _tick = 0;
            _play = true;
            _timer.Enabled = true;
            _timer.Start();
        }

        private async Task ReDraw(IList<DrawPointList> allPoints)
        {
            foreach (var points in allPoints)
            {
                foreach (var point in points.DrawPoints)
                {
                    point.LineColor = "orange";

                    if (point.Tool == "pencil")
                    {
                        await DrawLine(_mainContext, point, false);
                    }
                    else if (point.Tool == "marker")
                    {
                        await DrawLine(_mainContext, point, false);
                    }
                    else if (point.Tool == "eraser")
                    {
                        await DrawLine(_mainContext, point, false);
                    }
                    else if (point.Tool == "line")
                    {
                        await DrawLine(_mainContext, point, false);
                    }
                    else if (point.Tool == "arrow")
                    {
                        await DrawArrow(_mainContext, point, false);
                    }
                    else if (point.Tool == "rectangle")
                    {
                        await DrawRectangle(_mainContext, point, false);
                    }
                }
            }
        }

        private async Task Set(Batch2D context, DrawPoint drawPoint)
        {
            await context.LineWidthAsync(drawPoint.LineWidth);
            await context.StrokeStyleAsync(drawPoint.LineColor);
            await context.GlobalAlphaAsync(drawPoint.GlobalAlpha);
            await context.LineCapAsync(drawPoint.LineCap);
            await context.LineJoinAsync(drawPoint.LineJoin);
        }

        private async Task DrawLine(Context2D context, DrawPoint point, bool transfer)
        {
            Console.WriteLine("DrawLine");

            await using (Batch2D batch = context.CreateBatch())
            {
                await Set(batch, point);

                if (transfer)
                    await context.ClearRectAsync(0, 0, _position.Width, _position.Height);

                await batch.BeginPathAsync();
                await batch.MoveToAsync(point.StartX, point.StartY);
                await batch.LineToAsync(point.EndX, point.EndY);
                await batch.StrokeAsync();
            }
        }

        private async Task DrawArrow(Context2D context, DrawPoint point, bool transfer)
        {
            await using (Batch2D batch = context.CreateBatch())
            {
                await Set(batch, point);

                var arrowSize = point.LineWidth;

                var angle = Math.Atan2(point.EndY - point.StartY, point.EndX - point.StartX);

                if (transfer)
                    await batch.ClearRectAsync(0, 0, _position.Width, _position.Height);


                await context.StrokeStyleAsync(point.LineColor);

                await batch.BeginPathAsync();
                await batch.MoveToAsync(point.StartX, point.StartY);
                await batch.LineToAsync(point.EndX, point.EndY);
                await batch.StrokeAsync();

                await batch.BeginPathAsync();

                await batch.MoveToAsync(point.EndX, point.EndY);

                arrowSize = arrowSize * 5;

                if (!string.IsNullOrEmpty(point.FillColor))
                    await batch.StrokeStyleAsync(point.FillColor);

                await batch.LineToAsync(point.EndX - arrowSize * Math.Cos(angle - Math.PI / 7), point.EndY - arrowSize * Math.Sin(angle - Math.PI / 7));

                await batch.LineToAsync(point.EndX - arrowSize * Math.Cos(angle + Math.PI / 7), point.EndY - arrowSize * Math.Sin(angle + Math.PI / 7));

                await batch.LineToAsync(point.EndX, point.EndY);

                await batch.LineToAsync(point.EndX - arrowSize * Math.Cos(angle - Math.PI / 7), point.EndY - arrowSize * Math.Sin(angle - Math.PI / 7));

                await batch.StrokeAsync();
            }
        }

        private async Task DrawRectangle(Context2D context, DrawPoint point, bool transfer)
        {
            double with = point.EndX - point.StartX;
            double height = point.EndY - point.StartY;

            await using (Batch2D batch = context.CreateBatch())
            {
                await Set(batch, point);

                Console.WriteLine("StartX = {0}", point.StartX);
                Console.WriteLine("StartY = {0}", point.StartY);
                Console.WriteLine("EndX = {0}", point.EndX);
                Console.WriteLine("EndY = {0}", point.EndY);
                Console.WriteLine("GlobalAlpha = {0}", point.GlobalAlpha);
                Console.WriteLine("LineColor = {0}", point.LineColor);
                Console.WriteLine("FillColor = {0}", point.FillColor);
                Console.WriteLine("LineWidth = {0}", point.LineWidth);
                Console.WriteLine("LineCap = {0}", point.LineCap);
                Console.WriteLine("LineJoin = {0}", point.LineJoin);

                if (transfer)
                    await batch.ClearRectAsync(0, 0, _position.Width, _position.Height);

                await batch.StrokeStyleAsync(point.LineColor);
                await batch.StrokeRectAsync(point.StartX, point.StartY, with, height);

                if (!string.IsNullOrEmpty(point.FillColor))
                {
                    await batch.FillStyleAsync(point.FillColor);
                    await batch.FillRectAsync(point.StartX, point.StartY, with, height);
                }
            }
        }
    }
}
