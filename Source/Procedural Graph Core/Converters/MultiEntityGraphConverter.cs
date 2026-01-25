using FlaxEngine;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ProceduralGraph.Converters;

/// <summary>
/// A graph converter that supports multiple entity types and their associated factories.
/// allows for the conversion of mixed graphs containing various Actor and Entity types.
/// </summary>
public sealed class MultiEntityGraphConverter : GraphComponentConverter
{
    /// <summary>
    /// A builder for configuring and creating instances of <see cref="MultiEntityGraphConverter"/>.
    /// </summary>
    public sealed class Builder : Builder<Builder>
    {
        // Maps Actor Type -> Factory (Required because IGraphEntityFactory doesn't expose ActorType)
        internal readonly Dictionary<Type, IGraphEntityFactory> _actorToFactoryMap = [];
        private readonly HashSet<IGraphEntityFactory> _entityFactories = [];

        /// <summary>
        /// Registers an entity factory for a specific actor type.
        /// </summary>
        /// <typeparam name="TActor">The Actor type that this factory creates entities for.</typeparam>
        /// <param name="factory">The entity factory instance.</param>
        /// <returns>The builder instance.</returns>
        public Builder AddEntityFactory<TActor>(IGraphEntityFactory factory) where TActor : Actor
        {
            return AddEntityFactory(typeof(TActor), factory);
        }

        /// <summary>
        /// Registers an entity factory for a specific actor type.
        /// </summary>
        /// <param name="actorType">The Actor type that this factory creates entities for.</param>
        /// <param name="factory">The entity factory instance.</param>
        /// <returns>The builder instance.</returns>
        public Builder AddEntityFactory(Type actorType, IGraphEntityFactory factory)
        {
            ArgumentNullException.ThrowIfNull(actorType);
            ArgumentNullException.ThrowIfNull(factory);

            if (!actorType.IsAssignableTo(typeof(Actor)))
            {
                throw new ArgumentException($"Type '{actorType.FullName}' must inherit from {nameof(Actor)}.", nameof(actorType));
            }

            if (_actorToFactoryMap.ContainsKey(actorType))
            {
                throw new InvalidOperationException($"A factory for Actor type '{actorType.FullName}' has already been registered.");
            }

            _actorToFactoryMap[actorType] = factory;
            _entityFactories.Add(factory);

            return this;
        }

        /// <summary>
        /// Builds the converter.
        /// </summary>
        public MultiEntityGraphConverter Build()
        {
            return new MultiEntityGraphConverter(componentFactories, _actorToFactoryMap, _entityFactories);
        }
    }

    private readonly FrozenDictionary<Type, IGraphEntityFactory> _factoriesByActor;
    private readonly FrozenDictionary<Type, IGraphEntityFactory> _factoriesByEntity;
    private readonly FrozenDictionary<Type, IGraphEntityFactory> _factoriesByModel;

    internal MultiEntityGraphConverter(
        IEnumerable<IGraphComponentFactory> componentFactories,
        IDictionary<Type, IGraphEntityFactory> actorMap,
        IEnumerable<IGraphEntityFactory> distinctFactories)
        : base(componentFactories)
    {
        // Map: Actor Type -> Factory (From Builder manual mapping)
        _factoriesByActor = actorMap.ToFrozenDictionary();

        // Map: Entity Type -> Factory (From Factory Property)
        // We use the distinct list of factories to avoid duplicate keys if the same factory was registered multiple times
        var entityDict = new Dictionary<Type, IGraphEntityFactory>();
        var modelDict = new Dictionary<Type, IGraphEntityFactory>();

        foreach (var factory in distinctFactories)
        {
            // Safety check for duplicate EntityType handling
            if (entityDict.ContainsKey(factory.EntityType))
                throw new InvalidOperationException($"Multiple factories registered for Entity type '{factory.EntityType.FullName}'.");

            entityDict[factory.EntityType] = factory;

            // Safety check for duplicate ModelType handling
            if (modelDict.ContainsKey(factory.ModelType))
                throw new InvalidOperationException($"Multiple factories registered for Model type '{factory.ModelType.FullName}'.");

            modelDict[factory.ModelType] = factory;
        }

        _factoriesByEntity = entityDict.ToFrozenDictionary();
        _factoriesByModel = modelDict.ToFrozenDictionary();
    }

    /// <summary>
    /// Creates a new builder for the MultiEntityGraphConverter.
    /// </summary>
    public static Builder CreateBuilder() => new();

    /// <inheritdoc/>
    public override bool CanConvert([NotNullWhen(true)] Actor? actor)
    {
        return actor is { } && _factoriesByActor.ContainsKey(actor.GetType());
    }

    /// <inheritdoc/>
    public override bool CanConvert([NotNullWhen(true)] IGraph? node)
    {
        if (node is IGraphEntity entity && _factoriesByEntity.ContainsKey(entity.GetType()))
        {
            return true;
        }
        return base.CanConvert(node);
    }

    /// <inheritdoc/>
    public override bool CanConvert([NotNullWhen(true)] IGraphModel? model)
    {
        if (model is IGraphEntityModel && _factoriesByModel.ContainsKey(model.GetType()))
        {
            return true;
        }
        return base.CanConvert(model);
    }

    /// <inheritdoc/>
    public override IGraph ToGraph(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? parent = null)
    {
        ArgumentNullException.ThrowIfNull(actor);

        // Find the factory mapped to this specific Actor type
        ref readonly IGraphEntityFactory factory = ref _factoriesByActor.GetValueRefOrNullRef(actor.GetType());

        if (Unsafe.IsNullRef(in factory))
        {
            throw new InvalidOperationException($"No factory registered for Actor type '{actor.GetType().FullName}'.");
        }

        return factory.Create(actor, lifecycleManager, parent);
    }

    /// <inheritdoc/>
    // Note: The base class ToGraph(IGraphModel...) handles components. 
    // We override it to also support creating Entities directly from Models if this converter is used in that context.
    public override IGraph ToGraph(IGraphModel model, GraphLifecycleManager lifecycleManager, IGraphEntity? parent = null)
    {
        ArgumentNullException.ThrowIfNull(model);

        // Check if it's an entity model we have a factory for
        ref readonly IGraphEntityFactory entityFactory = ref _factoriesByModel.GetValueRefOrNullRef(model.GetType());

        if (!Unsafe.IsNullRef(in entityFactory) && model is IGraphEntityModel entityModel)
        {
            return entityFactory.Load(lifecycleManager, entityModel, parent);
        }

        // Fallback to base (Component lookup)
        return base.ToGraph(model, lifecycleManager, parent);
    }

    /// <inheritdoc/>
    protected override IGraphModel ToModel(IGraphEntity entity, GraphLifecycleManager lifecycleManager)
    {
        ArgumentNullException.ThrowIfNull(entity);

        ref readonly IGraphEntityFactory factory = ref _factoriesByEntity.GetValueRefOrNullRef(entity.GetType());

        if (Unsafe.IsNullRef(in factory))
        {
            throw new InvalidOperationException($"No factory registered for Entity type '{entity.GetType().FullName}'.");
        }

        return factory.GetModel(entity);
    }
}
