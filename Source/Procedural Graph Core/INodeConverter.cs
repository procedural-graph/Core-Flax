using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// Defines a contract for classes capable of converting a Flax Engine <see cref="Actor"/> into a graph node.
/// </summary>
public interface INodeConverter
{
    /// <summary>
    /// Determines whether the specified actor can be converted into an <see cref="INode"/>.
    /// </summary>
    /// <param name="actor">The actor to evaluate.</param>
    /// <returns>True if the actor was compatible and converted; otherwise false.</returns>
    bool CanConvert(Actor actor);

    /// <summary>
    /// Converts the specified actor into an <see cref="INode"/>.
    /// </summary>
    /// <param name="actor">The actor to convert.</param>
    /// <param name="models">The deserialized <see cref="Model"/> instances for this actor.</param>
    /// <returns>The resulting node.</returns>
    /// <exception cref="System.ArgumentException">Thrown if the specified actor cannot be converted.</exception>
    INode Convert(Actor actor, IEnumerable<Model> models);
}
