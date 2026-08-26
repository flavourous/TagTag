using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
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
        if (sender is Control { DataContext: TagItemViewModel t } && DataContext is TagCloudViewModel { IsTagging: false })
        {
            t.Tagging.Value = t.IsEditing = true;
        }
    }

    private bool _isDragging;
    private Point _startPoint;

    public ReactiveView Reactive { get; } = new();
    public partial class ReactiveView : ReactiveObject
    {
        [Reactive] public double _offsetTrigger = 0.0;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var pointerProps = e.GetCurrentPoint(mainPanel).Properties;
        
        // Only drag on left click or primary touch contact
        if (pointerProps.IsLeftButtonPressed)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(this.VisualRoot as Visual);
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_isDragging && mainPanel.RenderTransform is TranslateTransform transform)
        {
            var currentPoint = e.GetPosition(this.VisualRoot as Visual);
            var delta = currentPoint - _startPoint;
            
            _startPoint = currentPoint;
            transform.X += delta.X;
            transform.Y += delta.Y;

            Reactive.OffsetTrigger = Reactive.OffsetTrigger == 0.0 ? 0.001 : 0.0;
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
        }

        base.OnPointerReleased(e);
    }
}
