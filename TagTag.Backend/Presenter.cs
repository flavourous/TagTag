using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    public class Presenter
    {
        public static void RunTests()
        {
            var pt = new PresenterTester((m, v) =>
            {
                var p = new Presenter(m, v);
                p.Present();
                return p;
            });
            pt.Run();
        }

        // Static initator.  Could be Singleton.
        public static void Start(IView initiator)
        {
            // We'll get the remaining bits of the triad
            IModel model = PresenterTester.GenerateMock();

            Presenter presenter = new Presenter(model, initiator);

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
        private Presenter(IModel model, IView view)
        {
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
            model_proxy = new EManProxy(model);
            model_proxy.changed += main_menu.Refresh;
            model_proxy.changed += tag_menu.Refresh;
            view.eman = model_proxy;

            // use starting strategy to send menu
            main_menu.SelectEntity(null);
        }

        private void Main_menu_selected(IEntity en)
        {
            var entities = detailStrategy.GetEntities(en, model.GetEntities());
            view.SetDetailItems(entities);
        }
    }
}
