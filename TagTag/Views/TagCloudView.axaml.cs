using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI.Avalonia;
using TagTag.ViewModels;

namespace TagTag.Views;

public partial class TagCloudView : ReactiveUserControl<TagCloudViewModel>
{
    public TagCloudView() => InitializeComponent();

    private void Cloud_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control { DataContext: TagItemViewModel t } && DataContext is TagCloudViewModel vm)
        {
            var anyTagging = vm.IsTagging;
            if (anyTagging) t.Tagged.Value = !t.Tagged.Value;
            else t.Selected.Value = !t.Selected.Value;
        }
    }

    private void Cloud_Holding(object? sender, HoldingRoutedEventArgs e) => CloudSelect(sender);
    private void Cloud_DoubleTapped(object? sender, TappedEventArgs e) => CloudSelect(sender);
    private void CloudSelect(object? sender)
    {
        if (sender is Control { DataContext: TagItemViewModel t })
        {
            t.Tagging.Value = t.IsEditing = !t.IsEditing;
        }
    }
}
