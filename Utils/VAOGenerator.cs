using Silk.NET.OpenGL;

namespace Utils;

public class VAOGenerator
{
    private GL gl;

    public VAOGenerator(GL gl)
    {
        this.gl = gl;
    }


    public unsafe (uint vao, uint vbo, uint ebo) CreateVAO(float[] vertices, uint[] indices)
    {
        uint vao, vbo, ebo;
    
        gl.GenVertexArrays(1, out vao);
        gl.GenBuffers(1, out vbo);
        gl.GenBuffers(1, out ebo);
        
        gl.BindVertexArray(vao);

        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);

        fixed (float* v = vertices)
        {
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, GLEnum.StaticDraw);
        }

        gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        fixed (uint* i = indices)
        {
            gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, GLEnum.StaticDraw);
        }
        
        // Position attribute (location = 0)
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 9 * sizeof(float), (void*)0);
        gl.EnableVertexAttribArray(0);
        
        // Color attribute (location = 1)
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 9 * sizeof(float), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);
        
        // Normal attribute (location = 2)
        gl.VertexAttribPointer(2, 3, GLEnum.Float, false, 9 * sizeof(float), (void*)(6 * sizeof(float)));
        gl.EnableVertexAttribArray(2);
        
        gl.BindVertexArray(0);

        return (vao, vbo, ebo);
    }
}
