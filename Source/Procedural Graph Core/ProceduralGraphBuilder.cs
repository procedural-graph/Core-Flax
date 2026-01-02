using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;

namespace ProceduralGraph;

/// <summary>
/// <para>The central Editor Plugin and factory for the Procedural Graph system.</para>
/// <para>Manages the registration of node converters and facilitates the transformation of Flax Actors into processing Nodes.</para>
/// </summary>
public sealed class ProceduralGraphBuilder : EditorPlugin
{
    private readonly Dictionary<Type, Func<IProceduralGraphLifecycle, INodeConverter>> _factories;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProceduralGraphBuilder"/> class and sets up plugin metadata.
    /// </summary>
    public ProceduralGraphBuilder()
    {
        _factories = [];
        _description = new PluginDescription()
        {
            Name = "Procedural Graph Core",
            Author = "William Brocklesby",
            AuthorUrl = "https://william-brocklesby.com",
            Category = "Procedural Graph",
            Version = new(1, 0, 0)
        };
    }

    /// <summary>
    /// Registers a new node converter type.
    /// </summary>
    /// <param name="type">The type of converter to add.</param>
    /// <param name="factory">A function that returns a <typeparamref name="T"/> instance.</param>
    /// <exception cref="ArgumentException">Thrown if a node converter of the same type is already registered.</exception>
    public void AddConverter(Type type, Func<IProceduralGraphLifecycle, INodeConverter> factory)
    {
        if (_factories.TryAdd(type, factory))
        {
            return;
        }

        throw new ArgumentException("A node converter of the same type already exists.", nameof(type));
    }

    /// <summary>
    /// Registers a new node converter type.
    /// </summary>
    /// <param name="factory">A function that returns a <typeparamref name="T"/> instance.</param>
    /// <typeparam name="T">The type of the converter to add.</typeparam>
    public void AddConverter<T>(Func<IProceduralGraphLifecycle, T> factory) where T : INodeConverter
    {
        AddConverter(typeof(T), lifecycle => factory(lifecycle));
    }

    /// <summary>
    /// Removes a registered converter of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the converter to remove.</typeparam>
    /// <returns>True if the converter was found and removed; otherwise false.</returns>
    public bool RemoveConverter<T>() where T : INodeConverter
    {
        return RemoveConverter(typeof(T));
    }

    /// <summary>
    /// Removes a registered converter of the specified type.
    /// </summary>
    /// <param name="type">The type of the converter to remove.</param>
    /// <returns>True if the converter was found and removed; otherwise false.</returns>
    public bool RemoveConverter(Type type)
    {
        return _factories.Remove(type);
    }

    /// <summary>
    /// Instantiates all registered converters using the provided lifecycle.
    /// </summary>
    /// <param name="lifecycle">The lifecycle context required to instantiate converters.</param>
    /// <returns>An array of instantiated node converters.</returns>
    public INodeConverter[] BuildConverters(IProceduralGraphLifecycle lifecycle)
    {
        INodeConverter[] converters = GC.AllocateUninitializedArray<INodeConverter>(_factories.Count);
        int index = 0;

        foreach (Func<IProceduralGraphLifecycle, INodeConverter> factory in _factories.Values)
        {
            converters[index++] = factory.Invoke(lifecycle);
        }

        return converters;
    }
}
