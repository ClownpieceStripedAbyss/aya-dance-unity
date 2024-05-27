using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine;

/// <summary>Initialize the default VRC world scene from a template when the project is first launched</summary>
[InitializeOnLoad]
public class VRCSceneTemplateInitializer
{
    private const string SceneTemplatePath = "Packages/com.vrchat.worlds/Editor/VRCSDK/SDK3/VRCDefaultWorldScene.scenetemplate";
    private const string ScenePath = "Assets/Scenes/VRCDefaultWorldScene.unity";
    
    // called on editor launch or domain reload
    static VRCSceneTemplateInitializer()
    {
        // runs when the project is launched
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // init default scene if there are no other scene assets
            if (AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }).Length == 0)
            {
                // wait a tick to ensure the editor allows scene editing 
                EditorApplication.delayCall += () =>
                {
                    var template = (SceneTemplateAsset)AssetDatabase.LoadAssetAtPath(SceneTemplatePath, typeof(SceneTemplateAsset));
                    SceneTemplateService.Instantiate(template, false, ScenePath);
                    Debug.Log("Initialized default VRC world scene");
                };
            }
        }
    }
}
