#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace ProceduralGraph;

/// <summary>
/// Represents an abstract, bidirectional mapping between primary and foreign keys and values, supporting indexed access
/// and collection semantics for both relationships.
/// </summary>
/// <typeparam name="TPrimaryKey">
/// The type of the primary key used to identify primary values. 
/// Must not be <see langword="null"/>.
/// </typeparam>
/// <typeparam name="TPrimaryValue">
/// The type of the primary value associated with each primary key. 
/// Must not be <see langword="null"/>.
/// </typeparam>
/// <typeparam name="TForeignKey">
/// The type of the foreign key used to identify foreign values. 
/// Must not be <see langword="null"/>.
/// </typeparam>
/// <typeparam name="TForeignValue">
/// The type of the foreign value associated with each foreign key. 
/// Must not be <see langword="null"/>.
/// </typeparam>
public abstract class Map<TPrimaryKey, TPrimaryValue, TForeignKey, TForeignValue> : 
    IReadOnlyDictionary<TPrimaryKey, Index<TPrimaryValue, TForeignValue>>, 
    ICollection<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>>
    where TPrimaryKey : notnull
    where TPrimaryValue : notnull
    where TForeignKey : notnull
    where TForeignValue : notnull
{
    /// <summary>
    /// Gets the collection of primary indices, mapping each primary key to its corresponding index of primary and
    /// foreign values.
    /// </summary>
    protected abstract Dictionary<TPrimaryKey, Index<TPrimaryValue, TForeignValue>> PrimaryIndices { get; }

    /// <summary>
    /// Gets the collection of foreign indices, mapping each foreign key to its corresponding index of foreign and
    /// primary values.
    /// </summary>
    protected abstract Dictionary<TForeignKey, Index<TForeignValue, TPrimaryValue>> ForeignIndices { get; }

    /// <summary>
    /// Gets the index associated with the specified primary key.
    /// </summary>
    /// <param name="key">The primary key for which to retrieve the corresponding index.</param>
    /// <returns>The index mapped to the specified primary key.</returns>
    public Index<TPrimaryValue, TForeignValue> this[TPrimaryKey key] => PrimaryIndices[key];

    /// <summary>
    /// Gets an enumerable collection containing all primary keys in the current set.
    /// </summary>
    public IEnumerable<TPrimaryKey> Keys => PrimaryIndices.Keys;

    /// <summary>
    /// Gets an enumerable collection containing all indices in the current set.
    /// </summary>
    public IEnumerable<Index<TPrimaryValue, TForeignValue>> Values => PrimaryIndices.Values;

    /// <summary>
    /// Gets the number of elements contained in the collection.
    /// </summary>
    public int Count => PrimaryIndices.Count;

    bool ICollection<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>>.IsReadOnly => false;

    /// <summary>
    /// Retrieves the primary key associated with the specified value.
    /// </summary>
    /// <param name="value">The value for which to obtain the corresponding primary key. Cannot be null.</param>
    /// <returns>The primary key that uniquely identifies the specified value.</returns>
    protected abstract TPrimaryKey GetPrimaryKey(TPrimaryValue value);

    /// <summary>
    /// Retrieves the foreign key associated with the specified foreign value.
    /// </summary>
    /// <param name="value">The foreign value for which to obtain the corresponding foreign key. Cannot be null.</param>
    /// <returns>The foreign key that corresponds to the specified foreign value.</returns>
    protected abstract TForeignKey GetForeignKey(TForeignValue value);

    /// <summary>
    /// Creates a primary index for the specified primary value.
    /// </summary>
    /// <param name="key">The primary value for which to create the index. This value is used as the key in the resulting index.</param>
    /// <returns>An <see cref="Index{TPrimaryValue, TForeignValue}"/> instance associated with the specified primary value.</returns>
    protected abstract Index<TPrimaryValue, TForeignValue> CreatePrimaryIndex(TPrimaryValue key);

    /// <summary>
    /// Creates an index that maps foreign key values to primary key values for the specified foreign key.
    /// </summary>
    /// <param name="key">The foreign key value for which to create the index. This value is used to identify the set of primary key
    /// values associated with the foreign key.</param>
    /// <returns>An <see cref="Index{TForeignKey, TPrimaryKey}"/> instance associated with the specified foreign value.</returns>
    protected abstract Index<TForeignValue, TPrimaryValue> CreateForeignIndex(TForeignValue key);

    /// <summary>
    /// Determines whether the collection contains an entry with the specified primary key.
    /// </summary>
    /// <param name="key">The primary key to locate in the collection.</param>
    /// <returns><see langword="true"/> if an entry with the specified primary key exists in the collection; otherwise, <see langword="false"/>.</returns>
    public bool ContainsPrimaryKey(TPrimaryKey key)
    {
        return PrimaryIndices.ContainsKey(key);
    }

    /// <summary>
    /// Determines whether the collection contains an entry with the specified foreign key.
    /// </summary>
    /// <param name="key">The foreign key to locate in the collection.</param>
    /// <returns><see langword="true"/> if an entry with the specified foreign key exists in the collection; otherwise, <see langword="false"/>.</returns>
    public bool ContainsForeignKey(TForeignKey key)
    {
        return ForeignIndices.ContainsKey(key);
    }

    /// <summary>
    /// Determines whether the collection contains an entry with the specified primary key.
    /// </summary>
    /// <param name="value">The primary key to locate in the collection. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if an entry with the specified primary key exists in the collection; otherwise, <see langword="false"/>.</returns>
    public bool ContainsPrimaryKey(TPrimaryValue value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        TPrimaryKey key = GetPrimaryKey(value);
        return ContainsPrimaryKey(key);
    }

    /// <summary>
    /// Determines whether the collection contains an entry with the specified foreign key.
    /// </summary>
    /// <param name="value">The foreign key to locate in the collection. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if an entry with the specified foreign key exists in the collection; otherwise, <see langword="false"/>.</returns>
    public bool ContainsForeignKey(TForeignValue value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        TForeignKey key = GetForeignKey(value);
        return ForeignIndices.ContainsKey(key);
    }

    bool IReadOnlyDictionary<TPrimaryKey, Index<TPrimaryValue, TForeignValue>>.ContainsKey(TPrimaryKey key)
    {
        return ContainsPrimaryKey(key);
    }

    /// <inheritdoc/>
    public bool TryGetValue(TPrimaryKey key, [MaybeNullWhen(false)] out Index<TPrimaryValue, TForeignValue> value)
    {
        if (PrimaryIndices.TryGetValue(key, out Index<TPrimaryValue, TForeignValue>? index))
        {
            value = index;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to add the specified item to the collection using its primary key.
    /// </summary>
    /// <param name="item">The item to add to the collection.</param>
    /// <returns>
    /// <see langword="true"/> if the item was added successfully; otherwise, 
    /// <see langword="false"/> if an item with the same primary key already exists.
    /// </returns>
    public bool Add(TPrimaryValue item)
    {
        TPrimaryKey key = GetPrimaryKey(item);
        return Add(key, item, out _);
    }

    /// <summary>
    /// Gets the existing index associated with the specified foreign value, or creates and adds a new index if one does
    /// not already exist.
    /// </summary>
    /// <param name="item">The foreign value for which to retrieve or create an associated index. Cannot be <see langword="null"/>.</param>
    /// <param name="exists">
    /// When this method returns, contains <see langword="true"/> if an index for the specified foreign value already
    /// existed; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The index associated with the specified foreign value. If no index existed, a new one is created and returned.</returns>
    public Index<TForeignValue, TPrimaryValue> GetOrAdd(TForeignValue item, out bool exists)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        TForeignKey foreignKey = GetForeignKey(item);
        ref Index<TForeignValue, TPrimaryValue>? foreignIndex = ref CollectionsMarshal.GetValueRefOrAddDefault(ForeignIndices, foreignKey, out exists);

        if (!exists)
        {
            foreignIndex = CreateForeignIndex(item);
        }

        return foreignIndex!;
    }

    private bool Add(TPrimaryKey key, TPrimaryValue value, [NotNullWhen(true)] out Index<TPrimaryValue, TForeignValue>? result)
    {
        ref Index<TPrimaryValue, TForeignValue>? primaryIndex = ref CollectionsMarshal.GetValueRefOrAddDefault(PrimaryIndices, key, out bool exists);

        if (!exists)
        {
            primaryIndex = CreatePrimaryIndex(value);
        }

        result = primaryIndex;
        return exists;
    }

    /// <inheritdoc/>
    public void Clear()
    {
        PrimaryIndices.Clear();
    }

    /// <summary>
    /// Removes the specified item from the collection.
    /// </summary>
    /// <param name="item">The item to remove from the collection.</param>
    /// <returns><see langword="true"/> if the item was found and removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(TPrimaryValue item)
    {
        TPrimaryKey key = GetPrimaryKey(item);
        return Remove(key);
    }

    /// <summary>
    /// Removes the entry with the specified primary key from the collection.
    /// </summary>
    /// <param name="key">The primary key of the entry to remove.</param>
    /// <returns><see langword="true"/> if the entry was found and removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(TPrimaryKey key)
    {
        if (PrimaryIndices.Remove(key, out Index<TPrimaryValue, TForeignValue>? index))
        {
            index.Clear();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the element with the specified primary key from the collection, if it exists.
    /// </summary>
    /// <param name="key">The primary key of the element to remove.</param>
    /// <param name="value">When this method returns, contains the removed element associated with the specified key, if the key was found;
    /// otherwise, the default value for the type. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(TPrimaryKey key, [NotNullWhen(true)] out Index<TPrimaryValue, TForeignValue>? value)
    {
        return PrimaryIndices.TryGetValue(key, out value);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection of primary values and their associated foreign value
    /// sequences.
    /// </summary>
    /// <returns>
    /// An enumerator over key/value pairs, where each key is a primary value and each value is an enumerable sequence
    /// of associated foreign values.
    /// </returns>
    public IEnumerator<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>> GetEnumerator()
    {
        return PrimaryIndices.Values.Select(static index => new KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>(index.Key, index)).GetEnumerator();
    }

    IEnumerator<KeyValuePair<TPrimaryKey, Index<TPrimaryValue, TForeignValue>>> IEnumerable<KeyValuePair<TPrimaryKey, Index<TPrimaryValue, TForeignValue>>>.GetEnumerator()
    {
        return PrimaryIndices.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    void ICollection<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>>.Add(KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>> item)
    {
        TPrimaryKey key = GetPrimaryKey(item.Key);
        if (Add(key, item.Key, out Index<TPrimaryValue, TForeignValue>? result))
        {
            foreach (TForeignValue foreignValue in item.Value)
            {
                result.Add(foreignValue);
            }
        }
    }

    bool ICollection<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>>.Contains(KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>> item)
    {
        return ((ICollection<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>>)PrimaryIndices).Contains(item);
    }

    void ICollection<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>>.CopyTo(KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>>)PrimaryIndices).CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>>>.Remove(KeyValuePair<TPrimaryValue, IEnumerable<TForeignValue>> item)
    {
        return Remove(item.Key);
    }
}