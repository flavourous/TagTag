using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using TagTag.Backend;
using Xamarin.Forms;

namespace TagTag
{
    public class NoteEditor : ContentPage
    {
        public void SetNote(INote note, Action commit)
        {
            BindingContext = note;
            this.commit = commit;
        }
        Action commit;
        public NoteEditor()
        {
            var ename = new Entry { VerticalOptions = LayoutOptions.Start, Placeholder = "Name" };
            ename.SetBinding(Entry.TextProperty, "name", BindingMode.TwoWay);

            var etext = new Editor
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
            };
            etext.SetBinding(Editor.TextProperty, "text", BindingMode.TwoWay);

            ToolbarItems.Add(new ToolbarItem() { Text = "Save", Command = new Command(() => commit()) });

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children = { ename, etext }
            };
        }
    }
}
