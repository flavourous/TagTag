using TagTag.Backend;

namespace TagTag.Test;

public sealed class PresenterTests : IDisposable
{
    private readonly string databaseDirectory = Path.Combine(Path.GetTempPath(), "TagTag.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void Presenter_preserves_legacy_menu_detail_and_tagger_behaviour()
    {
        Directory.CreateDirectory(databaseDirectory);

        var platform = new TestPlatform(databaseDirectory);
        using var model = new ModelLiteDb(platform);
        CreateSampleModel(model);
        var view = new ManualView();

        Presenter.Start(view, platform, model);

        AssertMenuAndDetail(view, menuCount: 4, detailCount: 2);
        ToggleSelect(view.Menu, "tag 1");
        AssertMenuAndDetail(view, menuCount: 3, detailCount: 2);
        ToggleSelect(view.Menu, "tag 3");
        AssertMenuAndDetail(view, menuCount: 1, detailCount: 1);
        view.Menu.GoBack();
        AssertMenuAndDetail(view, menuCount: 3, detailCount: 2);
        view.Menu.GoBack();
        AssertMenuAndDetail(view, menuCount: 4, detailCount: 2);
        view.Menu.GoBack();
        AssertMenuAndDetail(view, menuCount: 4, detailCount: 2);

        var note2 = Find<INote>(model, "note 2");
        var note1 = Find<INote>(model, "note 1");
        var tag1 = Find<ITag>(model, "tag 1");

        note2.name = "Totally - lol";
        note2.text = "HAH";
        model.UpdateEntity(note2);
        Assert.Single(view.Menu.Items, item => item.entity.name == "Totally - lol");

        model.DeleteEntity(note2);
        AssertMenuAndDetail(view, menuCount: 3, detailCount: 1);

        Create<INote>(model, "LOLZOR");
        Create<INote>(model, "LOLZOR");
        Create<INote>(model, "LOLZOR");
        AssertMenuAndDetail(view, menuCount: 6, detailCount: 4);

        ToggleSelect(view.Menu, "tag 1");
        const string tagBasedNoteName = "Testing that we get the tag based on menu position";
        Create<INote>(model, tagBasedNoteName);
        Assert.Single(view.Menu.Items, item => item.entity.name == tagBasedNoteName);

        view.Tagger.BeginTagging(note1);
        Assert.All(view.Tagger.Items, item => Assert.IsAssignableFrom<ITag>(item.entity));
        Assert.All(view.Tagger.Items, item => Assert.False(item.selected));
        Assert.Equal(2, view.Tagger.Items.Count);

        var tagItem = Find(view.Tagger, tag1);
        tagItem.selected = true;
        Assert.Contains(tag1, note1.tags);
        view.Tagger.BeginTagging(note1);
        Assert.True(Find(view.Tagger, tag1).selected);

        Find(view.Tagger, tag1).selected = false;
        view.Tagger.BeginTagging(note1);
        Assert.False(Find(view.Tagger, tag1).selected);

        ToggleSelect(view.Tagger, "tag 1");
        Assert.Single(view.Tagger.Items);
        const string nestedTagName = "Testing tag";
        Create<ITag>(model, nestedTagName);
        Assert.Single(view.Tagger.Items, item => item.entity.name == nestedTagName);

        view.Menu.GoBack();
        while (view.Menu.Items.Count > 0)
        {
            model.DeleteEntity(view.Menu.Items[0].entity);
        }

        Assert.Empty(view.Menu.Items);
        Create<ITag>(model, "Root Tag");
        Assert.Single(view.Menu.Items);
    }

    [Fact]
    public void LiteDb_persists_entities_and_tag_memberships_after_reopening()
    {
        Directory.CreateDirectory(databaseDirectory);
        var platform = new TestPlatform(databaseDirectory);

        using (var model = new ModelLiteDb(platform))
        {
            var tag = Create<ITag>(model, "Work");
            var note = Create<INote>(model, "Plan", tag);
            note.text = "Prepare the LiteDB migration.";
            model.UpdateEntity(note);
        }

        using var reopenedModel = new ModelLiteDb(platform);
        var reopenedTag = Find<ITag>(reopenedModel, "Work");
        var reopenedNote = Find<INote>(reopenedModel, "Plan");

        Assert.Equal("Prepare the LiteDB migration.", reopenedNote.text);
        Assert.Contains(reopenedTag, reopenedNote.tags);
    }

    public void Dispose()
    {
        if (Directory.Exists(databaseDirectory)) Directory.Delete(databaseDirectory, recursive: true);
    }

    private static void CreateSampleModel(IEntityRepository model)
    {
        var tag1 = Create<ITag>(model, "tag 1");
        var tag2 = Create<ITag>(model, "tag 2");
        var tag3 = Create<ITag>(model, "tag 3", tag1);

        Create<INote>(model, "note 1");
        Create<INote>(model, "note 2");
        Create<INote>(model, "note 3", tag1);
        Create<INote>(model, "note 4", tag2);
        Create<INote>(model, "note 5", tag3);
        Create<INote>(model, "note 6", tag1, tag2);
    }

    private static T Create<T>(IEntityRepository entityManager, string name, params ITag[] tags) where T : IEntity
    {
        var entity = entityManager.CreateEntity<T>();
        entity.name = name;
        foreach (var tag in tags)
        {
            entityManager.AddTag(entity, tag);
            entityManager.UpdateEntity(tag);
        }

        return (T)entityManager.UpdateEntity(entity);
    }

    private static T Find<T>(IEntityRepository model, string name) where T : class, IEntity =>
        Assert.IsAssignableFrom<T>(Assert.Single(model.GetEntities(), entity => entity.name == name));

    private static IEntityItem<ITag> Find(ManualMenu menu, ITag tag) =>
        Assert.Single(menu.Items, item => item.entity.Equals(tag));

    private static void ToggleSelect(ManualMenu menu, string name) 
    {
        var entity = Assert.Single(menu.Items, item => item.entity.name == name);
        entity.selected = !entity.selected;
    }

    private static void AssertMenuAndDetail(ManualView view, int menuCount, int detailCount)
    {
        Assert.Equal(menuCount, view.Menu.Items.Count);
        Assert.Equal(detailCount, view.Detail.Count);
    }

    private sealed class ManualView : IView
    {
        public IEntityRepository Eman { get; private set; } = null!;
        public ManualMenu Menu { get; } = new();
        public ManualMenu Tagger { get; } = new();
        public IReadOnlyList<IEntity> Detail { get; private set; } = [];

        IEntityRepository IView.entities { set => Eman = value; }
        ITagMenu IView.cloud => Tagger;
        public void SetDetailItems(IEnumerable<IEntity> items) => Detail = items.ToArray();
    }

    private sealed class ManualMenu : ITagMenu
    {
        public IReadOnlyList<IEntityItem<ITag>> Items { get; private set; } = [];

        public event Action? MenuBack;
        public IEntity? tagging {get;set;}

        public void SetItems(IEnumerable<IEntityItem<ITag>> items) => Items = items.ToArray();
        public void SetTree(IEnumerable<string> tree) { }
        public void GoBack() => MenuBack?.Invoke();
        public void BeginTagging(IEntity entity) => tagging = entity;
    }

    private sealed class TestPlatform(string appData) : IPlatform
    {
        public int AppVersion => 1;
        public string AppData { get; } = appData;
        public void WriteLine(string message) { }
        public void DeleteFile(string path) => File.Delete(path);
        public Stream ReadFile(string path) => File.OpenRead(path);
    }
}
