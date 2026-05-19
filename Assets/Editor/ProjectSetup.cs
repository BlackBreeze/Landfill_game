using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Run via menu: Landfill > Setup Project Scenes
public static class ProjectSetup
{
    [MenuItem("Landfill/Setup Project Scenes")]
    public static void SetupScenes()
    {
        CreateGameScene();
        CreateUpgradeScene();
        AddScenesToBuildSettings();
        Debug.Log("Landfill: Project scenes created and added to Build Settings.");
    }

    static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = GameConstants.CameraOrthoSize;
        cam.transform.position = GameConstants.CameraPosition;
        cam.backgroundColor  = new Color(0.08f, 0.08f, 0.10f); // near-black bg
        cam.clearFlags       = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();

        // GameManager
        var gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        // Placeholder Board anchor
        new GameObject("Board");

        // Placeholder UI Canvas anchor
        var uiGO = new GameObject("UI");
        var canvas = uiGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        uiGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        string path = "Assets/Scenes/GameScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Created: {path}");
    }

    static void CreateUpgradeScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera — simple centred camera, UI only
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.backgroundColor  = new Color(0.10f, 0.07f, 0.05f);
        cam.clearFlags       = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();

        // GameManager — will be overridden by DontDestroyOnLoad instance in play
        var gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        // UI Canvas
        var uiGO = new GameObject("UI");
        var canvas = uiGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        uiGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        string path = "Assets/Scenes/UpgradeScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Created: {path}");
    }

    static void AddScenesToBuildSettings()
    {
        var scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity",    true),
            new EditorBuildSettingsScene("Assets/Scenes/UpgradeScene.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
    }
}
