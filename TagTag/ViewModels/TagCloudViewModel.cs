using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed partial class TagCloudViewModel : ReactiveObject, ITagMenu
{
    public IEntityRepository? Eman { get; set; }

    [Reactive] private ObservableCollection<TagItemViewModel> _items = [];
    public IEntity tagging { get; set; }
    public void SetItems(IEnumerable<IEntityItem<ITag>> items)
    {
        Items = [.. items.Select(x => new TagItemViewModel(x, Eman))];
    }
}
