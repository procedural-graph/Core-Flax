# Procedural Graph: Core (Flax Engine 1.11)

The runtime execution layer for the [Procedural Graph framework](https://github.com/procedural-graph/Core). It seamlessly integrates powerful procedural logic directly into your native Flax Engine workflows. It listens for scene and actor changes, manages asynchronous graph generation tasks, and coordinates the transformation of Flax Actors into procedural graph entities.

## Installation

1. Copy the `Procedural Graph Core` folder into the `Plugins` directory of your Flax project.
2. Open the `.flaxproj` file in your project's root directory using your preferred text editor and append a reference to the plugin:

```json
"References": [
 {
     "Name": "$(EnginePath)/Flax.flaxproj"
 },
 {
     "Name": "$(ProjectPath)/Plugins/Procedural Graph Core/Procedural Graph Core.flaxproj"
 }
]
```

3. Open the Flax Editor and ensure the plugin is enabled by navigating to **Tools -> Plugins**.

## Getting Started

To bridge your custom Flax Actors with the procedural generation system, you need to register Graph Converters. Converters translate engine-specific objects into engine-agnostic `IGraphNode` entities.

Create a new `EditorPlugin` in your project's `Source` directory to register your custom converters during initialization:

```csharp
using ProceduralGraph.FlaxEngine;
using ProceduralGraph.Generic.Converters;

namespace MyProject;

[PluginLoadOrder(DeinitializeBefore = typeof(ProceduralGraphPlugin), InitializeAfter = typeof(ProceduralGraphPlugin))]
internal class MyPlugin : EditorPlugin
{
    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to the initialization event to register your custom converters
        PluginManager.GetPlugin<ProceduralGraphPlugin>().Initializing += OnInitializing;
    }

    public override void Deinitialize()
    {
        base.Deinitialize();

        // Clean up the event subscription
        PluginManager.GetPlugin<ProceduralGraphPlugin>().Initializing -= OnInitializing;
    }

    private void OnInitializing(ProceduralGraphPlugin plugin, GraphConverterRegistrar registrar)
    {
        // Register your custom implementations of IGraphConverter here
        registrar.Add(new MyGraphConverter());
    }
}

```

_For more specific guidance on creating plugins for Flax Engine, please refer to the [official Flax Engine Plugin Documentation](https://docs.flaxengine.com/manual/scripting/plugins/index.html)._

- **`GraphConverterRegistrar`**: The central registry used to inject your custom conversion logic into the core framework.
- **`IGraphConverter`**: The interface responsible for converting between Flax Engine objects (like `Actor`s or `Scene`s) and procedural graph entities. Implementations of this interface evaluate if they `CanConvert` a specific object and handle the instantiation of the corresponding `IGraphNode`.
- **`LifecycleGraphNode<TKey, TValue>`**: The base node type that handles asynchronous start/stop lifecycles, ensuring proper resource allocation and background task cancellation when the Flax scene hierarchy changes.
