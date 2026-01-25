using FlaxEngine;
using System;
using System.Runtime.CompilerServices;

namespace ProceduralGraph.Converters.Generic;

/// <summary>
/// Provides an abstract base for factories that create and load graph entities from models, supporting entity lifecycle
/// management and hierarchical relationships.
/// </summary>
/// <typeparam name="TEntity">The type of graph entity created or loaded by the factory. Must implement <see cref="IGraphEntity"/>.</typeparam>
/// <typeparam name="TModel">The type of model used to load entities. Must implement <see cref="IGraphEntityModel"/>.</typeparam>
public abstract class GraphEntityFactory<TEntity, TModel> : IGraphEntityFactory
    where TEntity : IGraphEntity
    where TModel : IGraphEntityModel
{
    Type IGraphEntityFactory.EntityType => typeof(TEntity);

    Type IGraphEntityFactory.ModelType => typeof(TModel);

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
    /// <returns>An instance of <typeparamref name="TEntity"/> representing the newly created entity.</returns>
    public abstract TEntity Create(GraphLifecycleManager lifecycleManager, IGraphEntity? parent);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphEntity IGraphEntityFactory.Create(GraphLifecycleManager lifecycleManager, IGraphEntity? parent)
    {
        return Create(lifecycleManager, parent);
    }

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
    /// <returns>An instance of <typeparamref name="TEntity"/> representing the newly created entity.</returns>
    public abstract TEntity Create(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? parent);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphEntity IGraphEntityFactory.Create(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? parent)
    {
        return Create(actor, lifecycleManager, parent);
    }

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
    /// <returns>An instance of <typeparamref name="TEntity"/> representing the loaded entity.</returns>
    public abstract TEntity Load(GraphLifecycleManager lifecycleManager, TModel model, IGraphEntity? parent);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphEntity IGraphEntityFactory.Load(GraphLifecycleManager lifecycleManager, IGraphEntityModel model, IGraphEntity? parent)
    {
        return Load(lifecycleManager, (TModel)model, parent);
    }

    /// <summary>
    /// Retrieves the graph model associated with the specified graph entity.
    /// </summary>
    /// <param name="entity">The graph entity for which to retrieve the corresponding model. Cannot be <see langword="null"/>.</param>
    /// <param name="lifecycleManager">
    /// The lifecycle manager responsible for managing the entity's lifecycle during and after loading. 
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>The graph model that represents the specified entity.</returns>
    public abstract TModel GetModel(TEntity entity, GraphLifecycleManager lifecycleManager);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphModel IGraphEntityFactory.GetModel(IGraphEntity entity, GraphLifecycleManager lifecycleManager)
    {
        return GetModel((TEntity)entity, lifecycleManager);
    }
}