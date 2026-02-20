using System;

namespace ProceduralGraph.FlaxEngine;

/// <summary>
/// Represents the base type for a node in a graph structure.
/// </summary>
public abstract record GraphNodeModel
{
    /// <summary>
    /// Gets the unique identifier of the parent node associated with this model.
    /// </summary>
    public abstract Guid ParentID { get; }
}
