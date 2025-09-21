namespace Prefabs;

using System.Numerics;
using System.Drawing;
using Utils;

public class Cube : RenderObject
{
    public Cube(
        Vector3 position,
        Vector3 rotation,
        Vector3 scale,
        Color color)
        : base(position, rotation, scale)
    {
        indexCount = (uint)ObjectIndexes.Cube;
        base.SetVertices(new float[] {
            // Front face
            -0.5f, -0.5f, +0.5f, color.R / 255f, color.G / 255f, color.B / 255f,
            +0.5f, -0.5f, +0.5f, color.R / 255f, color.G / 255f, color.B / 255f,
            +0.5f, +0.5f, +0.5f, color.R / 255f, color.G / 255f, color.B / 255f,
            -0.5f, +0.5f, +0.5f, color.R / 255f, color.G / 255f, color.B / 255f,
            // Back face                  
            -0.5f, -0.5f, -0.5f, color.R / 255f, color.G / 255f, color.B / 255f,
            +0.5f, -0.5f, -0.5f, color.R / 255f, color.G / 255f, color.B / 255f,
            +0.5f, +0.5f, -0.5f, color.R / 255f, color.G / 255f, color.B / 255f,
            -0.5f, +0.5f, -0.5f, color.R / 255f, color.G / 255f, color.B / 255f,
        });
    }

    public override uint[] GetIndices()
    {
        return new uint[] {
            0, 1, 2, 2, 3, 0, // front
            1, 5, 6, 6, 2, 1, // right
            5, 4, 7, 7, 6, 5, // back
            4, 0, 3, 3, 7, 4, // left
            3, 2, 6, 6, 7, 3, // top
            4, 5, 1, 1, 0, 4,  // bottom
        };
    }

}