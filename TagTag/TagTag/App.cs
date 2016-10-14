using LibXamHelp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    public static class EXTS
    {
        public static View OnGrid(this View v, int row=0, int col=0, int rowspan=1, int colspan=1)
        {
            Grid.SetRow(v,row);
            Grid.SetColumn(v, col);
            Grid.SetRowSpan(v, rowspan);
            Grid.SetColumnSpan(v, colspan);
            return v;
        }
        public static String ToElipsis(this String s, int len)
        {
            String u =s;
            if (u == null) return null;
            var l1 = s.IndexOf(Environment.NewLine);
            if (l1 > -1) u = s.Substring(0, l1);
            if (u.Length <= len) return u;
            return u.Substring(0, len) + "…";
        }
        public static StackLayout StackBorder(this View view, StackOrientation o, double t, Color c)
        {
            var bv = new BoxView { BackgroundColor = c };
            if (o == StackOrientation.Horizontal)
            {
                view.HorizontalOptions = LayoutOptions.FillAndExpand;
                bv.WidthRequest = t;
            }
            else
            {
                view.VerticalOptions = LayoutOptions.FillAndExpand;
                bv.HeightRequest = t;
            }

            return new StackLayout
            {
                Spacing = 0,
                Orientation = o,
                Children = { view, bv }
            };
        }
    }

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
            public String name { get { return e.name; } }
            public String date { get {  return e.created.ToString("dd/MM/yyyy"); } }
            public String deets
            {
                get
                {
                    return e is INote ? (e as INote).text.ToElipsis(256) : "";
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

        void CEditTag(ITag tag, NRNameview rv, object id)
        {
            if (tag == null)
                tag = eman.CreateEntity<ITag>(id);

            rv.Edit(tag.name, s =>
            {
                tag.name = s;
                eman.UpdateEntity(tag);
            });

            mdp.IsPresented = true;
        }

        readonly ListView detail;
        readonly MenuView menu = new MenuView();
        readonly TaggerPage taggerPage = new TaggerPage();
        readonly NoteEditor ned = new NoteEditor();
        readonly Settings set = new Settings();

        class CEA  : EventArgs
        {
            public EventArgs inner;
            public NRNameview rv;
            public Object mid;
        }
        Func<Cell, MenuItem> Generate(String name, EventHandler hand, NRNameview rv, Object mid)
        {
            return c =>
            {
                var mi = new MenuItem { Text = name };
                mi.Clicked += (o, e) => hand(o, new CEA { inner = e, rv=rv, mid=mid });
                return mi;
            };
        }

        
        readonly MasterDetailPage mdp;
        NRNameview rv = new NRNameview();
        public App()
        {
            DependencyService.Register<IAppInfo, AppInfo>();
            var tag = Generate("Tag", Tag_Clicked,rv, menu.MenuID);
            var edit = Generate("Edit", Edit_Clicked,rv, menu.MenuID);
            var tedit = Generate("Edit", Edit_Clicked, taggerPage.rv, taggerPage.menu.MenuID);
            var delete = Generate("Delete", Delete_Clicked,rv, menu.MenuID);

            menu.mi = new [] { tag, edit, delete };
	        taggerPage.menu.mi = new [] { tedit };
            var ttn = new ToolbarItem { Text = "New" };
            ttn.Clicked += (o, e) => CEditTag(GetEnt(o) as ITag, taggerPage.rv, taggerPage.menu.MenuID);
            taggerPage.ToolbarItems.Add(ttn);
           

            detail = new ListView
            {
                HasUnevenRows = true,
                ItemTemplate = new DataTemplate(() =>
                {
                    Label title = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Fill,
                        LineBreakMode = LineBreakMode.TailTruncation
                    };
                    Label date = new Label
                    {
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        HorizontalTextAlignment = TextAlignment.End,
                        MinimumWidthRequest = 150,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = title.FontSize - 2.0
                    };
                    Label deets = new Label { FontSize = title.FontSize - 2.0 };
                    title.SetBinding(Label.TextProperty, "name");
                    date.SetBinding(Label.TextProperty, "date");
                    deets.SetBinding(Label.TextProperty, "deets");
                    return new ViewCell
                    {
                        View = new StackLayout
                        {
                            Orientation = StackOrientation.Vertical,
                            Children =
                            { 
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Horizontal,
                                    Spacing =0.0,
                                    Children = {title, date}
                                },
                                deets
                            },
                            Spacing = 2.5,
                            Padding = new Thickness(5.0),
                        },
                        ContextActions = { tag(null), edit(null), delete(null) },
                    };
                })
            };
            detail.ItemTapped += Detail_ItemTapped;
            ToolbarItem create = new ToolbarItem { Text = "New",  };
            create.Clicked += (o, e) => Create_Clicked(o, new CEA { inner = e, rv = rv, mid = menu.MenuID });

            ToolbarItem settings = new ToolbarItem { Text = "Settings",Order = ToolbarItemOrder.Secondary };
            settings.Clicked += (o, e) => SC();

            mdp = new MasterDetailPage
            {
                Title = "TagTag",
                ToolbarItems = { create, settings },
                MasterBehavior = MasterBehavior.Split,
                Master = new ContentPage
                {
                    Title = "TagTag",
                    Content = new Grid
                    {
                        Children = { menu, rv, },
                        BackgroundColor = Color.Silver.WithLuminosity(0.1)
                    }
                },
                Detail = new ContentPage
                {
                    Title = "TagTag",
                    Content = detail
                }
            };
            var np = new NavigationPage(mdp);
            MainPage = np;
        }

        private void Detail_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Edit_Clicked(e.Item, new CEA { inner = e, rv = this.rv, mid = menu.MenuID });
            detail.SelectedItem = null;
        }

        IEntity GetEnt(Object sender)
        {
            if (sender is DH) return (sender as DH).e;
            var bo = (sender as BindableObject).BindingContext;
            if (bo is IMenuItem) bo = (bo as IMenuItem).entity;
            if (bo is DH) bo = (bo as DH).e;
            return bo as IEntity;
        }

        private void Tag_Clicked(object sender, EventArgs e)
        {
            TagIt(GetEnt(sender));
        }

        async void SC()
        {
            await MainPage.Navigation.PushAsync(set);
        }

        private async void Create_Clicked(object sender, EventArgs e)
        {
            var result = await MainPage.DisplayActionSheet("New", "Cancel", null, "Note", "Tag");
            if (result == "Tag")
            {
                CEditTag(null, (e as CEA).rv, (e as CEA).mid);
            }
            else if (result == "Note") await CEditNote(null);
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            eman.DeleteEntity(GetEnt(sender));
        }

        private async void Edit_Clicked(object sender, EventArgs e)
        {
            var ent = GetEnt(sender);
            if (ent is INote) await CEditNote(ent as INote);
            else if (ent is ITag) CEditTag(ent as ITag, (e as CEA).rv, (e as CEA).mid);
        }

        async Task CEditNote(INote n)
        {
            if (n == null)
                n = eman.CreateEntity<INote>(menu.MenuID);

            ned.SetNote(n, () =>
            {
                eman.UpdateEntity(n);
                MainPage.Navigation.PopAsync();
            });

            await MainPage.Navigation.PushAsync(ned);
        }

        protected override void OnStart()
        {
            /* Handle when your app starts*/
            Presenter.Start(this, DependencyService.Get<IPlatform>());
            mdp.IsPresented = true;
            EULA.Display(MainPage, true);
        }
        protected override void OnSleep() {/* Handle when your app sleeps*/}
        protected override void OnResume() {/* Handle when your app resumes*/}
    }

    class AppInfo : IAppInfo
    {
        readonly IPlatform plat;
        public AppInfo()
        {
            plat = DependencyService.Get<IPlatform>(DependencyFetchTarget.GlobalInstance);
        }
        public string AppName { get { return "TagTag"; } }
        public int AppVersion { get { return plat.AppVersion; } }
    }
}
