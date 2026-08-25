using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public partial class NavBarViewModel(TagItemViewModel tag, Action navigate, bool isFirst) : ReactiveObject
{
    public bool IsRoot => TagVM is null;
    public TagItemViewModel TagVM { get; } = tag;
    public bool IsFirst { get; } = isFirst;

    public string UpperName => TagVM.Name.ToUpper();

    [ReactiveCommand]
    public void Navigate() => navigate();
}
