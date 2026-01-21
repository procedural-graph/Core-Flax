using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FlaxEditor;
using FlaxEngine;

namespace ProceduralGraph.Tree.Entities;

/// <summary>
/// A default implementation of a graph entity that handles asynchronous generation, property change debouncing, and thread safety.
/// </summary>
/// <typeparam name="TGenerator">The type of generator to use.</typeparam>
/// <typeparam name="TSelf">The type of the derived graph entity.</typeparam>
public class DefaultGraphEntity<TGenerator, TSelf> : GraphEntity 
    where TGenerator : IAsyncGenerator, IGeneratorFactory<TSelf, TGenerator> 
    where TSelf : DefaultGraphEntity<TGenerator, TSelf>
{
    /// <summary>
    /// The time to wait after a property change before triggering a rebuild, used to prevent excessive re-computation.
    /// </summary>
    protected readonly TimeSpan debouncePeriod;

    /// <summary>
    /// A flag indicating that parameters have changed and a rebuild is required.
    /// </summary>
    protected bool isDirty;

    /// <summary>
    /// Ensures that only one generation task runs at a time.
    /// </summary>
    protected readonly SemaphoreSlim semaphore;

    private readonly ObservableCollection<IGraphComponent> _components;
    /// <summary>
    /// Gets the components associated with this entity.
    /// </summary>
    public override ICollection<IGraphComponent> Components => _components;

    /// <inheritdoc/>
    public override IGraphEntity? Parent { get; }

    /// <inheritdoc/>
    public override Guid ID { get; }

    /// <inheritdoc/>
    public override Actor? Actor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultGraphEntity{TGenerator, TSelf}"/> class with the specified identifier, parent entity,
    /// lifecycle manager, optional actor, and debounce period.
    /// </summary>
    /// <param name="id">The unique identifier for the graph entity.</param>
    /// <param name="parent">The parent graph entity to which this entity will be attached. Cannot be <see langword="null"/>.</param>
    /// <param name="lifecycleManager">The lifecycle manager responsible for managing the state transitions of this entity.</param>
    /// <param name="actor">An optional actor associated with this entity. If <see langword="null"/>, no actor is assigned.</param>
    /// <param name="debounceSeconds">The debounce period, in seconds, used to delay certain operations. Must be non-negative.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <see langword="null"/>.</exception>
    public DefaultGraphEntity(Guid id, IGraphEntity parent, GraphLifecycleManager lifecycleManager, Actor? actor = null, double debounceSeconds = 0.2) : base(lifecycleManager)
    {
        Parent = parent ?? throw new ArgumentNullException(nameof(parent));

        debouncePeriod = TimeSpan.FromSeconds(debounceSeconds);
        Actor = actor;
        ID = id;

        _components = [];
        _components.ItemAdded += OnComponentAdded;
        _components.ItemRemoved += OnComponentRemoved;

        StateChanged += MarkAsDirty;

        semaphore = new(1, 1);
    }

    private void OnComponentAdded(IGraphComponent item)
    {
        item.Changed += OnComponentChanged;
        NotifyStateChanged();
    }

    private void OnComponentRemoved(IGraphComponent item)
    {
        item.Changed -= OnComponentChanged;
        NotifyStateChanged();
    }

    private void OnComponentChanged(IGraphComponent component)
    {
        NotifyStateChanged();
    }

    /// <summary>
    /// Marks the entity as dirty and attempts to start the generation loop.
    /// </summary>
    public virtual void MarkAsDirty()
    {
        Interlocked.Exchange(ref isDirty, true);
        if (semaphore.Wait(0))
        {
            StartGenerating();
        }
    }

    /// <summary>
    /// The core execution loop. Uses a <see cref="PeriodicTimer"/> to wait for the debounce period 
    /// and then executes the generator if the entity is still dirty.
    /// </summary>
    protected virtual async void StartGenerating()
    {
        CancellationToken cancellationToken = StoppingToken;
        TGenerator? generator = default;
        try
        {
            using PeriodicTimer periodicTimer = new(debouncePeriod);
            while (Interlocked.Exchange(ref isDirty, false) && await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (isDirty)
                {
                    continue;
                }

                Editor.Log($"Commencing regeneration of {this}.");
                DateTime startTime = DateTime.UtcNow;

                NotifyRegenerating();

                generator = TGenerator.Create((TSelf)this);
                await generator.GenerateAsync(cancellationToken);

                TimeSpan duration = DateTime.UtcNow - startTime;
                Editor.Log($"Regeneration of {this} completed in {duration.TotalSeconds:F2} seconds.");

                NotifyRegenerated();
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            DebugUtils.LogException(ex, this, "Failed to regenerate");
        }
        finally
        {
            semaphore.Release();
            if (generator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <inheritdoc/>
    protected override void Disposing()
    {
        try
        {
            base.Disposing();
        }
        finally
        {
            semaphore.Dispose();
            if (_components.Count > 0)
            {
                IDisposable[] disposableComponents = [.. _components.OfType<IDisposable>()];
                foreach (IDisposable disposable in disposableComponents)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
