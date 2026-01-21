using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph.Tree.Entities;

internal sealed class RootGraphEntity(Guid id, Scene scene, GraphLifecycleManager lifecycleManager, string path, IGraphConverter converter) : GraphEntity<Scene>(lifecycleManager)
{
    private readonly struct EntityVisitor : IEntityVisitor
    {
        public Actor Parent { get; init; }

        public bool Visit(IGraphEntity entity)
        {
            return Parent == entity.Actor;
        }
    }

    public override Guid ID { get; } = id;

    public string AssetPath { get; } = path;

    private readonly IGraphConverter _converter = converter ?? throw new ArgumentNullException(nameof(converter));

    public override Scene Actor { get; } = scene ?? throw new ArgumentNullException(nameof(scene));

    public override ICollection<IGraphComponent> Components => [];

    public override IGraphEntity? Parent => null;

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
        if (actor.Scene != Actor || !LifecycleManager.Converters.TryFind(actor, out IGraphConverter? converter) || !TryFindParent(actor, this, out IGraphEntity? parent))
        {
            return;
        }

        IGraphEntity entity = converter.ToEntity(actor, LifecycleManager, parent);
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
        if (actor.Scene == Actor && TryFindParent(actor, this, out IGraphEntity? parent))
        {
            parent.Actors.Remove(actor);
        }
    }

    private void OnSceneSaving(Scene scene, Guid guid)
    {
        if (scene == Actor)
        {
            object model = _converter.ToModel(this, LifecycleManager);
            Editor.SaveJsonAsset(AssetPath, model);
        }
    }

    private void OnDescendantRegenerated()
    {
        Editor.Instance.Scene.MarkSceneEdited(Actor);
    }

    private static bool TryFindParent(Actor actor, IGraphEntity entity, [NotNullWhen(true)] out IGraphEntity? result)
    {
        EntityVisitor visitor = new()
        { 
            Parent = actor.Parent
        };

        return entity.DepthFirstSearch(in visitor, out result);
    }
}
