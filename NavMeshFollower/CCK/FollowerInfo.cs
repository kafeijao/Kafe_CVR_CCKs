using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

using ABI.CCK.Components;
using NavMeshFollower.Properties;

namespace Kafe.NavMeshFollower.CCK;

[InitializeOnLoad]
public static class FollowerInfoInitializer {
    static FollowerInfoInitializer() {
        const string symbol = "KAFE_CVR_CCK_NAV_MESH_FOLLOWER_EXISTS";
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (defines.Contains(symbol)) return;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, $"{defines};{symbol}");
        Debug.Log($"Added {symbol} Scripting Symbol.");
    }
}

public class FollowerInfo : MonoBehaviour {

    [SerializeField] public string version = AssemblyInfoParams.Version;

    [SerializeField] public CVRSpawnable spawnable;
    [SerializeField] public NavMeshAgent navMeshAgent;

    [SerializeField] public Animator humanoidAnimator;

    [SerializeField] public bool hasLookAt;
    [SerializeField] public Transform lookAtTargetTransform;
    [SerializeField] public Transform headTransform;

    [SerializeField] public bool hasVRIK;

    // VRIK Left Arm
    [SerializeField] public bool hasLeftArmIK;
    [SerializeField] public Transform vrikLeftArmTargetTransform;
    [SerializeField] public Transform leftHandAttachmentPoint;

    // VRIK Right Arm
    [SerializeField] public bool hasRightArmIK;
    [SerializeField] public Transform vrikRightArmTargetTransform;
    [SerializeField] public Transform rightHandAttachmentPoint;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(FollowerInfo))]
public class FollowerInfoEditor : Editor {

    SerializedProperty spawnable;
    SerializedProperty navMeshAgent;
    SerializedProperty humanoidAnimator;

    SerializedProperty hasLookAt;
    SerializedProperty lookAtTargetTransform;
    SerializedProperty headTransform;

    SerializedProperty hasLeftArmIK;
    SerializedProperty vrikLeftArmTargetTransform;
    SerializedProperty leftHandAttachmentPoint;

    SerializedProperty hasRightArmIK;
    SerializedProperty vrikRightArmTargetTransform;
    SerializedProperty rightHandAttachmentPoint;

    private void OnEnable() {

        spawnable = serializedObject.FindProperty("spawnable");
        navMeshAgent = serializedObject.FindProperty("navMeshAgent");
        humanoidAnimator = serializedObject.FindProperty("humanoidAnimator");

        hasLookAt = serializedObject.FindProperty("hasLookAt");
        lookAtTargetTransform = serializedObject.FindProperty("lookAtTargetTransform");
        headTransform = serializedObject.FindProperty("headTransform");

        hasLeftArmIK = serializedObject.FindProperty("hasLeftArmIK");
        vrikLeftArmTargetTransform = serializedObject.FindProperty("vrikLeftArmTargetTransform");
        leftHandAttachmentPoint = serializedObject.FindProperty("leftHandAttachmentPoint");

        hasRightArmIK = serializedObject.FindProperty("hasRightArmIK");
        vrikRightArmTargetTransform = serializedObject.FindProperty("vrikRightArmTargetTransform");
        rightHandAttachmentPoint = serializedObject.FindProperty("rightHandAttachmentPoint");
    }

    private bool ValidateComponent(string propertyName, Component component, Transform spawnableTransform, bool? shouldBeEnabled){
        if (component == null) {
            EditorGUILayout.HelpBox($"{propertyName} needs to be assigned!", MessageType.Error);
            return false;
        }
        var valid = true;
        if (!component.transform.IsChildOf(spawnableTransform)) {
            EditorGUILayout.HelpBox($"{propertyName} needs to be either on the same component or deeper in the hierarchy of the CVRSpawnable!", MessageType.Error);
            valid = false;
        }
        if (shouldBeEnabled.HasValue && component is MonoBehaviour behavior && behavior.enabled != shouldBeEnabled.Value) {
            var status = shouldBeEnabled.Value ? "enabled" : "disabled";
            EditorGUILayout.HelpBox($"{propertyName} component should be {status}!", MessageType.Error);
            valid = false;
        }
        return valid;
    }


    public override void OnInspectorGUI() {

        if (Application.isPlaying) {
            EditorGUILayout.HelpBox("You can't edit this script play mode!", MessageType.Warning);
            return;
        }

        serializedObject.Update();

        var behavior = (FollowerInfo) target;

        EditorGUILayout.LabelField("Base Setup", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(spawnable);
        var spawnableObject = spawnable.objectReferenceValue as CVRSpawnable;
        if (spawnableObject == null) {
            EditorGUILayout.HelpBox("The CVRSpawnable needs to be assigned!", MessageType.Error);
            return;
        }
        ValidateComponent(nameof(spawnable), spawnableObject, spawnableObject.transform, true);

        EditorGUILayout.PropertyField(navMeshAgent);
        ValidateComponent(nameof(navMeshAgent), navMeshAgent.objectReferenceValue as NavMeshAgent, spawnableObject.transform, false);

        EditorGUILayout.Space();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Look At Setup", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(hasLookAt);
        if (hasLookAt.boolValue) {

            EditorGUILayout.PropertyField(lookAtTargetTransform);
            ValidateComponent(nameof(lookAtTargetTransform), lookAtTargetTransform.objectReferenceValue as Transform, spawnableObject.transform, null);

            EditorGUILayout.PropertyField(headTransform);
            ValidateComponent(nameof(headTransform), headTransform.objectReferenceValue as Transform, spawnableObject.transform, null);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Humanoid IK Setup", EditorStyles.boldLabel);

        if (hasRightArmIK.boolValue || hasLeftArmIK.boolValue) {
            EditorGUILayout.PropertyField(humanoidAnimator);
            var animator = humanoidAnimator.objectReferenceValue as Animator;
            if (ValidateComponent(nameof(humanoidAnimator), animator, spawnableObject.transform, null)) {
                if (!animator!.isHuman) {
                    EditorGUILayout.HelpBox($"{nameof(humanoidAnimator)} needs to be a humanoid animator, as we need to get the hand transform references.", MessageType.Error);
                }
            }
            EditorGUILayout.HelpBox($"{nameof(humanoidAnimator)} You can add the bool parameter #SpawnedByMe to this animator, it will set by the mod to be true on followers you spawn.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(hasLeftArmIK);
        if (hasLeftArmIK.boolValue) {
            EditorGUILayout.PropertyField(vrikLeftArmTargetTransform);
            ValidateComponent(nameof(vrikLeftArmTargetTransform), vrikLeftArmTargetTransform.objectReferenceValue as Transform, spawnableObject.transform, null);
            EditorGUILayout.PropertyField(leftHandAttachmentPoint);
            ValidateComponent(nameof(leftHandAttachmentPoint), leftHandAttachmentPoint.objectReferenceValue as Transform, spawnableObject.transform, null);
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(hasRightArmIK);
        if (hasRightArmIK.boolValue) {
            EditorGUILayout.PropertyField(vrikRightArmTargetTransform);
            ValidateComponent(nameof(vrikRightArmTargetTransform), vrikRightArmTargetTransform.objectReferenceValue as Transform, spawnableObject.transform, null);
            EditorGUILayout.PropertyField(rightHandAttachmentPoint);
            ValidateComponent(nameof(rightHandAttachmentPoint), rightHandAttachmentPoint.objectReferenceValue as Transform, spawnableObject.transform, null);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
