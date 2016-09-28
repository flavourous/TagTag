using SQLite;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;

namespace TagTag.Backend
{
    static class Ext
    {
        public static T EGet<T>(this SQLiteConnection c, Guid k) where T : BaseDB, new()
        {
            var dt = c.Table<T>().Where(d => d.pk == k);
            if (dt.Count() == 1) return dt.First();
            else return new T { committed = false, pk = k };
        }
        public static IEnumerable<Guid> EIds<T>(this SQLiteConnection c) where T : BaseDB
        {
            return from e in c.Table<T>() select e.pk;
        }
    }

    class BaseDB
    {
        [PrimaryKey, AutoIncrement]
        public int rowid { get; set; }

        [Indexed]
        public Guid pk { get; set; }

        public bool committed = true;

        public override bool Equals(object obj)
        {
            return obj is BaseDB && (obj as BaseDB).pk == pk;
        }
        public override int GetHashCode()
        {
            return pk.GetHashCode();
        }
        public static bool operator ==(BaseDB db1, BaseDB db2)
        {
            return db1.pk == db2.pk;
        }
        public static bool operator !=(BaseDB db1, BaseDB db2)
        {
            return db1.pk != db2.pk;
        }
    }

    [Table("Descriptors")]
    class Description : BaseDB
    {
        public DateTime created { get; set; }
        public string name { get; set; }
    }

    class DescriptorSystem
    {
        protected readonly SQLiteConnection conn;
        public DescriptorSystem(SQLiteConnection conn)
        {
            this.conn = conn;
            conn.CreateTable<Description>(CreateFlags.None);
        }
        public Description Get(Guid k) { return conn.EGet<Description>(k); }
        public IEnumerable<Guid> Get() { return conn.EIds<Description>(); }
        public void Delete(Description d)
        {
            if (d.committed) conn.Delete(d);
        }
        public void Update(Description d)
        {
            if (d.committed) conn.Update(d);
            else conn.Insert(d);
            d.committed = true;
        }
    }

    [Table("Notes")]
    class Note : BaseDB
    {
        public string text { get; set; }
    }

    class NoteSystem
    {
        readonly SQLiteConnection conn;
        public NoteSystem(SQLiteConnection conn)
        {
            this.conn = conn;
            conn.CreateTable<Note>(CreateFlags.None);
        }
        public IEnumerable<Guid> Get() { return conn.EIds<Note>(); }
        public Note Get(Guid k) { return conn.EGet<Note>(k); }
        public void Delete(Note n)
        {
            if (n.committed) conn.Delete(n);
        }
        public void Update(Note n)
        {
            if (n.committed) conn.Update(n);
            else conn.Insert(n);
            n.committed = true;
        }
    }

    [Table("Tags")]
    class Tag : BaseDB
    {
        public List<Guid> pending_add = new List<Guid>();
        public List<Guid> pending_remove = new List<Guid>();
    }

    [Table("TagTag")]
    class TagTag : BaseDB
    { 
        [Indexed]
        public Guid tag { get; set; }
        [Indexed]
        public Guid entity { get; set; }
    }

    class TagSystem
    {
        readonly SQLiteConnection conn;
        public TagSystem(SQLiteConnection conn)
        {
            this.conn = conn;
            conn.CreateTable<Tag>(CreateFlags.None);
            conn.CreateTable<TagTag>(CreateFlags.None);
        }

        public IEnumerable<Guid> Get() { return conn.EIds<Tag>(); }
        public Tag Get(Guid k) { return conn.EGet<Tag>(k); }
        public IEnumerable<Guid> Tags(Guid entity)
        {
            return from t in conn.Table<TagTag>()
                   where t.entity == entity
                   select t.tag;
        }
        public IEnumerable<Guid> Entities(Guid tag)
        {
            return from t in conn.Table<TagTag>()
                   where t.tag == tag
                   select t.entity;
        }
        public void Tag(Tag tag, Guid entity)
        {
            if(!tag.pending_remove.Remove(entity))
                tag.pending_add.Add(entity);
        }
        public void UnTag(Tag tag, Guid entity)
        {
            if(!tag.pending_add.Remove(entity))
                tag.pending_remove.Add(entity);
        }
        public void Delete(Tag t)
        {
            if (!t.committed) return;
            conn.Table<TagTag>().Delete(tt => tt.tag == t.pk);
            conn.Delete(t);
        }
        public void Update(Tag t)
        {
            if (!t.committed) conn.Insert(t);
            else conn.Update(t);
            t.committed = true;
            conn.Table<TagTag>().Delete(d => t.pending_remove.Contains(d.tag));
            conn.InsertAll(from c in t.pending_add
                           select new TagTag { tag = t.pk, entity = c });
            t.pending_add.Clear();
            t.pending_remove.Clear();
        }
    }

