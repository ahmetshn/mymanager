using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace BlazorApp1.Pages
{
    public partial class Widget
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

        private IList<DrawPointList> _currentPlayAllPoints;
        private IList<DrawPointList> _playAllPoints;
        private IList<DrawPointList> _reDrawAllPoints;

        private IList<DrawPoint> _points;
        private ElementReference _container;
        private Canvas _tempCanvas;
        private Canvas _mainCanvas;

        private Context2D _tempContext;
        private Context2D _mainContext;
        private Context2D _activeContext;

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

        private bool _transfer;

        private bool _showLineWidth;
        private bool _showLineColor;
        private bool _showFillColor;

        private double _globalAlpha;
        private double _lineWidth;
        private string _lineColor;
        private string _fillColor;
        private LineCap _lineCap { get; set; }
        private LineJoin _lineJoin { get; set; }

        private System.Timers.Timer _timer;
        private long _tick;
        //private PeriodicTimer _timer { get; set; }

        private bool _play;

        protected override void OnInitialized()
        {
            _showLineWidth = true;
            _showLineColor = true;
            _showFillColor = false;
            _tool = "pencil";

            _globalAlpha = 1;
            _lineWidth = 1;
            _lineColor = "black";
            _fillColor = string.Empty;
            _lineCap = LineCap.Round;
            _lineJoin = LineJoin.Miter;

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
            //Console.WriteLine(" Ticks  : {0} ", _tick);
            //Console.WriteLine(" Ticks  : {0} ", e.SignalTime.Ticks - _tick);
            //Console.WriteLine(" Event  : {0} ", e.SignalTime);

            if (_play)
            {
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

                //Console.WriteLine(drawPoints.Count);
                ReDraw(drawPoints);

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

        private DrawPoint GetDrawPoint()
        {
            DrawPoint drawPoint = new DrawPoint();
            drawPoint.Tick = _tick;
            drawPoint.Tool = _tool;
            drawPoint.StartX = mousex;
            drawPoint.StartY = mousey;
            drawPoint.EndX = last_mousex;
            drawPoint.EndY = last_mousey;
            drawPoint.GlobalAlpha = _globalAlpha;
            drawPoint.LineWidth = _lineWidth;
            drawPoint.LineColor = _lineColor;
            drawPoint.FillColor = _fillColor;

            return drawPoint;
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

                (canvasx, canvasy) = (_position.Left, _position.Top);

                _tempContext = await _tempCanvas.GetContext2DAsync(true);
                _mainContext = await _mainCanvas.GetContext2DAsync(true);


                //var scale = Math.Min((_position.Width / 2300), (_position.Height / 1800));
                //await _tempContext.JS.scale(scale, scale);
                //await _mainContext.JS.scale(scale, scale);
                //Console.WriteLine("_position.Width = {0} _position.Height = {1} scale = {2}", _position.Width, _position.Height, scale);

                // initialize settings
                await _tempContext.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
                await _tempContext.StrokeStyleAsync(clr);
                await _tempContext.LineWidthAsync(3);
                await _tempContext.LineJoinAsync(LineJoin.Round);
                await _tempContext.LineCapAsync(LineCap.Round);

                await _mainContext.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
                await _mainContext.StrokeStyleAsync(clr);
                await _mainContext.LineWidthAsync(3);
                await _mainContext.LineJoinAsync(LineJoin.Round);
                await _mainContext.LineCapAsync(LineCap.Round);
                // this retrieves the top left corner of the canvas container (which is equivalent to the top left corner of the canvas, as we don't have any margins / padding)

                _mainCanvas.AdditionalAttributes.Add("style", "z-index:15;");
                _tempCanvas.AdditionalAttributes.Add("style", "z-index:5;");


                _activeContext = _mainContext;
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

        private void MouseDownCanvas(MouseEventArgs e)
        {
            var clientX = e.ClientX;
            var clientY = e.ClientY;

            render_required = false;

            last_mousex = mousex = e.ClientX - canvasx;
            last_mousey = mousey = e.ClientY - canvasy;
            start_mousex = clientX - canvasx;
            start_mousey = clientY - canvasy;

            this.mousedown = true;

            _points = new List<DrawPoint>();
        }

        private async Task MouseUpCanvas(MouseEventArgs e)
        {
            render_required = false;
            mousedown = false;

            if (_transfer)
            {
                if (_tool == "line")
                {
                    _globalAlpha = 1;
                    _lineCap = LineCap.Round;
                    _lineJoin = LineJoin.Round;
                    DrawPoint drawPoint = GetDrawPoint();
                    _points.Add(drawPoint);
                    await DrawLine(_mainContext, drawPoint, false);
                }
                else if (_tool == "arrow")
                {
                    _globalAlpha = 1;
                    _lineCap = LineCap.Round;
                    _lineJoin = LineJoin.Round;
                    DrawPoint drawPoint = GetDrawPoint();
                    _points.Add(drawPoint);
                    await DrawArrow(_mainContext, drawPoint, false);
                }
                else if (_tool == "rectangle")
                {
                    _globalAlpha = 1;
                    _lineCap = LineCap.Square;
                    _lineJoin = LineJoin.Bevel;
                    DrawPoint drawPoint = GetDrawPoint();
                    _points.Add(drawPoint);
                    await DrawRectangle(_mainContext, drawPoint, false);
                }
            }

            await using (var context = _tempContext.CreateBatch())
            {
                await context.ClearRectAsync(0, 0, _position.Width, _position.Height);
            }

            last_mousex = 0;
            last_mousey = 0;

            DrawPointList drawPointList = new DrawPointList();
            drawPointList.DrawPoints = _points;
            _reDrawAllPoints.Add(drawPointList);
            _playAllPoints.Add(drawPointList);
            _points = null;
        }

        private async Task MouseMoveCanvasAsync(MouseEventArgs e)
        {
            render_required = false;
            if (!mousedown)
            {
                return;
            }

            var clientX = e.ClientX;
            var clientY = e.ClientY;

            if (_tool == "pencil")
            {
                mousex = e.ClientX - canvasx;
                mousey = e.ClientY - canvasy;

                _globalAlpha = 1;
                _lineCap = LineCap.Round;
                _lineJoin = LineJoin.Round;

                DrawPoint drawPoint = GetDrawPoint();
                _points.Add(drawPoint);
                await DrawLine(_activeContext, drawPoint, false);

                last_mousex = mousex;
                last_mousey = mousey;
            }
            else if (_tool == "marker")
            {
                mousex = e.ClientX - canvasx;
                mousey = e.ClientY - canvasy;

                _globalAlpha = 0.1;
                _lineCap = LineCap.Round;
                _lineJoin = LineJoin.Round;
                DrawPoint drawPoint = GetDrawPoint();
                _points.Add(drawPoint);
                await DrawLine(_activeContext, drawPoint, false);

                last_mousex = mousex;
                last_mousey = mousey;
            }
            else if (_tool == "eraser")
            {
                mousex = e.ClientX - canvasx;
                mousey = e.ClientY - canvasy;

                _globalAlpha = 1;
                _lineCap = LineCap.Round;
                _lineJoin = LineJoin.Round;
                _lineColor = "white";
                DrawPoint drawPoint = GetDrawPoint();

                _points.Add(drawPoint);
                await DrawLine(_activeContext, drawPoint, false);

                last_mousex = mousex;
                last_mousey = mousey;
            }
            else if (_tool == "line")
            {
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                _globalAlpha = 1;
                _lineCap = LineCap.Round;
                _lineJoin = LineJoin.Round;
                DrawPoint drawPoint = GetDrawPoint();

                await DrawLine(_activeContext, drawPoint, true);
            }
            else if (_tool == "arrow")
            {
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                _globalAlpha = 1;
                _lineCap = LineCap.Round;
                _lineJoin = LineJoin.Round;
                DrawPoint drawPoint = GetDrawPoint();

                await DrawArrow(_activeContext, drawPoint, true);
            }
            else if (_tool == "rectangle")
            {
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                _globalAlpha = 1;
                _lineCap = LineCap.Square;
                _lineJoin = LineJoin.Bevel;
                DrawPoint drawPoint = GetDrawPoint();

                await DrawRectangle(_activeContext, drawPoint, true);
            }

        }

        private async Task ChangeTool(string tool, bool transfer, bool showLineWidth, bool showLineColor, bool showFillColor)
        {
            if (tool == "undo")
            {
                if (_reDrawAllPoints.Count > 0)
                {
                    _reDrawAllPoints.RemoveAt(_reDrawAllPoints.Count - 1);

                    DrawPoint drawPoint = new DrawPoint();
                    drawPoint.Tool = tool;
                    drawPoint.Tick = _tick;

                    DrawPointList drawPointList = new DrawPointList();
                    drawPointList.DrawPoints = new List<DrawPoint>();
                    drawPointList.DrawPoints.Add(drawPoint);

                    _playAllPoints.Add(drawPointList);
                }

                await _mainContext.ClearRectAsync(0, 0, _position.Width, _position.Height);
                await ReDraw(_reDrawAllPoints);
            }
            else
            {
                _tool = tool;
                _transfer = transfer;
                _showLineWidth = showLineWidth;
                _showLineColor = showLineColor;
                _showFillColor = showFillColor;

                _mainCanvas.AdditionalAttributes.Remove("style");
                _tempCanvas.AdditionalAttributes.Remove("style");

                if (transfer)
                {
                    _mainCanvas.AdditionalAttributes.Add("style", "z-index:5;");
                    _tempCanvas.AdditionalAttributes.Add("style", "z-index:15;");

                    _activeContext = _tempContext;
                }
                else
                {
                    _mainCanvas.AdditionalAttributes.Add("style", "z-index:15;");
                    _tempCanvas.AdditionalAttributes.Add("style", "z-index:5;");

                    _activeContext = _mainContext;
                }

                StateHasChanged();
            }
        }

        private async Task ChangeLineWidht(double lineWidth)
        {
            _lineWidth = lineWidth;
        }

        private async Task ChangeLineColor(string lineColor)
        {
            _lineColor = '#' + lineColor;
        }

        private async Task ChangeFillColor(string fillColor)
        {
            _fillColor = '#' + fillColor;
        }

        private async Task Start()
        {
            _timer.Enabled = true;
            _timer.Start();
        }

        private async Task Stop()
        {
            _timer.Enabled = false;
            _timer.Stop();
        }

        private async Task Play()
        {
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

        private async Task Serilaze()
        {
            var str = JsonConvert.SerializeObject(_playAllPoints);
            Console.WriteLine(str);
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", str);
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

        private async Task DrawLine(Context2D context, DrawPoint point, bool transfer)
        {
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