using System;
using System.Runtime.CompilerServices;

namespace ProceduralGraph.Converters.Generic;

/// <summary>
/// Provides a generic base class for factories that create and load graph components for specific entity and model
/// types.
/// </summary>
/// <typeparam name="TEntity">The type of graph entity for which components are created. Must implement <see cref="IGraphEntity"/>.</typeparam>
/// <typeparam name="TModel">The type of graph model that provides context for loading components. Must implement <see cref="IGraphModel"/>.</typeparam>
/// <typeparam name="TComponent">The type of graph component produced by the factory. Must implement <see cref="IGraphComponent"/>.</typeparam>
public abstract class GraphComponentFactory<TEntity, TModel, TComponent> : IGraphComponentFactory
    where TEntity : IGraphEntity
    where TModel : IGraphModel
    where TComponent : IGraphComponent
{
    Type IGraphComponentFactory.ComponentType => typeof(TComponent);

    Type IGraphComponentFactory.ComponentModelType => typeof(TModel);

    Type IGraphComponentFactory.EntityType => typeof(TEntity);

    /// <summary>
    /// Loads a graph component for the specified entity using the provided graph model.
    /// </summary>
    /// <param name="entity">The graph entity for which to load the component. Cannot be <see langword="null"/>.</param>
    /// <param name="model">The graph model that provides context for loading the component. Cannot be <see langword="null"/>.</param>
    /// <returns>An <typeparamref name="TComponent"/> representing the loaded component for the specified entity and model.</returns>
    public abstract TComponent Load(TEntity entity, TModel model);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphComponent IGraphComponentFactory.Load(IGraphEntity entity, IGraphModel model)
    {
        return Load((TEntity)entity, (TModel)model);
    }

    /// <summary>
    /// Creates a new graph component for the specified graph entity.
    /// </summary>
    /// <param name="entity">The graph entity for which to create a component. Cannot be <see langword="null"/>.</param>
    /// <returns>An <typeparamref name="TComponent"/> instance representing the component associated with the specified entity.</returns>
    public abstract TComponent Create(TEntity entity);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphComponent IGraphComponentFactory.Create(IGraphEntity entity)
    {
        return Create((TEntity)entity);
    }

    /// <summary>
    /// Retrieves the graph model associated with the specified graph component.
    /// </summary>
    /// <param name="component">The graph component for which to retrieve the associated model. Cannot be <see langword="null"/>.</param>
    /// <returns>The graph model associated with the specified component.</returns>
    public abstract TModel GetModel(TComponent component);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphModel IGraphComponentFactory.GetModel(IGraphComponent component)
    {
        return GetModel((TComponent)component);
    }
}
