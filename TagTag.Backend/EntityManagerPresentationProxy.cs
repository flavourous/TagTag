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
        public EManProxy(IModel model)
        {
            this.model = model;
        }
        public T CreateEntity<T>(Object id) where T : IEntity
        {
            var ret = model.eman.CreateEntity<T>(null);
            var ct = id as ITag;
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
