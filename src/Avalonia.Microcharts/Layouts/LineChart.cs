﻿using System.Linq;
using SkiaSharp;

namespace Avalonia.Microcharts
{
    public class LineChart : PointChart
    {
        public LineChart()
        {
            PointSize = 10;
        }

        /// <summary>
        /// Gets or sets the size of the line.
        /// </summary>
        /// <value>The size of the line.</value>
        public float LineSize { get; set; } = 3;

        /// <summary>
        /// Gets or sets the line mode.
        /// </summary>
        /// <value>The line mode.</value>
        public LineMode LineMode { get; set; } = LineMode.Spline;

        /// <summary>
        /// Gets or sets the alpha of the line area.
        /// </summary>
        /// <value>The line area alpha.</value>
        public byte LineAreaAlpha { get; set; } = 32;

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var valueLabelSizes = MeasureValueLabels();
            var footerHeight = CalculateFooterHeight(valueLabelSizes);
            var headerHeight = CalculateHeaderHeight(valueLabelSizes);
            var itemSize = CalculateItemSize(width, height, footerHeight, headerHeight);
            var origin = CalculateYOrigin(itemSize.Height, headerHeight);
            var points = CalculatePoints(itemSize, origin, headerHeight);

            DrawArea(canvas, points, itemSize, origin);
            DrawLine(canvas, points, itemSize);
            DrawPoints(canvas, points);
            DrawFooter(canvas, points, itemSize, height, footerHeight);
            DrawValueLabel(canvas, points, itemSize, height, valueLabelSizes);
        }

        protected void DrawLine(SKCanvas canvas, SKPoint[] points, SKSize itemSize)
        {
            if (points.Length > 1 && LineMode != LineMode.None)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.White,
                    StrokeWidth = LineSize,
                    IsAntialias = true,
                })
                {
                    using (var shader = CreateGradient(points))
                    {
                        paint.Shader = shader;

                        var path = new SKPath();

                        path.MoveTo(points.First());

                        var last = (LineMode == LineMode.Spline) ? points.Length - 1 : points.Length;
                        for (int i = 0; i < last; i++)
                        {
                            if (LineMode == LineMode.Spline)
                            {
                                var entry = Entries.ElementAt(i);
                                var nextEntry = Entries.ElementAt(i + 1);
                                var cubicInfo = CalculateCubicInfo(points, i, itemSize);
                                path.CubicTo(cubicInfo.control, cubicInfo.nextControl, cubicInfo.nextPoint);
                            }
                            else if (LineMode == LineMode.Straight)
                            {
                                path.LineTo(points[i]);
                            }
                        }

                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        protected void DrawArea(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float origin)
        {
            if (LineAreaAlpha > 0 && points.Length > 1)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.White,
                    IsAntialias = true,
                })
                {
                    using (var shader = CreateGradient(points, LineAreaAlpha))
                    {
                        paint.Shader = shader;

                        var path = new SKPath();

                        path.MoveTo(points.First().X, origin);
                        path.LineTo(points.First());

                        var last = (LineMode == LineMode.Spline) ? points.Length - 1 : points.Length;
                        for (int i = 0; i < last; i++)
                        {
                            if (LineMode == LineMode.Spline)
                            {
                                var entry = Entries.ElementAt(i);
                                var nextEntry = Entries.ElementAt(i + 1);
                                var cubicInfo = CalculateCubicInfo(points, i, itemSize);
                                path.CubicTo(cubicInfo.control, cubicInfo.nextControl, cubicInfo.nextPoint);
                            }
                            else if (LineMode == LineMode.Straight)
                            {
                                path.LineTo(points[i]);
                            }
                        }

                        path.LineTo(points.Last().X, origin);

                        path.Close();

                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        private (SKPoint point, SKPoint control, SKPoint nextPoint, SKPoint nextControl) CalculateCubicInfo(SKPoint[] points, int i, SKSize itemSize)
        {
            var point = points[i];
            var nextPoint = points[i + 1];
            var controlOffset = new SKPoint(itemSize.Width * 0.8f, 0);
            var currentControl = point + controlOffset;
            var nextControl = nextPoint - controlOffset;
            return (point, currentControl, nextPoint, nextControl);
        }

        private SKShader CreateGradient(SKPoint[] points, byte alpha = 255)
        {
            var startX = points.First().X;
            var endX = points.Last().X;
            var rangeX = endX - startX;

            return SKShader.CreateLinearGradient(
                new SKPoint(startX, 0),
                new SKPoint(endX, 0),
                Entries.Select(x => x.Color.WithAlpha(alpha)).ToArray(),
                null,
                SKShaderTileMode.Clamp);
        }
    }
}