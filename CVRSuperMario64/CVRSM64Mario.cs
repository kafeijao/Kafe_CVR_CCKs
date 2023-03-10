using ABI.CCK.Components;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

[ExecuteInEditMode]
public class CVRSM64Mario : MonoBehaviour {

    [SerializeField] internal CVRSpawnable spawnable;
    [SerializeField] internal bool advancedOptions = false;

    // Material & Textures
    [SerializeField] internal Material material = null;
    [SerializeField] internal bool replaceTextures = true;
    [SerializeField] internal List<string> propertiesToReplaceWithTexture = new() { "_MainTex" };

    // Animators
    [SerializeField] internal List<Animator> animators = new();

    // Camera override
    [SerializeField] internal bool overrideCameraPosition = false;
    [SerializeField] internal Transform cameraPositionTransform;

    // Camera Mod Override
    [SerializeField] internal Transform cameraModTransform = null;
    [SerializeField] internal List<Renderer> cameraModTransformRenderersToHide = new();

    // Asset bundle to load the gizmo mesh
    [NonSerialized] private static Mesh _marioMeshCached;
    private const string LibSM64AssetBundleName = "libsm64cck.assetbundle";
    [NonSerialized] private const string MarioFbxAssetPath = "Assets/CVRSuperMario64/Mario.fbx";

    [InitializeOnLoadMethod]
    private static void OnInitialized() {
        try {
            var resourceStream = typeof(CVRSM64Mario).Assembly.GetManifestResourceStream(LibSM64AssetBundleName);
            using var memoryStream = new MemoryStream();
            if (resourceStream == null) {
                Debug.LogError($"Failed to load {LibSM64AssetBundleName}! There won't be any Mario Gizmos!");
                return;
            }
            resourceStream.CopyTo(memoryStream);
            var assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
            var fbxGo = assetBundle.LoadAsset<GameObject>(MarioFbxAssetPath);
            assetBundle.Unload(false);
            var skinnedMeshRenderer = fbxGo.GetComponentInChildren<SkinnedMeshRenderer>(true);
            _marioMeshCached = skinnedMeshRenderer.sharedMesh;
        }
        catch (Exception ex) {
            Debug.LogError("Failed to Load the asset bundle. There won't be any Mario Gizmos!\n" + ex.Message);
        }
    }

    private void OnDrawGizmos() {
        if (_marioMeshCached == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireMesh(_marioMeshCached, transform.position, transform.rotation, Vector3.one);
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64Mario))]
public class CVRSM64CMarioEditor : Editor {

    private const string TEXTURE_NAME_PREFIX = "sm64_proxy_";

    private static readonly Dictionary<string, AnimatorControllerParameterType> _syncedParameters = new() {
        {"Horizontal", AnimatorControllerParameterType.Float},
        {"Vertical", AnimatorControllerParameterType.Float},
        {"Jump", AnimatorControllerParameterType.Float},
        {"Kick", AnimatorControllerParameterType.Float},
        {"Stomp", AnimatorControllerParameterType.Float},
        {"Health", AnimatorControllerParameterType.Float},
        {"Flags", AnimatorControllerParameterType.Float},
        {"Action", AnimatorControllerParameterType.Float},
        {"HasCameraMod", AnimatorControllerParameterType.Float},
    };

    private static readonly Dictionary<string, AnimatorControllerParameterType> _localParameters = new() {
        {"HealthPoints", AnimatorControllerParameterType.Int},
        {"HasMod", AnimatorControllerParameterType.Bool},
        {"HasMetalCap", AnimatorControllerParameterType.Bool},
        {"HasWingCap", AnimatorControllerParameterType.Bool},
        {"HasVanishCap", AnimatorControllerParameterType.Bool},
        {"IsMine", AnimatorControllerParameterType.Bool},
        {"IsBypassed", AnimatorControllerParameterType.Bool},
    };

    SerializedProperty spawnable;
    SerializedProperty advancedOptions;

    SerializedProperty material;
    SerializedProperty replaceTextures;
    SerializedProperty propertiesToReplaceWithTexture;

    SerializedProperty animators;

    SerializedProperty overrideCameraPosition;
    SerializedProperty cameraPositionTransform;

    SerializedProperty cameraModTransform;
    SerializedProperty cameraModTransformRenderersToHide;

    private void OnEnable() {
        spawnable = serializedObject.FindProperty("spawnable");

        advancedOptions = serializedObject.FindProperty("advancedOptions");

        material = serializedObject.FindProperty("material");
        replaceTextures = serializedObject.FindProperty("replaceTextures");
        propertiesToReplaceWithTexture = serializedObject.FindProperty("propertiesToReplaceWithTexture");

        animators = serializedObject.FindProperty("animators");

        overrideCameraPosition = serializedObject.FindProperty("overrideCameraPosition");
        cameraPositionTransform = serializedObject.FindProperty("cameraPositionTransform");

        cameraModTransform = serializedObject.FindProperty("cameraModTransform");
        cameraModTransformRenderersToHide = serializedObject.FindProperty("cameraModTransformRenderersToHide");
    }

