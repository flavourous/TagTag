using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public partial class NoteViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel, IDisposable
{
    public string? UrlPathSegment => "";
    public IScreen HostScreen => screen;

    public string Date => noteItem.entity.created.ToString("d");
    [Reactive] private string _name;
    [Reactive] private string _text;

    public string EName => noteItem.entity.name;
    public string EText => (noteItem.entity as INote).text;

    private readonly IEntityItem<IEntity> noteItem;
    private readonly IEntityRepository repo;
    private readonly IScreen screen;
    private readonly TagCloudViewModel cloud;
    private readonly System.IObservable<bool> canSave;

    public NoteViewModel(IEntityItem<IEntity> noteItem, IEntityRepository repo,
        IScreen screen, TagCloudViewModel Cloud)
    {
        this.noteItem = noteItem;
        this.repo = repo;
        this.screen = screen;
        cloud = Cloud;
        _name = noteItem.entity.name;
        _text = (noteItem.entity as INote).text;
        Back = ReactiveCommand.CreateFromObservable(() => screen.Router.NavigateBack.Execute());

        canSave = this.WhenAnyValue(
            x => x.Name, x => x.Text,
            x => x.EName, x => x.EText,
            (n, t, en, et) => n != en || t != et);

        IDisposable subscription = null;
        subscription = screen.Router.CurrentViewModel
            .Subscribe(c =>
            {
                if(c == this && _expectingNav)
                {
                    _expectingNav = false;
                    noteItem.tagging.Value = false;
                    cloud.CanMutate = true;
                    cloud.IsTagging = false;
                }

                if(c is MainViewModel) subscription.Dispose();
            });
    }

    public ICommand Back { get; }

    private bool _expectingNav;

    [ReactiveCommand]
    public void Tag()
    {
        noteItem.tagging.Value = true;
        cloud.CanMutate = false;
        cloud.IsTagging = true;
        _expectingNav = true;
        screen.Router.Navigate.Execute(cloud);
    }

    public System.IObservable<bool> CanSave => canSave;

    public ViewModelActivator Activator => new();

    [ReactiveCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        var note = noteItem.entity as INote;
        note.name = Name;
        note.text = Text;
        repo.UpdateEntity(note);
        this.RaisePropertyChanged(nameof(EName));
        this.RaisePropertyChanged(nameof(EText));
    }

    [ReactiveCommand]
    public void Delete()
    {
        repo.DeleteEntity(noteItem.entity);
        screen.Router.NavigateBack.Execute();
    }

    private CompositeDisposable d = new();
    public void Dispose() => d.Dispose();
}
