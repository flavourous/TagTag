using System;
using System.Collections.Generic;

namespace TagTag.Backend
{
    public class HierarchicalMenuPresentationStrategy : IMenuPresentationStrategy
    {
        readonly bool tagsonly;
        public HierarchicalMenuPresentationStrategy(bool tagsonly) { this.tagsonly = tagsonly; }
        public IEnumerable<IMenuItem> GetItems(ITag root, IEnumerable<IEntity> models, IEntityHooks hooks)
        {
            throw new NotImplementedException();
        }
    }
}