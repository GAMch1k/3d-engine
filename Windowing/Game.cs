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

    private void MouseScroll(IMouse mouse, ScrollWheel wheel)
    {
        camera.MovementSpeed += wheel.Y;
        if (camera.MovementSpeed < 0.1f)
            camera.MovementSpeed = 0.1f;
        if (camera.MovementSpeed > 15f)
            camera.MovementSpeed = 15f;
    }

    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);
        gl.Enable(GLEnum.DepthTest);
        
        SceneManager scenemg = new SceneManager();
        scene = scenemg.Load(gl);
        camera = scenemg.camera;

        shaderProgram = CreateShaderProgram();

        _windowInput = window.CreateInput();
        for (int i = 0; i < _windowInput.Mice.Count; i++)
        {
            _windowInput.Mice[i].MouseMove += MouseMove;
            _windowInput.Mice[i].Scroll += MouseScroll;
        }

        for (int i = 0; i < _windowInput.Keyboards.Count; i++)
        {
            _windowInput.Keyboards[i].KeyDown += KeyDown;
            _windowInput.Keyboards[i].KeyUp += KeyUp;
        }
    }
    private void OnUpdate(double deltatime)
    {
        foreach (var obj in scene.GetObjects())
        {
            obj.Value.Update((float)deltatime);
        }

        if (keysPressed.Count != 0)
        {
            foreach (Key key in keysPressed)
            {
                switch (key)
                {
                    case Key.W:
                        camera.ProcessKeyboard(Camera.CameraMovement.Forward, (float)deltatime);
                        break;
                    case Key.S:
                        camera.ProcessKeyboard(Camera.CameraMovement.Backward, (float)deltatime);
                        break;
                    case Key.A:
                        camera.ProcessKeyboard(Camera.CameraMovement.Left, (float)deltatime);
                        break;
                    case Key.D:
                        camera.ProcessKeyboard(Camera.CameraMovement.Right, (float)deltatime);
                        break;
                    case Key.ShiftLeft:
                        camera.ProcessKeyboard(Camera.CameraMovement.Down, (float)deltatime);
                        break;
                    case Key.Space:
                        camera.ProcessKeyboard(Camera.CameraMovement.Up, (float)deltatime);
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

    private unsafe void OnRender(double deltatime)
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

        // Light uniforms
        Vector3 lightDir = new Vector3(0.0f, -1.0f, -1.0f);
        lightDir = Vector3.Normalize(lightDir);
        int lightDirLoc = gl.GetUniformLocation(shaderProgram, "uLightDir");
        int lightColorLoc = gl.GetUniformLocation(shaderProgram, "uLightColor");
        gl.Uniform3(lightDirLoc, lightDir.X, lightDir.Y, lightDir.Z);
        gl.Uniform3(lightColorLoc, 1.0f, 1.0f, 1.0f);

        scene.Render(gl, shaderProgram, view, proj);
    }

    private static uint CreateShaderProgram()
    {
        string vertexCode = @"
            #version 330 core                                                                         
            layout(location = 0) in vec3 aPos;                                                        
            layout(location = 1) in vec3 aColor;                                                      
            layout(location = 2) in vec3 aNormal;
                                                                                                    
            out vec3 vColor;                                                                          
            out vec3 vNormal;
            out vec3 vFragPos;
                                                                                                    
            uniform mat4 uModel;                                                                      
            uniform mat4 uView;                                                                       
            uniform mat4 uProjection;                                                                 
                                                                                                    
            void main()                                                                               
            {                                                                                         
                vColor = aColor;
                vFragPos = vec3(uModel * vec4(aPos, 1.0));
                vNormal = mat3(transpose(inverse(uModel))) * aNormal;
                mat4 mvp = uProjection * uView * uModel;                                              
                gl_Position = mvp * vec4(aPos, 1.0);                                                  
            }   
        ";

        string fragmentCode = @"
            #version 330 core
            in vec3 vColor;
            in vec3 vNormal;
            in vec3 vFragPos;

            out vec4 FragColor;

            uniform vec3 uLightDir;
            uniform vec3 uLightColor;

            void main()
            {
                vec3 norm = normalize(vNormal);
                vec3 lightDirNorm = normalize(-uLightDir);
                float diff = max(dot(norm, lightDirNorm), 0.0);
                vec3 diffuse = diff * uLightColor;
                vec3 ambient = 0.2 * uLightColor;
                vec3 result = (ambient + diffuse) * vColor;
                FragColor = vec4(result, 1.0);
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
