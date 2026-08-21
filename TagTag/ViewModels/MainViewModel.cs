using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed partial class DetailItemViewModel(IEntity entity, IEntityManager man) : ReactiveObject
{
    public IEntity Entity { get; } = entity;
    public string Name { get; set; } = string.IsNullOrWhiteSpace(entity.name) ? "<empty>" : entity.name;
    public string Date => Entity.created.ToString("d");
    public string Text { get; set; } = entity is INote note ? note.text : "";
    public bool IsNote => Entity is INote;

    [Reactive] public bool _isEditing, _isEditingDetail;
    [ReactiveCommand] public void BeginEditName() => IsEditing = true;
    [ReactiveCommand] public void BeginEditDetailCommand() => IsEditingDetail = true;
    [ReactiveCommand] public void Save()
    {
        IsEditingDetail = IsEditing = false;        
        Entity.name = Name;
        if(Entity is INote n) n.text = Text;
        man.UpdateEntity(Entity);
    }

    private static string ToEllipsis(string? value, int length)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var firstLine = value.Split(Environment.NewLine)[0];
        return firstLine.Length <= length ? firstLine : firstLine[..length] + "…";
    }
}

public sealed partial class TagItemViewModel(IMenuItem<ITag> tagItem, IEntityManager man) : ReactiveObject
{
    public bool Selected { get => tagItem.selected; set => tagItem.selected = value; }
    public string Name { get; set; } = string.IsNullOrWhiteSpace(tagItem.entity.name) ? "<empty>" : tagItem.entity.name;

    [Reactive] public bool _isEditing;
    [ReactiveCommand] public void BeginEditName() => IsEditing = true;
    public void Save()
    {
        IsEditing = false;
        tagItem.entity.name = Name;
        man.UpdateEntity(tagItem.entity);
    }
}

public sealed partial class TagCloudViewModel : ReactiveObject, ITagMenu
{
    public IEntityManager? Eman { get; set; }

    [Reactive] private ObservableCollection<TagItemViewModel> _items = [];
    public IEntity tagging { get; set; }
    public void SetItems(IEnumerable<IMenuItem<ITag>> items)
    {
        Items = [.. items.Select(x => new TagItemViewModel(x, Eman))];
    }
}

public sealed partial class MainViewModel(IPlatform platform) : ReactiveObject, IView
{
    IEntityManager IView.eman { set => Eman = TagCloud.Eman = value; }
    ITagMenu IView.tagger => TagCloud;

    public TagCloudViewModel TagCloud { get; } = new();
    public IEntityManager? Eman { get; private set; }

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

    public void Start()
    {
        Presenter.Start(this, platform);
    }


    public void SetDetailItems(IEnumerable<IEntity> items) =>
        DetailItems = new ObservableCollection<DetailItemViewModel>(items.Select(item => new DetailItemViewModel(item, Eman)));
}