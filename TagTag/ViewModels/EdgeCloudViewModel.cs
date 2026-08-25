using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using AvaloniaGraphControl;
using DynamicData;
using Microsoft.Msagl.Layout.Layered;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public class EdgeCloudViewModel : Edge
{
    public bool Root { get; }
    public EdgeCloudViewModel(bool root, object tail, object head, object label = null, Symbol tailSymbol = Symbol.None, Symbol headSymbol = Symbol.Arrow) : base(tail, head, label, tailSymbol, headSymbol)
    {
        Root = root;
    }
}
