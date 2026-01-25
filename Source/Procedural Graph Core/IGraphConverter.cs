#nullable enable
using FlaxEngine;
using System.Diagnostics.CodeAnalysis;

namespace ProceduralGraph;

/// <summary>
/// Defines the contract for converting between Flax Engine objects (models or actors) and procedural graph entities.
/// </summary>
public interface IGraphConverter
{
    /// <summary>
    /// Determines whether the specified object model can be converted by this converter.
    /// </summary>
    /// <param name="model">The object model to check for compatibility.</param>
    /// <returns><see langword="true"/> if the model can be converted; otherwise, <see langword="false"/>.</returns>
    bool CanConvert([NotNullWhen(true)] IGraphModel? model);

    /// <summary>
    /// Determines whether the specified Flax Actor can be converted by this converter.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> to check for compatibility.</param>
    /// <returns><see langword="true"/> if the actor can be converted; otherwise, <see langword="false"/>.</returns>
    bool CanConvert([NotNullWhen(true)] Actor? actor);

    /// <summary>
    /// Determines whether the specified node can be converted by this converter.
    /// </summary>
    /// <param name="entity">The <see cref="IGraph"/> to check for compatibility.</param>
    /// <returns><see langword="true"/> if the entity can be converted; otherwise, <see langword="false"/>.</returns>
    bool CanConvert([NotNullWhen(true)] IGraph? entity);

    /// <summary>
    /// Converts a model object into a procedural graph node.
    /// </summary>
    /// <param name="model">The source model to convert.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <param name="parent">The parent entity in the graph hierarchy, if applicable.</param>
    /// <returns>The converted <see cref="IGraph"/>.</returns>
    IGraph ToGraph(IGraphModel model, GraphLifecycleManager lifecycleManager, IGraphEntity? parent = default);

    /// <summary>
    /// Converts a Flax Actor into a procedural graph node.
    /// </summary>
    /// <param name="actor">The source <see cref="Actor"/> to convert.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <param name="parent">The parent entity in the graph hierarchy, if applicable.</param>
    /// <returns>The converted <see cref="IGraph"/>.</returns>
    IGraph ToGraph(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? parent = default);

    /// <summary>
    /// Converts a procedural graph node into it's model representation.
    /// </summary>
    /// <param name="node">The <see cref="IGraph"/> to transform.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural graph nodes.</param>
    /// <returns>The converted model object.</returns>
    IGraphModel ToModel(IGraph node, GraphLifecycleManager lifecycleManager);
}
