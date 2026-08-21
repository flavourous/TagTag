using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    class TagCloudPresenter(ITagMenu view, IModel model, Action changed) : IEntityHooks
    {
        IMenuItem<ITag>[] cloud { get; set; } = [];
        public IEnumerable<ITag> filter => cloud.Where(x => x.selected).Select(x => x.entity).ToArray();

        class TagMenuItem(ITag entity, bool selected, Action<bool> select) : IMenuItem<ITag>
        {
            public bool selected { get => field; set { field = value; select(value); } } = selected;
            public ITag entity { get; } = entity;
        }

        public void Refresh()
        {
            var sel = cloud.Where(x => x.selected).Select(x => x.entity).ToHashSet();
            var tags = model.GetEntities().OfType<ITag>();
            cloud = tags.Select(x => new TagMenuItem(x, sel.Contains(x), s => SelectEntity(x, s))).ToArray();
            view.SetItems(cloud);
        }

        private void SelectEntity(ITag tag, bool selected)
        {
            var t = view.tagging;

            if (t is not null)
            {
                var hasTag = t.tags.Contains(tag);
                if (hasTag) model.AddTag(t, tag);
                else model.RemoveTag(t, tag);
            }

            changed();
        }
    }
}
