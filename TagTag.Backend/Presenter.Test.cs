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
            foreach (var t in tags)
            {
                m.AddTag(en, t);
                m.eman.UpdateEntity(t);
            }
            return (T)m.eman.UpdateEntity(en);
        }
    }

    class PresenterTester
    {
        public static IModel GenerateMock(IModel model = null)
        {
            if (model == null) model = new MockModel();

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
        readonly IPlatform plat;
        public PresenterTester(Func<IModel, IView, Presenter> create, IPlatform plat) { this.create = create; this.plat = plat; }
        public void Run()
        {
            plat.WriteLine("Loading Mock View");
            var view = new ManualView();
            plat.WriteLine("Loading Sqlite Model");
            var model = GenerateMock(new ModelSQLite(plat));
            plat.WriteLine("Loading Presenter");
            var presenter = create(model, view);
            plat.WriteLine("Ok!");

            plat.WriteLine("Checking initial menu setup");
            // Check dat initial setup
            Debug.Assert(view.mm.menu.Count() == 4);
            Debug.Assert(view.detail.Count() == 2);
            view.mm.menu.Where(m=>m.entity.name == "tag 1").First().Activate(); // t1
            Debug.Assert(view.mm.menu.Count() == 3);
            Debug.Assert(view.detail.Count() == 2);
            view.mm.menu.Where(m => m.entity.name == "tag 3").First().Activate(); // t3
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

            plat.WriteLine("OK! ");
            // picking flatly from allentities here
            var n2 = model.GetEntities().Where(f => f.name == "note 2").First() as INote;
            var n1 = model.GetEntities().Where(f => f.name == "note 1").First() as INote;
            var t1 = model.GetEntities().Where(f => f.name == "tag 1").First() as ITag;

            plat.WriteLine("Cheking enity update capability on menu");
            // Test updating
            n2.name = "Totally - lol";
            n2.text = "HAH";
            view.eman.UpdateEntity(n2);
            Debug.Assert(view.mm.menu.Count(f => f.entity.name == "Totally - lol") == 1);
            plat.WriteLine("OK!");

            plat.WriteLine("Testing Deletion");
            // test deleting
            view.eman.DeleteEntity(n2);
            Debug.Assert(view.mm.menu.Count() == 3);
            Debug.Assert(view.detail.Count() == 1);
            plat.WriteLine("Ok!");

            plat.WriteLine("Testing adding notes");
            // test adding notes
            view.eman.CreateAs<INote>(view.mm.MenuID,"LOLZOR");
            view.eman.CreateAs<INote>(view.mm.MenuID,"LOLZOR");
            view.eman.CreateAs<INote>(view.mm.MenuID,"LOLZOR");
            Debug.Assert(view.mm.menu.Count() == 6);
            Debug.Assert(view.detail.Count() == 4);
            plat.WriteLine("Ok!");

            plat.WriteLine("Cheking adding results in a default tag based on menu location");
            // adding gets default tag on normal menu
            view.mm.menu.Where(m => m.entity.name == "tag 1").First().Activate(); // t1
            var dtts = "Testing that we get the tag based on menu position";
            view.eman.CreateAs<INote>(view.mm.MenuID, dtts);
            Debug.Assert(view.mm.menu.Count(d => d.entity.name == dtts) == 1);

            plat.WriteLine("Testing the tagger menu layout");
            // hmm lets test running the tag menu
            var ttm = view.tm;
            ttm.OnTagging(n1);
            //should start rooted at root root...
            Debug.Assert(ttm.menu.All(a => a.entity is ITag));
            Debug.Assert(ttm.menu.All(a => !a.ticked));
            Debug.Assert(ttm.menu.Count() == 2);
            plat.WriteLine("Tagging something");
            ttm.menu.Where(d=>d.entity.Equals(t1)).First().ticked = true;
            Debug.Assert(n1.tags.Contains(t1));
            // and does it present it back?
            ttm.OnTagging(n1); // starts the tagpresenter!
            Debug.Assert(ttm.menu.Where(d => d.entity.Equals(t1)).First().ticked);
            plat.WriteLine("Un-Tagging something");
            ttm.menu.Where(d => d.entity.Equals(t1)).First().ticked = false;
            ttm.OnTagging(n1); // starts the tagpresenter!
            Debug.Assert(!ttm.menu.Where(d => d.entity.Equals(t1)).First().ticked);
            // check tag menu is organized correctly
            view.tm.menu.Where(m => m.entity.name == "tag 1").First().Activate(); // t1
            Debug.Assert(view.tm.menu.Count() == 1);
            plat.WriteLine("Checking new tag can be added in correct tag");
            // check added tag is tagged
            var tt = "Testing tag";
            view.eman.CreateAs<ITag>(view.tm.MenuID, tt);
            Debug.Assert(view.tm.menu.Count(d => d.entity.name == tt) == 1);
            plat.WriteLine("Ok");

            plat.WriteLine("Deleting everything and checking a solitary tag entity appears on menu");
            view.mm.GoMenuBack(); // root plz! 
            // Now we delete everything!
            while (view.mm.menu.Count() > 0)
            {
                var n = view.mm.menu.Count();
                var ent = view.mm.menu.First().entity;
                view.eman.DeleteEntity(ent);
                var a = view.mm.menu.Count();
                plat.WriteLine(String.Format(" --> {0} to {1}, deleted {2}", n, a, ent.GetType().Name + ":" + ent.name));
            }
            Debug.Assert(view.mm.menu.Count() == 0); // cause lets check! also, the below problem manifests here but no letting us delete eveything.
            // and add one tag.
            view.eman.CreateAs<ITag>(null, "Root Tag");
            Debug.Assert(view.mm.menu.Count() == 1); // we had a bug where 1 tag would not be shown
            plat.WriteLine("Ok!");
        }
    }
}
