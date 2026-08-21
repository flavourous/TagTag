using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed class DetailItemViewModel
{
    public DetailItemViewModel(IEntity entity) => Entity = entity;

    public IEntity Entity { get; }
    public string Name => string.IsNullOrWhiteSpace(Entity.name) ? "<empty>" : Entity.name;
    public string Date => Entity.created.ToString("d");
    public string Details => Entity is INote note ? ToEllipsis(note.text, 256) : "";
    public bool IsNote => Entity is INote;

    private static string ToEllipsis(string? value, int length)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var firstLine = value.Split(Environment.NewLine)[0];
        return firstLine.Length <= length ? firstLine : firstLine[..length] + "…";
    }
}

public sealed partial class MenuViewModel : ReactiveObject, ITagMenu
{
    [Reactive] private ObservableCollection<IMenuItem> _items = [];
    [Reactive] private string _breadcrumb = "All items";
    [Reactive] private bool _canGoBack;

    public event Action? MenuBack;
    public event Action<IEntity>? tagging;

    [ReactiveCommand]
    public void GoBack() => MenuBack?.Invoke();

    public void BeginTagging(IEntity entity) => tagging?.Invoke(entity);

    [ReactiveCommand]
    public void Activate(IMenuItem? item) => item?.Activate();
    public void SetMenuItems(IEnumerable<IMenuItem> items) => Items = new ObservableCollection<IMenuItem>(items);
    public void SetTree(IEnumerable<string> tree)
    {
        var parts = tree.ToArray();
        Breadcrumb = parts.Length == 0 ? "All items" : string.Join(" → ", parts);
        CanGoBack = parts.Length > 0;
    }
}

public sealed partial class MainViewModel : ReactiveObject, IView
{
    private readonly IPlatform platform;

    public MainViewModel(IPlatform platform)
    {
        this.platform = platform;
    }

    public MenuViewModel Menu { get; } = new();
    public MenuViewModel Tagger { get; } = new();
    [Reactive] private ObservableCollection<DetailItemViewModel> _detailItems = [];
    [Reactive] private INote? _editingNote;
    [Reactive] private bool _isEditing;
    [Reactive] private bool _isTagging;
    [Reactive] private string _newTagName = "";
    [Reactive] private string _taggerTitle = "Tags";

    public IEntityManager? Eman { get; private set; }

    public void Start()
    {
        Presenter.Start(this, platform);
    }

    public void SetDetailItems(IEnumerable<IEntity> items) =>
        DetailItems = new ObservableCollection<DetailItemViewModel>(items.Select(item => new DetailItemViewModel(item)));

    [ReactiveCommand]
    private void NewNote()
    {
        if (Eman is null) return;
        EditingNote = Eman.CreateEntity<INote>();
        IsEditing = true;
    }

    [ReactiveCommand]
    private void NewTag()
    {
        if (Eman is null || string.IsNullOrWhiteSpace(NewTagName)) return;
        var tag = Eman.CreateEntity<ITag>();
        tag.name = NewTagName.Trim();
        Eman.UpdateEntity(tag);
        NewTagName = "";
    }

    [ReactiveCommand]
    private void SaveNote()
    {
        if (EditingNote is not null) Eman?.UpdateEntity(EditingNote);
        IsEditing = false;
        EditingNote = null;
    }

    [ReactiveCommand]
    private void Edit(IEntity? entity)
    {
        if (entity is INote note)
        {
            EditingNote = note;
            IsEditing = true;
        }
    }

    [ReactiveCommand]
    private void Delete(IEntity? entity) { if (entity is not null) Eman?.DeleteEntity(entity); }

    [ReactiveCommand]
    private void Tag(IEntity? entity)
    {
        if (entity is null) return;
        TaggerTitle = $"Tagging {entity.name}";
        Tagger.BeginTagging(entity);
        IsTagging = true;
    }

    [ReactiveCommand]
    private void CloseEditor() => IsEditing = false;

    [ReactiveCommand]
    private void CloseTagger() => IsTagging = false;

    IEntityManager IView.eman { set => Eman = value; }
    IMenu IView.menu => Menu;
    ITagMenu IView.tagger => Tagger;
}
