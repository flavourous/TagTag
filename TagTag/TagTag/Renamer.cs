using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    public class Renamer : ContentPage
    {
        readonly Entry en = new Entry();
        Action<String> commit = delegate { };
        public Renamer()
        {
            Button ok = new Button { Text = "Ok", Command = new Command(Ok) };
            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { en, ok }
            };
        }
        void Ok()
        {
            commit(en.Text);
            Navigation.PopAsync();
        }
        public void SetName(String name, Action<String> commit)
        {
            en.Text = name;
            this.commit = commit;
        }
    }
}
