namespace Interfaces;

using System.Numerics;
using Silk.NET.Maths;

public interface IRenderable
{
    Vector3 Position { get; set; }
    Vector3 Rotation { get; set; }
    Vector3 Scale { get; set; }
    uint indexCount { get; set; }

    float[] GetVertices();
    uint[] GetIndices();

    public Matrix4x4 GetModelMatrix();
}