using System;

namespace ProceduralGraph.Converters;

/// <summary>
/// Defines a factory for creating and loading graph components associated with graph entities.
/// </summary>
public interface IGraphComponentFactory
{
    /// <summary>
    /// Gets the type of component produced by this factory.
    /// </summary>
    Type ComponentType { get; }

    /// <summary>
    /// Gets the type of model associated with the component produced by this factory.
    /// </summary>
    Type ComponentModelType { get; }

    /// <summary>
    /// Gets the entity type supported by this factory.
    /// </summary>
    Type EntityType { get; }

    /// <summary>
    /// Loads a graph component for the specified entity using the provided graph model.
    /// </summary>
    /// <param name="entity">The graph entity for which to load the component. Cannot be <see langword="null"/>.</param>
    /// <param name="model">The graph model that provides context for loading the component. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IGraphComponent"/> representing the loaded component for the specified entity and model.</returns>
    IGraphComponent Load(IGraphEntity entity, IGraphModel model);

    /// <summary>
    /// Creates a new graph component for the specified graph entity.
    /// </summary>
    /// <param name="entity">The graph entity for which to create a component. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IGraphComponent"/> instance representing the component associated with the specified entity.</returns>
    IGraphComponent Create(IGraphEntity entity);

    /// <summary>
    /// Retrieves the graph model associated with the specified graph component.
    /// </summary>
    /// <param name="component">The graph component for which to retrieve the associated model. Cannot be <see langword="null"/>.</param>
    /// <returns>The graph model associated with the specified component.</returns>
    IGraphModel GetModel(IGraphComponent component);
}
