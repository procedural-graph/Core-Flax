#nullable enable
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ProceduralGraph.Hierarchy;

internal sealed class GraphRootEntityConverter : GenericGraphConverter<GraphRootModel, Scene, RootGraphEntity>
{
    public override RootGraphEntity ToGraph(GraphRootModel model, GraphLifecycleManager lifecycleManager, IGraphEntity? root = default)
    {
        Scene scene = Level.FindScene(model.SceneID) ?? throw new InvalidOperationException($"Scene with ID {model.SceneID} not found.");
        RootGraphEntity graph = new(model.ID, scene, lifecycleManager, AssetPath(scene), this);
        ReconstructHierarchy(model.Children, graph, lifecycleManager);
        ParseHierarchy(graph, lifecycleManager);
        return graph;
    }

    public override RootGraphEntity ToGraph(Scene scene, GraphLifecycleManager lifecycleManager, IGraphEntity? parent)
    {
        string path = AssetPath(scene);
        if (Content.LoadAsync<JsonAsset>(path)?.GetInstance<GraphRootModel>() is GraphRootModel rootModel && rootModel.SceneID == scene.ID)
        {
            return ToGraph(rootModel, lifecycleManager, null);
        }

        return new RootGraphEntity(Guid.NewGuid(), scene, lifecycleManager, path, this);
    }

    public override GraphRootModel ToModel(RootGraphEntity entity, GraphLifecycleManager lifecycleManager) => new()
    {
        ID = entity.ID,
        SceneID = entity.Actors.Values.Single().Key.ID,
        Children = [.. FlattenHierarchy(entity, lifecycleManager)]
    };

    private static string AssetPath(Scene scene)
    {
        return Path.Combine(Globals.ProjectContentFolder, "SceneData", scene.Filename, "Procedural Graph.json");
    }

    private static void ParseHierarchy(IGraphEntity entity, GraphLifecycleManager lifecycleManager)
    {
        ICollection<IGraphConverter> converters = lifecycleManager.Converters;
        List<Actor> stack = [];
        int index = PushChildren(entity, stack) - 1;
        do
        {
            Actor current = stack[index];

            if (!converters.TryFind(current, out IGraphConverter? converter))
            {
                stack.RemoveAt(index--);
                continue;
            }

            Index<Actor, IGraphEntity> foreignIndex = entity.Entities.GetOrAdd(current, out bool exists);
            if (!exists || foreignIndex.FirstOrDefault(converter.CanConvert) is not IGraphEntity childEntity)
            {
                childEntity = (IGraphEntity)converter.ToGraph(current, lifecycleManager, entity);
                foreignIndex.Add(childEntity);
            }

            int childCount = PushChildren(childEntity, stack);

            if (childCount == 0)
            {
                stack.RemoveAt(index--);
                continue;
            }

            CollectionsMarshal.SetCount(stack, index);
            index += childCount - 1;
        }
        while (index >= 0);
    }

    private static int PushChildren(IGraphEntity entity, List<Actor> stack)
    {
        int count = 0;

        foreach (Index<Actor, IGraphEntity> foreignIndex in entity.Actors.Values)
        {
            Actor[] children = foreignIndex.Key.Children;
            count += children.Length;
            stack.EnsureCapacity(stack.Count + children.Length);
            for (int i = children.Length - 1; i >= 0; i--)
            {
                stack.Add(children[i]);
            }
        }

        return count;
    }

    private static void ReconstructHierarchy(ReadOnlySpan<IGraphModel> children, [DisallowNull] IGraphEntity? parent, GraphLifecycleManager lifecycleManager)
    {
        if (children.IsEmpty)
        {
            return;
        }

        ICollection<IGraphConverter> converters = lifecycleManager.Converters;

        int index = 0;
        IGraphModel child;
        do
        {
            do
            {
                child = children[index++];
                IGraphConverter converter = converters.Find(child);
                IGraph node = converter.ToGraph(child, lifecycleManager, parent);

                switch (node)
                {
                    case IGraphEntity entity: parent.Entities.Add(entity); break;
                    case IGraphComponent component: parent.Components.Add(component); break;
                    default: throw new InvalidOperationException("Converted graph is neither an entity nor a component.");
                }

                if (index == children.Length)
                {
                    return;
                }
            }
            while (child.ParentID == parent.ID);
        }
        while (TryFindParent(ref parent, child));

        throw new InvalidOperationException($"Failed to reconstruct graph hierarchy. Could not find parent with ID {child.ParentID}.");
    }

    private static bool TryFindParent([NotNullWhen(true)] ref IGraphEntity? parent, IGraphModel child)
    {
        while (parent is not null)
        {
            if (parent.Entities.TryGetValue(child.ParentID, out Index<IGraphEntity, Actor>? index))
            {
                parent = index.Key;
                return true;
            }

            parent = parent.Parent;
        }

        return false;
    }

    private static IEnumerable<IGraphModel> FlattenHierarchy(IGraphEntity root, GraphLifecycleManager lifecycleManager)
    {
        ICollection<IGraphConverter> converters = lifecycleManager.Converters;
        List<IGraph> stack = [root];
        int index = 0;
        do
        {
            IGraph current = stack[index];
            IGraphConverter converter = converters.Find(current);
            yield return converter.ToModel(current, lifecycleManager);

            int count = current.Count;

            if (count == 0)
            {
                stack.RemoveAt(index--);
                continue;
            }

            CollectionsMarshal.SetCount(stack, index);
            stack.AddRange(current.Reverse());
            index += count - 1;
        }
        while (index >= 0);
    }
}
