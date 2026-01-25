using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProceduralGraph.Converters.Generic;

/// <summary>
/// Provides conversion logic between a single graph entity type, its corresponding model, and actor representation
/// within a graph component system.
/// </summary>
/// <typeparam name="TEntity">The type of the graph entity to convert. Must implement <see cref="IGraphEntity"/>.</typeparam>
/// <typeparam name="TModel">
/// The type of the graph entity model associated with <typeparamref name="TEntity"/>. 
/// Must implement <see cref="IGraphEntityModel"/>.
/// </typeparam>
/// <typeparam name="TActor">The type of actor associated with the entity. Must inherit from <see cref="Actor"/>.</typeparam>
public sealed class SingleEntityGraphConverter<TEntity, TModel, TActor> : GraphComponentConverter 
    where TEntity : IGraphEntity 
    where TModel : IGraphEntityModel
    where TActor : Actor
{
    /// <summary>
    /// Provides a builder for configuring and creating instances of graph entity converters using a specified entity
    /// factory.
    /// </summary>
    public sealed class Builder : Builder<Builder>
    {
        private readonly GraphEntityFactory<TEntity, TModel> _entityFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> class with the specified entity factory.
        /// </summary>
        /// <param name="entityFactory">The factory used to create and manage graph entities and models.</param>
        public Builder(GraphEntityFactory<TEntity, TModel> entityFactory)
        {
            ArgumentNullException.ThrowIfNull(entityFactory);
            _entityFactory = entityFactory;
        }

        /// <summary>
        /// Builds and returns a new instance of the SingleEntityGraphConverter configured with the specified entity and
        /// component factories.
        /// </summary>
        /// <returns>
        /// A <see cref="GraphComponentFactory{TEntity, TModel, TComponent}"/> instance initialized with the current factory
        /// settings.
        /// </returns>
        public SingleEntityGraphConverter<TEntity, TModel, TActor> Build()
        {
            return new SingleEntityGraphConverter<TEntity, TModel, TActor>(_entityFactory, componentFactories);
        }
    }

    private readonly GraphEntityFactory<TEntity, TModel> _entityFactory;

    internal SingleEntityGraphConverter(GraphEntityFactory<TEntity, TModel> entityFactory, Dictionary<CompositeKey, IGraphComponentFactory> componentFactories) : base(componentFactories)
    {
        _entityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
    }

    /// <inheritdoc/>
    public override bool CanConvert([NotNullWhen(true)] IGraph? node)
    {
        return node is TEntity || base.CanConvert(node);
    }

    /// <inheritdoc/>
    public override bool CanConvert([NotNullWhen(true)] IGraphModel? model)
    {
        return model is TModel || base.CanConvert(model);
    }

    /// <inheritdoc/>
    public override bool CanConvert([NotNullWhen(true)] Actor? actor)
    {
        return actor is TActor;
    }

    /// <inheritdoc/>
    public override IGraph ToGraph(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? parent = null)
    {
        return _entityFactory.Create(actor, lifecycleManager, parent);
    }

    /// <inheritdoc/>
    protected override IGraphModel ToModel(IGraphEntity entity, GraphLifecycleManager lifecycleManager)
    {
        return _entityFactory.GetModel((TEntity)entity);
    }
}

/// <summary>
/// Provides factory methods for creating builders to configure and construct single-entity graph converters.
/// </summary>
public static class SingleEntityGraphConverter
{
    /// <summary>
    /// Creates a new builder for configuring and constructing a <see cref="SingleEntityGraphConverter{TEntity, TModel, TActor}"/>
    /// using the specified entity factory.
    /// </summary>
    /// <typeparam name="TEntity">The type of the graph entity to convert. Must implement <see cref="IGraphEntity"/>.</typeparam>
    /// <typeparam name="TModel">
    /// The type of the graph entity model associated with <typeparamref name="TEntity"/>. 
    /// Must implement <see cref="IGraphEntityModel"/>.
    /// </typeparam>
    /// <typeparam name="TActor">The type of actor associated with the entity. Must inherit from <see cref="Actor"/>.</typeparam>
    /// <param name="entityFactory">The factory used to create and manage graph entities and models.</param>
    /// <returns>
    /// A <see cref="SingleEntityGraphConverter{TEntity, TModel, TActor}.Builder"/> instance for configuring and creating
    /// the converter.
    /// </returns>
    public static SingleEntityGraphConverter<TEntity, TModel, TActor>.Builder CreateBuilder<TEntity, TModel, TActor>(GraphEntityFactory<TEntity, TModel> entityFactory)
        where TEntity : IGraphEntity
        where TModel : IGraphEntityModel
        where TActor : Actor
    {
        return new SingleEntityGraphConverter<TEntity, TModel, TActor>.Builder(entityFactory);
    }
}
