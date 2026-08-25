using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public class EntityObservableViewModel<T> : ReactiveObject
{
    private readonly Backend.IObservable<T> Observable;
    public EntityObservableViewModel(Backend.IObservable<T> observable)
    {
        Observable = observable;
        observable.Observe = _ => 
        {
            this.RaisePropertyChanged(nameof(Enabled));
            this.RaisePropertyChanged(nameof(Value));
        };
    }

    public bool Enabled => Observable.Value is not null;
    public T Value 
    {
        get => Observable.Value;
        set
        {
            this.RaisePropertyChanging(nameof(Enabled));
            this.RaisePropertyChanging(nameof(Value));
            Observable.Value = value; 
            this.RaisePropertyChanged(nameof(Enabled));
            this.RaisePropertyChanged(nameof(Value));
        }
    }
}
