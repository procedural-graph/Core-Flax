using FlaxEngine;
using Newtonsoft.Json;
using System;

namespace ProceduralGraph.Hierarchy;

internal sealed record GraphRootModel : IGraphEntityModel
{
    [Serialize, ShowInEditor]
    public Guid ID { get; init; }

    [Serialize, ShowInEditor]
    public Guid SceneID { get; init; }
    Guid IGraphModel.ParentID => SceneID;

    [Serialize, ShowInEditor, JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
    public IGraphModel[] Children { get; init; } = [];
}
