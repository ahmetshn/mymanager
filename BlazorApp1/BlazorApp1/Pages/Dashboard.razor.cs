using Excubo.Blazor.Canvas;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Drawing;
using System.Reflection;

namespace BlazorApp1.Pages
{
    public partial class Dashboard
    {
        private ElementReference container;
        private Canvas _context;
        private Excubo.Blazor.Canvas.Contexts.Context2D ctx1;
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

        private class Position
        {
            public double Left { get; set; }
            public double Top { get; set; }
        }

        private async Task ToggleColorAsync()
        {
            clr = clr == "black" ? "red" : "black";
            await ctx1.StrokeStyleAsync(clr);
        }

        private async Task ChangeTool(string tool)
        {
            _tool = tool;
        }

        protected override void OnInitialized()
        {
            _width = "800";
            _height = "800";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                ctx1 = await _context.GetContext2DAsync(alpha: true);

                // initialize settings
                await ctx1.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
                await ctx1.StrokeStyleAsync(clr);
                await ctx1.LineWidthAsync(3);
                await ctx1.LineJoinAsync(LineJoin.Round);
                await ctx1.LineCapAsync(LineCap.Round);
                // this retrieves the top left corner of the canvas container (which is equivalent to the top left corner of the canvas, as we don't have any margins / padding)

                object[] objects = new object[1];
                object obj = $"let e = document.querySelector('[_bl_{container.Id}=\"\"]'); e = e.getBoundingClientRect(); e = {{ 'Left': e.x, 'Top': e.y }}; e";
                objects[0] = obj;

                var p = await js.InvokeAsync<Position>("eval", objects);

                (canvasx, canvasy) = (p.Left, p.Top);
            }
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

        private void MouseUpCanvas(MouseEventArgs e)
        {
            render_required = false;
            mousedown = false;

            last_mousex = 0;
            last_mousey = 0;
        }

        async Task MouseMoveCanvasAsync(MouseEventArgs e)
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
                //await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));

                await using (var ctx2 = ctx1.CreateBatch())
                {
                    await ctx2.BeginPathAsync();
                    await ctx2.MoveToAsync(mousex, mousey);
                    await ctx2.LineToAsync(last_mousex, last_mousey);
                    await ctx2.StrokeAsync();
                }

