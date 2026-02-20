using FlaxEditor;
using FlaxEngine;
using Newtonsoft.Json;
using ProceduralGraph.Collections;
using ProceduralGraph.Generic;
using ProceduralGraph.Generic.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ProceduralGraph.FlaxEngine;

/// <summary>
/// Represents an entity within a scene that participates in a graph-based relationship structure for actors.
/// </summary>
public sealed class SceneEntity : GraphEntity<Guid, Actor>, IProxyGraphNode<Actor>
{
    private sealed record Model
    {
        [Serialize, ShowInEditor]
        public required Guid ID { get; init; }

        [Serialize, ShowInEditor, JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public required List<GraphNodeModel> Children { get; init; }
    }

    private sealed class Converter : ProxyGraphEntityConverter<SceneEntity, Scene, Guid, Actor>
    {
        private readonly record struct HierarchyNode(Actor Actor, IGraphNode Parent);

        protected override SceneEntity ToEntity(Scene sceneMember, IGraph root, IGraphNode? parent = null)
        {
            IGraphConverterProvider converterProvider = root.Converters;

            HashSet<Actor> entitizedActors;
            SceneEntity sceneEntity;
            if (TryLoad(sceneMember, out Model? model))
            {
                sceneEntity = new(model.ID, sceneMember, root);
                ReadOnlySpan<GraphNodeModel> models = CollectionsMarshal.AsSpan(model.Children);
                entitizedActors = Load(sceneEntity, models, root);
            }
            else
            {
                sceneEntity = new(sceneMember, root);
                entitizedActors = [];
            }

            if (!TryGetChildren(sceneMember, out Actor[] children, out int childCount))
            {
                return sceneEntity;
            }

            Stack<HierarchyNode> nodes = new(childCount);
            PushChildren(children, sceneEntity, nodes);

            while (nodes.TryPop(out HierarchyNode hierarchyNode))
            {
                if (entitizedActors.Add(hierarchyNode.Actor))
                {
                    if (!converterProvider.TryFind(hierarchyNode.Actor, out IGraphConverter? converter))
                    {
                        continue;
                    }

                    IGraphNode? graphNode = converter.ToGraph(hierarchyNode.Actor, root, hierarchyNode.Parent);
                    hierarchyNode.Parent.Descendants.Add(graphNode);
                }

                if (TryGetChildren(hierarchyNode.Actor, out children, out childCount))
                {
                    nodes.EnsureCapacity(nodes.Count + childCount);
                    PushChildren(children, hierarchyNode.Parent, nodes);
                }
            }

            return sceneEntity;
        }

        private static HashSet<Actor> Load(SceneEntity sceneEntity, ReadOnlySpan<GraphNodeModel> children, IGraph root)
        {
            IGraphConverterProvider converterProvider = root.Converters;
            HashSet<Actor> entitizedActors = [];
            Dictionary<Guid, IGraphNode> nodes = new(1)
            {
                { sceneEntity.ID, sceneEntity }
            };
            
            for (int i = 0; i < children.Length; i++)
            {
                GraphNodeModel model = children[i];

                if (!converterProvider.TryFind(model, out IGraphConverter? converter))
                {
                    throw new InvalidOperationException($"No converter found for {model}.");
                }

                if (!nodes.TryGetValue(model.ParentID, out IGraphNode? parent))
                {
                    throw new InvalidOperationException($"Unable to resolve parent for {model}.");
                }

                IGraphNode? node;
                if (model is ProxyGraphEntityModel proxyModel)
                {
                    Actor actor = Level.FindActor(proxyModel.ActorID) ?? throw new InvalidOperationException($"Unable to find actor for {proxyModel}.");
                    entitizedActors.Add(actor);
                    node = converter.ToGraph(actor, root, proxyModel, parent);
                }
                else
                {
                    node = converter.ToGraph(model, root, parent);
                }

                parent.Descendants.Add(node);

                if (node is GraphEntity<Guid, Actor> entity)
                {
                    nodes.Add(entity.ID, entity);
                }
            }

            return entitizedActors;
        }

        private static bool TryGetChildren(Actor actor, out Actor[] children, out int childCount)
        { 
            children = actor.Children; 
            childCount = children.Length;
            return childCount > 0; 
        }

        private static void PushChildren(Actor[] children, IGraphNode parent, Stack<HierarchyNode> nodes)
        {
            for (int i = 0; i < children.Length; i++) 
            { 
                HierarchyNode node = new(children[i], parent); 
                nodes.Push(node); 
            }
        }

        private static bool TryLoad(Scene scene, [NotNullWhen(true)] out Model? model)
        {
            string path = AssetPath(scene);
            model = Content.LoadAsync<JsonAsset>(path)?.GetInstance<Model>();
            return model is { } && model.ID == scene.ID;
        }
    }

    private struct DescendantEnumerator(IGraphNode root) : IEnumerator<IGraphNode>
    {
        private readonly IGraphNode _root = root;
        private readonly Stack<IGraphNode> _nodes = new();
        private bool _initialized = false;

        private IGraphNode? _current;
        public readonly IGraphNode Current => _current!;
        readonly object IEnumerator.Current => _current!;

