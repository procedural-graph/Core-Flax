using System.Threading;

namespace ProceduralGraph;

/// <summary>
/// Represents a procedural graph.
/// </summary>
public interface IProceduralGraphLifecycle
{
    /// <summary>
    /// Gets a cancellation token that is triggered when the Procedural Graph is stopping or unloaded.
    /// </summary>
    CancellationToken StoppingToken { get; }
}
