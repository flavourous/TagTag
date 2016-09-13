using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    class NoTagFlowingDetailStrategy : IDetailPresentationStrategy
    {
        public IEnumerable<IEntity> GetEntities(IEntity selected, IEnumerable<IEntity> all)
        {
            if (selected is ITag)
                return from e in all where e.tags.Contains(selected as ITag) && !(e is ITag) select e;
            else if (selected == null)
                return from e in all where e.tags.Count() == 0 && !(e is ITag) select e;
            else
                return new IEntity[] { selected };
        }
    }
}