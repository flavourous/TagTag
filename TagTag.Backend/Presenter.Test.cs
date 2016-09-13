using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TagTag.Backend
{
    class ManualView : IView
    {
        public IEntityManager eman { get; set; }
        public event Action<IEntity, IMenu> tagger = delegate { };
        public void FireTagger(IEntity en, IMenu menu) { tagger(en, menu); }
        public IMenu menu { get; } = new ManMan();
        public ManMan mm { get { return menu as ManMan; } }
        public IEnumerable<IEntity> detail = new IEntity[0];
        public void SetDetailItems(IEnumerable<IEntity> items) { detail = items; }
    }
    class ManMan : IMenu
    {
        public IEnumerable<IMenuItem> menu = new IMenuItem[0];
        public void SetMenuItems(IEnumerable<IMenuItem> rootitems) { menu = rootitems; }
        public event Action MenuBack = delegate { };
        public void GoMenuBack() { MenuBack(); }
        public void SetTree(IEnumerable<string> tree) { }
    }

	static class Hlp
    {
        public static T CreateAs<T>(this IEntityManager m, String name) where T : IEntity
        {
            var en = m.CreateEntity<T>();
            en.name = name;
            return (T)m.UpdateEntity(en);
        }
        public static T CreateAs<T>(this IModel m, String name, params ITag[] tags) where T :IEntity
        {
            var en = m.eman.CreateEntity<T>();
            en.name = name;
            foreach (var t in tags) m.AddTag(en, t);
            return (T)m.eman.UpdateEntity(en);
        }
    }

    class PresenterTester
    {
        public static MockModel GenerateMock()
        {
            var model = new MockModel();

            var t1 = model.CreateAs<ITag>("tag 1");
            var t2 = model.CreateAs<ITag>("tag 2");
            var t3 = model.CreateAs<ITag>("tag 3", t1);

            var n1 = model.CreateAs<INote>("note 1");
            var n2 = model.CreateAs<INote>("note 2");
            var n3 = model.CreateAs<INote>("note 3", t1);
            var n4 = model.CreateAs<INote>("note 4", t2);
            var n5 = model.CreateAs<INote>("note 5", t3);
            var n6 = model.CreateAs<INote>("note 6", t1, t2);
            return model;
        }

        readonly Func<IModel, IView, Presenter> create;
        public PresenterTester(Func<IModel, IView, Presenter> create) { this.create = create; }
        public void Run()
        {
            var view = new ManualView();
            var model = GenerateMock();

            var presenter = create(model, view);

			// Check dat initial setup
            Debug.Assert(view.mm.menu.Count() == 4);
            Debug.Assert(view.detail.Count() == 2);
            view.mm.menu.First().Activate(); // t1
            Debug.Assert(view.mm.menu.Count() == 3);
            Debug.Assert(view.detail.Count() == 2);
            view.mm.menu.First().Activate(); // t3
            Debug.Assert(view.mm.menu.Count() == 1);
            Debug.Assert(view.detail.Count() == 1);
            view.mm.GoMenuBack();
            Debug.Assert(view.mm.menu.Count() == 3);
            Debug.Assert(view.detail.Count() == 2);
            view.mm.GoMenuBack();
            Debug.Assert(view.mm.menu.Count() == 4);
            Debug.Assert(view.detail.Count() == 2);
            view.mm.GoMenuBack();
            Debug.Assert(view.mm.menu.Count() == 4);
            Debug.Assert(view.detail.Count() == 2);

            // picking flatly from allentities here
            var n2 = model.GetEntities().ElementAt(4) as INote;
            var n1 = model.GetEntities().ElementAt(3) as INote;
            var t1 = model.GetEntities().ElementAt(0) as ITag;

            // Test updating
            n2.name = "Totally - lol";
            n2.text = "HAH";
            view.eman.UpdateEntity(n2);
            Debug.Assert(view.mm.menu.ElementAt(3).entity.name == n2.name);

			// test deleting
            view.eman.DeleteEntity(n2);
            Debug.Assert(view.mm.menu.Count() == 3);
            Debug.Assert(view.detail.Count() == 1);

            // test adding notes
            view.eman.CreateAs<INote>("LOLZOR");
            view.eman.CreateAs<INote>("LOLZOR");
            view.eman.CreateAs<INote>("LOLZOR");
            Debug.Assert(view.mm.menu.Count() == 6);
            Debug.Assert(view.detail.Count() == 4);

            // hmm lets test running the tag menu
            var ttm = new ManMan();
            view.FireTagger(n1, ttm); // starts the tagpresenter!
            //should start rooted at root root...
            Debug.Assert(ttm.menu.All(a => a.entity is ITag));
            Debug.Assert(ttm.menu.All(a => !a.ticked));
            Debug.Assert(ttm.menu.Count() == 2);
            ttm.menu.ElementAt(0).ticked = true;
            Debug.Assert(ttm.menu.ElementAt(0).entity == t1); // cause lol test fail
            Debug.Assert(n1.tags.Contains(t1));
            // and does it present it back?
            var ttm2 = new ManMan();
            view.FireTagger(n1, ttm2); // starts the tagpresenter!
            Debug.Assert(ttm2.menu.ElementAt(0).ticked);
        }
    }
}
