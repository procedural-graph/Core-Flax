using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph;

/// <summary>
/// Defines an interface for components that support asynchronous startup and graceful shutdown operations, along with
/// lifecycle management and disposal.
/// </summary>
public interface IAsyncLifecycle : IDisposable
{
    /// <summary>
    /// Gets a token that is triggered when the service is stopping.
    /// </summary>
    CancellationToken StoppingToken { get; }

    /// <summary>
    /// Initiates the asynchronous startup process for the component.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the service.</param>
    /// <returns>A ValueTask that represents the asynchronous startup operation.</returns>
    ValueTask StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Initiates an asynchronous operation to gracefully shutdown the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous stop operation.</returns>
    ValueTask StopAsync(CancellationToken cancellationToken);
}