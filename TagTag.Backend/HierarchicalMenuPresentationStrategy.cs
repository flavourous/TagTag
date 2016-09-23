using System;
using System.Collections.Generic;
using System.Linq;

namespace TagTag.Backend
{
    public class HierarchicalMenuPresentationStrategy : IMenuPresentationStrategy
    {
        class HMi : IMenuItem
        {
            readonly IEntityHooks hk;
            public HMi(IEntity en, IEntityHooks hk)
            {
                entity = en;
                _ticked = hk.isticked(en);
                this.hk = hk;
            }

            public IEntity entity { get; set; }
            bool _ticked = false;
            public bool ticked
            {
                get { return _ticked; }
                set
                {
                    if (value != _ticked)
                    {
                        _ticked = value;
                        hk.ticked(entity, value);
                    }
                }
            }
            public void Activate() { hk.activated(entity); }
        }
        readonly bool tagsonly;
        public HierarchicalMenuPresentationStrategy(bool tagsonly) { this.tagsonly = tagsonly; }
        public IEnumerable<IMenuItem> GetItems(ITag root, IEnumerable<IEntity> models, IEntityHooks hooks)
        {
            var ret = from m in models
                   where ((root == null && m.tags.Count() == 0) || m.tags.Contains(root)) && (!tagsonly || m is ITag)
                   select new HMi(m, hooks);
            if (!tagsonly && ret.Count() == 1 && ret.First().entity is ITag)
                return GetItems(ret.First().entity as ITag, models, hooks);
            return ret;
        }
    }
}