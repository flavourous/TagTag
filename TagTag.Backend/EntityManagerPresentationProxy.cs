using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    class EManProxy(IEntityRepository repo, TagCloudPresenter menuPresenter, Action changed) : IEntityRepository
    {
        public IEnumerable<IEntity> GetEntities() => repo.GetEntities();

        public T CreateEntity<T>() where T : IEntity
        {
            var ret = repo.CreateEntity<T>();
            foreach (var tag in menuPresenter.filter) repo.AddTag(ret, tag);
            return ret;
        }

        public void DeleteEntity(IEntity e)
        {
            repo.DeleteEntity(e);
            changed();
        }

        public IEntity UpdateEntity(IEntity e)
        {
            var ret = repo.UpdateEntity(e);
            changed();
            return ret;
        }

        public void RemoveTag(IEntity entity, ITag tag)
        {
            repo.RemoveTag(entity, tag);
            changed();
        }

        public void AddTag(IEntity entity, ITag tag)
        {
            repo.AddTag(entity, tag);
            changed();
        }
    }
}
