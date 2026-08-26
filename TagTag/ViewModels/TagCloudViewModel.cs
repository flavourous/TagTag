using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Controls;
using AvaloniaGraphControl;
using DynamicData;
using Microsoft.Msagl.Layout.Layered;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed partial class TagCloudViewModel(ISourceList<TagItemViewModel> Filter,
    Dictionary<ITag, TagItemViewModel> NodeLookup, IScreen screen)
    : ReactiveObject, ITagMenu, IRoutableViewModel
{
    public IEntityRepository? Eman { get; set; }

    private CompositeDisposable d = [];
    public bool IsTagging { get; private set; }

    public string? UrlPathSegment => throw new NotImplementedException();
    public IScreen HostScreen => throw new NotImplementedException();

    [ReactiveCommand]
    public void NewTag()
    {
        var tag = Eman.CreateEntity<ITag>();
        tag.name = "new tag";
        Eman.UpdateEntity(tag);
    }

    [ReactiveCommand]
    public void DeleteEntity(IEntity e)
    {
        Eman.DeleteEntity(e);
    }

    [Reactive] private Graph _graph;
    public void SetItems(IEnumerable<IEntityItem<ITag>> items)
    {
        d.Dispose();
        d = [];
        IsTagging = false;
        Filter.Clear();
        NodeLookup.Clear();

        foreach (var item in items)
        {
            var vm = new TagItemViewModel(item, Eman);
            NodeLookup[item.entity] = vm;
        }

        var root = new RootCloudViewModel();
        var g = new Graph();
        Func<object, object, int> value = (a, b) =>
        {
            if (a is TagItemViewModel at && b is TagItemViewModel bt)
            {
                var ia = at.Tag.tags.Any() ? 1 : 0;
                var ib = bt.Tag.tags.Any() ? 1 : 0;
                return ia - ib;
            }
            return 0;
        };
        g.HorizontalOrder = value;

        foreach (var (tag, node) in NodeLookup)
        {
            node.WhenAnyValue(x => x.Tagging.Value)
                .WhereNotNull()
                .Subscribe(v => IsTagging = v ?? IsTagging)
                .DisposeWith(d);

            Action add = () => Filter.Add(node), rm = () => Filter.Remove(node);
            node.WhenAnyValue(x => x.Selected.Value)
                .WhereNotNull()
                .Subscribe(v => (v is true ? add : rm)())
                .DisposeWith(d);

            void AddEdge(EdgeCloudViewModel edge) => g.Edges.Add(edge);
            if (!tag.tags.Any())
            {
                AddEdge(new(true, root, node));
            }
            foreach (var edge in tag.tags) AddEdge(new(false, NodeLookup[edge], node));
        }

        g.Orientation = Graph.Orientations.Horizontal;

        Graph = g;
    }
}
