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
        public IMenu menu { get; } = new ManMan();
        public ManMan mm { get { return menu as ManMan; } }

        public ManMan tm { get; } = new ManMan();
        ITagMenu IView.tagger{get{return tm;}}

        public IEnumerable<IEntity> detail = new IEntity[0];
        public void SetDetailItems(IEnumerable<IEntity> items) { detail = items; }
    }
    class ManMan : ITagMenu
    {
        public IEnumerable<IMenuItem> menu = new IMenuItem[0];

        public object MenuID { get; set; }

        public void SetMenuItems(IEnumerable<IMenuItem> rootitems) { menu = rootitems; }
        public event Action MenuBack = delegate { };
        public event Action<IEntity> tagging = delegate { };
        public void OnTagging(IEntity t) { tagging(t); }

        public void GoMenuBack() { MenuBack(); }
        public void SetTree(IEnumerable<string> tree) { }
    }

	static class Hlp
    {
        public static T CreateAs<T>(this IEntityManager m, object root, String name) where T : IEntity
        {
            var en = m.CreateEntity<T>(root);
            en.name = name;
            return (T)m.UpdateEntity(en);
        }
        public static T CreateAs<T>(this IModel m, object root, String name, params ITag[] tags) where T :IEntity
        {
            var en = m.eman.CreateEntity<T>(root);
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

            var t1 = model.CreateAs<ITag>(null,"tag 1");
            var t2 = model.CreateAs<ITag>(null,"tag 2");
            var t3 = model.CreateAs<ITag>(null,"tag 3", t1);

            var n1 = model.CreateAs<INote>(null,"note 1");
            var n2 = model.CreateAs<INote>(null,"note 2");
            var n3 = model.CreateAs<INote>(null,"note 3", t1);
            var n4 = model.CreateAs<INote>(null,"note 4", t2);
            var n5 = model.CreateAs<INote>(null,"note 5", t3);
            var n6 = model.CreateAs<INote>(null,"note 6", t1, t2);
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
            view.eman.CreateAs<INote>(view.mm.MenuID,"LOLZOR");
            view.eman.CreateAs<INote>(view.mm.MenuID,"LOLZOR");
            view.eman.CreateAs<INote>(view.mm.MenuID,"LOLZOR");
            Debug.Assert(view.mm.menu.Count() == 6);
            Debug.Assert(view.detail.Count() == 4);

            // hmm lets test running the tag menu
            var ttm = view.tm;
            ttm.OnTagging(n1);
            //should start rooted at root root...
            Debug.Assert(ttm.menu.All(a => a.entity is ITag));
            Debug.Assert(ttm.menu.All(a => !a.ticked));
            Debug.Assert(ttm.menu.Count() == 2);
            ttm.menu.ElementAt(0).ticked = true;
            Debug.Assert(ttm.menu.ElementAt(0).entity == t1); // cause lol test fail
            Debug.Assert(n1.tags.Contains(t1));
            // and does it present it back?
            ttm.OnTagging(n1); // starts the tagpresenter!
            Debug.Assert(ttm.menu.ElementAt(0).ticked);
        }
    }
}
