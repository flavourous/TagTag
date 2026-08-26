using System.Collections.ObjectModel;
using System.Windows.Input;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public partial class NoteViewModel(IEntityItem<IEntity> noteItem, IEntityRepository repo,
    IScreen screen, Action Cloud) : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment => "";
    public IScreen HostScreen => screen;

    public string Name { get; set; } = noteItem.entity.name;
    public string Date => noteItem.entity.created.ToString("d");
    public string Text { get; set; } = (noteItem.entity as INote).text;
    public ICommand Back { get; } = screen.Router.NavigateBack;

    [ReactiveCommand]
    public void Tag()
    {
        noteItem.tagging.Value = true;
        Cloud();

        bool done = false;
        noteItem.tagging.Observe = x =>
        {
            if(done || x is not false) return;
            done = true;
            screen.Router.NavigateBack.Execute();
        };
    }

    [ReactiveCommand]
    public void Save()
    {
        var note = noteItem.entity as INote;
        note.name = Name;
        note.text = Text;
        repo.UpdateEntity(note);
    }

    [ReactiveCommand]
    public void Delete() => repo.DeleteEntity(noteItem.entity);
}
