using FlaxEngine;
using System.Runtime.CompilerServices;

namespace ProceduralGraph.Tree.Entities.Converters;

/// <summary>
/// Provides a base class for converting between model, actor, and graph entity representations in a graph-based system.
/// </summary>
/// <typeparam name="TModel">The type of the source model to convert. Must be a reference type.</typeparam>
/// <typeparam name="TActor">The type of actor to convert. Must inherit from <see cref="Actor"/>.</typeparam>
/// <typeparam name="TEntity">The type of graph entity to convert to and from. Must implement <see cref="IGraphEntity"/>.</typeparam>
public abstract class GenericGraphConverter<TModel, TActor, TEntity> : IGraphConverter where TModel : class where TActor : Actor where TEntity : IGraphEntity
{
    /// <summary>
    /// Converts a <typeparamref name="TModel"/> into a <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="model">The source model to convert.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <param name="root">The optional parent entity in the graph hierarchy.</param>
    /// <returns>A new <typeparamref name="TEntity"/> if successful; otherwise, <see langword="null"/>.</returns>
    public abstract TEntity ToEntity(TModel model, GraphLifecycleManager lifecycleManager, IGraphEntity? root);

    /// <summary>
    /// Converts a <typeparamref name="TActor"/> into a <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="actor">The source <see cref="Actor"/> to convert.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <param name="root">The optional parent entity in the graph hierarchy.</param>
    /// <returns>A new <typeparamref name="TEntity"/> if successful; otherwise, <see langword="null"/>.</returns>
    public abstract TEntity ToEntity(TActor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? root);

    /// <summary>
    /// Converts a <typeparamref name="TEntity"/> back into its original <typeparamref name="TModel"/> representation.
    /// </summary>
    /// <param name="entity">The <see cref="IGraphEntity"/> to transform.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <returns>A new <typeparamref name="TModel"/>, or <see langword="null"/> if the conversion is not possible.</returns>
    public abstract TModel ToModel(TEntity entity, GraphLifecycleManager lifecycleManager);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IGraphConverter.CanConvert(object? model)
    {
        return model is TModel;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IGraphConverter.CanConvert(Actor? actor)
    {
        return actor is TActor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IGraphConverter.CanConvert(IGraphEntity? entity)
    {
        return entity is TEntity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphEntity IGraphConverter.ToEntity(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? root)
    {
        return ToEntity((TActor)actor, lifecycleManager, root);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphEntity IGraphConverter.ToEntity(object model, GraphLifecycleManager lifecycleManager, IGraphEntity? root)
    {
        return ToEntity((TModel)model, lifecycleManager, root);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    object IGraphConverter.ToModel(IGraphEntity entity, GraphLifecycleManager lifecycleManager)
    {
        return ToModel((TEntity)entity, lifecycleManager);
    }
}
