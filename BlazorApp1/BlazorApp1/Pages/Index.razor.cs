using Excubo.Blazor.Canvas;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorApp1.Pages
{
    public partial class Index
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

        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {


            if (firstRender)
            {

                //_context.AdditionalAttributes["width"] = "800";
                //_context.AdditionalAttributes["height"] = "800";

                ctx1 = await _context.GetContext2DAsync();
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

            Console.WriteLine(string.Format("MouseDownCanvas clientX={0} clientY={1}", clientX, clientY));

            render_required = false;
            this.last_mousex = mousex = e.ClientX - canvasx;
            this.last_mousey = mousey = e.ClientY - canvasy;
            start_mousex = last_mousex;
            start_mousey = last_mousey;

            this.mousedown = true;
        }

        private void TouchDownCanvas(TouchEventArgs e)
        {
            Console.WriteLine("TouchDownCanvas");

            TouchPoint touchPoint = e.TargetTouches[0];

            var clientX = touchPoint.ClientX;
            var clientY = touchPoint.ClientY;

            render_required = false;
            last_mousex = mousex = clientX - canvasx;
            last_mousey = mousey = clientY - canvasy;
            start_mousex = last_mousex;
            start_mousey = last_mousey;

            this.mousedown = true;
        }

        private void MouseUpCanvas(MouseEventArgs e)
        {
            render_required = false;
            mousedown = false;

            last_mousex = 0;
            last_mousey = 0;
        }

        private void TouchUpCanvas(TouchEventArgs e)
        {
            Console.WriteLine("TouchUpCanvas");

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
            mousex = e.ClientX - canvasx;
            mousey = e.ClientY - canvasy;
            await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);
            last_mousex = mousex;
            last_mousey = mousey;
        }

        async Task TouchMoveCanvasAsync(TouchEventArgs e)
        {
            TouchPoint touchPoint = e.TargetTouches[e.TargetTouches.Length - 1];

            var clientX = touchPoint.ClientX;
            var clientY = touchPoint.ClientY;

            Console.WriteLine("TouchMoveCanvasAsync");

            Console.WriteLine($"clientX = {clientX} clientY = {clientY} canvasx={canvasx} canvasy={canvasy}");

            render_required = false;
            if (!mousedown)
            {
                return;
            }
            mousex = clientX - canvasx;
            mousey = clientY - canvasy;
            await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);

            last_mousex = mousex;
            last_mousey = mousey;
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

                    await ctx2.StrokeRectAsync(start_mousex, start_mousey, x, y);
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
