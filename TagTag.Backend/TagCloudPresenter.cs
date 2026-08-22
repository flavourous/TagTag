using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    internal class TagCloudPresenter(ITagMenu view, IEntityRepository repo, TaggingPresenter taggingPresenter, Action changed)
    {
        private EntityItem<ITag>[] cloud { get; set; } = [];
        public IEnumerable<ITag> filter => cloud.Where(x => x.selected.Get() ?? false).Select(x => x.entity).ToArray();

        public void Refresh()
        {
            var sel = filter.ToHashSet();
            var tags = repo.GetEntities().OfType<ITag>();

            cloud = tags.Select(x => new EntityItem<ITag>(x)
            {
                selected = new (sel.Contains(x), _ => changed()),
                tagging = taggingPresenter.Tagging(x),
                tagged = taggingPresenter.Tagged(x)
            }).ToArray();
            
            taggingPresenter.SetSelectors(cloud.Select(x=>x.selected).ToArray());
            view.SetItems(cloud);
        }
    }
}
