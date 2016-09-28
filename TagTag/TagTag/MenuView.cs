using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    class cc : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(targetType == typeof(String))
                return value is ITag ? "[T]" : "[N]";
            return value is ITag ? Color.Default : Color.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MenuView : ContentView, ITagMenu
    {
        cc colc = new cc();
        public BoxView topBox = new BoxView();
        Label tree = new Label { Text = "→", VerticalOptions = LayoutOptions.Center };
        ListView menu;
        public IEnumerable<Func<Cell, MenuItem>> mi;
        public MenuView()
        {
            Button back = new Button { Text = "←", Command = new Command(() => MenuBack()) };
            menu = new ListView
            {
                HasUnevenRows = true,
                ItemTemplate = new DataTemplate(() =>
                {
                    Cell c = null;
                    if (usetick)
                    {
                        c = new SwitchCell();
                        c.SetBinding(SwitchCell.TextProperty, "entity.name");
                        c.SetBinding(SwitchCell.OnProperty, "ticked");
                    }
                    else
                    {
                        var imagesub = new Label
                        {
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.End,
                            FontAttributes = FontAttributes.Bold,
                        };
                        var title = new Label { VerticalTextAlignment = TextAlignment.Center };
                        title.SetBinding(Label.TextProperty, "entity.name");
                        imagesub.SetBinding(Label.TextProperty, "entity", BindingMode.Default, colc);
                        c = new ViewCell
                        {
                            View = new StackLayout
                            {
                                Orientation = StackOrientation.Horizontal,
                                Children = { imagesub, title },
                                Padding = new Thickness(10,5)
                            }
                        };
                    }
                    // injected items
                    if (mi != null) foreach (var m in mi) c.ContextActions.Add(m(c));
                    c.Tapped += C_Tapped;
                    return c;
                })
            };

            Grid.SetColumnSpan(topBox, 2);
            Grid.SetColumn(tree, 1);
            Grid.SetColumnSpan(menu, 2);
            Grid.SetRow(menu, 1);

            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width =  new GridLength(1,GridUnitType.Star) },
                },
                RowDefinitions =
                {
                    new RowDefinition{ Height= GridLength.Auto },
                    new RowDefinition{ Height=  new GridLength(1,GridUnitType.Star) }
                },
                Children = { topBox, tree, back, menu },
                RowSpacing = 0,
                ColumnSpacing =0
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
            this.tree.Text = "→" + String.Join("→", tree);
        }
    }
}