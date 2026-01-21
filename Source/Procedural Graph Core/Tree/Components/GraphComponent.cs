using System;
using System.Collections.Generic;
using System.Threading;

namespace ProceduralGraph.Entities.Components;

/// <summary>
/// Represents a base class for components within a graph structure that are associated with a model and an entity.
/// </summary>
/// <typeparam name="T">
/// The type of the model associated with the graph component. Must be a reference type with a parameterless constructor.
/// </typeparam>
public abstract class GraphComponent<T> : IGraphComponent where T : class, new()
{
    private static readonly EqualityComparer<T> _modelComparer = EqualityComparer<T>.Default;

    /// <inheritdoc/>
    public abstract Guid ID { get; }

    /// <inheritdoc/>
    public event Action<IGraphComponent>? Changed;

    private volatile T _model = new();
    /// <summary>
    /// Gets or sets the current model associated with this instance.
    /// </summary>
    /// <remarks>Setting this property raises the <see cref="Changed"/> event if the value changes.
    /// This property is thread-safe.</remarks>
    public T Model
    {
        get => _model;
        set
        {
            if (!_modelComparer.Equals(Interlocked.Exchange(ref _model, value), value))
            {
                Changed?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets the graph entity associated with this instance.
    /// </summary>
    public abstract IGraphEntity Entity { get; }
    IGraphEntity? IGraphNode.Parent => Entity;
}
