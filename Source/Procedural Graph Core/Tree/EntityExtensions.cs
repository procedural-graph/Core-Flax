using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ProceduralGraph.Tree;

internal static class EntityExtensions
{
    private sealed class EntityComparer : IComparer<IGraphEntity>
    {
        private static readonly Comparer<Guid?> _guidComparer = Comparer<Guid?>.Default;

        public int Compare(IGraphEntity? x, IGraphEntity? y)
        {
            return _guidComparer.Compare(y?.ID, x?.ID);
        }
    }

    private static readonly EntityComparer _entityComparer = new();

    public static IGraphEntity? FindAncestor<T>(this IGraphEntity entity, ref readonly T visitor)
        where T : IEntityVisitor, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        IGraphEntity? current = entity;

        while (current is not null)
        {
            if (visitor.Visit(current))
            {
                return current;
            }

            current = current.Parent;
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BreathFirstSearch<T>(this IGraphEntity root, ref readonly T visitor, [NotNullWhen(true)] out IGraphEntity? result) 
        where T : IEntityVisitor, allows ref struct
    {
        Queue<IGraphEntity>? queue = null;
        return BreathFirstSearch(root, in visitor, ref queue, out result);
    }

    public static bool BreathFirstSearch<T>(
        this IGraphEntity root,
        ref readonly T visitor,
        ref Queue<IGraphEntity>? queue,
        [NotNullWhen(true)] out IGraphEntity? result) 
        where T : IEntityVisitor, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(root, nameof(root));

        if (visitor.Visit(root))
        {
            result = root;
            return true;
        }

        if (!ConditionallyInit(root, ref queue))
        {
            result = null;
            return false;
        }

        IGraphEntity previous = root;

        while (queue.TryDequeue(out IGraphEntity? current))
        {
            Map<Guid, IGraphEntity, Guid, Actor> children = current.Entities;

            if (children.Count == 0)
            {
                previous = current;
                continue;
            }

            foreach (IGraphEntity child in children.Values)
            {
                if (visitor.Visit(child))
                {
                    result = child;
                    return true;
                }

                queue.Enqueue(child);
            }

            previous = current;
        }

        result = previous;
        return false;
    }

    private static bool ConditionallyInit(IGraphEntity root, [NotNullWhen(true)] ref Queue<IGraphEntity>? queue)
    {
        int childCount = root.Entities.Count;

        if (childCount == 0)
        {
            return false;
        }

        if (queue is null)
        {
            queue = new Queue<IGraphEntity>(childCount);           
        }
        else
        {
            queue.Clear();
            queue.EnsureCapacity(childCount);
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DepthFirstSearch<T>(this IGraphEntity root, ref readonly T visitor, [NotNullWhen(true)] out IGraphEntity? result) 
        where T : IEntityVisitor, allows ref struct
    {
        List<IGraphEntity>? entities = null;
        return DepthFirstSearch(root, in visitor, ref entities, out result);
    }

    public static bool DepthFirstSearch<T>(
        this IGraphEntity root,
        ref readonly T visitor,
        ref List<IGraphEntity>? entities,
        [NotNullWhen(true)] out IGraphEntity? result) 
        where T : IEntityVisitor, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(root, nameof(root));

        if (visitor.Visit(root))
        {
            result = root;
            return true;
        }

        if (!ConditionallyInit(root, ref entities))
        {
            result = null;
            return false;
        }

        int index = entities.Count - 1;

        while (index >= 0)
        {
            IGraphEntity current = entities[index];

            if (visitor.Visit(current))
            {
                result = current;
                return true;
            }

            Map<Guid, IGraphEntity, Guid, Actor> children = current.Entities;
            int childCount = children.Count;

            if (childCount == 0)
            {
                entities.RemoveAt(index--);
                continue;
            }

            CollectionsMarshal.SetCount(entities, index);

            entities.AddRange(children.Values.Select(KeyOf));
            entities.Sort(index, childCount, _entityComparer);
            index += childCount - 1;
        }

        result = null;
        return false;
    }

    private static bool ConditionallyInit(IGraphEntity root, [NotNullWhen(true)] ref List<IGraphEntity>? entities)
    {
        Map<Guid, IGraphEntity, Guid, Actor> children = root.Entities;
        int childCount = children.Count;

        if (childCount == 0)
        {
            return false;
        }

        if (entities is null)
        {
            entities = new List<IGraphEntity>(childCount);
        }
        else
        {
            entities.Clear();
        }

        entities.AddRange(children.Values.Select(KeyOf));
        entities.Sort(_entityComparer);

        return true;
    }

    private static IGraphEntity KeyOf(Index<IGraphEntity, Actor> index)
    {
        return index.Key;
    }
}
