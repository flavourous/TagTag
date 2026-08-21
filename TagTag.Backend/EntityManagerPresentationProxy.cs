using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    class EManProxy(IModel model, IEntityHooks menuPresenter, Action changed) : IEntityManager
    {
        public T CreateEntity<T>() where T : IEntity
        {
            var ret = model.eman.CreateEntity<T>();
            foreach (var tag in menuPresenter.filter) model.AddTag(ret, tag);
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
