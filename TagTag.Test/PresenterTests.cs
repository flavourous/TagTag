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
        Activate(view.Menu, "tag 1");
        AssertMenuAndDetail(view, menuCount: 3, detailCount: 2);
        Activate(view.Menu, "tag 3");
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
        view.Eman.UpdateEntity(note2);
        Assert.Single(view.Menu.Items, item => item.entity.name == "Totally - lol");

        view.Eman.DeleteEntity(note2);
        AssertMenuAndDetail(view, menuCount: 3, detailCount: 1);

        Create<INote>(view.Eman, "LOLZOR");
        Create<INote>(view.Eman, "LOLZOR");
        Create<INote>(view.Eman, "LOLZOR");
        AssertMenuAndDetail(view, menuCount: 6, detailCount: 4);

        Activate(view.Menu, "tag 1");
        const string tagBasedNoteName = "Testing that we get the tag based on menu position";
        Create<INote>(view.Eman, tagBasedNoteName);
        Assert.Single(view.Menu.Items, item => item.entity.name == tagBasedNoteName);

        view.Tagger.BeginTagging(note1);
        Assert.All(view.Tagger.Items, item => Assert.IsAssignableFrom<ITag>(item.entity));
        Assert.All(view.Tagger.Items, item => Assert.False(item.ticked));
        Assert.Equal(2, view.Tagger.Items.Count);

        var tagItem = Find(view.Tagger, tag1);
        tagItem.ticked = true;
        Assert.Contains(tag1, note1.tags);
        view.Tagger.BeginTagging(note1);
        Assert.True(Find(view.Tagger, tag1).ticked);

        Find(view.Tagger, tag1).ticked = false;
        view.Tagger.BeginTagging(note1);
        Assert.False(Find(view.Tagger, tag1).ticked);

        Activate(view.Tagger, "tag 1");
        Assert.Single(view.Tagger.Items);
        const string nestedTagName = "Testing tag";
        Create<ITag>(view.Eman, nestedTagName);
        Assert.Single(view.Tagger.Items, item => item.entity.name == nestedTagName);

        view.Menu.GoBack();
        while (view.Menu.Items.Count > 0)
        {
            view.Eman.DeleteEntity(view.Menu.Items[0].entity);
        }

        Assert.Empty(view.Menu.Items);
        Create<ITag>(view.Eman, "Root Tag");
        Assert.Single(view.Menu.Items);
    }

    [Fact]
    public void LiteDb_persists_entities_and_tag_memberships_after_reopening()
    {
        Directory.CreateDirectory(databaseDirectory);
        var platform = new TestPlatform(databaseDirectory);

        using (var model = new ModelLiteDb(platform))
        {
            var tag = Create<ITag>(model.eman, "Work");
            var note = Create<INote>(model.eman, "Plan", tag);
            note.text = "Prepare the LiteDB migration.";
            model.eman.UpdateEntity(note);
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

    private static void CreateSampleModel(IModel model)
    {
        var tag1 = Create<ITag>(model.eman, "tag 1");
        var tag2 = Create<ITag>(model.eman, "tag 2");
        var tag3 = Create<ITag>(model.eman, "tag 3", tag1);

        Create<INote>(model.eman, "note 1");
        Create<INote>(model.eman, "note 2");
        Create<INote>(model.eman, "note 3", tag1);
        Create<INote>(model.eman, "note 4", tag2);
        Create<INote>(model.eman, "note 5", tag3);
        Create<INote>(model.eman, "note 6", tag1, tag2);
    }

    private static T Create<T>(IEntityManager entityManager, string name, params ITag[] tags) where T : IEntity
    {
        var entity = entityManager.CreateEntity<T>();
        entity.name = name;
        foreach (var tag in tags)
        {
            ((IModel)entityManager).AddTag(entity, tag);
            entityManager.UpdateEntity(tag);
        }

        return (T)entityManager.UpdateEntity(entity);
    }

    private static T Find<T>(IModel model, string name) where T : class, IEntity =>
        Assert.IsAssignableFrom<T>(Assert.Single(model.GetEntities(), entity => entity.name == name));

    private static IMenuItem Find(ManualMenu menu, ITag tag) =>
        Assert.Single(menu.Items, item => item.entity.Equals(tag));

    private static void Activate(ManualMenu menu, string name) =>
        Assert.Single(menu.Items, item => item.entity.name == name).Activate();

    private static void AssertMenuAndDetail(ManualView view, int menuCount, int detailCount)
    {
        Assert.Equal(menuCount, view.Menu.Items.Count);
        Assert.Equal(detailCount, view.Detail.Count);
    }

    private sealed class ManualView : IView
    {
        public IEntityManager Eman { get; private set; } = null!;
        public ManualMenu Menu { get; } = new();
        public ManualMenu Tagger { get; } = new();
        public IReadOnlyList<IEntity> Detail { get; private set; } = [];

        IEntityManager IView.eman { set => Eman = value; }
        IMenu IView.menu => Menu;
        ITagMenu IView.tagger => Tagger;
        public void SetDetailItems(IEnumerable<IEntity> items) => Detail = items.ToArray();
    }

    private sealed class ManualMenu : ITagMenu
    {
        public IReadOnlyList<IMenuItem> Items { get; private set; } = [];

        public event Action? MenuBack;
        public event Action<IEntity>? tagging;

        public void SetMenuItems(IEnumerable<IMenuItem> items) => Items = items.ToArray();
        public void SetTree(IEnumerable<string> tree) { }
        public void GoBack() => MenuBack?.Invoke();
        public void BeginTagging(IEntity entity) => tagging?.Invoke(entity);
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
