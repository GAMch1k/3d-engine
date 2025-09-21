namespace Prefabs;

using Silk.NET.Maths;
using Interfaces;
using System.Numerics;

public class Object : IRenderable
{
    public Vector3 position { get; set; } = Vector3.Zero;
    public Vector3 rotation { get; set; } = Vector3.Zero;
    public Vector3 scale { get; set; } = Vector3.One;

    private float[] vertices;


    public Object(
        Vector3 position,
        Vector3 rotation,
        Vector3 scale
        )
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    protected void SetVertices(float[] vertices)
    {
        this.vertices = vertices;
    }

    public float[] GetVertices()
    {
        return vertices;
    }

    public virtual uint[] GetIndices(int multiply_index)
    {
        return new uint[] { };
    }

    public Matrix4x4 GetModelMatrix()
    {
        return Matrix4x4.CreateScale(scale) *
               Matrix4x4.CreateRotationX(rotation.X) *
               Matrix4x4.CreateRotationY(rotation.Y) *
               Matrix4x4.CreateRotationZ(rotation.Z) *
               Matrix4x4.CreateTranslation(position);
    }
}