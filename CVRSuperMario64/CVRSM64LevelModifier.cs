using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

public class CVRSM64LevelModifier : MonoBehaviour {

    public enum ModifierType {
        Water,
        Gas,
    }

    [SerializeField] internal List<Animator> animators = new();
    [SerializeField] internal ModifierType modifierType = ModifierType.Water;
}


[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64LevelModifier))]
public class CVRSM64LevelModifierEditor : Editor {

    private static readonly Dictionary<string, AnimatorControllerParameterType> _localParameters = new() {
        {"IsActive", AnimatorControllerParameterType.Bool},
        {"HasMod", AnimatorControllerParameterType.Bool},
    };

    SerializedProperty animators;
    SerializedProperty modifierType;

    private void OnEnable() {
        animators = serializedObject.FindProperty("animators");
        modifierType = serializedObject.FindProperty("modifierType");
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.PropertyField(animators);
        EditorGUILayout.PropertyField(modifierType);
        serializedObject.ApplyModifiedProperties();

        var behavior = (CVRSM64LevelModifier)target;

        // Animators
        if (!ValidateAnimators(behavior)) {
            if (GUILayout.Button(new GUIContent(
                    "Attempt to Auto-Setup Animators", "This will add/modify the type of the parameters required in the animators selected."))) {
                SetupAnimators(behavior);
            }
        }

        EditorGUILayout.HelpBox($"The position of this object will set the level which the mario engine will set the {behavior.modifierType.ToString()}!", MessageType.Info);
        EditorGUILayout.HelpBox($"There can only be one active object of this type, and the one picked will be the one that stands the highest", MessageType.Info);
        EditorGUILayout.HelpBox($"These animators will have the parameters {string.Join(", ", _localParameters.Keys)} managed by the mod!", MessageType.Info);
    }

    private static bool ValidateAnimators(CVRSM64LevelModifier behavior) {

        foreach (var animator in behavior.animators) {

            // Animator is null
            if (animator == null) {
                const string err2 = $"{nameof(CVRSM64Mario)} There is an empty animator in the animator's list...";
                EditorGUILayout.HelpBox(err2, MessageType.Error);
                if (Application.isPlaying) throw new Exception(err2);
                return false;
            }

            // Animator controller is null
            if (animator.runtimeAnimatorController == null) {
                var err2 =
                    $"{nameof(CVRSM64Mario)} There is an animator on {animator.gameObject.name} game object that has no animator controller...";
                EditorGUILayout.HelpBox(err2, MessageType.Error);
                if (Application.isPlaying) throw new Exception(err2);
                return false;
            }

            var animatorController = GetAnimatorController(animator);

            foreach (var localParameter in _localParameters) {

                var matchedName = animatorController.parameters.FirstOrDefault(controllerParameter =>
                    controllerParameter.name == localParameter.Key);

                // Missing the parameter name
                if (matchedName == null) {
                    var err1 =
                        $"{nameof(CVRSM64Mario)} {animator.gameObject.name}'s animator is missing a parameters named {localParameter.Key} of the type {localParameter.Value.ToString()}!";
                    EditorGUILayout.HelpBox(err1, MessageType.Error);
                    if (Application.isPlaying) throw new Exception(err1);
                    return false;
                }

                // Wrong parameter type
                if (matchedName.type != localParameter.Value) {
                    var err1 =
                        $"{nameof(CVRSM64Mario)} {animator.gameObject.name}'s animator parameter {localParameter.Key} has the wrong type, it is: {matchedName.type.ToString()} but should be: {localParameter.Value.ToString()}!";
                    EditorGUILayout.HelpBox(err1, MessageType.Error);
                    if (Application.isPlaying) throw new Exception(err1);
                    return false;
                }
            }
        }

        return true;
    }

    private static void SetupAnimators(CVRSM64LevelModifier behavior) {

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

    private static AnimatorController GetAnimatorController(Animator animator) {
        var runtimeController = animator.runtimeAnimatorController;
        if (runtimeController.GetType() == typeof(AnimatorOverrideController)) {
            var overrideController = (AnimatorOverrideController)runtimeController;
            runtimeController = overrideController.runtimeAnimatorController;
        }
        return (AnimatorController) AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(runtimeController), typeof(AnimatorController));
    }
}
