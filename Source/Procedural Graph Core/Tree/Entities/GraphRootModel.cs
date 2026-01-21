using FlaxEngine;
using Newtonsoft.Json;
using System;

namespace ProceduralGraph.Tree.Entities;

internal sealed record GraphRootModel : IGraphEntityModel
{
    [Serialize, ShowInEditor]
    public Guid EntityID { get; init; }
    Guid IGraphEntityModel.EntityID => EntityID;

    [Serialize, ShowInEditor]
    public Guid SceneID { get; init; }
    Guid IGraphNodeModel.ParentID => SceneID;

    [Serialize, ShowInEditor, JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
    public IGraphEntityModel[] Children { get; init; } = [];

}
