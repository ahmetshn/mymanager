using Excubo.Blazor.Canvas.Contexts;

namespace BlazorApp1
{
    public static class DrawHelper
    {
        private static async Task Set(Batch2D context, DrawPoint drawPoint)
        {
            await context.LineWidthAsync(drawPoint.LineWidth);
            await context.StrokeStyleAsync(drawPoint.LineColor);
            await context.GlobalAlphaAsync(drawPoint.GlobalAlpha);
            await context.LineCapAsync(drawPoint.LineCap);
            await context.LineJoinAsync(drawPoint.LineJoin);
        }

        public static async Task ReDraw(Context2D context, DrawPointList allPoints, double width, double height)
        {
            int counter = 0;

            foreach (var point in allPoints.DrawPoints)
            {
                counter++;
                //point.LineColor = "orange";

                if (point.Tool == "pencil")
                {
                    await DrawLine(context, point, width, height, false);
                }
                else if (point.Tool == "marker")
                {
                    await DrawLine(context, point, width, height, false);
                }
                else if (point.Tool == "eraser")
                {
                    await DrawLine(context, point, width, height, false);
                }
                else if (point.Tool == "line")
                {
                    await DrawLine(context, point, width, height, false);
                }
                else if (point.Tool == "arrow")
                {
                    await DrawArrow(context, point, width, height, false);
                }
                else if (point.Tool == "rectangle")
                {
                    await DrawRectangle(context, point, width, height, false);
                }
                else if (point.Tool == "undo")
                {
                    DrawPointList dpl = new DrawPointList();
                    dpl.Tick = allPoints.Tick;
                    dpl.DrawPoints = new List<DrawPoint>();

                    var tmp = 0;

                    foreach (var item in allPoints.DrawPoints)
                    {
                        tmp++;

                        if (tmp == (counter - 1))
                            break;

                        Console.WriteLine("item.Tool = {0}", item.Tool);
                        dpl.DrawPoints.Add(item);
                    }

                    Console.WriteLine("point.Tool = {0}", point.Tool);
                    Console.WriteLine("allPoints.DrawPoints.Count = {0}", allPoints.DrawPoints.Count);
                    Console.WriteLine("dpl.DrawPoints.Count = {0}", dpl.DrawPoints.Count);

                    await context.ClearRectAsync(0, 0, width, height);
                    await ReDraw(context, dpl, width, height);

                    //point.LineColor = "white";

                    //allPoints.DrawPoints.Where(p => p.Tick < point.Tick && p.Tool != "undo").OrderByDescending(p=>p.Tick).FirstOrDefault();

                    //await DrawRectangle(context, point, width, height, false);
                }
            }
        }

        public static async Task ReDraw(Context2D context, IList<DrawPointList> allPoints, double width, double height)
        {
            foreach (var points in allPoints)
            {
                foreach (var point in points.DrawPoints)
                {
                    //point.LineColor = "orange";

                    if (point.Tool == "pencil")
                    {
                        await DrawLine(context, point, width, height, false);
                    }
                    else if (point.Tool == "marker")
                    {
                        await DrawLine(context, point, width, height, false);
                    }
                    else if (point.Tool == "eraser")
                    {
                        await DrawLine(context, point, width, height, false);
                    }
                    else if (point.Tool == "line")
                    {
                        await DrawLine(context, point, width, height, false);
                    }
                    else if (point.Tool == "arrow")
                    {
                        await DrawArrow(context, point, width, height, false);
                    }
                    else if (point.Tool == "rectangle")
                    {
                        await DrawRectangle(context, point, width, height, false);
                    }
                }
            }
        }

        public static async Task DrawLine(Context2D context, DrawPoint point, double width, double height, bool transfer)
        {
            await using (Batch2D batch = context.CreateBatch())
            {
                await Set(batch, point);

                if (transfer)
                    await context.ClearRectAsync(0, 0, width, height);

                await batch.BeginPathAsync();
                await batch.MoveToAsync(point.StartX, point.StartY);
                await batch.LineToAsync(point.EndX, point.EndY);
                await batch.StrokeAsync();
            }
        }

        public static async Task DrawArrow(Context2D context, DrawPoint point, double width, double height, bool transfer)
        {
            await using (Batch2D batch = context.CreateBatch())
            {
                await Set(batch, point);

                var arrowSize = point.LineWidth;

                var angle = Math.Atan2(point.EndY - point.StartY, point.EndX - point.StartX);

                if (transfer)
                    await batch.ClearRectAsync(0, 0, width, height);


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

        public static async Task DrawRectangle(Context2D context, DrawPoint point, double width, double height, bool transfer)
        {
            await using (Batch2D batch = context.CreateBatch())
            {
                await Set(batch, point);

                if (transfer)
                    await batch.ClearRectAsync(0, 0, width, height);

                //double with = point.EndX - point.StartX;
                //double height = point.EndY - point.StartY;

                width = point.EndX - point.StartX;
                height = point.EndY - point.StartY;

                await batch.StrokeStyleAsync(point.LineColor);
                await batch.StrokeRectAsync(point.StartX, point.StartY, width, height);

                if (!string.IsNullOrEmpty(point.FillColor))
                {
                    await batch.FillStyleAsync(point.FillColor);
                    await batch.FillRectAsync(point.StartX, point.StartY, width, height);
                }
            }
        }

        public static async Task DrawImage(Context2D context, DrawPoint point, double width, double height,string image, bool transfer)
        {
            await using (Batch2D batch = context.CreateBatch())
            {
                await Set(batch, point);

                if (transfer)
                    await batch.ClearRectAsync(0, 0, width, height);

                await batch.DrawImageAsync(image, width, height);
            }
        }
    }
}
