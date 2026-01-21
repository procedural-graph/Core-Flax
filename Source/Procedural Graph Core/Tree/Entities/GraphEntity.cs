using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph.Tree.Entities;

/// <summary>
/// Represents a base class for entities within a graph structure, providing common lifecycle management, hierarchical
/// relationships, and component support.
/// </summary>
public abstract class GraphEntity : IGraphEntity
{
    private sealed class EntityCollection(GraphEntity entity) : Map<Guid, IGraphEntity, Guid, Actor>()
    {
        private readonly GraphEntity _entity = entity ?? throw new ArgumentNullException(nameof(entity));

        protected override Dictionary<Guid, Index<IGraphEntity, Actor>> PrimaryIndices => _entity._entities;
        protected override Dictionary<Guid, Index<Actor, IGraphEntity>> ForeignIndices => _entity._actors;

        protected override Index<Actor, IGraphEntity> CreateForeignIndex(Actor key) => new ActorIndex(key, _entity);
        protected override Index<IGraphEntity, Actor> CreatePrimaryIndex(IGraphEntity value) => new EntityIndex(value, _entity);

        protected override Guid GetForeignKey(Actor value) => value.ID;
        protected override Guid GetPrimaryKey(IGraphEntity value) => value.ID;
    }

    private sealed class ActorCollection(GraphEntity entity) : Map<Guid, Actor, Guid, IGraphEntity>()
    {
        private readonly GraphEntity _entity = entity ?? throw new ArgumentNullException(nameof(entity));

        protected override Dictionary<Guid, Index<Actor, IGraphEntity>> PrimaryIndices => _entity._actors;
        protected override Dictionary<Guid, Index<IGraphEntity, Actor>> ForeignIndices => _entity._entities;

        protected override Index<IGraphEntity, Actor> CreateForeignIndex(IGraphEntity key) => new EntityIndex(key, _entity);
        protected override Index<Actor, IGraphEntity> CreatePrimaryIndex(Actor value) => new ActorIndex(value, _entity);

        protected override Guid GetForeignKey(IGraphEntity value) => value.ID;
        protected override Guid GetPrimaryKey(Actor value) => value.ID;
    }

    private sealed class EntityIndex(IGraphEntity entity, GraphEntity collection) : Index<IGraphEntity, Actor>(entity)
    {
        private readonly GraphEntity _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        protected override async void Destroy()
        {
            try
            {
                await Key.StopAsync(_collection.StoppingToken).ConfigureAwait(false);
                _collection.NotifyStateChanged();
            }
            catch (Exception ex)
            {
                DebugUtils.LogException(ex, this);
            }
            finally
            {
                Key.Dispose();
                _collection._entities.Remove(Key.ID);
            }
        }

        protected override Index<Actor, IGraphEntity> GetOrAddIndex(Actor actor)
        {
            ref Index<Actor, IGraphEntity>? index = ref CollectionsMarshal.GetValueRefOrAddDefault(_collection._actors, actor.ID, out bool exists);

            if (!exists)
            {
                index = new ActorIndex(actor, _collection);
            }

            return index!;
        }
    }

    private sealed class ActorIndex(Actor actor, GraphEntity collection) : Index<Actor, IGraphEntity>(actor)
    {
        private readonly GraphEntity _collection = collection ?? throw new ArgumentNullException(nameof(collection));

        protected override void Destroy() => _collection._actors.Remove(Key.ID);

        protected override Index<IGraphEntity, Actor> GetOrAddIndex(IGraphEntity entity)
        {
            ref Index<IGraphEntity, Actor>? index = ref CollectionsMarshal.GetValueRefOrAddDefault(_collection._entities, entity.ID, out bool exists);

            if (!exists)
            {
                index = new EntityIndex(entity, _collection);
                _collection.StateChanged?.Invoke();
            }

            return index!;
        }
    }

    private readonly Dictionary<Guid, Index<IGraphEntity, Actor>> _entities;
    /// <summary>
    /// Gets the collection of graph entities managed by this instance.
    /// </summary>
    public Map<Guid, IGraphEntity, Guid, Actor> Entities { get; }


