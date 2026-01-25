#nullable enable
using FlaxEngine;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProceduralGraph;

/// <summary>
/// Provides extension methods for searching a collection of graph converters to find one capable of converting a
/// specified object.
/// </summary>
public static class GraphConverterExtensions
{
    /// <summary>
    /// Attempts to find a converter in the collection that can convert the specified <see cref="Actor"/>.
    /// </summary>
    /// <remarks>The method returns the first converter in the collection for which <see cref="IGraphConverter.CanConvert(Actor?)"/> returns true
    /// for the specified object. If no such converter is found, result is set to <see langword="null"/>.</remarks>
    /// <param name="converters">The collection of converters to search. Cannot be <see langword="null"/>.</param>
    /// <param name="actor">The object to be converted. Can be <see langword="null"/>.</param>
    /// <param name="result">When this method returns, contains the first converter that can convert the specified object, if found;
    /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if a suitable converter is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryFind(this ICollection<IGraphConverter> converters, [NotNullWhen(true)] Actor? actor, [NotNullWhen(true)] out IGraphConverter? result)
    {
        result = null;

        if (actor is null)
        {
            return false;
        }

        foreach (IGraphConverter converter in converters)
        {
            if (converter.CanConvert(actor))
            {
                result = converter;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Searches the specified collection of graph converters for one that supports the given <see cref="Actor"/>.
    /// </summary>
    /// <param name="converters">The collection of graph converters to search. Cannot be <see langword="null"/>.</param>
    /// <param name="actor">The <see cref="Actor"/> for which to find a compatible graph converter. Cannot be <see langword="null"/>.</param>
    /// <returns>The first graph converter in the collection that supports the specified <see cref="Actor"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no compatible graph converter is found for the specified <see cref="Actor"/>.</exception>
    public static IGraphConverter Find(this ICollection<IGraphConverter> converters, Actor actor)
    {
        if (converters.TryFind(actor, out IGraphConverter? converter))
        {
            return converter;
        }

        throw new KeyNotFoundException($"No converter found for actor of type {actor.GetType().FullName}.");
    }

    /// <summary>
    /// Attempts to find a converter in the collection that can convert the specified <see cref="IGraphEntity"/>.
    /// </summary>
    /// <remarks>The method returns the first converter in the collection for which <see cref="IGraphConverter.CanConvert(IGraph?)"/> returns true
    /// for the specified object. If no such converter is found, result is set to <see langword="null"/>.</remarks>
    /// <param name="converters">The collection of converters to search. Cannot be <see langword="null"/>.</param>
    /// <param name="node">The object to be converted. Can be <see langword="null"/>.</param>
    /// <param name="result">When this method returns, contains the first converter that can convert the specified object, if found;
    /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if a suitable converter is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryFind(this ICollection<IGraphConverter> converters, [NotNullWhen(true)] IGraph? node, [NotNullWhen(true)] out IGraphConverter? result)
    {
        result = null;

        if (node is null)
        {
            return false;
        }

        foreach (IGraphConverter converter in converters)
        {
            if (converter.CanConvert(node))
            {
                result = converter;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Searches the specified collection of graph converters for one that supports the given <see cref="IGraphEntity"/>.
    /// </summary>
    /// <param name="converters">The collection of graph converters to search. Cannot be <see langword="null"/>.</param>
    /// <param name="node">The <see cref="IGraph"/> for which to find a compatible graph converter. Cannot be <see langword="null"/>.</param>
    /// <returns>The first graph converter in the collection that supports the specified <see cref="IGraphEntity"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no compatible graph converter is found for the specified <see cref="IGraphEntity"/>.</exception>
    public static IGraphConverter Find(this ICollection<IGraphConverter> converters, IGraph node)
    {
        if (converters.TryFind(node, out IGraphConverter? converter))
        {
            return converter;
        }

        throw new KeyNotFoundException($"No converter found for entity of type {node.GetType().FullName}.");
    }

    /// <summary>
    /// Attempts to find a converter in the collection that can convert the specified <see cref="object"/>.
    /// </summary>
    /// <remarks>The method returns the first converter in the collection for which <see cref="IGraphConverter.CanConvert(IGraphModel?)"/> returns true
    /// for the specified object. If no such converter is found, result is set to <see langword="null"/>.</remarks>
    /// <param name="converters">The collection of converters to search. Cannot be <see langword="null"/>.</param>
    /// <param name="model">The object to be converted. Can be <see langword="null"/>.</param>
    /// <param name="result">When this method returns, contains the first converter that can convert the specified object, if found;
    /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if a suitable converter is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryFind(this ICollection<IGraphConverter> converters, [NotNullWhen(true)] IGraphModel? model, [NotNullWhen(true)] out IGraphConverter? result)
    {
        result = null;

        if (model is null)
        {
            return false;
        }

        foreach (IGraphConverter converter in converters)
        {
            if (converter.CanConvert(model))
            {
                result = converter;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Searches the specified collection of graph converters for one that supports the given <see cref="IGraphModel"/>.
    /// </summary>
    /// <param name="converters">The collection of graph converters to search. Cannot be <see langword="null"/>.</param>
    /// <param name="model">The <see cref="IGraphModel"/> for which to find a compatible graph converter. Cannot be <see langword="null"/>.</param>
    /// <returns>The first graph converter in the collection that supports the specified <see cref="object"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no compatible graph converter is found for the specified <see cref="object"/>.</exception>
    public static IGraphConverter Find(this ICollection<IGraphConverter> converters, IGraphModel model)
    {
        if (converters.TryFind(model, out IGraphConverter? converter))
        {
            return converter;
        }

        throw new KeyNotFoundException($"No converter found for entity of type {model.GetType().FullName}.");
    }
}