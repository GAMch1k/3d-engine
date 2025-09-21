namespace Prefabs;

using Silk.NET.Maths;
using Interfaces;
using System.Numerics;

public class RenderObject : GameObject, IRenderable
{
    public uint indexCount { get; set; }

    private float[] vertices;


    public RenderObject(
        Vector3 position,
        Vector3 rotation,
        Vector3 scale
        ) : base (position, rotation, scale)
    {}

    protected void SetVertices(float[] vertices)
    {
        this.vertices = vertices;
    }

    public float[] GetVertices()
    {
        return vertices;
    }

    public virtual uint[] GetIndices()
    {
        return new uint[] { };
    }

    public Matrix4x4 GetModelMatrix()
    {
        return Matrix4x4.CreateScale(Scale) *
               Matrix4x4.CreateRotationX(Rotation.X) *
               Matrix4x4.CreateRotationY(Rotation.Y) *
               Matrix4x4.CreateRotationZ(Rotation.Z) *
               Matrix4x4.CreateTranslation(Position);
    }
}