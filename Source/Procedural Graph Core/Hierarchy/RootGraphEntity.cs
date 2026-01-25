#nullable enable
using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph.Hierarchy;

internal sealed class RootGraphEntity : GraphEntity<Scene>
{
    public override Guid ID { get; }

    public string AssetPath { get; }

    private readonly IGraphConverter _converter;

    public override ICollection<IGraphComponent> Components => [];

    public override IGraphEntity? Parent => null;

    public RootGraphEntity(Guid id, Scene scene, GraphLifecycleManager lifecycleManager, string path, IGraphConverter converter) : base(lifecycleManager)
    {
        Actors.Add(scene);
        ID = id;
        AssetPath = path;
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    public override async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        Level.SceneSaving += OnSceneSaving;
        Level.ActorSpawned += OnActorSpawned;
        Level.ActorDeleted += OnActorDeleted;
        StateChanged += OnDescendantRegenerated;
    }

    protected override void OnStopRequested()
    {
        Level.SceneSaving -= OnSceneSaving;
        Level.ActorSpawned -= OnActorSpawned;
        Level.ActorDeleted -= OnActorDeleted;
        StateChanged -= OnDescendantRegenerated;
    }

    private async void OnActorSpawned(Actor actor)
    {
        if (Actors.ContainsPrimaryKey(actor.Scene) 
            || !LifecycleManager.Converters.TryFind(actor, out IGraphConverter? converter) 
            || !TryFind(actor.Parent, out Index<Actor, IGraphEntity>? index)
            || index.FirstOrDefault() is not IGraphEntity parent)
        {
            return;
        }

        IGraphEntity entity = (IGraphEntity)converter.ToGraph(actor, LifecycleManager, parent);
        try
        {
            await entity.StartAsync(parent.StoppingToken);
        }
        catch (Exception ex)
        {
            Editor.LogError($"An error occurred while starting graph entity for spawned actor '{actor.Name}': {ex}");
            entity.Dispose();
            return;
        }

        parent.Entities.Add(entity);
    }

    private void OnActorDeleted(Actor actor)
    {
        if (!Actors.ContainsPrimaryKey(actor.Scene))
        {
            return;
        }

        if (!TryFind(actor, out Index<Actor, IGraphEntity>? index))
        {
            return;
        }

        List<Task> stopTasks = new(index.Count);
        foreach (IGraphEntity entity in index)
        {
            ValueTask stopTask = entity.StopAsync(StoppingToken);
            if (!stopTask.IsCompletedSuccessfully)
            {
                stopTasks.Add(stopTask.AsTask());
            }
        }

        try
        {
            Task.WaitAll(stopTasks);
        }
        finally
        {
            foreach (IGraphEntity entity in index)
            {
                entity.Dispose();
            }

            index.Clear();
        }
    }

    private void OnSceneSaving(Scene scene, Guid guid)
    {
        if (Actors.ContainsPrimaryKey(guid))
        {
            object model = _converter.ToModel(this, LifecycleManager);
            Editor.SaveJsonAsset(AssetPath, model);
        }
    }

    private void OnDescendantRegenerated()
    {
        foreach (var index in Actors)
        {
            if (index.Key is Scene scene)
            {
                Editor.Instance.Scene.MarkSceneEdited(scene);
            }
        }
    }
}
