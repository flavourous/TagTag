using System.Reactive;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using TagTag.ViewModels;

namespace TagTag.Views;

public partial class NoteView : ReactiveUserControl<NoteViewModel>
{
    public NoteView() => InitializeComponent();

    private void Execute_Tag_Command(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: ICommand c } t)
        {
            if(c is IReactiveCommand<Unit, object> or IReactiveCommand<Unit, Unit>
                && c.CanExecute(Unit.Default)) c.Execute(Unit.Default);
            else if(c.CanExecute(t.DataContext)) c.Execute(t.DataContext);
        }
    }
}
