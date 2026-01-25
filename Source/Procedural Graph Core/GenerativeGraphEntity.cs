#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// Represents an abstract base class for entities within a generative graph that support asynchronous regeneration and
/// component management.
/// </summary>
/// <inheritdoc/>
public abstract class GenerativeGraphEntity<T> : GraphEntity<T> where T : Actor
{
    private bool _isDirty;
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// The time to wait after a property change before triggering a rebuild, used to prevent excessive re-computation.
    /// </summary>
    protected virtual TimeSpan DebouncePeriod { get; } = TimeSpan.FromSeconds(0.2);

    private readonly ObservableCollection<IGraphComponent> _components;
    /// <summary>
    /// Gets the components associated with this entity.
    /// </summary>
    public override ICollection<IGraphComponent> Components => _components;

    /// <inheritdoc/>
    public abstract override IGraphEntity? Parent { get; }

    /// <inheritdoc/>
    public abstract override Guid ID { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeGraphEntity{T}"/> class with the specified lifecycle manager.
    /// </summary>
    /// <param name="lifecycleManager">The lifecycle manager responsible for managing the state transitions of this entity.</param>
    public GenerativeGraphEntity(GraphLifecycleManager lifecycleManager) : base(lifecycleManager)
    {
        _components = [];
        _components.ItemAdded += OnComponentAdded;
        _components.ItemRemoved += OnComponentRemoved;

        _semaphore = new(1, 1);
    }

    private void OnComponentAdded(IGraphComponent item)
    {
        item.Changed += OnComponentChanged;
        OnStateChanged();
    }

    private void OnComponentRemoved(IGraphComponent item)
    {
        item.Changed -= OnComponentChanged;
        OnStateChanged();
    }

    private void OnComponentChanged(IGraphComponent component)
    {
        OnStateChanged();
    }

    /// <inheritdoc/>
    [SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly", Justification = "Fire-and-forget pattern is intentional here.")]
    protected override void OnStateChanged()
    {
        _ = TryGenerateAsync(StoppingToken);
        base.OnStateChanged();
    }

    /// <summary>
    /// Attempts to initiate a generation operation asynchronously if no other operation is currently in progress.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <see langword="true"/> if the generation
    /// was initiated and completed successfully; otherwise, <see langword="false"/>.
    /// </returns>
    protected async ValueTask<bool> TryGenerateAsync(CancellationToken cancellationToken)
    {
        try
        {
            Interlocked.Exchange(ref _isDirty, true);

            if (_semaphore.Wait(0, cancellationToken))
            {
                await DebounceAsync(cancellationToken);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
            {
                DebugUtils.LogException(ex, this, "Failed to generate");
            }
            
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task DebounceAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer periodicTimer = new(DebouncePeriod);
        while (Interlocked.Exchange(ref _isDirty, false) && await periodicTimer.WaitForNextTickAsync(cancellationToken))
        {
            if (_isDirty)
            {
                continue;
            }

            Editor.Log($"Commencing generation of {this}.");
            DateTime startTime = DateTime.UtcNow;

            OnRegenerating();

            await GenerateAsync(cancellationToken);

            TimeSpan duration = DateTime.UtcNow - startTime;
            Editor.Log($"Generation of {this} completed in {duration.TotalSeconds:F2} seconds.");

            OnRegenerated();
        }
    }

    /// <summary>
    /// Asynchronously generates this entity.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the generation operation.</param>
    /// <returns>A task that represents the asynchronous generation operation.</returns>
    protected abstract Task GenerateAsync(CancellationToken cancellationToken);

    /// <inheritdoc/>
    protected override void Disposing()
    {
        try
        {
            base.Disposing();
        }
        finally
        {
            _semaphore.Dispose();
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
