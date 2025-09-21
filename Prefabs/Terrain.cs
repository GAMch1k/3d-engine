using System.Numerics;
using LibNoise.Primitive;

namespace Prefabs;

public class Terrain : RenderObject
{
    public Vector2 GridSize;
    public float GridOffset;
    private uint[] _indeces;
    private SimplexPerlin _perlin = new SimplexPerlin();

    public Terrain(
        string name,
        Vector3 position,
        Vector3 rotation,
        Vector3 scale,
        Vector2 gridSize,
        float gridOffset)
        : base(position, rotation, scale)
    {
        Name = name;
        GridSize = gridSize;
        GridOffset = gridOffset;
        _generateVertices();
    }

    private void _generateVertices()
    {
        List<float> vertices = new List<float>();
        List<uint> indeces = new List<uint>();

        int height = (int)(GridSize.Y / GridOffset);

        Random random = new Random();

        for (int x = 0; x < GridSize.X / GridOffset; x++)
        {
            // Using Z because Y is just a Vector2 field, not the axis I need
            for (int z = 0; z < height; z++)
            {
                float yval = _perlin.GetValue(x * 0.1f, z * 0.1f) * (1f - -0.5f) + -0.5f;
                // position
                vertices.Add(-(GridSize.X / 2) + GridOffset * x);
                vertices.Add(yval);
                vertices.Add((GridSize.Y / 2) - GridOffset * z);

                // color
                vertices.Add(0.1f);
                vertices.Add(0.2f);
                vertices.Add(0.1f);

                // normal
                vertices.Add(0);
                vertices.Add(1);
                vertices.Add(0);


                if (x <= 0) continue;
                if (z >= height - 1) continue;
                int indexnow = x * height + z;
                indeces.Add((uint)(indexnow - height + 1));
                indeces.Add((uint)(indexnow - height));
                indeces.Add((uint)indexnow);

                indeces.Add((uint)indexnow + 1);
                indeces.Add((uint)(indexnow - height + 1));
                indeces.Add((uint)indexnow);
            }
        }

        Vector3[] positions = new Vector3[(int)(vertices.Count / 9)];
        for (int i = 0; i < positions.Length; i++)
        {
            int baseIdx = i * 9;
            positions[i] = new Vector3(
                vertices[baseIdx + 0], vertices[baseIdx + 1], vertices[baseIdx + 2]
            );
        }

        // Accumulate normals per vertex
        Vector3[] accumulatedNormals = new Vector3[positions.Length];
        for (int i = 0; i < accumulatedNormals.Length; i++)
            accumulatedNormals[i] = Vector3.Zero;

        // Process each triangle
        for (int tri = 0; tri < indeces.Count; tri += 3)
        {
            uint idxA = indeces[tri], idxB = indeces[tri + 1], idxC = indeces[tri + 2];
            Vector3 A = positions[idxA], B = positions[idxB], C = positions[idxC];

            Vector3 edge1 = B - A;
            Vector3 edge2 = C - A;
            Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

            accumulatedNormals[idxA] += faceNormal;
            accumulatedNormals[idxB] += faceNormal;
            accumulatedNormals[idxC] += faceNormal;
        }

        // Normalize per-vertex
        for (int i = 0; i < positions.Length; i++)
        {
            accumulatedNormals[i] = Vector3.Normalize(accumulatedNormals[i]);
        }

        // Insert normalized normals back into vertices (replace hardcoded ones)
        for (int i = 0; i < positions.Length; i++)
        {
            int baseIdx = i * 9;
            int normalIdx = baseIdx + 6; // After position (0-2) + color (3-5)
            vertices[normalIdx + 0] = accumulatedNormals[i].X;
            vertices[normalIdx + 1] = accumulatedNormals[i].Y;
            vertices[normalIdx + 2] = accumulatedNormals[i].Z;
        }

        SetVertices(vertices.ToArray());
        _indeces = indeces.ToArray();
        indexCount = (uint)indeces.Count();
    }

    public override uint[] GetIndices()
    {
        return _indeces;
    }

}