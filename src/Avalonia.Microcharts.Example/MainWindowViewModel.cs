using SkiaSharp;

namespace Avalonia.Microcharts.Example
{
  public class MainWindowViewModel
  {
    public Entry[] Entries = new Entry[]
    {
            new Entry()
            {
                Value = 200,
                Label = "January",
                ValueLabel = "200",
                Color = SKColor.Parse("#266489")
            },
            new Entry()
            {
                Value = 400,
                Label = "February",
                ValueLabel = "400",
                Color = SKColor.Parse("#68B9C0")
            },
            new Entry()
            {
                Value = -100,
                Label = "March",
                ValueLabel = "-100",
                Color = SKColor.Parse("#90D585")
            }
    };

    public Chart[] Charts { get; set; }

    public MainWindowViewModel()
    {
      Charts = new Chart[]
      {
                new BarChart() {Entries = Entries},
                new PointChart() {Entries = Entries},
                new LineChart() {Entries = Entries},
                new DonutChart() {Entries = Entries},
                new RadialGaugeChart() {Entries = Entries},
                new RadarChart() {Entries = Entries}
      };
    }
  }
}