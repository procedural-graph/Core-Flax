using FlaxEngine;
using Newtonsoft.Json;
using System;

namespace ProceduralGraph;

internal sealed class NodeModel
{
    [Serialize, ShowInEditor]
    public Guid ActorID { get; set; }

    [Serialize, ShowInEditor, JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public GraphModel Model { get; set; }

    public NodeModel(Guid actorID, GraphModel model)
    {
        ActorID = actorID;
        Model = model;
    }

    public NodeModel()
    {
        ActorID = Guid.Empty;
        Model = default!;
    }
}