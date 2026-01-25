#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProceduralGraph;

/// <summary>
/// Represents a base class for components within a graph structure that are associated with a model and an entity.
/// </summary>
/// <typeparam name="TModel">
/// The type of the model associated with the graph component, must be a reference type that implements <see cref="IGraphModel"/>.
/// </typeparam>
public abstract class GraphComponent<TModel> : IGraphComponent where TModel : class, IGraphModel 
{
    /// <inheritdoc/>
    public event Action<IGraphComponent>? Changed;

    private TModel _model;
    /// <summary>
    /// Gets or sets the current model associated with this instance.
    /// </summary>
    /// <remarks>
    /// <para>Setting this property raises the <see cref="Changed"/> event if the value changes.</para>
    /// <para>This property is thread-safe.</para>
    /// </remarks>
    public TModel Model
    {
        get => _model;
        set => InterlockedRaiseAndSetIfChanged(ref _model, value);
    }

    /// <summary>
    /// Gets the graph entity associated with this instance.
    /// </summary>
    public abstract IGraphEntity Entity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphComponent{T}"/> class using the specified model.
    /// </summary>
    /// <param name="model">The model object that provides the data for the graph component. Cannot be <see langword="null"/>.</param>
    public GraphComponent(TModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
    }

    /// <summary>
    /// Atomically sets the specified field to a new value and raises the <see cref="Changed"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The reference type of the field to be updated.</typeparam>
    /// <param name="field">A reference to the field whose value will be updated if it differs from the new value.</param>
    /// <param name="newValue">The new value to assign to the field if it is different from the current value.</param>
    /// <returns><see langword="true"/> if the field was updated and the Changed event was raised; otherwise, <see langword="false"/>.</returns>
    protected bool InterlockedRaiseAndSetIfChanged<T>(ref T field, T newValue) where T : class
    {
        T oldValue = Interlocked.Exchange(ref field, newValue);

        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            Changed?.Invoke(this);
            return true;
        }

        return false;
    }
}
