using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    class Tnc : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vs = value as String;
            return String.IsNullOrEmpty(vs) ? "<empty>" : vs;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class App : Application, IView
    {
        public IEntityManager eman { get; set; }

        IMenu IView.menu { get { return menu; } }
        ITagMenu IView.tagger { get { return taggerPage.menu; } }
        class DH
        {
            public DH(IEntity e){this.e = e;}
            public readonly IEntity e;
            public String namedate { get { return e.name + " - " + e.created.ToString("dd/MM/yyyy"); } }
            public String deets
            {
                get
                {
                    return e is INote ? (e as INote).text : "";
                }
            }
        }
        public void SetDetailItems(IEnumerable<IEntity> items) { detail.ItemsSource = from d in items select new DH(d); }

        void TagIt(IEntity en)
        {
            taggerPage.Title = "Tagging " + en.name;
            taggerPage.OnTagging(en);
            MainPage.Navigation.PushAsync(taggerPage);
        }

        void CEditTag(ITag tag)
        {
            if (tag == null)
                tag = eman.CreateEntity<ITag>(menu.MenuID);

            rv.Edit(tag.name, s =>
            {
                tag.name = s;
                eman.UpdateEntity(tag);
            });
        }

        readonly ListView detail;
        readonly MenuView menu = new MenuView();
        readonly TaggerPage taggerPage = new TaggerPage();
        readonly NoteEditor ned = new NoteEditor();

        class CEA  : EventArgs
        {
            public EventArgs inner;
            public Cell c;
        }
        Func<Cell, MenuItem> Generate(String name, EventHandler hand)
        {
            return c =>
            {
                var mi = new MenuItem { Text = name };
                mi.Clicked += (o, e) => hand(o, new CEA { inner = e, c = c });
                return mi;
            };
        }

        NRNameview rv = new NRNameview();
        public App()
        {
            var tag = Generate("Tag", Tag_Clicked);
            var edit = Generate("Edit", Edit_Clicked);
            var delete = Generate("Delete", Delete_Clicked);

            menu.mi = new [] { tag, edit, delete };
	        taggerPage.menu.mi = new [] { edit };
            taggerPage.addTag += TaggerPage_addTag;

            detail = new ListView
            {
                HasUnevenRows = true,
                ItemTemplate = new DataTemplate(() =>
                {
                    var c = new TextCell();
                    c.SetBinding(TextCell.TextProperty, "namedate");
                    c.SetBinding(TextCell.DetailProperty, "deets");
                    c.ContextActions.Add(tag(c));
                    c.ContextActions.Add(edit(c));
                    c.ContextActions.Add(delete(c));
                    return c;
                })
            };
            ToolbarItem create = new ToolbarItem { Text = "New" };
            create.Clicked += Create_Clicked;

            var mdp = new MasterDetailPage
            {
                ToolbarItems = { create },
                MasterBehavior = MasterBehavior.Split,
                Master = new ContentPage
                {
                    Title = "TagTag",
                    Content = new Grid {
                        Children = {
                            menu, rv
                        }
                    }
                },
                Detail = new ContentPage
                {
                    Content = detail
                }
            };
            MainPage = new NavigationPage(mdp);
        }

        

        private void TaggerPage_addTag(string obj)
        {
            var tag = eman.CreateEntity<ITag>(taggerPage.menu.MenuID);
            tag.name = obj;
            eman.UpdateEntity(tag);
        }

        IEntity GetEnt(Object sender)
        {
            var bo = (sender as BindableObject).BindingContext;
            if (bo is IMenuItem) bo = (bo as IMenuItem).entity;
            if (bo is DH) bo = (bo as DH).e;
            return bo as IEntity;
        }

        private void Tag_Clicked(object sender, EventArgs e)
        {
            TagIt(GetEnt(sender));
        }

        private async void Create_Clicked(object sender, EventArgs e)
        {
            var result = await MainPage.DisplayActionSheet("New", "Cancel", null, "Note", "Tag");
            if (result == "Tag") CEditTag(null);
            else if (result == "Note") CEditNote(null);
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            eman.DeleteEntity(GetEnt(sender));
        }

        private void Edit_Clicked(object sender, EventArgs e)
        {
            var ent = GetEnt(sender);
            if (ent is INote) CEditNote(ent as INote);
            else if (ent is ITag) CEditTag(ent as ITag);
        }

        void CEditNote(INote n)
        {
            if (n == null)
                n = eman.CreateEntity<INote>(menu.MenuID);

            ned.SetNote(n, () =>
            {
                eman.UpdateEntity(n);
                MainPage.Navigation.PopAsync();
            });
            MainPage.Navigation.PushAsync(ned);
        }

        protected override void OnStart()
        {
            /* Handle when your app starts*/
            Presenter.Start(this);
        }
        protected override void OnSleep() {/* Handle when your app sleeps*/}
        protected override void OnResume() {/* Handle when your app resumes*/}
    }
}
