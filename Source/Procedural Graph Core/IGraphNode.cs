using System;

namespace ProceduralGraph;

/// <summary>
/// Represents a node within the procedural graph.
/// </summary>
public interface IGraphNode
{
    /// <summary>
    /// Gets the unique identifier for this instance.
    /// </summary>
    Guid ID { get; }

    /// <summary>
    /// Gets the parent node of this instance, if any.
    /// </summary>
    IGraphEntity? Parent { get; }
}
