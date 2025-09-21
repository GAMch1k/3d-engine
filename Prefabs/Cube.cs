namespace Prefabs;

using System.Numerics;
using System.Drawing;
using Utils;

public class Cube : RenderObject
{
    public Cube(
        string name,
        Vector3 position,
        Vector3 rotation,
        Vector3 scale,
        Color color)
        : base(position, rotation, scale)
    {
        Name = name;
        indexCount = (uint)ObjectIndexes.Cube;

        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        base.SetVertices(new float[] {
            // Front face (verts 0-3, normal: 0, 0, 1)
            -0.5f, -0.5f,  0.5f, r, g, b, 0f, 0f, 1f,
             0.5f, -0.5f,  0.5f, r, g, b, 0f, 0f, 1f,
             0.5f,  0.5f,  0.5f, r, g, b, 0f, 0f, 1f,
            -0.5f,  0.5f,  0.5f, r, g, b, 0f, 0f, 1f,

            // Back face (verts 4-7, normal: 0, 0, -1)
            -0.5f, -0.5f, -0.5f, r, g, b, 0f, 0f,-1f,
             0.5f, -0.5f, -0.5f, r, g, b, 0f, 0f,-1f,
             0.5f,  0.5f, -0.5f, r, g, b, 0f, 0f,-1f,
            -0.5f,  0.5f, -0.5f, r, g, b, 0f, 0f,-1f,

            // Right face (verts 8-11, normal: 1, 0, 0)
             0.5f, -0.5f,  0.5f, r, g, b, 1f, 0f, 0f,
             0.5f, -0.5f, -0.5f, r, g, b, 1f, 0f, 0f,
             0.5f,  0.5f, -0.5f, r, g, b, 1f, 0f, 0f,
             0.5f,  0.5f,  0.5f, r, g, b, 1f, 0f, 0f,

            // Left face (verts 12-15, normal: -1, 0, 0)
            -0.5f, -0.5f,  0.5f, r, g, b,-1f, 0f, 0f,
            -0.5f, -0.5f, -0.5f, r, g, b,-1f, 0f, 0f,
            -0.5f,  0.5f, -0.5f, r, g, b,-1f, 0f, 0f,
            -0.5f,  0.5f,  0.5f, r, g, b,-1f, 0f, 0f,

            // Top face (verts 16-19, normal: 0, 1, 0)
            -0.5f,  0.5f,  0.5f, r, g, b, 0f, 1f, 0f,
             0.5f,  0.5f,  0.5f, r, g, b, 0f, 1f, 0f,
             0.5f,  0.5f, -0.5f, r, g, b, 0f, 1f, 0f,
            -0.5f,  0.5f, -0.5f, r, g, b, 0f, 1f, 0f,

            // Bottom face (verts 20-23, normal: 0, -1, 0)
            -0.5f, -0.5f,  0.5f, r, g, b, 0f,-1f, 0f,
             0.5f, -0.5f,  0.5f, r, g, b, 0f,-1f, 0f,
             0.5f, -0.5f, -0.5f, r, g, b, 0f,-1f, 0f,
            -0.5f, -0.5f, -0.5f, r, g, b, 0f,-1f, 0f
        });
    }

    public override uint[] GetIndices()
    {
        return new uint[] {
            0, 1, 2, 0, 2, 3, // front
            8, 9, 10, 8, 10, 11, // right
            4, 6, 5, 4, 7, 6, // back
            15, 14, 13, 15, 13, 12, // left
            16, 17, 18, 16, 18, 19, // top
            23, 22, 21, 23, 21, 20  // bottom
        };
    }

}

public class LevitatingCube : Cube
{
    public float _originalPosition { get; set; }
    public float _amplitude { get; set; } = 0.3f;
    public float _frequency { get; set; } = 0.2f;
    public float _timeaccumulator { get; set; } = 0f;
    public LevitatingCube(
        string name,
        Vector3 position,
        Vector3 rotation,
        Vector3 scale,
        Color color
    ) : base(name, position, rotation, scale, color)
    {
        _originalPosition = position.Y;
    }
}
