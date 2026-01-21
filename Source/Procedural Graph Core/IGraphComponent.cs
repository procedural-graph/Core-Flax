using System;

namespace ProceduralGraph;

/// <summary>
/// Defines a component within a graph that supports asynchronous generation and notifies listeners when its state
/// changes.
/// </summary>
public interface IGraphComponent : IGraphNode
{
    /// <summary>
    /// Occurs when the state of the component changes.
    /// </summary>
    event Action<IGraphComponent>? Changed;
}