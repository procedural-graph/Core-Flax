using System;

namespace ProceduralGraph.Tree.Entities;

/// <summary>
/// Represents an entity within the procedural graph that has unique identifiers for itself and it's parent.
/// </summary>
public interface IGraphEntityModel : IGraphNodeModel
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    Guid EntityID { get; }
    Guid IGraphNodeModel.NodeID => EntityID;
}
