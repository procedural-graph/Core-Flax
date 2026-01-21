using FlaxEngine;
using System;
using System.Collections.Generic;

namespace ProceduralGraph;

/// <summary>
/// Defines the contract for a node within a graph structure, providing access to its parent, descendants, and
/// serialization capabilities.
/// </summary>
public interface IGraphEntity : IGraphNode, IAsyncLifecycle
{
    /// <summary>
    /// Occurs when the state of the entity has changed.
    /// </summary>
    event Action? StateChanged;

    /// <summary>
    /// Occurs when the regeneration process is about to begin.
    /// </summary>
    event Action? Regenerating;

    /// <summary>
    /// Occurs after the entity has been regenerated.
    /// </summary>
    event Action? Regenerated;

    /// <summary>
    /// Gets the actor associated with this instance, if any.
    /// </summary>
    Actor? Actor { get; }

    /// <summary>
    /// Gets the graph entities parented to this entity, indexed by their unique identifiers and associated actors.
    /// </summary>
    Map<Guid, IGraphEntity, Guid, Actor> Entities { get; }

    /// <summary>
    /// Gets the Flax Engine <see cref="FlaxEngine.Actor"/>s parented to this entity, indexed by their unique identifiers and associated entities.
    /// </summary>
    Map<Guid, Actor, Guid, IGraphEntity> Actors { get; }

    /// <summary>
    /// Gets the collection of components attached to this graph entity.
    /// </summary>
    ICollection<IGraphComponent> Components { get; }
}
