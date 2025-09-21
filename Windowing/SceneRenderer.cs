namespace Windowing;

using Interfaces;
using System.Numerics;
using Silk.NET.OpenGL;
using Utils;

public class SceneRenderer
{
    private Dictionary<string, IRenderable> _objects = new Dictionary<string, IRenderable>();
    private Dictionary<IRenderable, Type> _objectTypes = new Dictionary<IRenderable, Type>();
    private Dictionary<Type, uint> _objectIndexCounts = new Dictionary<Type, uint>();
    private Dictionary<IRenderable, uint> _objectVAOs = new Dictionary<IRenderable, uint>();
    private VAOGenerator vaoGen;

    public SceneRenderer(GL gl)
    {
        vaoGen = new VAOGenerator(gl);
    }

    public void RegisterObjectType<T>(uint indexCount) where T : IRenderable
    {
        _objectIndexCounts[typeof(T)] = indexCount;
    }

    public void AddObject<T>(T obj) where T : IRenderable
    {
        _objects[obj.Name] = obj;
        _objectTypes[obj] = typeof(T);

        var vaoData = vaoGen.CreateVAO(obj.GetVertices(), obj.GetIndices());
        _objectVAOs[obj] = vaoData.vao;
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

    public Dictionary<string, IRenderable> GetObjects()
    {
        return _objects;
    }

    public unsafe void Render(GL gl, uint shaderProgram, Matrix4x4 view, Matrix4x4 projection)
    {
        int viewLoc = gl.GetUniformLocation(shaderProgram, "uView");
        int projLoc = gl.GetUniformLocation(shaderProgram, "uProjection");
        gl.UniformMatrix4(viewLoc, 1, false, (float*)&view);
        gl.UniformMatrix4(projLoc, 1, false, (float*)&projection);

        int modelLoc = gl.GetUniformLocation(shaderProgram, "uModel");

        foreach (var obj in _objects)
        {
            var model = obj.Value.GetModelMatrix();
            gl.UniformMatrix4(modelLoc, 1, false, (float*)&model);

            float[] model3x3 = new float[9] {                                                             
                model.M11, model.M12, model.M13,                                                          
                model.M21, model.M22, model.M23,                                                          
                model.M31, model.M32, model.M33                                                           
            };                                                                                            
                                                                                                        
            int model3x3Loc = gl.GetUniformLocation(shaderProgram, "uModel3x3");                          
            fixed (float* ptr = model3x3)                                                                 
            {                                                                                             
                gl.UniformMatrix3(model3x3Loc, 1, false, ptr);                                            
            } 

            gl.BindVertexArray(_objectVAOs[obj.Value]);
            gl.DrawElements(GLEnum.Triangles, obj.Value.indexCount, GLEnum.UnsignedInt, null);
        }
    }
}