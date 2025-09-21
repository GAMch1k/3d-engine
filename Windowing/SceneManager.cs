using System.Drawing;
using System.Numerics;
using Prefabs;
using Silk.NET.OpenGL;
using Utils;

namespace Windowing;


public class SceneManager
{
    private List<GameObject> gameObjects = new List<GameObject>();
    private Scene scene;
    public Camera camera;

    public Scene Load(GL gl)
    {
        scene = new Scene(gl);

        scene.RegisterObjectType<Cube>((uint)ObjectIndexes.Cube);

        LoadTestScene();

        return scene;
    }

    private void LoadTestScene()
    {
        camera = new Camera(
            "Main Camera",
            new Vector3(0, 0, 5),
            Vector3.Zero,
            Vector3.One
        );

        Random random = new Random();
        for (int i = 0; i < 400; i++)
        {
            Cube cube = new Cube(
                $"random-{i}",
                new Vector3(
                    (float)(random.NextDouble() * (15 - -15) + -15),
                    (float)(random.NextDouble() * (15 - -15) + -15),
                    (float)(random.NextDouble() * (15 - -15) + -15)
                ),
                new Vector3(
                    (float)(random.NextDouble() * (15 - -15) + -15),
                    (float)(random.NextDouble() * (15 - -15) + -15),
                    (float)(random.NextDouble() * (15 - -15) + -15)
                ),
                new Vector3(
                    (float)(random.NextDouble() * (1.5 - 0.5) + 0.5),
                    (float)(random.NextDouble() * (1.5 - 0.5) + 0.5),
                    (float)(random.NextDouble() * (1.5 - 0.5) + 0.5)
                ),
                Color.LightGray
            );
            cube.OnUpdate += deltatime =>
            {
                cube.Position += new Vector3(0, (float)(random.NextDouble() * (0.6 - -0.6) + -0.6), 0) * new Vector3(deltatime);
                // cube.Position += new Vector3(0, -9.8f, 0) * new Vector3(deltatime);

            };
            scene.AddObject(cube);
            gameObjects.Add(cube);
        }

        Cube cube1 = new Cube(
            "cube blue",
            new Vector3(0, 0, 0),
            Vector3.Zero,
            new Vector3(0.5f, 0.5f, 0.5f),
            Color.Blue);

        cube1.OnUpdate += deltatime =>
        {
            cube1.Rotation += new Vector3(-0.5f, 1, -1) * new Vector3((float)deltatime);
        };

        Cube cube2 = new Cube(
            "cube red",
            new Vector3(0, -1, 0),
            Vector3.Zero,
            Vector3.One,
            Color.Red);
        
        cube2.OnUpdate += deltatime =>
        {
            cube2.Rotation += new Vector3(0, -1, 0) * new Vector3((float)deltatime);
        };

        scene.AddObject(cube1);
        scene.AddObject(cube2);
    }

    public void Clear()
    {
        gameObjects = new List<GameObject>();
    }
}