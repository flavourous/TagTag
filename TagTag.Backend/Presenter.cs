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
        IMenuPresentationStrategy menuStrategy;
        IDetailPresentationStrategy detailStrategy;

        // Only constructable by that static above.
        private Presenter(IModel model, IView view)
        {
            this.model = model;
            this.view = view;

            menuStrategy = new HierarchicalMenuPresentationStrategy(false);
            detailStrategy = new NoTagFlowingDetailStrategy();
        }

        // Run presentation.
        MenuPresenter main_menu;
        EManProxy model_proxy;
        void Present()
        {
            // main menu presenter
            main_menu = new MenuPresenter(view.menu, model, menuStrategy);
            main_menu.selected += Main_menu_selected;

            // attach some stuff
            model_proxy = new EManProxy(model, () => main_menu.head);
            model_proxy.changed += main_menu.Refresh;
            view.eman = model_proxy;

            // when tagging we do
            view.tagger += View_tagger;

            // use starting strategy to send menu
            main_menu.SelectEntity(null);

        }

        private void View_tagger(IEntity en, IMenu obj)
        {
            // so it's ready for data...but wtf are we tagging?!
            var tag_menu_presenter = new MenuPresenter(en, obj, model, new HierarchicalMenuPresentationStrategy(true));
            //fored strat.
            tag_menu_presenter.SelectEntity(null); // rooty?
            // yeah...carry on!
        }

        private void Main_menu_selected(IEntity en)
        {
            var entities = detailStrategy.GetEntities(en, model.GetEntities());
            view.SetDetailItems(entities);
        }
    }
}
