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
        ITagMenu IView.tagger { get { return tagger; } }
        public void SetDetailItems(IEnumerable<IEntity> items) { detail.ItemsSource = items; }

        readonly ListView detail;
        readonly MenuView menu, tagger;
        public App()
        {
            tagger = new MenuView
            {
            };
            menu = new MenuView { usetick = false };
            detail = new ListView
            {
                HasUnevenRows = true,
                ItemTemplate = new DataTemplate(() =>
                {
                    Label l = new Label(), t = new Label(), d = new Label();
                    // FIXME use grid. want xamhelperslib. usemore converters.
                    var sl = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        Children =
                        {
                            new StackLayout
                            {
                                Orientation = StackOrientation.Horizontal,
                                Children = {l, new  Label { Text = " - " }, d }
                            },
                            t
                        }
                    };
                    l.SetBinding(Label.TextProperty, "name");
                    t.SetBinding(Label.TextProperty, "text", BindingMode.Default, new Tnc());
                    d.SetBinding(Label.TextProperty, "created", BindingMode.Default, null, "dd/MM/yy");

                    var edit = new MenuItem { Text = "Edit" };
                    edit.Clicked += Edit_Clicked;
                    var delete = new MenuItem { Text = "Delete" };
                    delete.Clicked += Delete_Clicked;
                    return new ViewCell
                    {
                        View = sl,
                        ContextActions = { edit, delete }
                    };
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
                    Content = menu
                },
                Detail = new ContentPage
                {
                    Content = detail
                }
            };
            MainPage = new NavigationPage(mdp);
        }

        NoteEditor ned = new NoteEditor();

        private void Create_Clicked(object sender, EventArgs e)
        {
            var nn = eman.CreateEntity<INote>(menu.MenuID);
            EditNote(nn);
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            eman.DeleteEntity((sender as BindableObject).BindingContext as IEntity);
        }

        private void Edit_Clicked(object sender, EventArgs e)
        {
            var ent = (sender as BindableObject).BindingContext as IEntity;
            EditNote(ent as INote);
        }

        void EditNote(INote n)
        {
            ned.SetNote(n, () => eman.UpdateEntity(n));
            MainPage.Navigation.PushAsync(ned);
        }


        //void MenuMode(Object o)
        //{
        //    var md = MainPage as MasterDetailPage;
        //    md.MasterBehavior = md.MasterBehavior == MasterBehavior.Popover ? MasterBehavior.Split : MasterBehavior.Popover;
        //    sp.Text = md.MasterBehavior == MasterBehavior.Popover ? "Split" : "Popover";
        //}


        protected override void OnStart()
        {
            /* Handle when your app starts*/
            Presenter.Start(this);
        }
        protected override void OnSleep() {/* Handle when your app sleeps*/}
        protected override void OnResume() {/* Handle when your app resumes*/}
    }
}
