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

public partial class TagCloudView : ReactiveUserControl<TagCloudViewModel>
{
    public TagCloudView() => InitializeComponent();

    private void Execute_Tag_Command(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: ICommand c } t)
        {
            if(c is IReactiveCommand<Unit, object> or IReactiveCommand<Unit, Unit>
                && c.CanExecute(Unit.Default)) c.Execute(Unit.Default);
            else if(c.CanExecute(t.DataContext)) c.Execute(t.DataContext);
        }
    }

    private bool _isDragging;
    private Point _startPoint;

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

    private void TextBox_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var tb = sender as TextBox;
        if(tb.IsVisible && e.Property == TextBox.IsVisibleProperty)
        {
            tb.Focus();
            tb.SelectAll();
        }
    }
}
