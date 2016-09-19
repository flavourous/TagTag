using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    public class MenuView : ContentView, ITagMenu
    {
        Label tree = new Label { Text = ">", VerticalOptions = LayoutOptions.Center };
        ListView menu;
        public IEnumerable<Func<Cell, MenuItem>> mi;
        public MenuView()
        {
            Button back = new Button { Text = "Back", Command = new Command(() => MenuBack()) };
            menu = new ListView
            {
                HasUnevenRows = true,
                ItemTemplate = new DataTemplate(() =>
                {
                    Cell c = null;
                    if (usetick)
                    {
                        Label l = new Label { HorizontalOptions = LayoutOptions.StartAndExpand, VerticalOptions = LayoutOptions.Center };
                        l.SetBinding(Label.TextProperty, "entity.name");
                        Switch s = new Switch { HorizontalOptions = LayoutOptions.End };
                        s.SetBinding(Switch.IsToggledProperty, "ticked");
                        c = new ViewCell
                        {
                            View = new StackLayout
                            {
                                Children = { l, s },
                                Orientation = StackOrientation.Horizontal
                            }
                        };
                    }
                    else
                    {
                        Label l = new Label();
                        l.SetBinding(Label.TextProperty, "entity.name");
                        c = new ViewCell { View = l };
                    }
                    // injected items
                    if (mi != null) foreach (var m in mi) c.ContextActions.Add(m(c));
                    c.Tapped += C_Tapped;
                    return c;
                })
            };

            Grid.SetColumn(back, 1);
            Grid.SetColumnSpan(menu, 2);
            Grid.SetRow(menu, 1);

            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width =  new GridLength(1,GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions =
                {
                    new RowDefinition{ Height= GridLength.Auto },
                    new RowDefinition{ Height=  new GridLength(1,GridUnitType.Star) }
                },
                Children = { tree, back, menu }
            };
        }

        private void C_Tapped(object sender, EventArgs e)
        {
            var ent = (sender as BindableObject).BindingContext as IMenuItem;
            ent.Activate();
        }

        bool usetick = false;
        public object MenuID { get; set; }
        public event Action MenuBack = delegate { };
        public event Action<IEntity> tagging = delegate { };
        public void OnTagging(IEntity en) { usetick = en != null; tagging(en); }
        public void SetMenuItems(IEnumerable<IMenuItem> items) { menu.ItemsSource = items; }
        public void SetTree(IEnumerable<string> tree)
        {
            this.tree.Text = ">" + String.Join(">", tree);
        }
    }
}