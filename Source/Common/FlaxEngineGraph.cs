using System;
using FlaxEngine;
using ProceduralGraph.Generic;

namespace ProceduralGraph.FlaxEngine;

/// <summary>
/// Represents a graph of Flax Engine actors, providing access to actor relationships and conversion utilities within a
/// scene.
/// </summary>
public sealed class FlaxEngineGraph : Graph<Guid, Actor>
{
    private static readonly ActorInfoProvider _actorInfoProvider = new();

    /// <inheritdoc/>
    public override IGraphConverterProvider Converters { get; }

    /// <inheritdoc/>
    protected override ISceneMemberInfoProvider<Guid, Actor> SceneMemberInfoProvider => _actorInfoProvider;

    /// <inheritdoc/>
    public override ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlaxEngineGraph"/> class using the specified graph converter provider.
    /// </summary>
    /// <param name="converters">The provider that supplies graph converters used by the <see cref="FlaxEngineGraph"/>. Cannot be <see langword="null"/>.</param>
    /// <param name="logger">The logger used for logging information, warnings, and errors within the graph. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="converters"/> is <see langword="null"/>.</exception>
    public FlaxEngineGraph(IGraphConverterProvider converters, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(converters, nameof(converters));
        Converters = converters;

        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        Logger = logger;
    }
}
