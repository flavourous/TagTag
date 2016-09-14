using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    public class TaggerPage : ContentPage
    {
        public event Action<String> addTag = delegate { };
        public readonly MenuView menu = new MenuView();
        public TaggerPage()
        {
            Entry tagname = new Entry { Placeholder = "Enter tag name" };
            Button add = new Button { Text = "Add", Command = new Command(() => addTag(tagname.Text)) };

            Grid.SetColumn(add, 1);
            Grid.SetColumnSpan(menu, 2);
            Grid.SetRow(tagname, 1);
            Grid.SetRow(add, 1);

         

            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width =  new GridLength(1,GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions =
                {
                    new RowDefinition{ Height=  new GridLength(1,GridUnitType.Star) },
                    new RowDefinition{ Height= GridLength.Auto }
                },
                Children = { menu, tagname, add }
            };
        }
        public void OnTagging(IEntity en)
        {
            menu.OnTagging(en);
            //set a title?:?
        }
    }
}
