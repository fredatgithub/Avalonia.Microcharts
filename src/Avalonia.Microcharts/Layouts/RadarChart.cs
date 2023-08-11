using System;
using System.Linq;
using SkiaSharp;

namespace Avalonia.Microcharts
{
    public class RadarChart : Chart
    {
        private const float Epsilon = 0.01f;

        /// <summary>
        /// Gets or sets the size of the line.
        /// </summary>
        /// <value>The size of the line.</value>
        public float LineSize { get; set; } = 3;

        public SKColor BorderLineColor { get; set; } = SKColors.LightGray.WithAlpha(110);

        public float BorderLineSize { get; set; } = 2;

        public PointMode PointMode { get; set; } = PointMode.Circle;

        public float PointSize { get; set; } = 14;

        private float AbsoluteMinimum => Entries.Select(x => x.Value).Concat(new[] { MaxValue, MinValue, InternalMinValue ?? 0 }).Min(x => Math.Abs(x));

        private float AbsoluteMaximum => Entries.Select(x => x.Value).Concat(new[] { MaxValue, MinValue, InternalMinValue ?? 0 }).Max(x => Math.Abs(x));

        private float ValueRange => AbsoluteMaximum - AbsoluteMinimum;

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var total = Entries.Count();

            if (total > 0)
            {
                var captionHeight = Entries.Max(x =>
                {
                    var result = 0.0f;

                    var hasLabel = !string.IsNullOrEmpty(x.Label);
                    var hasValueLabel = !string.IsNullOrEmpty(x.ValueLabel);
                    if (hasLabel || hasValueLabel)
                    {
                        var hasOffset = hasLabel && hasValueLabel;
                        var captionMargin = LabelTextSize * 0.60f;
                        var space = hasOffset ? captionMargin : 0;

                        if (hasLabel)
                        {
                            result += LabelTextSize;
                        }

                        if (hasValueLabel)
                        {
                            result += LabelTextSize;
                        }
                    }

                    return result;
                });

                var center = new SKPoint(width / 2, height / 2);
                var radius = ((Math.Min(width, height) - (2 * Margin)) / 2) - captionHeight;
                var rangeAngle = (float)((Math.PI * 2) / total);
                var startAngle = (float)Math.PI;

                var nextEntry = Entries.First();
                var nextAngle = startAngle;
                var nextPoint = GetPoint(nextEntry.Value, center, nextAngle, radius);

                DrawBorder(canvas, center, radius);

                for (int i = 0; i < total; i++)
                {
                    var angle = nextAngle;
                    var entry = nextEntry;
                    var point = nextPoint;

                    var nextIndex = (i + 1) % total;
                    nextAngle = startAngle + (rangeAngle * nextIndex);
                    nextEntry = Entries.ElementAt(nextIndex);
                    nextPoint = GetPoint(nextEntry.Value, center, nextAngle, radius);

                    // Border center bars
                    using (var paint = new SKPaint()
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = BorderLineSize,
                        Color = BorderLineColor,
                        IsAntialias = true,
                    })
                    {
                        var borderPoint = GetPoint(MaxValue, center, angle, radius);
                        canvas.DrawLine(point.X, point.Y, borderPoint.X, borderPoint.Y, paint);
                    }

                    // Values points and lines
                    using (var paint = new SKPaint()
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = BorderLineSize,
                        Color = entry.Color.WithAlpha((byte)(entry.Color.Alpha * 0.75f)),
                        PathEffect = SKPathEffect.CreateDash(new[] { BorderLineSize, BorderLineSize * 2 }, 0),
                        IsAntialias = true,
                    })
                    {
                        var amount = Math.Abs(entry.Value - AbsoluteMinimum) / ValueRange;
                        canvas.DrawCircle(center.X, center.Y, radius * amount, paint);
                    }

                    canvas.DrawGradientLine(center, entry.Color.WithAlpha(0), point, entry.Color.WithAlpha((byte)(entry.Color.Alpha * 0.75f)), LineSize);
                    canvas.DrawGradientLine(point, entry.Color, nextPoint, nextEntry.Color, LineSize);
                    canvas.DrawPoint(point, entry.Color, PointSize, PointMode);

                    // Labels
                    var labelPoint = GetPoint(MaxValue, center, angle, radius + LabelTextSize  + (PointSize / 2));
                    var alignment = SKTextAlign.Left;

                    if ((Math.Abs(angle - (startAngle + Math.PI)) < Epsilon) || (Math.Abs(angle - Math.PI) < Epsilon))
                    {
                        alignment = SKTextAlign.Center;
                    }
                    else if (angle > (float)(startAngle + Math.PI))
                    {
                        alignment = SKTextAlign.Right;
                    }

                    canvas.DrawCaptionLabels(entry.Label, entry.TextColor, entry.ValueLabel, entry.Color, LabelTextSize, labelPoint, alignment);
                }
            }
        }

        /// <summary>
        /// Finds point cordinates of an entry.
        /// </summary>
        /// <returns>The point.</returns>
        /// <param name="value">The value.</param>
        /// <param name="center">The center.</param>
        /// <param name="angle">The entry angle.</param>
        /// <param name="radius">The radius.</param>
        private SKPoint GetPoint(float value, SKPoint center, float angle, float radius)
        {
            var amount = Math.Abs(value - AbsoluteMinimum) / ValueRange;
            var point = new SKPoint(0, radius * amount);
            var rotation = SKMatrix.MakeRotation(angle);
            return center + rotation.MapPoint(point);
        }

        private void DrawBorder(SKCanvas canvas, SKPoint center, float radius)
        {
            using (var paint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = BorderLineSize,
                Color = BorderLineColor,
                IsAntialias = true,
            })
            {
                canvas.DrawCircle(center.X, center.Y, radius, paint);
            }
        }
    }
}