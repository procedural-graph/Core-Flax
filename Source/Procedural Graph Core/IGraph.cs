#nullable enable
using System.Collections.Generic;

namespace ProceduralGraph;

/// <summary>
/// Represents a node within the procedural graph.
/// </summary>
public interface IGraph : IReadOnlyCollection<IGraph>
{
    /// <summary>
    /// Gets the parent node of this instance, if any.
    /// </summary>
    IGraph? Parent { get; }
}
