using LibXamHelp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TagTag
{
    class SI
    {
        public String Name { get; set; }
        public String Sub { get; set; }
        public Action Act { get; set; }
    }
    public class Settings : ContentPage
    {
        readonly ListView mlv;
        public Settings()
        {
            
            mlv = new ListView
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                ItemTemplate = new DataTemplate(() =>
                {
                    var tc = new TextCell();
                    tc.SetBinding(TextCell.TextProperty, new Binding("Name"));
                    tc.SetBinding(TextCell.DetailProperty, new Binding("Sub"));
                    return tc;
                }),
                ItemsSource = new List<SI>
                {
                    new SI
                    {
                        Name = "EULA",
                        Sub = "Tap to view",
                        Act = () => EULA.Display(this, false)
                    }
                }
            };
            Title = "Settings";
            Content = mlv;
            mlv.ItemTapped += Settings_ItemTapped;
        }

        private void Settings_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            (e.Item as SI).Act();
            mlv.SelectedItem = null;
        }
    }
}
