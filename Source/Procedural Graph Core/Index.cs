using System;
using System.Collections;
using System.Collections.Generic;

namespace ProceduralGraph;

/// <summary>
/// Represents an abstract, bidirectional index that associates keys of <typeparamref name="TKey"/> with values of 
/// <typeparamref name="TValue"/>, supporting efficient lookup and navigation between related elements.
/// </summary>
/// <typeparam name="TKey">The type of the key associated with each index. Must not be <see langword="null"/>.</typeparam>
/// <typeparam name="TValue">The type of the value associated with each index. Must not be <see langword="null"/>.</typeparam>
public abstract class Index<TKey, TValue> : ICollection<TValue> where TKey : notnull where TValue : notnull
{
    private Dictionary<TValue, Index<TValue, TKey>>? _indices;

    /// <summary>
    /// Gets the key associated with the current element.
    /// </summary>
    public TKey Key { get; }

    /// <inheritdoc/>
    public int Count => _indices is { } ? _indices.Count : 0;

    bool ICollection<TValue>.IsReadOnly => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Index{TKey, TValue}"/> class with the specified key.
    /// </summary>
    /// <param name="key">The key associated with this <see cref="Index{TKey, TValue}"/>. Cannot be <see langword="null"/>.</param>
    public Index(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        Key = key;
    }

    /// <summary>
    /// Retrieves the existing index associated with the specified value, or creates and adds a new index if one does
    /// not already exist.
    /// </summary>
    /// <param name="value">The value for which to retrieve or create an index. Cannot be <see langword="null"/>.</param>
    /// <returns>
    /// An <see cref="Index{TValue, TKey}"/> instance associated with the specified value. If no index exists, a new one
    /// is created and returned.
    /// </returns>
    protected abstract Index<TValue, TKey> GetOrAddIndex(TValue value);

    /// <summary>
    /// Removes this index from the containing collection.
    /// </summary>
    protected abstract void Destroy();

    /// <inheritdoc/>
    public bool Add(TValue item)
    {
        Index<TValue, TKey> index = GetOrAddIndex(item);

        _indices ??= [];

        if (_indices.TryAdd(item, index))
        {
            index._indices ??= [];
            index._indices.Add(Key, this);
            return true;
        }

        return false;
    }
    void ICollection<TValue>.Add(TValue item) => Add(item);

    /// <inheritdoc/>
    public void Clear()
    {
        if (_indices is null)
        {
            return;
        }

        foreach (Index<TValue, TKey> foreignIndex in _indices.Values)
        {
            Dictionary<TKey, Index<TKey, TValue>> foreignIndices = foreignIndex._indices!;
            if (foreignIndices.Remove(Key) && foreignIndices.Count == 0)
            {
                foreignIndex.Destroy();
            }
        }

        _indices.Clear();

        Destroy();
    }

    /// <inheritdoc/>
    public bool Contains(TValue item)
    {
        return _indices is { } && _indices.ContainsKey(item);
    }

    void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex, nameof(arrayIndex));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length, nameof(arrayIndex));
        if (_indices is { })
        {
            _indices.Keys.CopyTo(array, arrayIndex);
        }
    }

    /// <inheritdoc/>
    public bool Remove(TValue item)
    {
        if (_indices is { } && _indices.Remove(item, out Index<TValue, TKey>? foreignIndex))
        {
            Dictionary<TKey, Index<TKey, TValue>> foreignIndices = foreignIndex._indices!;
            if (foreignIndices.Remove(Key) && foreignIndices.Count == 0)
            {
                foreignIndex.Destroy();
            }

            if (_indices.Count == 0)
            {
                Destroy();
            }

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<TValue> GetEnumerator()
    {
        if (_indices is { })
        {
            return _indices.Keys.GetEnumerator();
        }

        IEnumerable<TValue> empty = [];
        return empty.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}