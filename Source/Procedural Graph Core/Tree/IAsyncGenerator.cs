using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph.Tree;

/// <summary>
/// Defines an asynchronous generator that executes procedural generation logic when invoked.
/// </summary>
public interface IAsyncGenerator
{
    /// <summary>
    /// Executes the generation process.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task GenerateAsync(CancellationToken cancellationToken);
}
