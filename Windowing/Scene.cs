namespace Windowing;

using Interfaces;
using System.Numerics;
using Silk.NET.OpenGL;
using Utils;

public class Scene
{
    private List<IRenderable> _objects = new List<IRenderable>();
    private Dictionary<IRenderable, Type> _objectTypes = new Dictionary<IRenderable, Type>();
    private Dictionary<Type, uint> _objectIndexCounts = new Dictionary<Type, uint>();

    public void RegisterObjectType<T>(uint indexCount) where T : IRenderable
    {
        _objectIndexCounts[typeof(T)] = indexCount;
    }

    public void AddObject<T>(T obj) where T : IRenderable
    {
        _objects.Add(obj);
        _objectTypes[obj] = typeof(T);
    }

    public uint GetIndexCountForObject(IRenderable obj)
    {
        if (_objectTypes.TryGetValue(obj, out Type type) && 
            _objectIndexCounts.TryGetValue(type, out uint indexCount))
        {
            return indexCount;
        }
        
        throw new InvalidOperationException($"No index count registered for object type: {obj.GetType()}");
    }

    public void Update(double deltatime)
    {

    }

    public unsafe void Render(GL gl, uint shaderProgram, Matrix4x4 view, Matrix4x4 projection)
    {
        VAOGenerator vaoGen = new VAOGenerator(gl);

        gl.UseProgram(shaderProgram);

        int viewLoc = gl.GetUniformLocation(shaderProgram, "uView");
        int projLoc = gl.GetUniformLocation(shaderProgram, "uProjection");
        gl.UniformMatrix4(viewLoc, 1, false, (float*)&view);
        gl.UniformMatrix4(projLoc, 1, false, (float*)&projection);

        int modelLoc = gl.GetUniformLocation(shaderProgram, "uModel");

        foreach (var obj in _objects)
        {
            var model = obj.GetModelMatrix();
            gl.UniformMatrix4(modelLoc, 1, false, (float*)&model);
            
            // Here you would bind different VAOs for different object types
            gl.BindVertexArray(vaoGen.CreateVAO(obj.GetVertices(), obj.GetIndices(0)).vao);
            gl.DrawElements(GLEnum.Triangles, GetIndexCountForObject(obj), GLEnum.UnsignedInt, null);
        }
    }
}