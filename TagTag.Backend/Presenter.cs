using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        internal static void Start(IView initiator, IPlatform platform, IModel model)
        {
            var presenter = new PresenterImpl(model, initiator, platform);
            presenter.Present();
        }

        class PresenterImpl(IModel model, IView view, IPlatform platform)
        {
            // Run presentation.
            TagCloudPresenter tagMenu;
            public void Present()
            {
                tagMenu = new TagCloudPresenter(view.tagger, model, Refresh);
                view.eman = new EManProxy(model, tagMenu, DataRefresh);
                DataRefresh();
            }

            private void DataRefresh()
            {
                Refresh();
                tagMenu.Refresh();
            }

            private void Refresh()
            {
                bool Match(IEntity e) => tagMenu.filter.Any() switch
                {
                    true => tagMenu.filter.All(e.tags.Contains),
                    false => !e.tags.Any()
                };

                var entities = model.GetEntities().Where(x => x is not ITag && Match(x));
                view.SetDetailItems(entities);
            }
        }
    }
}
