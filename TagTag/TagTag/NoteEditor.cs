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

            // There is a bug in editor - when text becomes large, it denies action mode, because the 
            // pressed layer becomes a textview - says "does not support selection".  Think it should be
            // edittext...
            var etext = new Editor
            {
                VerticalOptions = LayoutOptions.Fill,
            };
            //etext.SetBinding(Editor.TextProperty, "editing.text", BindingMode.TwoWay);
            etext.Text = "Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text Long text";

            ToolbarItems.Add(new ToolbarItem() { Text = "Save", Command = new Command(() => commit()) });

            Content = new Grid
            {
                VerticalOptions = LayoutOptions.Fill,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                Children =
                {
                    ename,
                    etext.OnGrid(row:1),
                }
            };
        }
    }
}
