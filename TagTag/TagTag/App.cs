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
        public void SetDetailItems(IEnumerable<IEntity> items) { detail.ItemsSource = items; }

        void TagIt(IEntity en)
        {
            taggerPage.Title = "Tagging " + en.name;
            taggerPage.OnTagging(en);
            MainPage.Navigation.PushAsync(taggerPage);
        }
	void CreateOrEditTag(ITag tag)
	{
	    if(tag == null)
	    	tag = eman.CreateEntity<ITag>(menu.MenuID);

            ren.SetNote(tag.name, s => {
	    	tag.name = s;
		eman.UpdateEntity(tag);
	    });
            MainPage.Navigation.PushModalAsync(ren);
	}

        readonly ListView detail;
        readonly MenuView menu = new MenuView();
        readonly TaggerPage taggerPage = new TaggerPage();
        readonly NoteEditor ned = new NoteEditor();
	readonly Renamer ren = new Renamer();
        public App()
        {
            var tag = new MenuItem { Text = "Tag" };
            tag.Clicked += Tag_Clicked;
            var edit = new MenuItem { Text = "Edit" };
            edit.Clicked += Edit_Clicked;
            var delete = new MenuItem { Text = "Delete" };
            delete.Clicked += Delete_Clicked;
            
            menu.mi = new MenuItem[] { tag, edit, delete };
	    taggerPage.mi = new MenuItem[] { edit };
            taggerPage.addTag += TaggerPage_addTag;

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

                    return new ViewCell
                    {
                        View = sl,
                        ContextActions = { tag, edit, delete }
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
	    return bo as IEntity;
	}

        private void Tag_Clicked(object sender, EventArgs e)
        {
            TagIt(GetEnt(sender));
        }

        private void Create_Clicked(object sender, EventArgs e)
        {
	    var result = MainPage.DisplayActionSheet("Cancel", null, "Note", "Tag");
	    if(result == "Tag") CreateOrEditTag(null); // can use TagIt for multi-add
	    else if(result == "Note")
	    {
                var nn = eman.CreateEntity<INote>(menu.MenuID);
                EditNote(nn);
	    }
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            eman.DeleteEntity(GetEnt(sender));
        }

        private void Edit_Clicked(object sender, EventArgs e)
        {
	    var ent = GetEnt(sender);
	    if(ent is INote) EditNote(ent as INote);
	    else if(ent is ITag) CreateOrEditTag(ent as ITag);
        }

        void EditNote(INote n)
        {
            ned.SetNote(n, () => eman.UpdateEntity(n));
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