        public bool MoveNext()
        {
            if (!_initialized)
            {
                _initialized = true;
                _current = _root;
                return true;
            }

            if (_current is null)
            {
                return false;
            }

            ICollection<IGraphNode> descendants = _current.Descendants;
            int descendantCount = descendants.Count;
            if (descendantCount > 0)
            {
                _nodes.EnsureCapacity(_nodes.Count + descendantCount);
                switch (descendants)
                {
                    case ConcurrentGroupedCollection<Guid, GraphEntity<Guid, Actor>> concurrentGroupedCollection:
                        PushChildren(_nodes, concurrentGroupedCollection);
                        break;
                    case GenerativeGraphEntity<Guid, Actor>.DescendantCollection descendantCollection:
                        PushChildren(_nodes, descendantCollection);
                        break;
                    default:
                        PushChildren(_nodes, descendants);
                        break;
                }
            }

            return _nodes.TryPop(out _current);
        }

        public void Reset()
        {
            _nodes.Clear();
            _initialized = false;
        }

        readonly void IDisposable.Dispose() { }

        private static void PushChildren(Stack<IGraphNode> stack, ConcurrentGroupedCollection<Guid, GraphEntity<Guid, Actor>> descendants)
        {
            using ConcurrentGroupedCollection<Guid, GraphEntity<Guid, Actor>>.Enumerator enumerator = descendants.GetEnumerator();
            PushChildren(stack, enumerator);
        }

        private static void PushChildren(Stack<IGraphNode> stack, GenerativeGraphEntity<Guid, Actor>.DescendantCollection descendants)
        {
            using GenerativeGraphEntity<Guid, Actor>.DescendantCollection.Enumerator enumerator = descendants.GetEnumerator();
            PushChildren(stack, enumerator);
        }

        private static void PushChildren(Stack<IGraphNode> stack, ICollection<IGraphNode> descendants)
        {
            using IEnumerator<IGraphNode> enumerator = descendants.GetEnumerator();
            PushChildren(stack, enumerator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PushChildren<T>(Stack<IGraphNode> stack, T enumerator) where T : IEnumerator<IGraphNode>
        {
            while (enumerator.MoveNext())
            {
                stack.Push(enumerator.Current);
            }
        }
    }

    /// <inheritdoc/>
    public override Guid ID { get; }

    /// <inheritdoc/>
    public override GraphEntity<Guid, Actor>? Parent => null;

    /// <summary>
    /// Gets the scene associated with this instance.
    /// </summary>
    public Scene Scene { get; }
    Actor IProxyGraphNode<Actor>.SceneMember => Scene;

    /// <inheritdoc/>
    protected override IGraph Graph { get; }

    /// <inheritdoc/>
    public override event Action? Regenerating;

    /// <inheritdoc/>
    public override event Action? Regenerated;

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneEntity"/> class with the specified scene, graph, and logger.
    /// </summary>
    /// <param name="scene">The scene to which this entity belongs. Cannot be <see langword="null"/>.</param>
    /// <param name="graph">The graph representing the relationships between actors in the scene. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if scene, graph, or logger is <see langword="null"/>.</exception>
    public SceneEntity(Scene scene, IGraph graph)
    {
        ID = Guid.NewGuid();
        Scene = scene ?? throw new ArgumentNullException(nameof(scene));
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneEntity"/> class with the specified ID, scene, graph, and logger.
    /// </summary>
    /// <param name="id">The unique identifier for this entity.</param>
    /// <param name="scene">The scene to which this entity belongs. Cannot be <see langword="null"/>.</param>
    /// <param name="graph">The graph representing the relationships between actors in the scene. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if scene, graph, or logger is <see langword="null"/>.</exception>
    public SceneEntity(Guid id, Scene scene, IGraph graph)
    {
        ID = id;
        Scene = scene ?? throw new ArgumentNullException(nameof(scene));
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    /// <inheritdoc/>
    protected override void OnStateChanged()
    {
        Editor.Instance.Scene.MarkSceneEdited(Scene);
        base.OnStateChanged();
    }

    internal void Save()
    {
        Model model = new()
        {
            ID = ID,
            Children = GatherModels(Graph, this)
        };

        string path = AssetPath(Scene);
        Editor.SaveJsonAsset(path, model);
    }

    private static List<GraphNodeModel> GatherModels(IGraph graph, IGraphNode root)
    {
        IGraphConverterProvider converters = graph.Converters;

        List<GraphNodeModel> models = [];
        using DescendantEnumerator descendantEnumerator = new(root);
        while (descendantEnumerator.MoveNext())
        {
            IGraphNode node = descendantEnumerator.Current;
            if (converters.TryFind(node, out IGraphConverter? converter) && converter.ToModel(node, graph) is GraphNodeModel model)
            {
                models.Add(model);
            }
        }

        return models;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string AssetPath(Scene scene)
    {
        return Path.Combine(Globals.ProjectContentFolder, "SceneData", scene.Filename, "Procedural Graph.json");
    }

    internal static IGraphConverter CreateDefaultConverter() => new Converter();
}
