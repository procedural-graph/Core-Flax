namespace ProceduralGraph.Tree;

internal interface IEntityVisitor
{
    bool Visit(IGraphEntity entity);
}
