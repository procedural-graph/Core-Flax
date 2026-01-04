using System.Collections;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace ProceduralGraph.Interface;

/// <summary>
/// GraphModelCollectionEditor class.
/// </summary>
[CustomEditor(typeof(ObservableCollection<GraphModel>))]
internal sealed class GraphModelCollectionEditor : ListEditor
{
    private ObservableCollection<GraphModel> Target => (Values[0] as ObservableCollection<GraphModel>)!;

    public override int Count => Target.Count;

    protected override IList Allocate(int size)
    {
        return new ObservableCollection<GraphModel>(size);
    }

    protected override IList CloneValues()
    {
        return Target.Clone();
    }

    protected override void Resize(int newSize)
    {
        Target.Resize(newSize);
    }
}
