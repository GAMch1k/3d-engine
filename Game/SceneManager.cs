using System.Drawing;
using System.Numerics;
using Prefabs;
using Silk.NET.OpenGL;
using Utils;

namespace Game;


public class SceneManager
{
    public Scene CurrentScene;
    private GL gl;
    public SceneManager(GL gl)
    {
        this.gl = gl;
    }

    public SceneRenderer Load(ScenesList scene)
    {
        CurrentScene = new Scene(gl);

        switch (scene)
        {
            case ScenesList.FloatingCubes:
                CurrentScene.LoadFloatingCubesScene();
                break;
            case ScenesList.ProceduralGeneration:
                CurrentScene.LoadProceduralGenerationScene();
                break;
            default:
                throw new Exception("Wrong scene id provided. Please use SceneList enum");

        }
        return CurrentScene.GetSceneRenderer();

    }

    public enum ScenesList : int
    {
        FloatingCubes,
        ProceduralGeneration,
    }
}