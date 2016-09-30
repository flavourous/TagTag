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
        public INote editing { get; set; }
        public void SetNote(INote note, Action commit)
        {
            editing = note;
            this.commit = commit;
            OnPropertyChanged("editing");
        }
        Action commit;
        public NoteEditor()
        {
            Title = "TagTag: Note";
            BindingContext = this;
            var ename = new Entry { VerticalOptions = LayoutOptions.Start, Placeholder = "Name" };
            ename.SetBinding(Entry.TextProperty, "editing.name", BindingMode.TwoWay);
            

            var etext = new Editor
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
            };
            etext.SetBinding(Editor.TextProperty, "editing.text", BindingMode.TwoWay);

            ToolbarItems.Add(new ToolbarItem() { Text = "Save", Command = new Command(() => commit()) });

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children = { ename, etext }
            };
        }
    }
}
