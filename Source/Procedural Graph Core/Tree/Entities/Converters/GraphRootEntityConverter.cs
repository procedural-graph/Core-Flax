using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProceduralGraph.Tree.Entities.Converters;

internal sealed class GraphRootEntityConverter : GenericGraphConverter<GraphRootModel, Scene, RootGraphEntity>
{
    private readonly ref struct HierarchyFlattener : IEntityVisitor
    {
        public List<IGraphEntityModel> Models { get; init; }

        public IReadOnlyList<IGraphConverter> Converters { get; init; }

        public GraphLifecycleManager LifecycleManager { get; init; }

        public bool Visit(IGraphEntity entity)
        {
            IGraphConverter converter = Converters.Find(entity);
            IGraphEntityModel model = converter.ToModel<IGraphEntityModel>(entity, LifecycleManager);
            Models.EnsureCapacity(Models.Count + entity.Entities.Count);
            Models.Add(model);
            return false;
        }
    }

    private readonly ref struct Backtracker : IEntityVisitor
    {
        public required IGraphEntityModel Child { get; init; }

        public bool Visit(IGraphEntity entity)
        {
            return Child.ParentID == entity.ID;
        }
    }

    public override RootGraphEntity ToEntity(GraphRootModel model, GraphLifecycleManager lifecycleManager, IGraphEntity? root = default)
    {
        Scene scene = Level.FindScene(model.SceneID) ?? throw new InvalidOperationException($"Scene with ID {model.SceneID} not found.");
        RootGraphEntity graph = new(model.EntityID, scene, lifecycleManager, AssetPath(scene), this);

        IGraphEntityModel[] children = model.Children;

        if (children.Length == 0)
        {
            return graph;
        }

        IReadOnlyList<IGraphConverter> converters = lifecycleManager.Converters;

        int index = 1;
        IGraphEntity? parent = graph;
        IGraphEntityModel child = children[0];
        do
        {
            while (child.ParentID == parent!.ID)
            {
                IGraphConverter converter = converters.Find(child);
                IGraphEntity entity = converter.ToEntity(model, lifecycleManager, root);
                parent.Entities.Add(entity);

                if (index == children.Length)
                {
                    return graph;
                }

                child = children[index++];
            }
        }
        while (TryFindParentInSiblings(ref parent, child) || TryFindParentInAncestors(child, ref parent));

        throw new InvalidOperationException($"Failed to reconstruct graph hierarchy for root entity {graph.ID}. Could not find parent with ID {child.ParentID}.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFindParentInSiblings([DisallowNull] ref IGraphEntity? parent, IGraphEntityModel child)
    {
        if (parent.Entities.TryGetValue(child.ParentID, out Index<IGraphEntity, Actor>? index))
        {
            parent = index.Key;
            return true;
        }

        return false;
    }

    private static bool TryFindParentInAncestors(IGraphEntityModel child, [NotNullWhen(true)] ref IGraphEntity? parent)
    {
        if (parent is null)
        {
            return false;
        }

        Backtracker backtracker = new()
        {
            Child = child
        };

        parent = parent.FindAncestor(in backtracker);

        return parent is { };
    }

    public override RootGraphEntity ToEntity(Scene scene, GraphLifecycleManager lifecycleManager, IGraphEntity? parent)
    {
        string path = AssetPath(scene);
        if (Content.LoadAsync<JsonAsset>(path)?.GetInstance<GraphRootModel>() is GraphRootModel rootModel && rootModel.SceneID == scene.ID)
        {
            return ToEntity(rootModel, lifecycleManager, null);
        }

        return new RootGraphEntity(Guid.NewGuid(), scene, lifecycleManager, path, this);
    }

    private static string AssetPath(Scene scene)
    {
        return Path.Combine(Globals.ProjectContentFolder, "SceneData", scene.Filename, "Procedural Graph.json");
    }

    public unsafe override GraphRootModel ToModel(RootGraphEntity entity, GraphLifecycleManager lifecycleManager)
    {
        List<IGraphEntityModel> models = new(entity.Entities.Count);

        HierarchyFlattener flattener = new()
        {
            Models = models,
            Converters = lifecycleManager.Converters,
            LifecycleManager = lifecycleManager
        };

        entity.DepthFirstSearch(in flattener, out _);

        return new GraphRootModel
        {
            EntityID = entity.ID,
            SceneID = entity.Actor!.ID,
            Children = [.. models]
        };
    }
}
