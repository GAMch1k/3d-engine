namespace Prefabs;

using System.Numerics;
using System.Drawing;

public class Cube : Object
{
    public Cube(
        Vector3 position,
        Vector3 rotation,
        Vector3 scale,
        Color color)
        : base(position, rotation, scale)
    {
        base.SetVertices(new float[] {
            // Front face
            position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f, color.R, color.G, color.B,
            position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f, color.R, color.G, color.B,
            position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f, color.R, color.G, color.B,
            position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f, color.R, color.G, color.B,

            // Back face
            position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f, color.R, color.G, color.B,
            position.X + 0.5f, position.Y - 0.5f, position.Z - 0.5f, color.R, color.G, color.B,
            position.X + 0.5f, position.Y + 0.5f, position.Z - 0.5f, color.R, color.G, color.B,
            position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f, color.R, color.G, color.B,
        });
    }

    public override uint[] GetIndices(int multiply_index)
    {
        uint[] indices = new uint[] {
            0, 1, 2, 2, 3, 0, // front
            1, 5, 6, 6, 2, 1, // right
            5, 4, 7, 7, 6, 5, // back
            4, 0, 3, 3, 7, 4, // left
            3, 2, 6, 6, 7, 3, // top
            4, 5, 1, 1, 0, 4,  // bottom
        };

        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] += (uint)multiply_index;
        }

        return indices;
    }

}