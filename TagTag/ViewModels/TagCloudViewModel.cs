using System.Collections.ObjectModel;
using AvaloniaGraphControl;
using DynamicData;
using Microsoft.Msagl.Layout.Layered;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public class RootCloudViewModel() {}
public class EdgeCloudViewModel : Edge
{
    public bool Root {get;}
    public EdgeCloudViewModel(bool root, object tail, object head, object label = null, Symbol tailSymbol = Symbol.None, Symbol headSymbol = Symbol.Arrow) : base(tail, head, label, tailSymbol, headSymbol)
    {
        Root = root;
    }
}
public sealed partial class TagCloudViewModel : ReactiveObject, ITagMenu
{
    public IEntityRepository? Eman { get; set; }

    [Reactive] private Graph _graph;
    public IEntity tagging { get; set; }
    public void SetItems(IEnumerable<IEntityItem<ITag>> items)
    {
        var g = new Graph();
        var nodes = items.Select(x => new TagItemViewModel(x, Eman)).ToDictionary(x=>x.Tag);
        var parents = nodes.Values
            .SelectMany(x=>x.Tag.tags.Select(t=>(parent: nodes[t], child:x)))
            .ToLookup(x=>x.child)
            .ToDictionary(x=>x.Key, x=>x.Select(g=>g.parent).ToArray());
        var root = new RootCloudViewModel();
        
        foreach(var node in nodes.Values)
        {
            if(!parents.TryGetValue(node, out var p) || !p.Any())
                g.Edges.Add(new EdgeCloudViewModel(true, root, node));

            foreach(var edge in node.Tag.tags)
                g.Edges.Add(new EdgeCloudViewModel(false, nodes[edge], node));
        }

        g.Orientation = Graph.Orientations.Horizontal;

        Graph = g;
    }
}
