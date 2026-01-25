#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace ProceduralGraph;

/// <summary>
/// Defines a component within a graph that supports asynchronous generation and notifies listeners when its state
/// changes.
/// </summary>
public interface IGraphComponent : IGraph
{
    /// <summary>
    /// Gets the <see cref="IGraphEntity"/> associated with this component.
    /// </summary>
    IGraphEntity Entity { get; }
    IGraph? IGraph.Parent => Entity;

    /// <summary>
    /// Occurs when the state of the component changes.
    /// </summary>
    event Action<IGraphComponent>? Changed;

    int IReadOnlyCollection<IGraph>.Count => 0;

    IEnumerator<IGraph> IEnumerable<IGraph>.GetEnumerator()
    {
        IEnumerable<IGraph> empty = [];
        return empty.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}