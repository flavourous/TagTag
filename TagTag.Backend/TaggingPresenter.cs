using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    internal class TaggingPresenter(IEntityRepository repo, Action TaggingCompleted)
    {
        private IEntity beingTagged;
        private Dictionary<IEntity, Observable<bool?>> tagging = [];
        private Dictionary<IEntity, Observable<bool?>> tagged = [];
        private IEnumerable<Observable<bool?>> selectors = [];
        private Dictionary<Observable<bool?>, bool?> oldValues = [];

        public void Clear()
        {
            tagging.Clear();
            tagged.Clear();
        }

        public void SetSelectors(IEnumerable<Observable<bool?>> x)  => selectors = x;

        public Observable<bool?> Tagging(IEntity x) => tagging[x] = new(false, t =>
        {
            if (t is true)
            {
                beingTagged = x;
                oldValues = selectors.ToDictionary(x => x, x => x.Get());
                foreach(var s in selectors) s.SetAndRaise(null); // no filtering
            }
            else
            {
                beingTagged = null;
                foreach(var s in selectors) s.SetAndRaise(oldValues[s]);
            }

            foreach (var other in tagging.Keys.Where(k => !k.Equals(x)))
                tagging[other].SetAndRaise(t is true ? null : false);

            foreach (var other in tagged.Keys.Where(k => !k.Equals(x)))
                tagged[other].SetAndRaise(t is true ? x.tags.Contains(other) : null);

            if (t is not true) TaggingCompleted();
        });

        public Observable<bool?> Tagged(IEntity x) => tagged[x] = new(null, t =>
        {
            if (x is not ITag tag) return;
            if (t is true) repo.AddTag(beingTagged, tag);
            else repo.RemoveTag(beingTagged, tag);
        });
    }
}
