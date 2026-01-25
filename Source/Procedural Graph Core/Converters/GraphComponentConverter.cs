using FlaxEngine;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProceduralGraph.Converters;

/// <summary>
/// Provides an abstract base for converting between graph models, actors, and entities using registered component
/// factories.
/// </summary>
public abstract class GraphComponentConverter : IGraphConverter
{
    /// <summary>
    /// Represents a unique identity for a graph component, combining a node identity and a model identity.
    /// </summary>
    /// <param name="NodeIdentity">The identity of the graph node associated with this component.</param>
    /// <param name="ModelIdentity">The identity of the graph model associated with this component.</param>
    public readonly record struct CompositeKey(NodeKey NodeIdentity, ModelKey ModelIdentity) :
        IAlternateEqualityComparer<ModelKey, CompositeKey>,
        IAlternateEqualityComparer<NodeKey, CompositeKey>
    {
        /// <inheritdoc/>
        public CompositeKey Create(ModelKey alternate) => new(NodeIdentity, alternate);
        /// <inheritdoc/>
        public CompositeKey Create(NodeKey alternate) => new(alternate, ModelIdentity);
        /// <inheritdoc/>
        public bool Equals(ModelKey alternate, CompositeKey other) => alternate.Equals(other.ModelIdentity);
        /// <inheritdoc/>
        public bool Equals(NodeKey alternate, CompositeKey other) => alternate.Equals(other.NodeIdentity);
        /// <inheritdoc/>
        public int GetHashCode(ModelKey alternate) => alternate.GetHashCode();
        /// <inheritdoc/>
        public int GetHashCode(NodeKey alternate) => alternate.GetHashCode();
    }

    /// <summary>
    /// Represents a unique identity for a graph model based on its associated type.
    /// </summary>
    /// <param name="ModelType">The type that defines the identity of the graph model. Cannot be <see langword="null"/>.</param>
    public readonly record struct ModelKey(Type ModelType) : IAlternateEqualityComparer<Type, ModelKey>
    {
        /// <inheritdoc/>
        public ModelKey Create(Type alternate) => new(alternate);
        /// <inheritdoc/>
        public bool Equals(Type alternate, ModelKey other) => alternate == other.ModelType;
        /// <inheritdoc/>
        public int GetHashCode(Type alternate) => alternate.GetHashCode();
        /// <summary>
        /// Defines an implicit conversion from a <see cref="ModelKey"/> instance to its associated model Type.
        /// </summary>
        /// <param name="identity">The GraphModelIdentity instance to convert.</param>
        public static implicit operator Type(ModelKey identity) => identity.ModelType;
        /// <summary>
        /// Converts a <see cref="Type"/> instance to a <see cref="ModelKey"/> representing the specified model
        /// type.
        /// </summary>
        /// <param name="modelType">
        /// The type of the model to be represented as a <see cref="ModelKey"/>. 
        /// Cannot be <see langword="null"/>.
        /// </param>
        public static implicit operator ModelKey(Type modelType) => new(modelType);
    }

    /// <summary>
    /// Represents the identity of a graph node based on its associated type.
    /// </summary>
    /// <param name="NodeType">The type that uniquely identifies the graph node. Cannot be <see langword="null"/>.</param>
    public readonly record struct NodeKey(Type NodeType) : IAlternateEqualityComparer<Type, NodeKey>
    {
        /// <inheritdoc/>
        public NodeKey Create(Type alternate) => new(alternate);
        /// <inheritdoc/>
        public bool Equals(Type alternate, NodeKey other) => alternate == other.NodeType;
        /// <inheritdoc/>
        public int GetHashCode(Type alternate) => alternate.GetHashCode();
        /// <summary>
        /// Defines an implicit conversion from a <see cref="NodeKey"/> instance to its associated node Type.
        /// </summary>
        /// <param name="identity">The GraphModelIdentity instance to convert.</param>
        public static implicit operator Type(NodeKey identity) => identity.NodeType;
        /// <summary>
        /// Converts a <see cref="Type"/> instance to a <see cref="ModelKey"/> representing the specified node
        /// type.
        /// </summary>
        /// <param name="nodeType">
        /// The type of the node to be represented as a <see cref="NodeKey"/>. 
        /// Cannot be <see langword="null"/>.
        /// </param>
        public static implicit operator NodeKey(Type nodeType) => new(nodeType);
    }

    /// <summary>
    /// Provides a base class for building and configuring graph component factories in a fluent manner.
    /// </summary>
    /// <typeparam name="TBuilder">
    /// The type of the concrete builder that derives from this class. This enables fluent method chaining in derived
    /// builder implementations.
    /// </typeparam>
    public abstract class Builder<TBuilder> where TBuilder : Builder<TBuilder>
    {
        /// <summary>
        /// Stores the set of registered graph component factories used to create graph components.
        /// </summary>
        protected readonly Dictionary<CompositeKey, IGraphComponentFactory> componentFactories = [];