                last_mousex = mousex;
                last_mousey = mousey;
            }
            else if (_tool == "rectangle")
            {
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                double with = last_mousex - start_mousex;
                double height = last_mousey - start_mousey;

                await using (var ctx2 = ctx1.CreateBatch())
                {
                    await ctx2.ClearRectAsync(0, 0, 800, 800);
                    await ctx2.StrokeStyleAsync("red");
                    await ctx2.StrokeRectAsync(start_mousex, start_mousey, with, height);
                }
            }
            else if (_tool == "line")
            {
                //mousex = e.ClientX - canvasx;
                //mousey = e.ClientY - canvasy;
                //await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;



                await using (var ctx2 = ctx1.CreateBatch())
                {

                    await ctx2.ClearRectAsync(0, 0, 800, 800);
                    await ctx2.BeginPathAsync();
                    await ctx2.MoveToAsync(mousex, mousey);
                    await ctx2.LineWidthAsync(20);
                    await ctx2.LineToAsync(last_mousex, last_mousey);
                    // await ctx2.LineToAsync(100, 200);
                    await ctx2.StrokeAsync();
                }

                //last_mousex = mousex;
                //last_mousey = mousey;
            }
            else if (_tool == "arrowz")
            {
                //mousex = e.ClientX - canvasx;
                //mousey = e.ClientY - canvasy;
                //await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                await using (var ctx2 = ctx1.CreateBatch())
                {
                    //var mx = point[0];
                    //var my = point[1];

                    //var lx = point[2];
                    //var ly = point[3];

                    //var arrowSize = arrowHandler.arrowSize;

                    //if (arrowSize == 10)
                    //{
                    //    arrowSize = (options ? options[0] : lineWidth) * 5;
                    //}

                    //var angle = Math.atan2(ly - my, lx - mx);

                    //context.beginPath();
                    //context.moveTo(mx, my);
                    //context.lineTo(lx, ly);

                    //this.handleOptions(context, options);

                    //context.beginPath();
                    //context.moveTo(lx, ly);
                    //context.lineTo(lx - arrowSize * Math.cos(angle - Math.PI / 7), ly - arrowSize * Math.sin(angle - Math.PI / 7));
                    //context.lineTo(lx - arrowSize * Math.cos(angle + Math.PI / 7), ly - arrowSize * Math.sin(angle + Math.PI / 7));
                    //context.lineTo(lx, ly);
                    //context.lineTo(lx - arrowSize * Math.cos(angle - Math.PI / 7), ly - arrowSize * Math.sin(angle - Math.PI / 7));

                    //this.handleOptions(context, options);

                    string color = string.Empty;

                    var arrowSize = 25;

                    var angle = Math.Atan2(last_mousey - mousey, last_mousex - mousex);

                    await ctx2.ClearRectAsync(0, 0, 800, 800);
                    await ctx2.BeginPathAsync();
                    await ctx2.MoveToAsync(mousex, mousey);
                    await ctx2.LineWidthAsync(arrowSize);
                    await ctx2.LineCapAsync(LineCap.Square);
                    await ctx2.LineJoinAsync(LineJoin.Miter);

                    color = "yellow";
                    await ctx2.StrokeStyleAsync(color);
                    Console.WriteLine(color);
                    await ctx2.LineToAsync(last_mousex, last_mousey);
                    await ctx2.StrokeAsync();

                    //context.lineTo(lx - arrowSize * Math.cos(angle - Math.PI / 7), ly - arrowSize * Math.sin(angle - Math.PI / 7));
                    color = "red";
                    await ctx2.StrokeStyleAsync(color);
                    Console.WriteLine(color);
                    await ctx2.LineToAsync(last_mousex - arrowSize * Math.Cos(angle - Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle - Math.PI / 7));
                    await ctx2.StrokeAsync();
                    return;


                    //context.lineTo(lx - arrowSize * Math.cos(angle + Math.PI / 7), ly - arrowSize * Math.sin(angle + Math.PI / 7));
                    color = "aqua";
                    await ctx2.StrokeStyleAsync(color);
                    Console.WriteLine(color);
                    await ctx2.LineToAsync(last_mousex - arrowSize * Math.Cos(angle + Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle + Math.PI / 7));
                    await ctx2.StrokeAsync();
                    return;

                    color = GetColor(color);
                    await ctx2.StrokeStyleAsync(color);
                    Console.WriteLine(color);
                    await ctx2.LineToAsync(last_mousex, last_mousey);
                    //context.lineTo(lx - arrowSize * Math.cos(angle - Math.PI / 7), ly - arrowSize * Math.sin(angle - Math.PI / 7));
                    await ctx2.StrokeAsync();

                    color = GetColor(color);
                    await ctx2.StrokeStyleAsync(color);
                    Console.WriteLine(color);
                    await ctx2.LineToAsync(last_mousex - arrowSize * Math.Cos(angle - Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle - Math.PI / 7));
                    await ctx2.StrokeAsync();
                }

                //last_mousex = mousex;
                //last_mousey = mousey;
            }
            else if (_tool == "arrow")
            {
                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));

                //mousex = e.ClientX - canvasx;
                //mousey = e.ClientY - canvasy;
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                string color = string.Empty;
                color = GetColor(color);

                await using (var context = ctx1.CreateBatch())
                {
                    //var mx = point[0];
                    //var my = point[1];

                    //var lx = point[2];
                    //var ly = point[3];

                    var arrowSize = 10;

                    //if (arrowSize == 10)
                    //{
                    //    arrowSize = (options ? options[0] : lineWidth) * 5;
                    //}

                    await context.LineWidthAsync(arrowSize);
                    await context.LineCapAsync(LineCap.Round);
                    await context.LineJoinAsync(LineJoin.Round);

                    var angle = Math.Atan2(last_mousey - mousey, last_mousex - mousex);
                    await context.ClearRectAsync(0, 0, 800, 800);

                    await context.BeginPathAsync();
                    await context.MoveToAsync(mousex, mousey);
                    await context.LineToAsync(last_mousex, last_mousey);
                    await context.StrokeAsync();

                    //this.handleOptions(context, options);

                    await context.BeginPathAsync();

                    await context.MoveToAsync(last_mousex, last_mousey);

                    arrowSize = arrowSize * 5;

                    await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle - Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle - Math.PI / 7));

                    await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle + Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle + Math.PI / 7));

                    await context.LineToAsync(last_mousex, last_mousey);

                    await context.LineToAsync(last_mousex - arrowSize * Math.Cos(angle - Math.PI / 7), last_mousey - arrowSize * Math.Sin(angle - Math.PI / 7));

                    //this.handleOptions(context, options);
                    await context.FillAsync(FillRule.EvenOdd);
                    await context.StrokeAsync();
                }
            }
            else if (_tool == "maker")
            {
                mousex = e.ClientX - canvasx;
                mousey = e.ClientY - canvasy;
                //await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));

                await using (var ctx2 = ctx1.CreateBatch())
                {
                    await ctx2.LineStyles.LineCapAsync(LineCap.Round);
                    await ctx2.LineStyles.LineJoinAsync(LineJoin.Round);
                    await ctx2.LineStyles.LineWidthAsync(50);

                    await ctx2.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
                    await ctx2.GlobalAlphaAsync(0.1);
                    int _radius = 150;
                    //await ctx2.StrokeStyleAsync(0, 0, _radius * 0.95, 0, 0, _radius * 1.05, (0.0, "#333"), (0.5, "white"), (1, "#333"));

                    await ctx2.StrokeStyleAsync("yellow");

                    await ctx2.BeginPathAsync();
                    await ctx2.MoveToAsync(mousex, mousey);
                    await ctx2.LineToAsync(last_mousex, last_mousey);
                    await ctx2.StrokeAsync();
                }

                last_mousex = mousex;
                last_mousey = mousey;
            }
            else if (_tool == "eraser")
            {
                //mousex = e.ClientX - canvasx;
                //mousey = e.ClientY - canvasy;
                //await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

                Console.WriteLine(string.Format("mousex={0} mousey={1} last_mousex={2} last_mousey={3}", mousex, mousey, last_mousex, last_mousey));
                last_mousex = clientX - canvasx;
                last_mousey = clientY - canvasy;

                await using (var ctx2 = ctx1.CreateBatch())
                {
                    await ctx2.StrokeStyleAsync("white");
                    await ctx2.BeginPathAsync();
                    await ctx2.MoveToAsync(mousex, mousey);
                    await ctx2.LineWidthAsync(20);
                    await ctx2.LineToAsync(last_mousex, last_mousey);
                    // await ctx2.LineToAsync(100, 200);
                    await ctx2.StrokeAsync();
                }
            }
        }

        private string GetColor(string color)
        {
            IList<string> colors = new List<string>();
            colors.Add("Red");
            colors.Add("Blue");
            colors.Add("Yellow");
            colors.Add("Orange");
            colors.Add("Green");

            //Random rnd = new Random();
            //var index = rnd.Next(0, colors.Count);

            //return colors[index];

            if (!string.IsNullOrEmpty(color))
                colors = colors.Where(x => x != color).ToList();

            return colors.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        }

        async Task DrawCanvasAsync(double prev_x, double prev_y, double x, double y, string clr)
        {
            if (_tool == "pencil")
            {
                await using (var ctx2 = ctx1.CreateBatch())
                {
                    await ctx2.BeginPathAsync();
                    await ctx2.MoveToAsync(prev_x, prev_y);
                    await ctx2.LineToAsync(x, y);
                    await ctx2.StrokeAsync();
                }
            }
            else if (_tool == "rectangle")
            {
                await using (var ctx2 = ctx1.CreateBatch())
                {
                    await ctx2.ClearRectAsync(0, 0, 800, 800);
                    //   var x = e.pageX - canvas.offsetLeft,
                    // y = e.pageY - canvas.offsetTop;

                    Console.WriteLine(string.Format("start_mousex = {0} - start_mousey = {1} - x = {2} - y = {3}", start_mousex, start_mousey, x, y));

                    await ctx2.StrokeRectAsync(start_mousex, start_mousey, start_mousex + x, start_mousey + y);
                }
            }
        }

        private bool render_required = true;
        protected override bool ShouldRender()
        {
            if (!render_required)
            {
                render_required = true;
                return false;
            }
            return base.ShouldRender();
        }
    }
}
