using FlaxEngine;
using ProceduralGraph.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProceduralGraph.FlaxEngine;

internal sealed class ActorInfoProvider : ISceneMemberInfoProvider<Guid, Actor>
{
    public bool Equals(Actor? x, Actor? y)
    {
        return x == y;
    }

    public bool Equals(Guid alternate, Actor other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return alternate == other.ID;
    }

    public IReadOnlyCollection<Actor> GetChildren(Actor value)
    {
        return value.Children;
    }

    public int GetHashCode([DisallowNull] Actor? obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return obj.ID.GetHashCode();
    }

    public int GetHashCode(Guid alternate)
    {
        return alternate.GetHashCode();
    }

    public Guid GetKey(Actor value)
    {
        return value.ID;
    }

    public Actor? GetParent(Actor value)
    {
        return value.Parent;
    }

    public Actor GetRoot(Actor value)
    {
        return value.Scene;
    }

    public bool TryFind(Guid key, [NotNullWhen(true)] out Actor? value)
    {
        value = Level.FindActor(key);
        return value is { };
    }
}
