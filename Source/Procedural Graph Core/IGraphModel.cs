using System;

namespace ProceduralGraph;

/// <summary>
/// Represents a model that provides access to the unique identifier of a parent entity within a graph structure.
/// </summary>
public interface IGraphModel
{
    /// <summary>
    /// Gets the unique identifier of the parent entity.
    /// </summary>
    Guid ParentID { get; }
}
