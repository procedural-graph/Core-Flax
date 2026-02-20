using FlaxEditor;
using FlaxEngine;
using ProceduralGraph.Generic;
using ProceduralGraph.Generic.Converters;
using System;
using System.Threading;

namespace ProceduralGraph.FlaxEngine;

/// <summary>
/// Provides the core runtime execution layer for the Procedural Graph system within the editor. Manages scene and actor
/// events, coordinates asynchronous graph generation tasks, and facilitates the transformation of Flax Actors into
/// procedural graph entities.
/// </summary>
public sealed class ProceduralGraphPlugin : EditorPlugin
{
    private static readonly Logger _logger = new();

    private CancellationTokenSource? _stoppingCts;
    private FlaxEngineGraph? _graph;

    /// <summary>
    /// Occurs when the plugin is initializing and graph converters can be registered.
    /// </summary>
    public event Action<ProceduralGraphPlugin, GraphConverterRegistrar>? Initializing;

    /// <summary>
    /// Initializes a new instance of <see cref="ProceduralGraphPlugin"/>.
    /// </summary>
    public ProceduralGraphPlugin() : base()
    {
        _description = new PluginDescription()
        {
            Name = "Procedural Graph: Core",
            Author = "William Brocklesby",
            AuthorUrl = "https://william-brocklesby.com",
            Category = "Procedural Graph",
            Description = "The runtime execution layer for the Procedural Graph framework. It listens for scene and actor changes, manages asynchronous graph generation tasks, and coordinates the transformation of Flax Actors into procedural graph entities.",
            RepositoryUrl = "https://github.com/procedural-graph/Core-Flax.git",
            Version = new(3, 0, 0)
        };
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        if (_stoppingCts is null || !_stoppingCts.TryReset())
        {
            _stoppingCts = new CancellationTokenSource();
        }

        GraphConverterRegistrar registrar = [SceneEntity.CreateDefaultConverter()];
        Initializing?.Invoke(this, registrar);
        GraphConverterProvider provider = registrar.BuildConverterProvider();

        _graph = new FlaxEngineGraph(provider, _logger);
        _graph.Start(_stoppingCts.Token);

        Level.SceneLoaded += OnSceneLoaded;
        Level.SceneUnloading += OnSceneUnloading;
        Level.ActorSpawned += OnActorSpawned;
        Level.ActorDeleted += OnActorDeleted;
        Level.SceneSaving += OnSceneSaving;
    }

    private void OnActorSpawned(Actor actor)
    {
        _graph!.Add(actor, out _);
    }

    private void OnActorDeleted(Actor actor)
    {
        _graph!.Remove(actor);
    }

    private void OnSceneSaving(Scene scene, Guid guid)
    {
        if (_graph!.TryGetValue(scene, out GraphEntity<Guid, Actor>? entity) && entity is SceneEntity sceneEntity)
        {
            sceneEntity.Save();
        }
    }

    private void OnSceneLoaded(Scene scene, Guid guid)
    {
        _graph!.Add(scene, out _);
    }

    private void OnSceneUnloading(Scene scene, Guid guid)
    {
        _graph!.Remove(scene);
    }

    /// <inheritdoc/>
    override public void Deinitialize()
    {
        Level.SceneLoaded -= OnSceneLoaded;
        Level.SceneUnloading -= OnSceneUnloading;
        Level.SceneSaving -= OnSceneSaving;
        Level.ActorSpawned -= OnActorSpawned;
        Level.ActorDeleted -= OnActorDeleted;

        try
        {
            CancellationTokenSource stoppingCts = _stoppingCts!;
            (FlaxEngineGraph graph, _graph) = (_graph!, null);

            if (Engine.IsRequestingExit)
            {
                try
                {
                    stoppingCts.Cancel();
                }
                finally
                {
                    stoppingCts.Dispose();
                    graph.Dispose();
                }
                
                return;
            }

            CancellationTokenSource shutdownCts = new();
            Engine.RequestingExit += shutdownCts.Cancel;

            try
            {
                stoppingCts.Cancel();
                graph.Lifetime.Wait(shutdownCts.Token);
            }
            finally
            {
                graph.Dispose();

                Engine.RequestingExit -= shutdownCts.Cancel;
                shutdownCts.Dispose();

                stoppingCts.Dispose();
            }
        }
        finally
        {
            base.Deinitialize();
        }
    }
}
