using FlaxEngine;
using System;

namespace ProceduralGraph.Converters;

/// <summary>
/// Defines a factory for creating and loading graph entity instances within a managed graph lifecycle.
/// </summary>
public interface IGraphEntityFactory
{
    /// <summary>
    /// Gets the type of the entity produced by this factory.
    /// </summary>
    Type EntityType { get; }

    /// <summary>
    /// Gets the type of the model associated with the entity this factory produces.
    /// </summary>
    Type ModelType { get; }

    /// <summary>
    /// Creates a new instance of an entity.
    /// </summary>
    /// <param name="lifecycleManager">
    /// The graph lifecycle manager responsible for tracking the created entity's lifecycle. 
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="parent">
    /// The parent entity in the graph hierarchy, 
    /// or <see langword="null"/> if the new entity has no parent.
    /// </param>
    /// <returns>An instance of <see cref="IGraphEntity"/> representing the newly created entity.</returns>
    IGraphEntity Create(GraphLifecycleManager lifecycleManager, IGraphEntity? parent);

    /// <summary>
    /// Creates a new instance of an entity.
    /// </summary>
    /// <param name="actor">
    /// The actor associated with the new entity.
    /// </param>
    /// <param name="lifecycleManager">
    /// The graph lifecycle manager responsible for tracking the created entity's lifecycle. 
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="parent">
    /// The parent entity in the graph hierarchy, 
    /// or <see langword="null"/> if the new entity has no parent.
    /// </param>
    /// <returns>An instance of <see cref="IGraphEntity"/> representing the newly created entity.</returns>
    IGraphEntity Create(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? parent);

    /// <summary>
    /// Loads an entity from the specified model.
    /// </summary>
    /// <param name="lifecycleManager">
    /// The lifecycle manager responsible for managing the entity's lifecycle during and after loading. 
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="model">
    /// The model containing the data and configuration used to construct the entity. 
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="parent">
    /// The parent entity to which the loaded entity will be attached, 
    /// or <see langword="null"/> if the entity has no parent.
    /// </param>
    /// <returns>An instance of <see cref="IGraphEntity"/> representing the loaded entity.</returns>
    IGraphEntity Load(GraphLifecycleManager lifecycleManager, IGraphEntityModel model, IGraphEntity? parent);

    /// <summary>
    /// Retrieves the graph model associated with the specified graph entity.
    /// </summary>
    /// <param name="entity">The graph entity for which to retrieve the corresponding model. Cannot be <see langword="null"/>.</param>
    /// <param name="lifecycleManager">
    /// The lifecycle manager responsible for managing the entity's lifecycle during and after loading. 
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>The graph model that represents the specified entity.</returns>
    IGraphModel GetModel(IGraphEntity entity, GraphLifecycleManager lifecycleManager);
}
