using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph.Tree;

/// <summary>
/// Defines a synchronous generator that executes procedural generation logic when invoked.
/// </summary>
public interface IGenerator : IAsyncGenerator
{
    /// <summary>
    /// Executes the generation process.
    /// </summary>
    void Generate();

    async Task IAsyncGenerator.GenerateAsync(CancellationToken cancellationToken)
    {
        await Task.Run(Generate, cancellationToken);
    }
}
