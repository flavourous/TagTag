using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    class EManProxy : IEntityManager
    {
        public event Action changed = delegate { };
        readonly IModel model;
        readonly Func<ITag> createTag;
        public EManProxy(IModel model, Func<ITag> createTag)
        {
            this.model = model;
            this.createTag = createTag;
        }
        public T CreateEntity<T>() where T : IEntity
        {
            var ret = model.eman.CreateEntity<T>();
            var ct = createTag();
            if (ct != null) model.AddTag(ret, ct);
            changed();
            return ret;
        }
        public void DeleteEntity(IEntity e)
        {
            model.eman.DeleteEntity(e);
            changed();
        }
        public IEntity UpdateEntity(IEntity e)
        {
            var ret = model.eman.UpdateEntity(e);
            changed();
            return ret;
        }
    }
}
