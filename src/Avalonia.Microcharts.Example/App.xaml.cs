using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Avalonia.Microcharts.Example
{
  public class App : Application
  {
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
      {
        desktopLifetime.MainWindow = new MainWindow();
      }

      base.OnFrameworkInitializationCompleted();
    }
  }
}