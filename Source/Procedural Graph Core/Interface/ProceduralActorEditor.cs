using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace ProceduralGraph.Interface;

/// <summary>
/// Custom editor base class for actors that are represented or managed by a procedural graph node.
/// Provides integration between the Flax Editor UI and the procedural graph lifecycle.
/// </summary>
public abstract class ProceduralActorEditor : GenericEditor
{
    /// <summary>
    /// Gets the manager responsible for handling the synchronization and lifecycle of graph-linked objects.
    /// </summary>
    protected GraphLifecycleManager? LifecycleManager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProceduralActorEditor"/> class and retrieves the <see cref="GraphLifecycleManager"/> plugin.
    /// </summary>
    public ProceduralActorEditor() : base()
    {
        LifecycleManager = PluginManager.GetPlugin<GraphLifecycleManager>();
    }

    /// <summary>
    /// Attempts to find the procedural graph node associated with the actor currently being edited.
    /// </summary>
    /// <param name="node">When this method returns, contains the associated <see cref="IGraphNode"/> if found; otherwise, null.</param>
    /// <returns><c>true</c> if a corresponding node was found in the lifecycle manager; otherwise, <c>false</c>.</returns>
    protected bool TryFindNode([NotNullWhen(true)] out IGraphNode? node)
    {
        if (LifecycleManager != null && LifecycleManager.TryFindNode(Values.FirstOrDefault() as Actor, out node))
        {
            return true;
        }

        node = default;
        return false;
    }
}