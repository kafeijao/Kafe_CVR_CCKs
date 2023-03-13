using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

public class CVRSM64Teleporter : MonoBehaviour {
    [Tooltip("Determines whether the teleporter is currently active or not, you can animate this value.")]
    [SerializeField] internal bool isActive = true;

    [Tooltip("Determines whether the teleporter can be used in both directions, you can animate this value.")]
    [SerializeField] internal bool isTwoWays = false;

    [Tooltip("The transform of the object where the teleporter is located.")]
    [SerializeField] internal Transform sourcePoint = null;

    [Tooltip("The transform of the object where the player will be teleported.")]
    [SerializeField] internal Transform targetPoint = null;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64Teleporter))]
public class CVRSM64TeleporterEditor : Editor {

    SerializedProperty isActive;
    SerializedProperty isTwoWays;
    SerializedProperty sourcePoint;
    SerializedProperty targetPoint;

    private void OnEnable() {
        isActive = serializedObject.FindProperty("isActive");
        isTwoWays = serializedObject.FindProperty("isTwoWays");
        sourcePoint = serializedObject.FindProperty("sourcePoint");
        targetPoint = serializedObject.FindProperty("targetPoint");
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.PropertyField(isActive);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(sourcePoint);
        EditorGUILayout.PropertyField(targetPoint);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(isTwoWays);
        serializedObject.ApplyModifiedProperties();

        var behavior = (CVRSM64Teleporter) target;

        if (behavior.sourcePoint == null) {
            var err = $"The {nameof(CVRSM64Teleporter)} component requires a valid GameObject as the Source Point!";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        if (behavior.targetPoint == null) {
            var err = $"The {nameof(CVRSM64Teleporter)} component requires a valid GameObject as the Target Point!";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        EditorGUILayout.HelpBox(
            $"You CAN animate isActive via animation to enable/disable the teleporter. Disabling/Enabling the " +
            $"game object should also work!",
            MessageType.Info);

        EditorGUILayout.HelpBox(
            $"The target/source rotation also matter, if the pivot is set to Local in the unity view, the blue " +
            $"arrow represent where mario will be facing when teleports!",
            MessageType.Info);

        if (behavior.isTwoWays) {
            EditorGUILayout.HelpBox(
                $"Since Is Two Ways is enabled, this teleporter will work (as the name suggests) two ways! " +
                $"Teleporting from the source to target (when standing on the source) and from the target to the " +
                $"source (when standing on the target).",
                MessageType.Info);
        }
    }
}
