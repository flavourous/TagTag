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
        public readonly MenuView menu = new MenuView();
        public NRNameview rv = new NRNameview();
        public TaggerPage()
        {
            Content = new Grid
            {
                Children = { menu, rv }
            };
        }
        public void OnTagging(IEntity en)
        {
            menu.OnTagging(en);
        }
    }
}
