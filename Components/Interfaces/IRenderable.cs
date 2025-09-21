namespace Interfaces;

using System.Numerics;
using Silk.NET.Maths;

public interface IRenderable
{
    Vector3 position { get; set; }
    Vector3 rotation { get; set; }
    Vector3 scale { get; set; }

    float[] GetVertices();
    uint[] GetIndices(int multiply_index);

    public Matrix4x4 GetModelMatrix();
}