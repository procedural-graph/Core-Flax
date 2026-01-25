using System;

namespace ProceduralGraph;

/// <summary>
/// Defines the contract for a node within a graph structure, providing identification and hierarchical relationship
/// information.
/// </summary>
public interface IGraphEntityModel : IGraphModel
{
    /// <summary>
    /// Gets the unique identifier for the instance.
    /// </summary>
    Guid ID { get; }
}
