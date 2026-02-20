using System;

namespace ProceduralGraph.FlaxEngine;

/// <summary>
/// Represents the base model for a graph entity that proxies a Flax Engine actor.
/// </summary>
public abstract record ProxyGraphEntityModel : GraphEntityModel
{
    /// <summary>
    /// Gets the unique identifier of the Flax Engine actor associated with this model.
    /// </summary>
    public abstract Guid ActorID { get; }
}
