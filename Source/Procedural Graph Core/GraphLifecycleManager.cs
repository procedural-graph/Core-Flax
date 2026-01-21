using FlaxEditor;
using FlaxEngine;
using ProceduralGraph.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ProceduralGraph;

/// <summary>
/// Manages the lifecycle of procedural graph entities within the Flax Engine editor, handling their creation,
/// execution, and disposal in response to scene and actor events.
/// </summary>
public sealed class GraphLifecycleManager : EditorPlugin
{
    private ref struct EntityLocator : IEntityVisitor
    {
        public Guid TargetID { readonly get; init; }

        public ref Index<Actor, IGraphEntity>? Result;

        public bool Visit(IGraphEntity entity) => entity.Actors.TryGetValue(TargetID, out Result);
    }

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
    public IReadOnlyList<IGraphConverter> Converters => _converters;

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
    /// Searches for graph entities associated with the specified actor within all available graphs.
    /// </summary>
    /// <param name="actor">The actor whose associated entities are to be found. If <paramref name="actor"/> is <see langword="null"/>, the
    /// method returns an empty collection.</param>
    /// <returns>An enumerable collection of <see cref="IGraphEntity"/> objects associated with the specified actor. Returns an
    /// empty collection if no matching entities are found or if <paramref name="actor"/> is <see langword="null"/>.</returns>
    public IEnumerable<IGraphEntity> FindEntities(Actor? actor)
    {
        if (actor == null || !_graphs.TryGetValue(actor.Scene, out IGraphEntity? root))
        {
            return [];
        }

        Index<Actor, IGraphEntity>? result = null;
        EntityLocator locator = new()
        {
            TargetID = actor.ID,
            Result = ref result
        };

        foreach (IGraphEntity graph in _graphs.Values)
        {
            if (graph.DepthFirstSearch(in locator, out _))
            {
                return result!;
            }
        }

        return [];
    }

    private void OnSceneLoaded(Scene scene, Guid guid)
    {
        IGraphConverter converter = _converters.Find(scene);
        IGraphEntity graph = converter.ToEntity(scene, this, null);
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