    static class Systems
    {
        public class SRef { public Func<IEnumerable<Guid>> All; public Func<Guid, IEntity> Get; }
        public static Dictionary<Type, SRef> active_systems { get; private set; }

        public static DescriptorSystem s_deets { get; private set; }
        public static TagSystem s_tags { get; private set; }
        public static NoteSystem s_notes { get; private set; }
        public static void Initalize(SQLiteConnection conn)
        {
            if (s_deets != null) return;
            s_deets = new DescriptorSystem(conn);
            s_notes = new NoteSystem(conn);
            s_tags = new TagSystem(conn);

            active_systems = new Dictionary<Type, SRef>
            {
                { typeof(INote), new SRef { All = Systems.s_notes.Get, Get = k => new NoteVM(k) } },
                { typeof(ITag), new SRef { All = Systems.s_tags.Get, Get = k => new TagVM(k) } },
            };
        }
    }

    abstract class EntityVM : IEntity
    {
        public override int GetHashCode()
        {
            return pk.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is EntityVM && (obj as EntityVM).pk == pk;
        }

        readonly Description deet;
        public readonly Guid pk;
        protected EntityVM(Guid k)
        {
            this.pk = k;
            this.deet = Systems.s_deets.Get(k);
            deet.created = DateTime.Now;
        }

        public virtual void Update() { Systems.s_deets.Update(deet); }
        public virtual void Delete()
        {
            // clear old tags.
            foreach (var t in from k in Systems.s_tags.Tags(pk)
                              select Systems.s_tags.Get(k))
                Systems.s_tags.UnTag(t, pk);
            Systems.s_deets.Delete(deet);
        }
        public String name { get { return deet.name; } set { deet.name = value; } }
        public DateTime created { get { return deet.created; } set { deet.created = value; } }
        public IEnumerable<ITag> tags { get { return from t in Systems.s_tags.Tags(pk) select new TagVM(t); } }
    }
    class TagVM : EntityVM, ITag
    {
        readonly Tag tag;
        public TagVM(Guid k) : base(k)
        {
            tag = Systems.s_tags.Get(k);
        }
        public void Tag(EntityVM entity) { Systems.s_tags.Tag(tag, entity.pk); }
        public void UnTag(EntityVM entity) { Systems.s_tags.Tag(tag, entity.pk); }
        public override void Delete()
        {
            base.Delete();
            Systems.s_tags.Delete(tag);
        }
        public override void Update()
        {
            base.Update();
            Systems.s_tags.Update(tag);
        }
    }
    class NoteVM : EntityVM, INote
    {
        readonly Note rnote;
        public NoteVM(Guid k) : base(k)
        {
            rnote = Systems.s_notes.Get(k);
        }
        public String text { get { return rnote.text; } set { rnote.text = value; } }
        public override void Delete()
        {
            base.Delete();
            Systems.s_notes.Delete(rnote);
        }
        public override void Update()
        {
            base.Update();
            Systems.s_notes.Update(rnote);
        }
    }
    

    class ModelSQLite : IModel, IEntityManager
    {
        

        public ModelSQLite(IPlatform platform)
        {
            
            var db = Path.Combine(platform.AppData, "data.db");
#if DEBUG
            platform.DeleteFile(db);
#endif
            var conn = new SQLiteConnection(platform.sqlite, db, true);
            Systems.Initalize(conn);
        }

        public IEntityManager eman { get { return this; } }
        public void AddTag(IEntity entity, ITag tag)
        {
            (tag as TagVM).Tag(entity as EntityVM);
        }
        public void RemoveTag(IEntity entity, ITag tag)
        {
            (tag as TagVM).UnTag(entity as EntityVM);
        }
        public void DeleteEntity(IEntity d)
        {
            (d as EntityVM).Delete();
        }
        public IEntity UpdateEntity(IEntity e)
        {
            (e as EntityVM).Update();
            return e;
        }
        public T CreateEntity<T>(object id) where T : IEntity
        {
            var system = Systems.active_systems[typeof(T)];
            return (T)system.Get(Guid.NewGuid());
        }
        public IEnumerable<IEntity> GetEntities()
        {
            IEnumerable<IEntity> ret = new IEntity[0];
            foreach (var s in Systems.active_systems.Values)
                ret = ret.Concat(from e in s.All() select s.Get(e));
            return ret;
        }
    }
}
