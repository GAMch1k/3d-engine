namespace Interfaces;

using System.Numerics;

public interface IRenderable : IGameObject
{
    uint indexCount { get; set; }

    float[] GetVertices();
    uint[] GetIndices();

    public Matrix4x4 GetModelMatrix();
}