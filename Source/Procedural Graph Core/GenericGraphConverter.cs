#nullable enable
using FlaxEngine;
using System.Runtime.CompilerServices;

namespace ProceduralGraph;

/// <summary>
/// Provides a base class for converting between model, actor, and graph node representations in a graph-based system.
/// </summary>
/// <typeparam name="TModel">The type of the source model to convert. Must be a reference type.</typeparam>
/// <typeparam name="TActor">The type of actor to convert. Must inherit from <see cref="Actor"/>.</typeparam>
/// <typeparam name="TNode">The type of graph node to convert to and from. Must implement <see cref="IGraph"/>.</typeparam>
public abstract class GenericGraphConverter<TModel, TActor, TNode> : IGraphConverter where TModel : class, IGraphModel where TActor : Actor where TNode : IGraph
{
    /// <summary>
    /// Converts a <typeparamref name="TActor"/> into a <typeparamref name="TNode"/>.
    /// </summary>
    /// <param name="actor">The source <see cref="Actor"/> to convert.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <param name="root">The optional parent entity in the graph hierarchy.</param>
    /// <returns>A new <typeparamref name="TNode"/> if successful; otherwise, <see langword="null"/>.</returns>
    public abstract TNode ToGraph(TActor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? root);

    /// <summary>
    /// Converts a <typeparamref name="TModel"/> into a <typeparamref name="TNode"/>.
    /// </summary>
    /// <param name="model">The source model to convert.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <param name="root">The optional parent entity in the graph hierarchy.</param>
    /// <returns>A new <typeparamref name="TNode"/> if successful; otherwise, <see langword="null"/>.</returns>
    public abstract TNode ToGraph(TModel model, GraphLifecycleManager lifecycleManager, IGraphEntity? root);

    /// <summary>
    /// Converts a <typeparamref name="TNode"/> into it's <typeparamref name="TModel"/> representation.
    /// </summary>
    /// <param name="node">The <typeparamref name="TNode"/> to transform.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <returns>A new <typeparamref name="TModel"/>, or <see langword="null"/> if the conversion is not possible.</returns>
    public abstract TModel ToModel(TNode node, GraphLifecycleManager lifecycleManager);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IGraphConverter.CanConvert(IGraphModel? model)
    {
        return model is TModel;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IGraphConverter.CanConvert(Actor? actor)
    {
        return actor is TActor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IGraphConverter.CanConvert(IGraph? node)
    {
        return node is TNode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraph IGraphConverter.ToGraph(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? root)
    {
        return ToGraph((TActor)actor, lifecycleManager, root);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraph IGraphConverter.ToGraph(IGraphModel model, GraphLifecycleManager lifecycleManager, IGraphEntity? root)
    {
        return ToGraph((TModel)model, lifecycleManager, root);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IGraphModel IGraphConverter.ToModel(IGraph node, GraphLifecycleManager lifecycleManager)
    {
        return ToModel((TNode)node, lifecycleManager);
    }
}