        /// <summary>
        /// Adds a component factory to the builder for use in constructing the graph.
        /// </summary>
        /// <param name="factory">The factory that creates components to be added to the graph. Cannot be null. Each factory must be unique by
        /// component type or model type.</param>
        /// <returns>The current <see cref="Builder{TBuilder}"/> instance, enabling method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a factory for the same component type or model type has already been added.</exception>
        public TBuilder AddComponent(IGraphComponentFactory factory)
        {
            CompositeKey identity = new(factory.ComponentType, factory.ComponentModelType);

            if (componentFactories.TryAdd(identity, factory))
            {
                return (TBuilder)this;
            }
            
            throw new InvalidOperationException($"A factory for component type '{factory.ComponentType.FullName}' or model type '{factory.ComponentModelType.FullName}' has already been added.");
        }

        /// <summary>
        /// Adds a new graph component to the builder using the specified component factory type.
        /// </summary>
        /// <typeparam name="TComponentFactory">
        /// The type of the component factory to instantiate and add. 
        /// Must implement <see cref="IGraphComponentFactory"/> and have a public parameterless constructor.
        /// </typeparam>
        /// <returns>The builder instance with the new component added.</returns>
        public TBuilder AddComponent<TComponentFactory>() where TComponentFactory : IGraphComponentFactory, new()
        {
            return AddComponent(new TComponentFactory());
        }
    }

    private readonly FrozenDictionary<CompositeKey, IGraphComponentFactory> _componentFactories;
    private readonly FrozenDictionary<CompositeKey, IGraphComponentFactory>.AlternateLookup<ModelKey> _componentFactoriesByModel;
    private readonly FrozenDictionary<CompositeKey, IGraphComponentFactory>.AlternateLookup<NodeKey> _componentFactoriesByComponent;

    internal GraphComponentConverter(Dictionary<CompositeKey, IGraphComponentFactory> factories)
    {
        ArgumentNullException.ThrowIfNull(factories, nameof(factories));
        _componentFactories = factories.ToFrozenDictionary();
    }

    /// <inheritdoc/>
    public virtual bool CanConvert([NotNullWhen(true)] IGraphModel? model)
    {
        return model is { } && _componentFactoriesByModel.ContainsKey(model.GetType());
    }

    /// <inheritdoc/>
    public abstract bool CanConvert([NotNullWhen(true)] Actor? actor);

    /// <inheritdoc/>
    public virtual bool CanConvert([NotNullWhen(true)] IGraph? node)
    {
        return node is IGraphComponent component && _componentFactoriesByComponent.ContainsKey(component.GetType());
    }

    /// <inheritdoc/>
    public virtual IGraph ToGraph(IGraphModel model, GraphLifecycleManager lifecycleManager, IGraphEntity? parent = null)
    {
        ArgumentNullException.ThrowIfNull(parent, nameof(parent));

        if (_componentFactoriesByModel.TryGetValue(model.GetType(), out IGraphComponentFactory? factory))
        {
            return factory.Load(parent, model);
        }

        throw new InvalidOperationException($"No factory registered for model type '{model.GetType().FullName}'.");
    }

    /// <inheritdoc/>
    public abstract IGraph ToGraph(Actor actor, GraphLifecycleManager lifecycleManager, IGraphEntity? parent = null);

    /// <inheritdoc/>
    public IGraphModel ToModel(IGraph node, GraphLifecycleManager lifecycleManager) => node switch
    {
        IGraphComponent component => ToModel(component),
        IGraphEntity entity => ToModel(entity, lifecycleManager),
        _ => throw new InvalidOperationException($"{node.GetType().FullName} is neither a component nor an entity.")
    };

    /// <summary>
    /// Converts the specified graph entity to its corresponding model representation.
    /// </summary>
    /// <param name="entity">The graph entity to convert. Cannot be .</param>
    /// <param name="lifecycleManager">
    /// The manager handling the creation and disposal of procedural graph nodes. 
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>An <see cref="IGraphModel"/> instance representing the converted graph entity.</returns>
    protected abstract IGraphModel ToModel(IGraphEntity entity, GraphLifecycleManager lifecycleManager);

    private IGraphModel ToModel(IGraphComponent component)
    {
        Type type = component.GetType();

        if (_componentFactoriesByComponent.TryGetValue(type, out IGraphComponentFactory? factory))
        {
            return factory.GetModel(component);
        }

        throw new InvalidOperationException($"No factory registered for component type '{type.FullName}'.");
    }
}
