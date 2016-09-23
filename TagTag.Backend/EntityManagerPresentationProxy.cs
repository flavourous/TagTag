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
            if (ct != null)
            {
                model.AddTag(ret, ct);
                rootTagsToUpdate[ret] = ct;
            }
            return ret;
        }
        public void DeleteEntity(IEntity e)
        {
            model.eman.DeleteEntity(e);
            changed();
        }
        Dictionary<IEntity, ITag> rootTagsToUpdate = new Dictionary<IEntity, ITag>();
        public IEntity UpdateEntity(IEntity e)
        {
            var ret = model.eman.UpdateEntity(e);
            if(rootTagsToUpdate.ContainsKey(e))
            {
                model.eman.UpdateEntity(rootTagsToUpdate[e]);
                rootTagsToUpdate.Remove(e);
            }
            changed();
            return ret;
        }
    }
}
