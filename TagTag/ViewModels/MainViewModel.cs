using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed partial class MainViewModel(IPlatform platform) : ReactiveObject, IView
{
    IEntityRepository IView.entities { set => Eman = TagCloud.Eman = value; }
    ITagMenu IView.cloud => TagCloud;

    public TagCloudViewModel TagCloud { get; } = new();
    public IEntityRepository? Eman { get; private set; }

    [Reactive] private IEntity _tagging;
    [Reactive] private ObservableCollection<DetailItemViewModel> _detailItems = [];

    [ReactiveCommand]
    public void NewNote()
    {
        var note = Eman.CreateEntity<INote>();
        note.name = "new note";
        Eman.UpdateEntity(note);
    }

    [ReactiveCommand]
    public void NewTag()
    {
        var tag = Eman.CreateEntity<ITag>();
        tag.name = "new tag";
        Eman.UpdateEntity(tag);
    }

    [ReactiveCommand] public void DeleteEntity(IEntity e) => Eman.DeleteEntity(e);

    public void Start() => Presenter.Start(this, platform);

    public void SetDetailItems(IEnumerable<IEntityItem<IEntity>> items)
    {
        var viewModels = items.Select(item => new DetailItemViewModel(item, Eman));
        DetailItems = [.. viewModels];
    } 
}