using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    public class Presenter
    {
        // Static initator.  Could be Singleton.
        public static void Start(IView initiator, IPlatform platform)
        {
            Start(initiator, platform, new ModelLiteDb(platform));
        }

        internal static void Start(IView initiator, IPlatform platform, IEntityRepository repo)
        {
            var presenter = new PresenterImpl(repo, initiator, platform);
            presenter.Present();
        }

        class PresenterImpl(IEntityRepository repo, IView view, IPlatform platform)
        {
            // Run presentation.
            TagCloudPresenter tagMenu;
            TaggingPresenter taggingPresenter;
            public void Present()
            {
                taggingPresenter = new TaggingPresenter(repo, () => Refresh(false));
                tagMenu = new TagCloudPresenter(view.cloud, repo, taggingPresenter, () => Refresh(true));
                view.entities = new EManProxy(repo, tagMenu, () => Refresh(false));
                Refresh(false);
            }

            private void Refresh(bool tagFilterOnly)
            {
                if(!tagFilterOnly)
                {
                    // entity data change, could be delete
                    taggingPresenter.Clear();
                    tagMenu.Refresh();
                }
                
                bool Match(IEntity e) => tagMenu.filter.Any() switch
                {
                    true => tagMenu.filter.All(e.tags.Contains),
                    false => !e.tags.Any()
                };

                var entities = repo.GetEntities().Where(x => x is not ITag && Match(x));
                view.SetDetailItems(entities.Select(x => new EntityItem<IEntity>(x)
                {
                    selected = new Observable<bool?>(null, delegate { }),
                    tagging = taggingPresenter.Tagging(x),
                    tagged = new Observable<bool?>(null, delegate { })
                }));
            }
        }
    }
}
