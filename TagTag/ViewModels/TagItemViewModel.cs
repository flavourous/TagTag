using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed partial class TagItemViewModel(IEntityItem<ITag> tagItem, IEntityRepository man) : ReactiveObject
{
    public ITag Tag => tagItem.entity;
    public EntityObservableViewModel<bool?> Selected { get; } = new(tagItem.selected);
    public EntityObservableViewModel<bool?> Tagging { get; } = new(tagItem.tagging);
    public EntityObservableViewModel<bool?> Tagged { get; } = new(tagItem.tagged);
    public string Name { get; } = string.IsNullOrWhiteSpace(tagItem.entity.name) ? "<empty>" : tagItem.entity.name;

    [Reactive] public bool _isEditing;
    [ReactiveCommand] public void Save()
    {
        IsEditing = false;
        tagItem.entity.name = Name;
        man.UpdateEntity(tagItem.entity);
    }
}
