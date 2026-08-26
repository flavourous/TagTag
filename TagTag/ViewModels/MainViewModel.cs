using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Msagl.Drawing;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed partial class MainViewModel : ReactiveObject, IView, IScreen
{
    IEntityRepository IView.entities { set => Eman = TagCloud.Eman = value; }
    ITagMenu IView.cloud => TagCloud;

    public TagCloudViewModel TagCloud { get; }
    public IEntityRepository? Eman { get; private set; }

    public ObservableCollection<DetailItemViewModel> DetailItems { get; } = [];
    [Reactive] private NavBarViewModel[] _nextTags = [];
    [Reactive] private NavBarViewModel[] _parentTags = [];
    [Reactive] private NavBarViewModel[] _currentTags = [];

    public RoutingState Router { get; } = new();

    private CompositeDisposable d = [];
    private readonly IPlatform platform;
    private Dictionary<ITag, TagItemViewModel> nodes = [];

    public MainViewModel(IPlatform platform)
    {
        this.platform = platform;
        var home = new NavBarViewModel(null, NavigateTo(null), true);
        CurrentTags = [home];
        SourceList<TagItemViewModel> filter = new();
        TagCloud = new(filter, nodes, this);

        filter.Connect()
              .QueryWhenChanged()
              .Subscribe(filtered =>
              {
                  CurrentTags = filtered.Any() switch
                  {
                      true => filtered.Select((x, i) => new NavBarViewModel(x, NavigateTo(x), i == 0)).ToArray(),
                      false => [home]
                  };

                  var parents = filtered
                    .SelectMany(x => x.Tag.tags.Select(t => nodes[t]))
                    .DistinctBy(x => x.Tag)
                    .ToArray();

                  AtRoot = !filtered.Any();
                  AtMultiple = filtered.Count() > 1;
                  if(AtRoot) NavigateChildren = true;

                  ParentTags = (filtered.Any(), parents.Any()) switch
                  {
                      (_, true) => parents
                        .Select((x, i) => new NavBarViewModel(x, NavigateTo(x), i == 0))
                        .ToArray(),
                      (true, false) => [home],
                      (false, false) => []
                  };
              });
    }

    private Action NavigateTo(TagItemViewModel x) => () =>
    {
        var cct = CurrentTags.ToArray();
        foreach (var ct in cct) if (ct.TagVM is { } c) c.Selected.Value = false;
        if(x is { } v) v.Selected.Value = true;
        NavigateChildren = true;
    };

    [Reactive] public bool _navigateChildren = true;
    [Reactive] public bool _atRoot = true;
    [Reactive] public bool _atMultiple = false;

    [ReactiveCommand] public void ToggleParents()
    {
        var ll = ParentTags.Length;
        if(NavigateChildren && ll == 1) ParentTags.Single().Navigate();
        if(ll > 1) NavigateChildren = !NavigateChildren;
    } 

    [ReactiveCommand] public void Cloud() => Router.Navigate.Execute(TagCloud);

    [ReactiveCommand]
    public void NewNote()
    {
        var note = Eman.CreateEntity<INote>();
        note.name = "new note";
        Eman.UpdateEntity(note);
    }

    [ReactiveCommand]
    public void Navigate(DetailItemViewModel x)
    {
        Router.Navigate.Execute(new NoteViewModel(x.EntityItem, Eman, this, TagCloud));
    }

    public void Start() => Presenter.Start(this, platform);

    public void SetDetailItems(IEnumerable<IEntityItem<IEntity>> items)
    {
        d.Dispose();
        d = [];

        var tags = items.Where(x => x.entity is ITag).ToArray();
        var entries = items.Where(x => x.entity is not ITag).ToArray();

        var detailItems = entries.Select((x, i) => new DetailItemViewModel(x, i == 0 && !tags.Any())).ToArray();
        DetailItems.Clear();
        DetailItems.AddRange(detailItems);

        NextTags = tags
            .Select(x => nodes[x.entity as ITag])
            .Select((x, i) => new NavBarViewModel(x, NavigateTo(x), i == 0))
            .ToArray();
    }
}