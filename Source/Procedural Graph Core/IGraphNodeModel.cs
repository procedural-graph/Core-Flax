using System;

namespace ProceduralGraph;

/// <summary>
/// Represents a node within the procedural graph that has unique identifiers for itself and it's parent.
/// </summary>
public interface IGraphNodeModel
{
    /// <summary>
    /// Gets the unique identifier of the node.
    /// </summary>
    Guid NodeID { get; }

    /// <summary>
    /// Gets the unique identifier of the parent node.
    /// </summary>
    Guid ParentID { get; }
}
