using ABI.CCK.Components;
using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

public class CVRSM64Mario : MonoBehaviour {
    [SerializeField] internal Material material;
    [SerializeField] internal bool replaceTextures = true;
    [SerializeField] internal List<string> propertiesToReplaceWithTexture = new() { "_MainTex" };
    [SerializeField] internal CVRSpawnable spawnable;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64Mario))]
public class CVRSM64CMarioEditor : Editor {

    private const string TEXTURE_NAME_PREFIX = "sm64_proxy_";

    private readonly HashSet<string> _marioSyncedInputs = new() {
        "Horizontal",
        "Vertical",
        "Jump",
        "Kick",
        "Stomp",
    };

    SerializedProperty spawnable;
    SerializedProperty material;
    SerializedProperty replaceTextures;
    SerializedProperty propertiesToReplaceWithTexture;

    private void OnEnable() {
        spawnable = serializedObject.FindProperty("spawnable");
        material = serializedObject.FindProperty("material");
        replaceTextures = serializedObject.FindProperty("replaceTextures");
        propertiesToReplaceWithTexture = serializedObject.FindProperty("propertiesToReplaceWithTexture");
    }

    public override void OnInspectorGUI() {

        if (Application.isPlaying) {
            EditorGUILayout.HelpBox("This script doesn't work in play mode!", MessageType.Warning);
            return;
        }

        var behavior = (CVRSM64Mario) target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(spawnable);
        EditorGUILayout.PropertyField(material);
        serializedObject.ApplyModifiedProperties();

        if (!ValidateSpawnable(behavior)) {

            if (GUILayout.Button(new GUIContent(
                    "Attempt to Auto-Setup Mario", "This will add/modify a CVRSpawnable Script , " +
                                   "and set it up with the Mario Requirements " +
                                   "(add synced params, and optionally a sub-sync transform)."))) {
                SetupSpawnable(behavior);
            }
            return;
        }

        if (!ValidateMaterial(behavior)) return;

        serializedObject.ApplyModifiedProperties();
    }

    private bool ValidateMaterial(CVRSM64Mario behavior) {

        if (behavior.material == null) {
            EditorGUILayout.HelpBox("Leaving an empty material will ensure to use our default Mario Material.", MessageType.Info);
            return false;
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

        return true;
    }

    private const CVRSpawnableSubSync.SyncFlags AllFlags =
        CVRSpawnableSubSync.SyncFlags.RotationX |
        CVRSpawnableSubSync.SyncFlags.RotationY |
        CVRSpawnableSubSync.SyncFlags.RotationZ |
        CVRSpawnableSubSync.SyncFlags.TransformX |
        CVRSpawnableSubSync.SyncFlags.TransformY |
        CVRSpawnableSubSync.SyncFlags.TransformZ;

    private void SetupSpawnable(CVRSM64Mario behavior) {

        // Missing spawnable -> Create it
        if (behavior.spawnable == null) {
            behavior.spawnable = behavior.gameObject.AddComponent<CVRSpawnable>();
        }

        // Configure basic spawnable settings
        behavior.spawnable.spawnHeight = 0.2f;
        behavior.spawnable.useAdditionalValues = true;

        // Add each synced value to the spawnable
        foreach (var inputName in _marioSyncedInputs) {

            var parameter = behavior.spawnable.syncValues.FirstOrDefault(value => value.name == inputName);

            // Missing parameter -> Create it
            if (parameter == null) {
                var syncedValue = new CVRSpawnableValue {
                    name = inputName
                };
                behavior.spawnable.syncValues.Add(syncedValue);
            }
        }

        // Fix the sub-sync transform in case it's not on the root
        var isOnRoot = behavior.spawnable.gameObject == behavior.gameObject;
        if (!isOnRoot) {
            var subSync = behavior.spawnable.subSyncs.FirstOrDefault(value => value.transform == behavior.transform);
            if (subSync == null) {
                subSync = new CVRSpawnableSubSync {
                    precision = CVRSpawnableSubSync.SyncPrecision.Full,
                    transform = behavior.transform,
                    syncedValues = AllFlags,
                };
                behavior.spawnable.subSyncs.Add(subSync);
            }
            subSync.precision = CVRSpawnableSubSync.SyncPrecision.Full;
            subSync.syncedValues = AllFlags;
        }
    }

    private bool ValidateSpawnable(CVRSM64Mario behavior) {

        var parentSpawnable = behavior.gameObject.GetComponentInParent<CVRSpawnable>();

        if (behavior.spawnable == null) {

            if (parentSpawnable == null) {
                var err = $"{nameof(CVRSM64Mario)} requires a {nameof(CVRSpawnable)} defined!";
                EditorGUILayout.HelpBox(err, MessageType.Error);
                if (Application.isPlaying) throw new Exception(err);
                return false;
            }

            behavior.spawnable = parentSpawnable;
        }

        if (behavior.spawnable != parentSpawnable) {
            var err = $"The {nameof(CVRSpawnable)} defined in {nameof(CVRSM64Mario)} [{behavior.gameObject.name}] " +
                      $"is not on the same game object nor on a parent game object! Or you multiple {nameof(CVRSpawnable)}? " +
                      $"What have you done ???";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return false;
        }

        var rootSpawnable = behavior.GetComponent<CVRSpawnable>();

        var hasMarioSubSync = false;

        if (rootSpawnable == null) {
            foreach (var subSync in behavior.spawnable.subSyncs) {
                if (subSync.transform == behavior.transform) {
                    if (subSync.precision != CVRSpawnableSubSync.SyncPrecision.Full) {
                        var err = $"We recommend setting the {nameof(CVRSpawnable)} sub-sync for the mario " +
                                  $"transform to Full. Currently set to: " +
                                  $"{Enum.GetName(typeof(CVRSpawnableSubSync.SyncPrecision), subSync.precision)}...";
                        EditorGUILayout.HelpBox(err, MessageType.Warning);
                    }

                    foreach (var syncFlag in (CVRSpawnableSubSync.SyncFlags[])Enum.GetValues(typeof(CVRSpawnableSubSync.SyncFlags))) {
                        if (!subSync.syncedValues.HasFlag(syncFlag)) {
                            var err = $"We need the sub-sync for the mario transform to sync EVERYTHING. Currently missing: {Enum.GetName(typeof(CVRSpawnableSubSync.SyncFlags), syncFlag)}";
                            EditorGUILayout.HelpBox(err, MessageType.Error);
                            if (Application.isPlaying) throw new Exception(err);
                            return false;
                        }
                    }

                    hasMarioSubSync = true;
                }
            }
        }

        if (rootSpawnable) {
            EditorGUILayout.HelpBox($"Mario position is being synced due being on the same game object as the " +
                                    $"{nameof(CVRSpawnable)} component.", MessageType.Info);
        }
        else if (hasMarioSubSync) {
            EditorGUILayout.HelpBox($"Mario position is being synced due being on the a sunb-sync transform " +
                                    $"of the {nameof(CVRSpawnable)} component.", MessageType.Info);
        }
        else {
            var err = $"{nameof(CVRSM64Mario)} requires to be placed either on the game object that has" +
                      $" the {nameof(CVRSpawnable)} or on a transform that is set as a sub-sync.";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return false;
        }

        foreach (var marioSyncedInput in _marioSyncedInputs) {
            var parameter = behavior.spawnable.syncValues.FirstOrDefault(value => value.name == marioSyncedInput);
            if (parameter != null) continue;
            var err = $"{nameof(CVRSM64Mario)} requires a {nameof(CVRSpawnable)} with a synced value named: {marioSyncedInput}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return false;
        }

        return true;
    }
}
