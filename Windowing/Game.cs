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

    private SceneRenderer scene;

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

    private void MouseMove(IMouse mouse, Vector2 position)
    {
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
        if (camera.MovementSpeed > 25f)
            camera.MovementSpeed = 25f;
    }

    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);
        gl.Enable(GLEnum.DepthTest);
        gl.Enable(GLEnum.CullFace);
        gl.CullFace(GLEnum.Back);
        gl.FrontFace(GLEnum.Ccw);

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

        // Light uniforms
        Vector3 lightDir = new Vector3(0.0f, 1.0f, -0.2f);
        lightDir = Vector3.Normalize(lightDir);
        int lightDirLoc = gl.GetUniformLocation(shaderProgram, "uLightDir");
        int lightColorLoc = gl.GetUniformLocation(shaderProgram, "uLightColor");

        gl.Uniform3(lightDirLoc, lightDir.X, lightDir.Y, lightDir.Z);
        gl.Uniform3(lightColorLoc, 0.8f, 0.8f, 0.8f);

        int specularLoc = gl.GetUniformLocation(shaderProgram, "uSpecularStrength");
        int shininessLoc = gl.GetUniformLocation(shaderProgram, "uShininess");
        gl.Uniform1(specularLoc, 0.7f);  // Medium shine; 0.0=no spec, 1.0=very shiny
        gl.Uniform1(shininessLoc, 32.0f);  // Sharp highlights; tweak to 16 for softer

        int cameraPosLoc = gl.GetUniformLocation(shaderProgram, "uCameraPos");
        gl.Uniform3(cameraPosLoc, camera.Position.X, camera.Position.Y, camera.Position.Z);

        scene.Render(gl, shaderProgram, view, proj);
    }

    private unsafe static uint CreateShaderProgram()
    {
        string vertexCode = @"
            #version 330 core
            layout(location = 0) in vec3 aPos;
            layout(location = 1) in vec3 aColor;
            layout(location = 2) in vec3 aNormal;

            out vec3 vColor;
            out vec3 vNormal;
            out vec3 vFragPos;
            out vec3 vViewPos;  // New: For specular (view direction)

            uniform mat4 uModel;
            uniform mat3 uModel3x3;
            uniform mat4 uView;
            uniform mat4 uProjection;
            uniform vec3 uCameraPos;  // New: Camera position for view dir

            void main()
            {
                vColor = aColor;
                vFragPos = vec3(uModel * vec4(aPos, 1.0));
                vNormal = mat3(transpose(inverse(uModel3x3))) * aNormal;
                vViewPos = uCameraPos;  // Pass camera pos to frag
                gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
            }
        ";

        string fragmentCode = @"
            #version 330 core
            in vec3 vColor;
            in vec3 vNormal;
            in vec3 vFragPos;
            in vec3 vViewPos;

            out vec4 FragColor;

            uniform vec3 uLightDir;
            uniform vec3 uLightColor;
            uniform float uSpecularStrength;  // New: Controls shine (0.0-1.0)
            uniform float uShininess;  // New: Exponent for highlight sharpness (2-256)

            void main()
            {
                vec3 norm = normalize(vNormal);
                vec3 lightDirNorm = normalize(uLightDir);  // To light
                vec3 viewDir = normalize(vViewPos - vFragPos);  // From frag to camera
                vec3 reflectDir = reflect(-lightDirNorm, norm);  // Reflected light

                // Diffuse
                float diff = max(dot(norm, lightDirNorm), 0.0);
                vec3 diffuse = 0.8 * diff * uLightColor;

                // Specular (Phong)
                float spec = pow(max(dot(viewDir, reflectDir), 0.0), uShininess);
                vec3 specular = uSpecularStrength * spec * uLightColor;

                // Ambient
                vec3 ambient = 0.4 * uLightColor;

                // Combine
                vec3 result = (ambient + diffuse + specular) * vColor;

                // Gamma correction (2.2 for sRGB)
                result = pow(result, vec3(1.0 / 2.2));

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

        int linkSuccess;
        gl.GetProgram(program, GLEnum.LinkStatus, &linkSuccess);
        if (linkSuccess == 0)
        {
            string log = gl.GetProgramInfoLog(program);
            Console.WriteLine($"SHADER LINK ERROR: {log}");
            return 0;
        }
        Console.WriteLine("Shader linked successfully.");

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
