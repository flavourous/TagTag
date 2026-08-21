using LiteDB;

namespace TagTag.Backend;

internal abstract class EntityDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public DateTime Created { get; set; }
    public string Name { get; set; } = "";
    public List<Guid> TagIds { get; set; } = [];
}

internal sealed class NoteDocument : EntityDocument
{
    public string Text { get; set; } = "";
}

internal sealed class TagDocument : EntityDocument;

public sealed class ModelLiteDb : IModel, IEntityManager, IDisposable
{
    private const string NotesCollection = "notes";
    private const string TagsCollection = "tags";

    private readonly LiteDatabase database;
    private readonly ILiteCollection<NoteDocument> notes;
    private readonly ILiteCollection<TagDocument> tags;

    public ModelLiteDb(IPlatform platform)
    {
        Directory.CreateDirectory(platform.AppData);
        database = new LiteDatabase(Path.Combine(platform.AppData, "data.db"));
        notes = database.GetCollection<NoteDocument>(NotesCollection);
        tags = database.GetCollection<TagDocument>(TagsCollection);
        notes.EnsureIndex(note => note.TagIds);
        tags.EnsureIndex(tag => tag.TagIds);
    }

    public IEntityManager eman => this;

    public void AddTag(IEntity entity, ITag tag) => UpdateTagMembership(entity, tag, add: true);

    public void RemoveTag(IEntity entity, ITag tag) => UpdateTagMembership(entity, tag, add: false);

    public void DeleteEntity(IEntity entity)
    {
        if (entity is not EntityDbModel DbModel) return;

        var entityId = DbModel.Id;
        foreach (var note in notes.FindAll().Where(note => note.TagIds.Remove(entityId))) notes.Update(note);
        foreach (var tag in tags.FindAll().Where(tag => tag.TagIds.Remove(entityId))) tags.Update(tag);

        if (DbModel is NoteDbModel) notes.Delete(entityId);
        else if (DbModel is TagDbModel) tags.Delete(entityId);
    }

    public IEntity UpdateEntity(IEntity entity)
    {
        if (entity is NoteDbModel note) notes.Upsert(note.Document);
        else if (entity is TagDbModel tag) tags.Upsert(tag.Document);
        return entity;
    }

    public T CreateEntity<T>() where T : IEntity
    {
        var document = EntityDocumentFactory.Create<T>(DateTime.Now);
        return (T)(IEntity)(document switch
        {
            NoteDocument note => new NoteDbModel(this, note),
            TagDocument tag => new TagDbModel(this, tag),
            _ => throw new NotSupportedException($"Unsupported entity type {typeof(T).Name}.")
        });
    }

    public IEnumerable<IEntity> GetEntities() =>
        notes.FindAll().Select(note => (IEntity)new NoteDbModel(this, note))
            .Concat(tags.FindAll().Select(tag => (IEntity)new TagDbModel(this, tag)));

    public void Dispose() => database.Dispose();

    private void UpdateTagMembership(IEntity entity, ITag tag, bool add)
    {
        if (entity is not EntityDbModel entityDbModel || tag is not TagDbModel tagDbModel) return;

        var tagIds = entityDbModel.Document.TagIds;
        if (add && !tagIds.Contains(tagDbModel.Id)) tagIds.Add(tagDbModel.Id);
        if (!add) tagIds.Remove(tagDbModel.Id);
        UpdateEntity(entityDbModel);
    }

    private IEnumerable<ITag> GetTags(EntityDocument document) =>
        tags.Find(tag => document.TagIds.Contains(tag.Id))
            .Select(tag => (ITag)new TagDbModel(this, tag));

    private abstract class EntityDbModel(ModelLiteDb model, EntityDocument document) : IEntity
    {
        protected ModelLiteDb Model { get; } = model;
        internal EntityDocument Document { get; } = document;
        public Guid Id => Document.Id;
        public string name { get => Document.Name; set => Document.Name = value; }
        public DateTime created => Document.Created;
        public IEnumerable<ITag> tags => Model.GetTags(Document);

        public override bool Equals(object? obj) => obj is EntityDbModel other && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }

    private sealed class NoteDbModel(ModelLiteDb model, NoteDocument document) : EntityDbModel(model, document), INote
    {
        internal new NoteDocument Document => (NoteDocument)base.Document;
        public string text { get => Document.Text; set => Document.Text = value; }
    }

    private sealed class TagDbModel(ModelLiteDb model, TagDocument document) : EntityDbModel(model, document), ITag
    {
        internal new TagDocument Document => (TagDocument)base.Document;
    }

    private static class EntityDocumentFactory
    {
        public static EntityDocument Create<T>(DateTime created) where T : IEntity
        {
            EntityDocument document = typeof(T) switch
            {
                var type when type == typeof(INote) => new NoteDocument(),
                var type when type == typeof(ITag) => new TagDocument(),
                _ => throw new NotSupportedException($"Unsupported entity type {typeof(T).Name}.")
            };

            document.Id = Guid.NewGuid();
            document.Created = created;
            return document;
        }
    }
}
