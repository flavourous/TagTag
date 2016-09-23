using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    class MockEM : IEntityManager
    {
        readonly Func<List<IEntity>> gl;
        public MockEM(Func<List<IEntity>> gl)
        {
            this.gl = gl;
        }
        Dictionary<Type, Func<EntBase>> creators = new Dictionary<Type, Func<EntBase>>
        {
            { typeof(INote), () => new MockNote() },
            {typeof(ITag), () => new MockTag() }
        };
        public event Action<IEntity> created;
        public T CreateEntity<T>(Object ident /* used only by presenter but didnt decorate */) where T : IEntity
        {
            var n = creators[typeof(T)]();
            n.IsCommitted = false;
            created?.Invoke(n);
            return  (T)(n as IEntity);
        }
        public event Action<IEntity> deleted;
        public void DeleteEntity(IEntity e)
        {
            gl().Remove(e);
            deleted?.Invoke(e);
        }
        public event Action<IEntity> updated;
        public IEntity UpdateEntity(IEntity e)
        {
            var eb = (e as EntBase);
            if (!eb.IsCommitted)
            {
                eb.IsCommitted = true;
                gl().Add(e);
            }
            updated?.Invoke(e);
            return e;
        }
    }
    static class Psh
    {
        public static T Push<T>(this T itm, ITag t) where T : EntBase
        {
            itm.Tags.Add(t);
            return itm;
        }
    }
    class EntBase : IEntity
    {
        public bool IsCommitted = true;
        public DateTime created { get { return DateTime.Now; } }
        public string name { get; set; }
        public List<ITag> Tags = new List<ITag>();
        public IEnumerable<ITag> tags { get { return Tags; } }
    }
    class MockNote : EntBase, INote
    {
        public string text { get; set; }
    }
    class MockTag : EntBase, ITag
    {

    }
    class MockModel : IModel
    {
        public readonly MockEM mem;
        public MockModel() { mem = new MockEM(() => entities); }
        public IEntityManager eman { get { return mem; } }
        List<IEntity> entities = new List<IEntity>();
        public IEnumerable<IEntity> GetEntities() { return entities; }

        public void AddTag(IEntity e, ITag t)
        {
            if (e == t) return;
            (e as EntBase).Tags.Add(t);
        }

        public void RemoveTag(IEntity e, ITag t)
        {
            (e as EntBase).Tags.Remove(t);
        }
    }
}
