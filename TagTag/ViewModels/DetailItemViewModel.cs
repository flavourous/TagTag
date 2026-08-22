using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed partial class DetailItemViewModel(IEntityItem<IEntity> entityItem, IEntityRepository man) : ReactiveObject
{
    public IEntity Entity => entityItem.entity;
    public string Name { get; set; } = string.IsNullOrWhiteSpace(entityItem.entity.name) ? "<empty>" : entityItem.entity.name;
    public string Date => entityItem.entity.created.ToString("d");
    public string Text { get; set; } = entityItem.entity is INote note ? note.text : "";
    public EntityObservableViewModel<bool?> Tagging { get; } = new(entityItem.tagging);

    [Reactive] public bool _isEditing, _isEditingDetail;
    [ReactiveCommand] public void BeginEditName() => IsEditing = true;
    [ReactiveCommand] public void BeginEditDetailCommand() => IsEditingDetail = true;
    [ReactiveCommand] public void Save()
    {
        IsEditingDetail = IsEditing = false;        
        Entity.name = Name;
        if(Entity is INote n) n.text = Text;
        man.UpdateEntity(Entity);
    }
}
