using FlaxEditor.CustomEditors;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph;

/// <summary>
/// Represents a processing unit within the procedural graph.
/// </summary>
public interface IGraphNode : IDisposable
{
    /// <summary>
    /// Gets a cancellation token that is triggered when the node is stopping or disposing.
    /// </summary>
    CancellationToken StoppingToken { get; }

    /// <summary>
    /// Gets the configuration parameters for this node.
    /// </summary>
    ICollection<GraphModel> Models { get; }

    /// <summary>
    /// Gets or the Actor associated with this node.
    /// </summary>
    Actor Actor { get; }

    /// <summary>
    /// Gets a <see cref="CustomValueContainer"/> of configurable values to display in the editor.
    /// </summary>
    CustomValueContainer ValueContainer { get; }

    /// <summary>
    /// Requests that the node stop any background processing immediately.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for the stopping process itself.</param>
    /// <returns>A task representing the shutdown operation.</returns>
    Task StopAsync(CancellationToken cancellationToken);
}