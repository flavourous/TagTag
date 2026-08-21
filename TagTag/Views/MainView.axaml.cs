using Avalonia.Controls;
using ReactiveUI.Avalonia;
using TagTag.ViewModels;

namespace TagTag.Views;

public partial class MainView : ReactiveUserControl<ViewModels.MainViewModel>
{
    public MainView() => InitializeComponent();

    private void TextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key != Avalonia.Input.Key.Return) return;

        var dc = (sender as Control).DataContext;
        if (dc is TagItemViewModel t) t.Save();
        if (dc is DetailItemViewModel d) d.Save();
    }

    private void TextBox_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if(e.Property.Name == nameof(IsVisible) && e.NewValue is true) 
        {
            var tb = sender as TextBox;
            tb.Focus();
            tb.CaretIndex = tb.Text.Length;
        }
    }
}
