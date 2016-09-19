using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    public class NRNameview : ContentView
    {
        Entry ren;
        Grid rn;
        Action<String> commit;
        public void Edit(String init, Action<String> commit)
        {
            this.commit = commit;
            ren.Text = init;
            IsVisible = true;
        }
        private void Cancel_Clicked(object sender, EventArgs e)
        {
            IsVisible = false;
        }
        private void Ok_Clicked(object sender, EventArgs e)
        {
            IsVisible = false;
            commit(ren.Text);
        }
        public NRNameview()
        {
            ren = new Entry();
            Grid.SetColumnSpan(ren, 3);
            Button ok = new Button { Text = "Ok" };
            Grid.SetRow(ok, 1);
            Grid.SetColumn(ok, 2);
            Button cancel = new Button { Text = "Cancel" };
            Grid.SetRow(cancel, 1);
            ok.Clicked += Ok_Clicked;
            cancel.Clicked += Cancel_Clicked;

            var ig = new Grid
            {
                RowDefinitions = {
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Auto) },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto) },
                },
                Children = { ren, ok, cancel }
            };
            Grid.SetRow(ig, 1);
            Grid.SetColumn(ig, 1);
            rn = new Grid
            {
                BackgroundColor = Color.FromRgba(.2, .2, .2, .8),
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star) },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star) },
                },
                Children = { ig }
            };
            Content = rn;
            IsVisible = false;
        }
    }
}
