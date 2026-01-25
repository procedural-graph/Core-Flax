#nullable enable
using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace ProceduralGraph;

/// <summary>
/// Manages the lifecycle of procedural graph entities within the Flax Engine editor, handling their creation,
/// execution, and disposal in response to scene and actor events.
/// </summary>
public sealed class GraphLifecycleManager : EditorPlugin
{
    private readonly Dictionary<Scene, IGraphEntity> _graphs;

    private CancellationTokenSource? _stoppingCts;
    /// <summary>
    /// Gets a cancellation token that is triggered when the <see cref="Plugin"/> is unloaded.
    /// </summary>
    public CancellationToken StoppingToken => _stoppingCts!.Token;

    private readonly List<IGraphConverter> _converters;
    /// <summary>
    /// Gets a collection of graph converters which facilitate the transformation of Flax Actors into entities.
    /// </summary>
    public ICollection<IGraphConverter> Converters => _converters;

    /// <summary>
    /// Initializes a new instance of <see cref="GraphLifecycleManager"/>.
    /// </summary>
    public GraphLifecycleManager() : base()
    {
        _graphs = [];
        _converters = [];
        _description = new PluginDescription()
        {
            Name = "Procedural Graph: Core",
            Author = "William Brocklesby",
            AuthorUrl = "https://william-brocklesby.com",
            Category = "Procedural Graph",
            Description = "The runtime execution layer for the Procedural Graph system. It listens for scene and actor changes, manages asynchronous graph generation tasks to maintain editor responsiveness, and coordinates the transformation of Flax Actors into procedural graph entities.",
            RepositoryUrl = "https://github.com/will11600/Procedural-Graph-Client.git",
            Version = new(2, 0, 0)
        };
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        try
        {
            _stoppingCts = new CancellationTokenSource();
            Level.SceneLoaded += OnSceneLoaded;
            Level.SceneUnloading += OnSceneUnloading;
        }
        finally
        {
            base.Initialize();
        }
    }

    /// <inheritdoc/>
    public override void Deinitialize()
    {
        try
        {
            _stoppingCts!.Cancel();
        }
        finally
        {
            Level.SceneLoaded -= OnSceneLoaded;
            Level.SceneUnloading -= OnSceneUnloading;

            _stoppingCts?.Dispose();

            foreach (IDisposable disposable in _graphs.Values.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            base.Deinitialize();
        }
    }

    /// <summary>
    /// Attempts to locate the specified actor within the graph and retrieves its corresponding index if found.
    /// </summary>
    /// <param name="actor">The actor to locate within the graph.</param>
    /// <param name="result">
    /// When this method returns, contains the index of the specified actor if found; otherwise, <see langword="null"/>. This parameter is
    /// passed uninitialized.</param>
    /// <returns><see langword="true"/> if the actor was found and the corresponding index is returned in result; otherwise, <see langword="false"/>.</returns>
    public bool TryFind([NotNullWhen(true)] Actor? actor, [NotNullWhen(true)] out Index<Actor, IGraphEntity>? result)
    {
        if (actor != null && _graphs.TryGetValue(actor.Scene, out IGraphEntity? root))
        {
            return root.TryFind(actor, out result);
        }

        result = null;
        return false;
    }

    private void OnSceneLoaded(Scene scene, Guid guid)
    {
        IGraphConverter converter = _converters.Find(scene);
        IGraphEntity graph = (IGraphEntity)converter.ToGraph(scene, this, null);
        _graphs.Add(scene, graph);
    }

    private async void OnSceneUnloading(Scene scene, Guid guid)
    {
        if (!_graphs.Remove(scene, out IGraphEntity? graph))
        {
            return;
        }

        try
        {
            await graph.StopAsync(StoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            DebugUtils.LogException(ex);
        }
        finally
        {
            graph.Dispose();
        }
    }
}
