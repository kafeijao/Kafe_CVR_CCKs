using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

[RequireComponent(typeof(CVRSM64InputSpawnable))]
public class CVRSM64CMario : MonoBehaviour {
    [SerializeField] internal Material material;
    [SerializeField] internal bool replaceTextures = true;
    [SerializeField] internal List<string> propertiesToReplaceWithTexture = new() { "_MainTex" };
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64CMario))]
public class CVRSM64CMarioEditor : Editor {

    private const string TEXTURE_NAME_PREFIX = "sm64_proxy_";

    SerializedProperty material;
    SerializedProperty replaceTextures;
    SerializedProperty propertiesToReplaceWithTexture;

    private void OnEnable() {
        material = serializedObject.FindProperty("material");
        replaceTextures = serializedObject.FindProperty("replaceTextures");
        propertiesToReplaceWithTexture = serializedObject.FindProperty("propertiesToReplaceWithTexture");
    }

    public override void OnInspectorGUI() {

        if (Application.isPlaying) {
            EditorGUILayout.HelpBox("This script doesn't work in play mode!", MessageType.Warning);
            return;
        }

        var behavior = (CVRSM64CMario) target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(material);
        serializedObject.ApplyModifiedProperties();

        if (behavior.material == null) {
            EditorGUILayout.HelpBox("Leaving an empty material will ensure to use our default Mario Material.", MessageType.Info);
            return;
        }

        var shader = behavior.material.shader;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Texture Replacement Settings", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Replace Textures will allow you to pick which material slots should be replaced with Mario's texture.", MessageType.Info);
        EditorGUILayout.HelpBox("The Replace textures will only cover some parts of Mario's texture, the rest is set via vertex colors.", MessageType.Info);
        EditorGUILayout.HelpBox($"Instead of picking shader properties to be replaced with the texture, you can slot a texture with the name starting with {TEXTURE_NAME_PREFIX}", MessageType.Info);

        var textureCount = ShaderUtil.GetPropertyCount(shader);

        // Check if has any proxy texture
        var hasProxyTexture = false;
        for (var i = 0; i < textureCount; i++) {
            if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv) continue;

            var texturePropertyName = ShaderUtil.GetPropertyName(shader, i);
            var texture = behavior.material.GetTexture(texturePropertyName);
            if(texture != null && texture.name.StartsWith(TEXTURE_NAME_PREFIX)) {
                hasProxyTexture = true;
                break;
            }
        }

        EditorGUI.BeginDisabledGroup(hasProxyTexture);
        var checkboxGui = new GUIContent("Replace Textures", hasProxyTexture
            ? $"There is a texture name in the material that starts with {TEXTURE_NAME_PREFIX}. This is a keyword for a texture to be replaced!"
            : "");
        EditorGUILayout.PropertyField(replaceTextures, checkboxGui);
        EditorGUI.EndDisabledGroup();

        if (hasProxyTexture) replaceTextures.boolValue = true;

        if (replaceTextures.boolValue) {
            for (var i = 0; i < textureCount; i++) {
                if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv) continue;

                var texturePropertyName = ShaderUtil.GetPropertyName(shader, i);
                var texture = behavior.material.GetTexture(texturePropertyName);
                var isProxyTexture = texture != null && texture.name.StartsWith(TEXTURE_NAME_PREFIX);

                var propertySelected = behavior.propertiesToReplaceWithTexture.Contains(texturePropertyName);
                EditorGUI.BeginDisabledGroup(isProxyTexture);
                var guiContent = new GUIContent(texturePropertyName, isProxyTexture
                    ? $"The texture name in this property starts with {TEXTURE_NAME_PREFIX}. This is a keyword for a texture to be replaced!"
                    : "");
                var newSelection = EditorGUILayout.ToggleLeft(guiContent, propertySelected) || isProxyTexture;
                EditorGUI.EndDisabledGroup();

                if (newSelection == propertySelected) continue;
                if (newSelection) {
                    behavior.propertiesToReplaceWithTexture.Add(texturePropertyName);
                }
                else {
                    behavior.propertiesToReplaceWithTexture.Remove(texturePropertyName);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
