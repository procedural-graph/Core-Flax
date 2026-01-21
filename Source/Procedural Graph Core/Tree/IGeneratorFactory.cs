namespace ProceduralGraph.Tree;

/// <summary>
/// Defines a factory for creating asynchronous generator instances.
/// </summary>
/// <typeparam name="TEntity">The type of the entity associated with the generator.</typeparam>
/// <typeparam name="TSelf">The type of the factory implementing this interface.</typeparam>
public interface IGeneratorFactory<TEntity, TSelf> where TEntity : IGraphNode where TSelf : IGeneratorFactory<TEntity, TSelf>
{
    /// <summary>
    /// Creates a new instance of <typeparamref name="TSelf"/> from the specified entity.
    /// </summary>
    /// <param name="entity">The entity from which to create the new instance. Must not be null.</param>
    /// <returns>A new instance of <typeparamref name="TEntity"/> initialized with the specified entity.</returns>
    static abstract TSelf Create(TEntity entity);
}