using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

public class CVRSM64ColliderDynamic : MonoBehaviour {

    [SerializeField] SM64TerrainType terrainType = SM64TerrainType.Grass;
    [SerializeField] SM64SurfaceType surfaceType = SM64SurfaceType.Default;

    // [SerializeField] private bool ignoreForSpawner = true;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64ColliderDynamic))]
public class CVRSM64ColliderDynamicEditor : Editor {

    SerializedProperty terrainType;
    SerializedProperty surfaceType;
    // SerializedProperty ignoreForSpawner;

    private void OnEnable() {
        terrainType = serializedObject.FindProperty("terrainType");
        surfaceType = serializedObject.FindProperty("surfaceType");
        // ignoreForSpawner = serializedObject.FindProperty("ignoreForSpawner");
    }

    private Type[] AllowedColliderTypes = {
        typeof(BoxCollider),
        typeof(CapsuleCollider),
        typeof(MeshCollider),
        typeof(SphereCollider),
        typeof(TerrainCollider),
    };

    public override void OnInspectorGUI() {

        // Check if it has a parent mario
        var behavior = (CVRSM64ColliderDynamic) target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(terrainType);
        EditorGUILayout.PropertyField(surfaceType);
        serializedObject.ApplyModifiedProperties();

        // Check if the collider is inside of a Mario Script
        if (behavior.GetComponentInParent<CVRSM64Mario>() != null) {
            // EditorGUILayout.Space();
            // EditorGUILayout.HelpBox(
            //     $"Since this {nameof(CVRSM64ColliderDynamic)} is nested in a {nameof(CVRSM64Mario)} component, " +
            //     $"you can pick whether this collider is disabled for person controlling the mario or not (avoids self" +
            //     $"collision with the mario).",
            //     MessageType.Info);
            // EditorGUILayout.PropertyField(ignoreForSpawner);
            var err = $"{nameof(CVRSM64ColliderDynamic)} shouldn't be nested in a {nameof(CVRSM64Mario)} component, as " +
                      $"this might result in self collisions. Don't ignore this warning unless you know what you're doing...";
            EditorGUILayout.HelpBox(err, MessageType.Warning);
        }

        var colliders = behavior.GetComponents<Collider>();
        var allowedCollidersString = string.Join(", ", AllowedColliderTypes.Select(i => i.ToString()));

        if (colliders.Length <= 0) {
            var err = $"The {nameof(CVRSM64ColliderDynamic)} component requires a collider. Allowed types: {allowedCollidersString}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        if (colliders.Length > 1) {
            var err = $"The {nameof(CVRSM64ColliderDynamic)} component can only have a single collider. Allowed types: {allowedCollidersString}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        EditorGUILayout.HelpBox(
            $"Only use this component on colliders where the game object will move. " +
            $"Otherwise use the {nameof(CVRSM64ColliderStatic)} component instead. A dynamic collider way heavier " +
            $"than a static collider performance wise.",
            MessageType.Info);

        EditorGUILayout.HelpBox(
            $"You CAN mark the collider as Trigger to avoid collisions with other unity stuff, even if " +
            $"the collider is set as Trigger it will still affect Mario.",
            MessageType.Info);
    }
}
