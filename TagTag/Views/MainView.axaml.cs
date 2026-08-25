using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI.Avalonia;
using TagTag.ViewModels;

namespace TagTag.Views;

public partial class MainView : ReactiveUserControl<ViewModels.MainViewModel>
{
    public MainView() => InitializeComponent();

    private void Execute_Tag_Command(object? sender, TappedEventArgs e)
    {
        if (sender is Control { Tag: ICommand c }) c.Execute(null);
    }
}
