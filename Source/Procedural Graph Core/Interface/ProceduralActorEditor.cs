using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace ProceduralGraph.Interface;

[CustomEditor(typeof(Actor))]
internal sealed class ProceduralActorEditor : GenericEditor
{
    public override void Initialize(LayoutElementsContainer layout)
    {
        base.Initialize(layout);
        if (TryGetPlugin(out GraphLifecycleManager? client) && client.TryFindNode(Values.FirstOrDefault() as Actor, out IGraphNode? node))
        {
            var group = layout.Group("Procedural Generation");
            group.Property("Models", node.ValueContainer);
        }
    }

    private static bool TryGetPlugin<T>([NotNullWhen(true)] out T? plugin) where T : Plugin
    {
        plugin = PluginManager.GetPlugin<T>();
        return plugin != null;
    }
}
