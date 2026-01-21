using FlaxEngine;

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
    bool CanConvert(object? model);

    /// <summary>
    /// Determines whether the specified Flax Actor can be converted by this converter.
    /// </summary>
    /// <param name="actor">The <see cref="Actor"/> to check for compatibility.</param>
    /// <returns><see langword="true"/> if the actor can be converted; otherwise, <see langword="false"/>.</returns>
    bool CanConvert(Actor? actor);

    /// <summary>
    /// Determines whether the specified graph entity can be converted back to its original model.
    /// </summary>
    /// <param name="entity">The <see cref="IGraphEntity"/> to check for compatibility.</param>
    /// <returns><see langword="true"/> if the entity can be converted; otherwise, <see langword="false"/>.</returns>
    bool CanConvert(IGraphEntity? entity);

    /// <summary>
    /// Converts a model object into a procedural graph entity.
    /// </summary>
    /// <param name="model">The source model to convert.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <param name="root">The root entity in the graph hierarchy, if applicable.</param>
    /// <returns>The converted <see cref="IGraphEntity"/>.</returns>
    IGraphEntity ToEntity(object model, GraphLifecycleManager lifecycleManager, IGraphEntity? root = default);

    /// <summary>
    /// Converts a Flax Actor into a procedural graph entity.
    /// </summary>
    /// <param name="actor">The source <see cref="Actor"/> to convert.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <param name="root">The root entity in the graph hierarchy, if applicable.</param>
    /// <returns>The converted <see cref="IGraphEntity"/>.</returns>
    IGraphEntity ToEntity(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? root = default);

    /// <summary>
    /// Converts a procedural graph entity into it's model representation.
    /// </summary>
    /// <param name="entity">The <see cref="IGraphEntity"/> to transform.</param>
    /// <param name="lifecycleManager">The manager handling the creation and disposal of procedural entities.</param>
    /// <returns>The converted model object.</returns>
    object ToModel(IGraphEntity entity, GraphLifecycleManager lifecycleManager);
}
