using System;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;
using Silk.NET.Input;
using System.Drawing;

using Prefabs;



namespace Windowing;

class Game
{
    private static IWindow window;
    private static GL gl;

    private static uint vao, vbo, ebo, shaderProgram;

    // Cube vertices (posX, posY, posZ, r, g, b)
    private float[] vertices = {};

    // Indices for cube (two triangles per face)
    private uint[] indices = {};

    private Vector3D<float> camera_position;
    private float cameraSpeed = 1f;

    private List<Key> keysPressed = new List<Key> {};

    public Game(int width, int height)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = "Silk Window";

        window = Window.Create(options);
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;

        camera_position = new Vector3D<float>(0f, 0f, -5f);
    }

    public void Run()
    {
        window.Run();
    }

    private void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            Console.WriteLine("Exiting...");
            window.Close();
        }

        keysPressed.Add(key);
    }

    private void KeyUp(IKeyboard keyboard, Key key, int keyCode)
    {
        keysPressed.Remove(key);
    }

    private void UpdateRenderData()
    {
        List<float> vert = new List<float> { };
        List<uint> ind = new List<uint> { };
        int last_vert_ammount = 0;

        Cube cube = new Cube(
            new Vector3(0, 0, 0),
            Vector3.Zero,
            Vector3.One,
            Color.Black);

        vert.AddRange(cube.GetVertices());
        ind.AddRange(cube.GetIndices(last_vert_ammount));
        last_vert_ammount += cube.GetVertices().Count() / 6;

        Cube cube2 = new Cube(
            new Vector3(0, -1, 0),
            Vector3.Zero,
            Vector3.One,
            Color.White);

        vert.AddRange(cube2.GetVertices());
        ind.AddRange(cube2.GetIndices(last_vert_ammount));
        last_vert_ammount += cube.GetVertices().Count() / 6;

        vertices = vert.ToArray<float>();
        indices = ind.ToArray<uint>();
    }

    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);

        IInputContext input = window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
            input.Keyboards[i].KeyDown += KeyDown;
        for (int i = 0; i < input.Keyboards.Count; i++)
            input.Keyboards[i].KeyUp += KeyUp;

        // Enable depth testing
        gl.Enable(GLEnum.DepthTest);

        UpdateRenderData();

        // Create VAO, VBO, EBO
        vao = gl.GenVertexArray();
        vbo = gl.GenBuffer();
        ebo = gl.GenBuffer();

        gl.BindVertexArray(vao);

        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertices, GLEnum.StaticDraw);

        gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indices, GLEnum.StaticDraw);

        // Position attribute (3 floats)
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)0);
        gl.EnableVertexAttribArray(0);

        // Color attribute (3 floats)
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        // Compile shaders
        shaderProgram = CreateShaderProgram();

    }

    private void OnUpdate(double delta)
    {
        if (keysPressed.Count != 0)
        {
            foreach (Key key in keysPressed)
            {
                switch (key)
                {
                    case Key.W:
                        camera_position.Z += cameraSpeed * (float)delta;
                        break;
                    case Key.S:
                        camera_position.Z -= cameraSpeed * (float)delta;
                        break;
                    case Key.A:
                        camera_position.X += cameraSpeed * (float)delta;
                        break;
                    case Key.D:
                        camera_position.X -= cameraSpeed * (float)delta;
                        break;
                    case Key.Q:
                        camera_position.Y += cameraSpeed * (float)delta;
                        break;
                    case Key.E:
                        camera_position.Y -= cameraSpeed * (float)delta;
                        break;
                }
            }
        }
    }

    private unsafe void OnRender(double delta)
    {
        gl.ClearColor(0.1f, 0.1f, 0.2f, 1f);
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        gl.UseProgram(shaderProgram);

        // Build MVP matrix
        var model = Matrix4x4.CreateRotationY(0f);
        var view = Matrix4x4.CreateTranslation(camera_position.X, camera_position.Y, camera_position.Z);
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(
            (float)Math.PI / 4f,
            window.Size.X / (float)window.Size.Y,
            0.1f,
            100f);


        var mvp = model * view * proj;

        // Upload matrix to shader
        int loc = gl.GetUniformLocation(shaderProgram, "uMVP");
        gl.UniformMatrix4(loc, 1, false, (float*)&mvp);

        // Draw cube
        gl.BindVertexArray(vao);
        gl.DrawElements(GLEnum.Triangles, (uint)indices.Length, GLEnum.UnsignedInt, null);
    }

    private static uint CreateShaderProgram()
    {
        string vertexCode = @"
            #version 330 core
            layout(location = 0) in vec3 aPos;
            layout(location = 1) in vec3 aColor;
            out vec3 vColor;
            uniform mat4 uMVP;
            void main()
            {
                vColor = aColor;
                gl_Position = uMVP * vec4(aPos, 1.0);
            }
        ";

        string fragmentCode = @"
            #version 330 core
            in vec3 vColor;
            out vec4 FragColor;
            void main()
            {
                FragColor = vec4(vColor, 1.0);
            }
        ";

        uint vertex = gl.CreateShader(GLEnum.VertexShader);
        gl.ShaderSource(vertex, vertexCode);
        gl.CompileShader(vertex);

        uint fragment = gl.CreateShader(GLEnum.FragmentShader);
        gl.ShaderSource(fragment, fragmentCode);
        gl.CompileShader(fragment);

        uint program = gl.CreateProgram();
        gl.AttachShader(program, vertex);
        gl.AttachShader(program, fragment);
        gl.LinkProgram(program);

        gl.DeleteShader(vertex);
        gl.DeleteShader(fragment);

        return program;
    }
}
