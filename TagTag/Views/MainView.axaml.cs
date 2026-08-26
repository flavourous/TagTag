using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;
using TagTag.ViewModels;

namespace TagTag.Views;

public partial class MainView : ReactiveUserControl<ViewModels.MainViewModel>
{
    public MainView() => InitializeComponent();

    private void Execute_Tag_Command(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: ICommand c } t)
        {
            if(c is IReactiveCommand<Unit, object> or IReactiveCommand<Unit, Unit>) c.Execute(Unit.Default);
            else c.Execute(t.DataContext);
        }
    }
}
