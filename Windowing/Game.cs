using System;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;
using Silk.NET.Input;
using System.Drawing;

using Prefabs;
using Utils;
using Interfaces;



namespace Windowing;

class Game
{
    private static IWindow window;
    private static GL gl;

    private static uint shaderProgram;

    Camera camera;
    private Vector2 _lastMousePosition;
    private bool _firstMouse = true;
    private bool _mouseCaptured = false;
    private List<Key> keysPressed = new List<Key> { };
    IInputContext _windowInput;

    private Scene scene;

    public Game(int width, int height)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = "Silk Window";

        window = Window.Create(options);
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
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

    private void MouseMove(IMouse mouse, Vector2 position) {
        if (!_mouseCaptured) return;
        
        if (_firstMouse)
        {
            _lastMousePosition = position;
            _firstMouse = false;
            return;
        }
        
        float xOffset = position.X - _lastMousePosition.X;
        float yOffset = _lastMousePosition.Y - position.Y;
        
        _lastMousePosition = position;
        
        camera.ProcessMouseMovement(xOffset, yOffset);
    }

    private void AddObjectsToScene()
    {
        scene.RegisterObjectType<Cube>((uint)ObjectIndexes.Cube);

        Cube cube = new Cube(
            new Vector3(0, 0, 0),
            Vector3.Zero,
            Vector3.One,
            Color.Blue);

        Cube cube2 = new Cube(
            new Vector3(0, -1, 0),
            Vector3.Zero,
            Vector3.One,
            Color.Red);

        scene.AddObject(cube);
        scene.AddObject(cube2);
    }

    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);
        gl.Enable(GLEnum.DepthTest);

        camera = new Camera(
            new Vector3(0, 0, 5),
            Vector3.Zero,
            Vector3.One
        );
        scene = new Scene(gl);

        AddObjectsToScene();

        shaderProgram = CreateShaderProgram();

        

        _windowInput = window.CreateInput();
        for (int i = 0; i < _windowInput.Mice.Count; i++)
            _windowInput.Mice[i].MouseMove += MouseMove;
        for (int i = 0; i < _windowInput.Keyboards.Count; i++)
                _windowInput.Keyboards[i].KeyDown += KeyDown;
        for (int i = 0; i < _windowInput.Keyboards.Count; i++)
            _windowInput.Keyboards[i].KeyUp += KeyUp;
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
                        camera.ProcessKeyboard(Camera.CameraMovement.Forward, (float)delta);
                        break;
                    case Key.S:
                        camera.ProcessKeyboard(Camera.CameraMovement.Backward, (float)delta);
                        break;
                    case Key.A:
                        camera.ProcessKeyboard(Camera.CameraMovement.Left, (float)delta);
                        break;
                    case Key.D:
                        camera.ProcessKeyboard(Camera.CameraMovement.Right, (float)delta);
                        break;
                    case Key.ShiftLeft:
                        camera.ProcessKeyboard(Camera.CameraMovement.Down, (float)delta);
                        break;
                    case Key.Space:
                        camera.ProcessKeyboard(Camera.CameraMovement.Up, (float)delta);
                        break;
                    case Key.C:
                        _mouseCaptured = !_mouseCaptured;
                        for (int i = 0; i < _windowInput.Mice.Count; i++)
                        {
                            _windowInput.Mice[i].Cursor.CursorMode =
                                _mouseCaptured ? CursorMode.Raw : CursorMode.Normal; 
                        }
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

        var view = camera.GetViewMatrix();
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45f),
            window.Size.X / (float)window.Size.Y,
            0.1f,
            100f);

        int viewLoc = gl.GetUniformLocation(shaderProgram, "uView");
        int projLoc = gl.GetUniformLocation(shaderProgram, "uProjection");
        gl.UniformMatrix4(viewLoc, 1, false, (float*)&view);
        gl.UniformMatrix4(projLoc, 1, false, (float*)&proj);

        scene.Render(gl, shaderProgram, view, proj);
    }

    private static uint CreateShaderProgram()
    {
        string vertexCode = @"
            #version 330 core                                                                         
            layout(location = 0) in vec3 aPos;                                                        
            layout(location = 1) in vec3 aColor;                                                      
                                                                                                    
            out vec3 vColor;                                                                          
                                                                                                    
            uniform mat4 uModel;                                                                      
            uniform mat4 uView;                                                                       
            uniform mat4 uProjection;                                                                 
                                                                                                    
            void main()                                                                               
            {                                                                                         
                vColor = aColor;                                                                      
                mat4 mvp = uProjection * uView * uModel;                                              
                gl_Position = mvp * vec4(aPos, 1.0);                                                  
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
        CheckShaderCompileError(vertex, "vertex");

        uint fragment = gl.CreateShader(GLEnum.FragmentShader);
        gl.ShaderSource(fragment, fragmentCode);
        gl.CompileShader(fragment);
        CheckShaderCompileError(fragment, "fragment");


        uint program = gl.CreateProgram();
        gl.AttachShader(program, vertex);
        gl.AttachShader(program, fragment);
        gl.LinkProgram(program);

        gl.DeleteShader(vertex);
        gl.DeleteShader(fragment);

        return program;
    }

    private unsafe static void CheckShaderCompileError(uint shader, string type)
    {
        int success;
        gl.GetShader(shader, GLEnum.CompileStatus, &success);
        if (success == 0)
        {
            string infoLog = gl.GetShaderInfoLog(shader);
            Console.WriteLine($"SHADER COMPILE ERROR ({type}): {infoLog}");
        }
    }
}
