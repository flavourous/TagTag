using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    class IconConverter : IValueConverter
    {
        ImageSource t = ImageSource.FromResource("TagTag.Images.tag-icon.png");
        ImageSource n = ImageSource.FromResource("TagTag.Images.note-icon.png");
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ITag ? t : n;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MenuView : ContentView, ITagMenu
    {
        IconConverter iconConv = new IconConverter();
        BoxView topBox = new BoxView { BackgroundColor = Color.Silver.WithLuminosity(0.2) };
        Label tree = new Label { Text = "", VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.MiddleTruncation };
        Button back;
        ListView menu;
        public IEnumerable<Func<Cell, MenuItem>> mi;
        public MenuView()
        {
            
            back = new Button { Command = new Command(() => MenuBack()), WidthRequest = 40, HeightRequest = 40 };
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
                        var imagesub = new Image{ };
                        var title = new Label { VerticalTextAlignment = TextAlignment.Center };
                        imagesub.SetBinding(Image.SourceProperty, "entity", BindingMode.Default, iconConv);
                        title.SetBinding(Label.TextProperty, "entity.name");
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

            var backet = new Grid
            {
                WidthRequest = 40,
                HeightRequest = 40,
                Children =
                {
                    back,
                    new Image { Source = ImageSource.FromResource("TagTag.Images.back-icon.png"),
                    VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center }
                }
            };

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
                Children = { topBox, tree, backet, menu },
                RowSpacing = 0,
                ColumnSpacing =0
            };
        }

        private void C_Tapped(object sender, EventArgs e)
        {
            var ent = (sender as BindableObject).BindingContext as IMenuItem;
            this.back.IsEnabled = true;
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
            this.tree.Text = String.Join(" → ", tree);
            this.back.IsEnabled = tree.Count() > 0;
        }
    }
}