    private readonly Dictionary<Guid, Index<Actor, IGraphEntity>> _actors;
    /// <summary>
    /// Gets the collection of actors managed by this instance.
    /// </summary>
    public Map<Guid, Actor, Guid, IGraphEntity> Actors { get; }

    private volatile bool _disposed;
    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    protected bool Disposed => _disposed;

    /// <inheritdoc/>
    public abstract Actor? Actor { get; }

    /// <inheritdoc/>
    public abstract ICollection<IGraphComponent> Components { get; }

    /// <inheritdoc/>
    public abstract Guid ID { get; }

    /// <inheritdoc/>
    public abstract IGraphEntity? Parent { get; }

    private CancellationTokenSource? _stoppingCts;

    /// <inheritdoc/>
    public CancellationToken StoppingToken => _stoppingCts!.Token;

    /// <summary>
    /// Gets the lifecycle manager associated with this instance.
    /// </summary>
    protected GraphLifecycleManager LifecycleManager { get; }

    /// <inheritdoc/>
    public event Action? StateChanged;

    /// <inheritdoc/>
    public event Action? Regenerating;

    /// <inheritdoc/>
    public event Action? Regenerated;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphEntity"/> class with the specified <see cref="GraphLifecycleManager"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="lifecycleManager"/> is <see langword="null"/>.</exception>
    /// <param name="lifecycleManager">
    /// The <see cref="GraphLifecycleManager"/> that controls the lifecycle of this entity and its children. 
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="lifecycleManager"/> is <see langword="null"/>.</exception>"
    public GraphEntity(GraphLifecycleManager lifecycleManager)
    {
        LifecycleManager = lifecycleManager ?? throw new ArgumentNullException(nameof(lifecycleManager));

        _entities = [];
        Entities = new EntityCollection(this);

        _actors = [];
        Actors = new ActorCollection(this);
    }

    /// <inheritdoc/>
    public virtual ValueTask StartAsync(CancellationToken cancellationToken)
    {
        if (_stoppingCts is null || !_stoppingCts.TryReset())
        {
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _stoppingCts.Token.Register(OnStopRequested);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Invoked when a stop request is received.
    /// </summary>
    protected virtual void OnStopRequested()
    {
        // Override in derived classes to handle stop requests
    }

    /// <inheritdoc/>
    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        if (_stoppingCts is null)
        {
            return;
        }

        Task cancel = _stoppingCts.CancelAsync();
        Task wait = cancel.WaitAsync(cancellationToken);
        try
        {
            await wait.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    /// <summary>
    /// Raises the StateChanged event to notify subscribers that the object's state has changed.
    /// </summary>
    protected virtual void NotifyStateChanged() => StateChanged?.Invoke();

    /// <summary>
    /// Raises the Regenerating event to notify subscribers that a regeneration process is occurring.
    /// </summary>
    protected virtual void NotifyRegenerating() => Regenerating?.Invoke();

    /// <summary>
    /// Raises the Regenerated event to notify subscribers that regeneration has occurred.
    /// </summary>
    protected virtual void NotifyRegenerated() => Regenerated?.Invoke();

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{GetType().Name} ({ID})";
    }

    /// <inheritdoc/>
    protected virtual void Disposing()
    {
        try
        {
            DisposeCts();
        }
        finally
        {
            Dictionary<Guid, Index<IGraphEntity, Actor>>.ValueCollection entityValues = _entities.Values;
            if (entityValues.Count > 0)
            {
                IDisposable[] disposableEntities = [.. entityValues.OfType<IDisposable>()];
                foreach (IDisposable child in disposableEntities)
                {
                    child.Dispose();
                }
            }
        }
    }

    private void DisposeCts()
    {
        if (_stoppingCts is null)
        {
            return;
        }

        try
        {
            if (!_stoppingCts.IsCancellationRequested)
            {
                _stoppingCts.Cancel();
            }
        }
        finally
        {
            _stoppingCts.Dispose();
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Disposing();
        }

        _disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
