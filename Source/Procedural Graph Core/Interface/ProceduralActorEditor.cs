#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.Scripting;
using FlaxEngine;

namespace ProceduralGraph.Interface;

/// <summary>
/// Custom editor base class for actors that are represented or managed by a procedural graph entity.
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
    /// Attempts to find the procedural graph entity associated with the actor currently being edited.
    /// </summary>
    /// <param name="entity">When this method returns, contains the associated <see cref="IGraphEntity"/> if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a corresponding entity was found in the lifecycle manager; otherwise, <see langword="false"/>.</returns>
    protected bool TryFindEntity([NotNullWhen(true)] out IGraphEntity? entity)
    {
        if (LifecycleManager is { } && LifecycleManager.TryFind(Values.FirstOrDefault() as Actor, out Index<Actor, IGraphEntity>? index))
        {
            entity = index.FirstOrDefault();
            return entity is { };
        }

        entity = null;
        return false;
    }

    /// <summary>
    /// Creates a value container for accessing the collection of components associated with the specified graph entity.
    /// </summary>
    /// <param name="entity">
    /// The graph entity whose component collection will be exposed by the value container. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// A <see cref="CustomValueContainer"/> that provides access to the components of the specified entity as an
    /// observable collection.
    /// </returns>
    protected static CustomValueContainer ComponentCollectionValueContainer(IGraphEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        CollectionAttribute collectionAttribute = new()
        {
            Display = CollectionAttribute.DisplayType.Header
        };

        ScriptType type = new(typeof(ObservableCollection<IGraphComponent>));

        return new CustomValueContainer(type, (object instance, int index) => entity.Components, attributes: [collectionAttribute])
        {
            entity.Components
        };
    }
}