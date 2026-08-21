using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    delegate void emcd(ITag removed = null);
    class EManProxy(IModel model, IMenu menu, IEntityHooks menuPresenter) : IEntityManager
    {
        public event emcd changed = delegate { };
        public T CreateEntity<T>() where T : IEntity
        {
            var ret = model.eman.CreateEntity<T>();
            var ct = menuPresenter.head;
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
            changed(e as ITag);
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
