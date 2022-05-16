﻿using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

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

            //start_mousex = clientX - canvasx;
            //start_mousey = clientY - canvasy;


            render_required = false;

            this.last_mousex = mousex = e.ClientX - canvasx;
            this.last_mousey = mousey = e.ClientY - canvasy;
            start_mousex = clientX - canvasx;
            start_mousey = clientY - canvasy;


            this.mousedown = true;

            Console.WriteLine(string.Format("Page clientX={0} clientY={1}", clientX, clientY));
            Console.WriteLine(string.Format("Canvas start_mousex={0} start_mousey={1}", start_mousex, start_mousey));


            //var clientX = e.ClientX;
            //var clientY = e.ClientY;

            //Console.WriteLine(string.Format("MouseDownCanvas clientX={0} clientY={1}", clientX, clientY));

            //render_required = false;
            //this.last_mousex = mousex = e.ClientX - canvasx;
            //this.last_mousey = mousey = e.ClientY - canvasy;
            //start_mousex = last_mousex;
            //start_mousey = last_mousey;

            //this.mousedown = true;
        }

        private async Task MouseUpCanvas(MouseEventArgs e)
        {
            render_required = false;
            mousedown = false;

            Console.WriteLine("-----------------------------");
            Console.WriteLine(string.Format("transfer={0} _tool= {1}", _transfer, _tool));

            if (_transfer)
            {
                if (_tool == "line")
                {

                    // Kalınlık ve renk ayarlarınından sonra bu kısıım yapılacak

                    Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));
                    //last_mousex = clientX - canvasx;
                    //last_mousey = clientY - canvasy;

                    await using (var context = _mainContext.CreateBatch())
                    {
                        await context.LineWidthAsync(_lineWidth);
                        await context.StrokeStyleAsync(_lineColor);
                        await context.GlobalAlphaAsync(_globalAlpha);

                        await context.BeginPathAsync();
                        await context.MoveToAsync(mousex, mousey);
                        await context.LineToAsync(last_mousex, last_mousey);
                        await context.StrokeAsync();
                    }
                }
                else if (_tool == "arrow")
                {
                    //Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));

                    await using (var context = _mainContext.CreateBatch())
                    {
                        var arrowSize = _lineWidth;

                        await context.LineWidthAsync(_lineWidth);
                        await context.StrokeStyleAsync(_lineColor);
                        await context.GlobalAlphaAsync(_globalAlpha);
                        await context.LineCapAsync(LineCap.Round);
                        await context.LineJoinAsync(LineJoin.Round);

                        var angle = Math.Atan2(last_mousey - mousey, last_mousex - mousex);

                        await context.BeginPathAsync();
                        await context.MoveToAsync(mousex, mousey);
                        await context.LineToAsync(last_mousex, last_mousey);
                        await context.StrokeAsync();

                        await context.BeginPathAsync();

                        await context.MoveToAsync(last_mousex, last_mousey);

                        arrowSize = arrowSize * 5;

                        if (!string.IsNullOrEmpty(_fillColor))
                            await context.StrokeStyleAsync(_fillColor);

                        await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle - Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle - Math.PI / 7));

                        await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle + Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle + Math.PI / 7));

                        await context.LineToAsync(last_mousex, last_mousey);

                        await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle - Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle - Math.PI / 7));

                        //this.handleOptions(context, options);

                        await context.StrokeAsync();
                    }
                }
                else if (_tool == "rectangle")
                {
                    double with = last_mousex - start_mousex;
                    double height = last_mousey - start_mousey;

                    await using (var context = _mainContext.CreateBatch())
                    {
                        await context.LineWidthAsync(_lineWidth);

                        await context.GlobalAlphaAsync(_globalAlpha);
                        await context.LineCapAsync(LineCap.Square);
                        await context.LineJoinAsync(LineJoin.Miter);

                        await context.StrokeStyleAsync(_lineColor);
                        await context.StrokeRectAsync(start_mousex, start_mousey, with, height);

                        await context.FillStyleAsync(_fillColor);
                        await context.FillRectAsync(start_mousex, start_mousey, with, height);
                    }
                }


            }

            //await using (var context = _tempContext.CreateBatch())
            //{
            //    await context.ClearRectAsync(0, 0, _position.Width, _position.Height);
            //}

            last_mousex = 0;
            last_mousey = 0;
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

            Console.WriteLine(string.Format("_tool = {0}", _tool));

            if (_tool == "pencil")
            {
                mousex = e.ClientX - canvasx;
                mousey = e.ClientY - canvasy;
                //await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));

                await using (var context = _activeContext.CreateBatch())
                {
                    await context.LineWidthAsync(_lineWidth);
                    await context.StrokeStyleAsync(_lineColor);
                    await context.GlobalAlphaAsync(_globalAlpha);

                    await context.BeginPathAsync();
                    await context.StrokeStyleAsync(_lineColor);
                    await context.MoveToAsync(mousex, mousey);
                    await context.LineToAsync(last_mousex, last_mousey);
                    await context.StrokeAsync();
                }

                last_mousex = mousex;
                last_mousey = mousey;
            }
            else if (_tool == "marker")
            {
                mousex = e.ClientX - canvasx;
                mousey = e.ClientY - canvasy;

                //Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));

                await using (var context = _activeContext.CreateBatch())
                {
                    await context.LineWidthAsync(_lineWidth);
                    await context.StrokeStyleAsync(_lineColor);

                    await context.LineStyles.LineCapAsync(LineCap.Round);
                    await context.LineStyles.LineJoinAsync(LineJoin.Round);

                    await context.GlobalAlphaAsync(0.1);

                    await context.BeginPathAsync();
                    await context.MoveToAsync(mousex, mousey);
                    await context.LineToAsync(last_mousex, last_mousey);
                    await context.StrokeAsync();
                }

                last_mousex = mousex;
                last_mousey = mousey;
            }
            else if (_tool == "eraser")
            {
                mousex = e.ClientX - canvasx;
                mousey = e.ClientY - canvasy;
                //await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));

                await using (var context = _activeContext.CreateBatch())
                {
                    await context.LineWidthAsync(_lineWidth);
                    await context.StrokeStyleAsync(_lineColor);
                    await context.GlobalAlphaAsync(_globalAlpha);

                    await context.BeginPathAsync();
                    await context.StrokeStyleAsync("white");
                    await context.MoveToAsync(mousex, mousey);
                    await context.LineToAsync(last_mousex, last_mousey);
                    await context.StrokeAsync();
                }

                last_mousex = mousex;
                last_mousey = mousey;
            }
            else if (_tool == "line")
            {
                //mousex = e.ClientX - canvasx;
                //mousey = e.ClientY - canvasy;
                //await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                await using (var context = _activeContext.CreateBatch())
                {
                    await context.LineWidthAsync(_lineWidth);
                    await context.StrokeStyleAsync(_lineColor);
                    await context.GlobalAlphaAsync(_globalAlpha);

                    await context.ClearRectAsync(0, 0, _position.Width, _position.Height);
                    await context.BeginPathAsync();
                    await context.MoveToAsync(mousex, mousey);
                    await context.LineToAsync(last_mousex, last_mousey);
                    await context.StrokeAsync();
                }

                //last_mousex = mousex;
                //last_mousey = mousey;
            }
            else if (_tool == "arrow")
            {
                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));

                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                await using (var context = _activeContext.CreateBatch())
                {
                    var arrowSize = _lineWidth;

                    await context.LineWidthAsync(_lineWidth);
                    await context.StrokeStyleAsync(_lineColor);
                    await context.GlobalAlphaAsync(_globalAlpha);
                    await context.LineCapAsync(LineCap.Round);
                    await context.LineJoinAsync(LineJoin.Round);

                    var angle = Math.Atan2(last_mousey - mousey, last_mousex - mousex);
                    await context.ClearRectAsync(0, 0, _position.Width, _position.Height);

                    await context.BeginPathAsync();
                    await context.MoveToAsync(mousex, mousey);
                    await context.LineToAsync(last_mousex, last_mousey);
                    await context.StrokeAsync();

                    //this.handleOptions(context, options);

                    await context.BeginPathAsync();

                    await context.MoveToAsync(last_mousex, last_mousey);

                    arrowSize = arrowSize * 5;

                    if (!string.IsNullOrEmpty(_fillColor))
                        await context.StrokeStyleAsync(_fillColor);

                    await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle - Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle - Math.PI / 7));

                    await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle + Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle + Math.PI / 7));

                    await context.LineToAsync(last_mousex, last_mousey);

                    await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle - Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle - Math.PI / 7));

                    await context.StrokeAsync();
                }
            }
            else if (_tool == "rectangle")
            {
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                double with = last_mousex - start_mousex;
                double height = last_mousey - start_mousey;

                await using (var context = _activeContext.CreateBatch())
                {
                    await context.LineWidthAsync(_lineWidth);

                    await context.GlobalAlphaAsync(_globalAlpha);
                    await context.LineCapAsync(LineCap.Square);
                    await context.LineJoinAsync(LineJoin.Miter);

                    await context.ClearRectAsync(0, 0, _position.Width, _position.Height);

                    await context.StrokeStyleAsync(_lineColor);
                    await context.StrokeRectAsync(start_mousex, start_mousey, with, height);

                    await context.FillStyleAsync(_fillColor);
                    await context.FillRectAsync(start_mousex, start_mousey, with, height);
                }
            }
            else if (_tool == "undo")
            {

                ////mousex = e.ClientX - canvasx;
                ////mousey = e.ClientY - canvasy;
                ////await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                //Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));
                //last_mousex = clientX - canvasx;
                //last_mousey = clientY - canvasy;

                //await using (var ctx2 = _activeContext.CreateBatch())
                //{
                //    await ctx2.StrokeStyleAsync("white");
                //    await ctx2.BeginPathAsync();
                //    await ctx2.MoveToAsync(mousex, mousey);
                //    await ctx2.LineWidthAsync(20);
                //    await ctx2.LineToAsync(last_mousex, last_mousey);
                //    // await ctx2.LineToAsync(100, 200);
                //    await ctx2.StrokeAsync();
                //}
            }
        }

        private async Task ChangeTool(string tool, bool transfer, bool showLineWidth, bool showLineColor, bool showFillColor)
        {
            _tool = tool;
            _transfer = transfer;
            _showLineWidth = showLineWidth;
            _showLineColor = showLineColor;
            _showFillColor = showFillColor;

            _mainCanvas.AdditionalAttributes.Remove("style");
            _tempCanvas.AdditionalAttributes.Remove("style");

            //if (tool == "line" || tool == "arrow" || tool == "rectangle")

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
    }
}
