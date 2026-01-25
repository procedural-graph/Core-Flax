#nullable enable
using FlaxEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProceduralGraph;

/// <summary>
/// Defines the contract for a node within a graph structure, providing access to its parent, descendants, and
/// serialization capabilities.
/// </summary>
public interface IGraphEntity : IGraph, IAsyncLifecycle
{
    /// <summary>
    /// Gets the unique identifier for this graph entity.
    /// </summary>
    Guid ID { get; }

    /// <summary>
    /// Gets the parent entity in the graph hierarchy, if one exists.
    /// </summary>
    new IGraphEntity? Parent { get; }
    IGraph? IGraph.Parent => Parent;

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
    /// Gets the graph entities parented to this entity, indexed by their unique identifiers and associated actors.
    /// </summary>
    Map<Guid, IGraphEntity, Guid, Actor> Entities { get; }

    /// <summary>
    /// Gets the Flax Engine <see cref="Actor"/>s parented to this entity, indexed by their unique identifiers and associated entities.
    /// </summary>
    Map<Guid, Actor, Guid, IGraphEntity> Actors { get; }

    /// <summary>
    /// Gets the collection of components attached to this graph entity.
    /// </summary>
    ICollection<IGraphComponent> Components { get; }

    /// <summary>
    /// Attempts to locate the specified actor and retrieve its associated index.
    /// </summary>
    /// <remarks>
    /// Searches this entity and all it's descendants.
    /// </remarks>
    /// <param name="actor">The actor to locate within the collection.</param>
    /// <param name="result">
    /// When this method returns, contains the index associated with the specified actor if found; otherwise, <see langword="null"/>. 
    /// This parameter is passed uninitialized.
    /// </param>
    /// <returns>true if the actor was found and the index was retrieved; otherwise, false.</returns>
    bool TryFind([NotNullWhen(true)] Actor? actor, [NotNullWhen(true)] out Index<Actor, IGraphEntity>? result);

    int IReadOnlyCollection<IGraph>.Count => Components.Count + Entities.Count;

    IEnumerator<IGraph> IEnumerable<IGraph>.GetEnumerator()
    {
        foreach (IGraphComponent component in Components)
        {
            yield return component;
        }

        foreach (IGraphEntity entity in Entities.Values.Select(static i => i.Key).OrderBy(static e => e.ID))
        {
            yield return entity;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
