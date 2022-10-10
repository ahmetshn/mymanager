using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Timers;

namespace BlazorApp1.Pages
{
    public partial class Widget
    {
        [Inject]
        public IJSRuntime JS { get; set; }

        private IList<DrawPointList> _currentPlayAllPoints;
        private IList<DrawPointList> _playAllPoints;
        private IList<DrawPointList> _reDrawAllPoints;

        private DrawPointList _drawPointList;
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

        private PeriodicTimer _timerRecorder;
        private PeriodicTimer _timerPlayer;
        private TimeSpan _tick;
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

            _tick = TimeSpan.Zero;
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

            _drawPointList = new DrawPointList();
            _drawPointList.Tick = _tick;
            _drawPointList.Tool = _tool;

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
                    await DrawHelper.DrawLine(_mainContext, drawPoint, _position.Width, _position.Height, false);
                }
                else if (_tool == "arrow")
                {
                    _globalAlpha = 1;
                    _lineCap = LineCap.Round;
                    _lineJoin = LineJoin.Round;
                    DrawPoint drawPoint = GetDrawPoint();
                    _points.Add(drawPoint);
                    await DrawHelper.DrawArrow(_mainContext, drawPoint, _position.Width, _position.Height, false);
                }
                else if (_tool == "rectangle")
                {
                    _globalAlpha = 1;
                    _lineCap = LineCap.Square;
                    _lineJoin = LineJoin.Bevel;
                    DrawPoint drawPoint = GetDrawPoint();
                    _points.Add(drawPoint);
                    await DrawHelper.DrawRectangle(_mainContext, drawPoint, _position.Width, _position.Height, false);
                }
            }

            await using (var context = _tempContext.CreateBatch())
            {
                await context.ClearRectAsync(0, 0, _position.Width, _position.Height);
            }

            last_mousex = 0;
            last_mousey = 0;

            _drawPointList.DrawPoints = _points;
            _reDrawAllPoints.Add(_drawPointList);
            _playAllPoints.Add(_drawPointList);
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
                await DrawHelper.DrawLine(_activeContext, drawPoint, _position.Width, _position.Height, false);

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
                await DrawHelper.DrawLine(_activeContext, drawPoint, _position.Width, _position.Height, false);

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
                await DrawHelper.DrawLine(_activeContext, drawPoint, _position.Width, _position.Height, false);

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

                await DrawHelper.DrawLine(_activeContext, drawPoint, _position.Width, _position.Height, true);
            }
            else if (_tool == "arrow")
            {
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                _globalAlpha = 1;
                _lineCap = LineCap.Round;
                _lineJoin = LineJoin.Round;
                DrawPoint drawPoint = GetDrawPoint();

                await DrawHelper.DrawArrow(_activeContext, drawPoint, _position.Width, _position.Height, true);
            }
            else if (_tool == "rectangle")
            {
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                _globalAlpha = 1;
                _lineCap = LineCap.Square;
                _lineJoin = LineJoin.Bevel;
                DrawPoint drawPoint = GetDrawPoint();

                await DrawHelper.DrawRectangle(_activeContext, drawPoint, _position.Width, _position.Height, true);
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
                    drawPoint.Tick = _tick;
                    drawPointList.Tick = _tick;
                    drawPointList.Tool = "undo";

                    _playAllPoints.Add(drawPointList);
                }

                await _mainContext.ClearRectAsync(0, 0, _position.Width, _position.Height);
                await DrawHelper.ReDraw(_mainContext, _reDrawAllPoints, _position.Width, _position.Height);
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
            StartTimer();
            await JS.InvokeVoidAsync("startRecording");
        }

        private async Task Stop()
        {
            StopTimer();
            await JS.InvokeVoidAsync("stopRecording");

            //_timer.Enabled = false;
            //_timer.Stop();

        }

        private List<DrawPoint> dps;
        private TimeSpan _befoereTimeSpan;

        private async Task Play()
        {
            _befoereTimeSpan = TimeSpan.Zero;

            DrawPointList[] array = new DrawPointList[_playAllPoints.Count];
            _playAllPoints.CopyTo(array, 0);

            _currentPlayAllPoints = new List<DrawPointList>();
            _currentPlayAllPoints = array.ToList();

            dps = new List<DrawPoint>();

            foreach (var item in _currentPlayAllPoints)
            {
                dps.AddRange(item.DrawPoints);
            }

            await _mainContext.ClearRectAsync(0, 0, _position.Width, _position.Height);

            await JS.InvokeVoidAsync("playSound");

            _tick = TimeSpan.Zero;
            startTime = DateTime.Now;
            timerPlay = new System.Timers.Timer(1);
            timerPlay.Elapsed += OnPlayTimedEvent;
            timerPlay.AutoReset = true;
            timerPlay.Enabled = true;
            isRunning = true;
        }

        private async Task Serilaze()
        {
            var str = JsonConvert.SerializeObject(_playAllPoints);
            Console.WriteLine(str);
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", str);
        }


        const string DEFAULT_TIME = "00:00:00";
        string elapsedTime = DEFAULT_TIME;
        System.Timers.Timer timerRecord = new System.Timers.Timer(1);
        System.Timers.Timer timerPlay = new System.Timers.Timer(1);
        DateTime startTime = DateTime.Now;
        bool isRunning = false;

        private void OnRecordTimedEvent(Object source, ElapsedEventArgs e)
        {
            DateTime currentTime = e.SignalTime;
            _tick = currentTime.Subtract(startTime);
            elapsedTime = $"{_tick}";
            StateHasChanged();
        }

        void StartTimer()
        {
            startTime = DateTime.Now;
            timerRecord = new System.Timers.Timer(1);
            timerRecord.Elapsed += OnRecordTimedEvent;
            timerRecord.AutoReset = true;
            timerRecord.Enabled = true;
            isRunning = true;
        }

        void StopTimer()
        {
            isRunning = false;
            Console.WriteLine($"Elapsed Time: {elapsedTime}");
            timerRecord.Enabled = false;
            //elapsedTime = DEFAULT_TIME;
            timerPlay.Enabled = false;
        }



        private void OnPlayTimedEvent(Object source, ElapsedEventArgs e)
        {

            DateTime currentTime = e.SignalTime;
            _tick = currentTime.Subtract(startTime);
            elapsedTime = $"{_tick}";


            IList<DrawPoint> drawPointList = dps.Where(p => p.Tick >= _befoereTimeSpan && p.Tick <= _tick).ToList();

            DrawHelper.ReDraw(_mainContext, new DrawPointList { DrawPoints = drawPointList }, _position.Width, _position.Height);

            _befoereTimeSpan = _tick;
            StateHasChanged();
        }
    }
}