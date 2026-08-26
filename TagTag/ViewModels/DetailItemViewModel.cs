using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TagTag.Backend;

namespace TagTag.ViewModels;

public sealed partial class DetailItemViewModel(IEntityItem<IEntity> entityItem, bool isFirst) : ReactiveObject
{
    public string Name { get; set; } = entityItem.entity.name;
    public string Date => entityItem.entity.created.ToString("d");
    public string Text { get; set; } = entityItem.entity switch
    {
        INote n => LineElipsis(n.text), 
        _ => throw new NotImplementedException()
    };
    public bool HasText => !string.IsNullOrWhiteSpace(Text);

    public IEntityItem<IEntity> EntityItem { get; } = entityItem;
    public bool IsFirst { get; } = isFirst;

    private static string LineElipsis(string text)
    {
        if(text is null) return null;
        var lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var t = string.Join(Environment.NewLine, lines.Take(2));
        return lines.Length > 2 ? t + "…" : t;
    }
}