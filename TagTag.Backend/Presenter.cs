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
        public static void RunTests(IPlatform plat)
        {
            var pt = new PresenterTester((m, v) =>
            {
                var p = new Presenter(m, v, plat);
                p.Present();
                return p;
            }, plat);
            pt.Run();
        }

        // Static initator.  Could be Singleton.
        public static void Start(IView initiator, IPlatform platform)
        {
            // We'll get the remaining bits of the triad
            IModel model = new ModelSQLite(platform);

            Presenter presenter = new Presenter(model, initiator, platform);

            // then begin
            presenter.Present();
        }

        // Injected dependencies
        readonly IModel model;
        readonly IView view;

        // Currently used strategies.
        IMenuPresentationStrategy menuStrategy, taggerStrategy;
        IDetailPresentationStrategy detailStrategy;

        // Only constructable by that static above.
        readonly IPlatform platform;
        private Presenter(IModel model, IView view, IPlatform platform)
        {
            this.platform = platform;
            this.model = model;
            this.view = view;

            taggerStrategy = new HierarchicalMenuPresentationStrategy(true); // possibly looks like decorator instead
            menuStrategy = new HierarchicalMenuPresentationStrategy(false);  // of args there.
            detailStrategy = new NoTagFlowingDetailStrategy();
        }

        // Run presentation.
        MenuPresenter main_menu, tag_menu;
        EManProxy model_proxy;
        void Present()
        {
            
            // main menu presenter
            main_menu = new MenuPresenter(view.menu, model, menuStrategy);
            main_menu.selected += Main_menu_selected;

            // tag menu presenter
            tag_menu = new MenuPresenter(view.tagger, model, taggerStrategy);

            // attach some stuff
            LinkMenuPresenters(main_menu, tag_menu);
            model_proxy = new EManProxy(model);
            model_proxy.changed += main_menu.Refresh;
            model_proxy.changed += tag_menu.Refresh;
            view.eman = model_proxy;

            // use starting strategy to send menu
            main_menu.SelectEntity(null);
        }

        void LinkMenuPresenters(params MenuPresenter[] pres)
        {
            foreach (var p in pres)
                p.changed += () =>
                {
                    foreach (var pr in pres)
                        pr.Refresh();
                };
        }

        private void Main_menu_selected(IEntity en)
        {
            var entities = detailStrategy.GetEntities(en, model.GetEntities());
            view.SetDetailItems(entities);
        }
    }
}
