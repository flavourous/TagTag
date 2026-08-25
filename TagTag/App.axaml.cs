using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagTag.ViewModels;
using TagTag.Views;

namespace TagTag;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var mainViewModel = CompositionRoot.Build().GetRequiredService<MainViewModel>();
        mainViewModel.Start();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };

            desktop.MainWindow.PointerPressed += (o, e) =>
            {
                var props = e.GetCurrentPoint(null).Properties;
                if (props.IsRightButtonPressed && mainViewModel.Router.NavigationStack.Count > 0)
                {
                    mainViewModel.Router.NavigateBack.Execute();
                    e.Handled = true;
                }
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView { DataContext = mainViewModel };
            
            singleView.MainView.Loaded += (_, _) =>
                TopLevel.GetTopLevel(singleView.MainView)!.BackRequested += (s, e) =>
                {
                    if (mainViewModel.Router.NavigationStack.Count > 1)
                    {
                        mainViewModel.Router.NavigateBack.Execute();
                        e.Handled = true;
                    }
                };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
