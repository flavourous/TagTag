using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TagTag.ViewModels;
using ReactiveUI;

namespace TagTag;

public class ViewLocator : IDataTemplate, IViewLocator
{
    public Control Build(object? data)
    {
        if (data is null)
            return null;

        return Build(data.GetType());
    }

    private static Control Build(Type viewModelType)
    {
        var name = viewModelType.FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = name };
    }

    public bool Match(object? data) => true;

    public IViewFor? ResolveView<TViewModel>(TViewModel? viewModel, string? contract = null)
        where TViewModel : class
    {
        return Build(viewModel) as IViewFor;
    }

    public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract = null)
        where TViewModel : class
    {
        return Build(typeof(TViewModel)) as IViewFor<TViewModel>;
    }

    public IViewFor? ResolveView(object? viewModel, string? contract = null)
    {
        return Build(viewModel) as IViewFor;
    }
}
