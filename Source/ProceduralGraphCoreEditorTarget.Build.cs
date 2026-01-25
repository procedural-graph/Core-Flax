using Flax.Build;

namespace ProceduralGraph;

public class ProceduralGraphCoreEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();
        Modules.Add(nameof(CoreModule));
    }
}
