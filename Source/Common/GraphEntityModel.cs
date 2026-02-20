using System;

namespace ProceduralGraph.FlaxEngine;

/// <summary>
/// Represents the base type for all entity models in a graph structure.
/// </summary>
public abstract record GraphEntityModel : GraphNodeModel
{
    /// <summary>
    /// Gets the unique identifier of the entity associated with this model.
    /// </summary>
    public abstract Guid ID { get; }
}