    public override void OnInspectorGUI() {

        if (Application.isPlaying) {
            EditorGUILayout.HelpBox("This script doesn't work in play mode!", MessageType.Warning);
            return;
        }

        var behavior = (CVRSM64Mario) target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(spawnable);
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(advancedOptions);
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

        if (advancedOptions.boolValue) {

            // Material
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(material);
            ValidateMaterial(behavior);

            // Animators
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(animators);
            if (!ValidateAnimators(behavior)) {
                if (GUILayout.Button(new GUIContent(
                        "Attempt to Auto-Setup Animators", "This will add/modify the type of the parameters required in the animators selected."))) {
                    SetupAnimators(behavior);
                }
            }
            EditorGUILayout.HelpBox($"These animators will have the parameters {string.Join(", ", _localParameters.Keys)} managed by the mod!", MessageType.Info);

            // Camera override
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(overrideCameraPosition);
            EditorGUILayout.PropertyField(cameraPositionTransform);
            if (behavior.cameraPositionTransform != null) {
                if (!behavior.cameraPositionTransform.IsChildOf(behavior.spawnable.transform)) {
                    EditorGUILayout.HelpBox($"{behavior.cameraPositionTransform.name} is not in the present " +
                                            $"on nor inside of the spawnable hierarchy...", MessageType.Error);
                }
            }
            EditorGUILayout.HelpBox($"You can slot a transform here, and whenever you set the override camera " +
                                    $"position to true, this transform will be used as camera position to control the Mario. " +
                                    $"You can animate the overrideCameraPosition boolean!", MessageType.Info);
            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(cameraModTransform);
            if (behavior.cameraModTransform != null) {
                if (!behavior.cameraModTransform.IsChildOf(behavior.spawnable.transform)) {
                    EditorGUILayout.HelpBox($"{behavior.cameraModTransform.name} is not in the present " +
                                            $"on nor inside of the spawnable hierarchy...", MessageType.Error);
                }
                if (!ValidateSubSync(behavior, behavior.cameraModTransform)) {
                    EditorGUILayout.HelpBox($"The transform you slotted is not a sub-sync, so it will not be " +
                                            $"synced for other players :(", MessageType.Warning);
                }
            }
            EditorGUILayout.HelpBox($"You can slot a transform here, this transform will be set to the position and " +
                                    $"rotation of the camera when the camera mod is opened and controlling this mario.", MessageType.Info);

            EditorGUILayout.PropertyField(cameraModTransformRenderersToHide);
            foreach (var renderer in behavior.cameraModTransformRenderersToHide) {
                if (renderer == null) {
                    EditorGUILayout.HelpBox($"You have a null renderer in the renderer list...", MessageType.Error);
                    continue;
                }
                if (!renderer.transform.IsChildOf(behavior.spawnable.transform)) {
                    EditorGUILayout.HelpBox($"The Renderer in {renderer.transform.name} is not in the present " +
                                            $"on nor inside of the spawnable hierarchy...", MessageType.Error);
                }
            }
            EditorGUILayout.HelpBox($"You can slots renderers here that you want to hide from the CVR Camera " +
                                    $"when you are controlling using the CVR Camera.", MessageType.Info);

            EditorGUILayout.Separator();
        }

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

    private static void SetupSpawnable(CVRSM64Mario behavior) {

        // Missing spawnable -> Create it
        if (behavior.spawnable == null) {
            behavior.spawnable = behavior.gameObject.AddComponent<CVRSpawnable>();
        }

        // Configure basic spawnable settings
        behavior.spawnable.spawnHeight = 0.2f;
        behavior.spawnable.useAdditionalValues = true;

        // Add each synced value to the spawnable
        foreach (var inputName in _syncedParameters) {

            var parameter = behavior.spawnable.syncValues.FirstOrDefault(value => value.name == inputName.Key);

            // Missing parameter -> Create it
            if (parameter == null) {
                var syncedValue = new CVRSpawnableValue {
                    name = inputName.Key
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

    private static bool ValidateSubSync(CVRSM64Mario behavior, Transform transform) {
        foreach (var subSync in behavior.spawnable.subSyncs) {
            if (subSync.transform != transform) continue;

            if (subSync.precision != CVRSpawnableSubSync.SyncPrecision.Full) {
                var err = $"We recommend setting the {nameof(CVRSpawnable)} sub-sync for the mario " +
                          $"transform to Full. Currently set to: " +
                          $"{Enum.GetName(typeof(CVRSpawnableSubSync.SyncPrecision), subSync.precision)}...";
                EditorGUILayout.HelpBox(err, MessageType.Warning);
            }

            foreach (var syncFlag in (CVRSpawnableSubSync.SyncFlags[])Enum.GetValues(typeof(CVRSpawnableSubSync.SyncFlags))) {
                if (subSync.syncedValues.HasFlag(syncFlag)) continue;
                var err = $"We need the sub-sync for the mario transform to sync EVERYTHING. Currently missing: {Enum.GetName(typeof(CVRSpawnableSubSync.SyncFlags), syncFlag)}";
                EditorGUILayout.HelpBox(err, MessageType.Error);
                if (Application.isPlaying) throw new Exception(err);
                return false;
            }
            return true;
        }
        return false;
    }

    private static bool ValidateSpawnable(CVRSM64Mario behavior) {

        var parentSpawnable = behavior.gameObject.GetComponentInParent<CVRSpawnable>();

        if (behavior.spawnable == null) {

            if (parentSpawnable == null) {
                const string err = $"{nameof(CVRSM64Mario)} requires a {nameof(CVRSpawnable)} defined!";
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
            if (ValidateSubSync(behavior, behavior.transform)) hasMarioSubSync = true;
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
            const string err = $"{nameof(CVRSM64Mario)} requires to be placed either on the game object that has" +
                               $" the {nameof(CVRSpawnable)} or on a transform that is set as a sub-sync.";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return false;
        }

        foreach (var marioSyncedInput in _syncedParameters) {
            var parameter = behavior.spawnable.syncValues.FirstOrDefault(value => value.name == marioSyncedInput.Key);
            if (parameter != null) continue;
            var err = $"{nameof(CVRSM64Mario)} requires a {nameof(CVRSpawnable)} with a synced value named: {marioSyncedInput.Key}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return false;
        }

        return true;
    }

    private static bool ValidateAnimators(CVRSM64Mario behavior) {

        var possibleAnimators = behavior.spawnable.GetComponentsInChildren<Animator>(true);

        foreach (var animator in behavior.animators) {

            // Animator is null
            if (animator == null) {
                const string err2 = $"{nameof(CVRSM64Mario)} There is an empty animator in the animator's list...";
                EditorGUILayout.HelpBox(err2, MessageType.Error);
                if (Application.isPlaying) throw new Exception(err2);
                return false;
            }

            // Check if it's a possible animator
            if (!possibleAnimators.Contains(animator)) {
                var err = $"{nameof(CVRSM64Mario)} the {animator.gameObject.name} animator is outside of the spawnable hierarchy...";
                EditorGUILayout.HelpBox(err, MessageType.Error);
                if (Application.isPlaying) throw new Exception(err);
                return false;
            }

            // Animator controller is null
            if (animator.runtimeAnimatorController == null) {
                var err2 = $"{nameof(CVRSM64Mario)} There is an animator on {animator.gameObject.name} game object that has no animator controller...";
                EditorGUILayout.HelpBox(err2, MessageType.Error);
                if (Application.isPlaying) throw new Exception(err2);
                return false;
            }

            var animatorController = GetAnimatorController(animator);

            foreach (var localParameter in _localParameters) {

                var matchedName = animatorController.parameters.FirstOrDefault(controllerParameter => controllerParameter.name == localParameter.Key);

                // Missing the parameter name
                if (matchedName == null) {
                    var err1 = $"{nameof(CVRSM64Mario)} {animator.gameObject.name}'s animator is missing a parameters named {localParameter.Key} of the type {localParameter.Value.ToString()}!";
                    EditorGUILayout.HelpBox(err1, MessageType.Error);
                    if (Application.isPlaying) throw new Exception(err1);
                    return false;
                }

                // Wrong parameter type
                if (matchedName.type != localParameter.Value) {
                    var err1 = $"{nameof(CVRSM64Mario)} {animator.gameObject.name}'s animator parameter {localParameter.Key} has the wrong type, it is: {matchedName.type.ToString()} but should be: {localParameter.Value.ToString()}!";
                    EditorGUILayout.HelpBox(err1, MessageType.Error);
                    if (Application.isPlaying) throw new Exception(err1);
                    return false;
                }
            }
        }

        return true;
    }

    private static AnimatorController GetAnimatorController(Animator animator) {
        var runtimeController = animator.runtimeAnimatorController;
        if (runtimeController.GetType() == typeof(AnimatorOverrideController)) {
            var overrideController = (AnimatorOverrideController)runtimeController;
            runtimeController = overrideController.runtimeAnimatorController;
        }
        return (AnimatorController) AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(runtimeController), typeof(AnimatorController));
    }

    private static void SetupAnimators(CVRSM64Mario behavior) {

        // Remove null animators or animators with no controllers
        behavior.animators.RemoveAll(item => item == null || item.runtimeAnimatorController == null);

        foreach (var animator in behavior.animators) {

            var animatorController = GetAnimatorController(animator);

            foreach (var localParameter in _localParameters) {
                var matchedName = animatorController.parameters.FirstOrDefault(controllerParameter => controllerParameter.name == localParameter.Key);

                // Missing the parameter name -> Make new
                if (matchedName == null) {
                    animatorController.AddParameter(localParameter.Key, localParameter.Value);
                    continue;
                }

                // Wrong parameter type
                if (matchedName.type != localParameter.Value) {
                    animatorController.RemoveParameter(matchedName);
                    animatorController.AddParameter(localParameter.Key, localParameter.Value);
                }
            }
        }
    }
}
