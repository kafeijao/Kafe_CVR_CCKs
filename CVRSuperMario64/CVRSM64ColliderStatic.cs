using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

public class CVRSM64ColliderStatic : MonoBehaviour {
    [SerializeField] SM64TerrainType terrainType = SM64TerrainType.Grass;
    [SerializeField] SM64SurfaceType surfaceType = SM64SurfaceType.Default;

    public SM64TerrainType TerrainType => terrainType;
    public SM64SurfaceType SurfaceType => surfaceType;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64ColliderStatic))]
public class CVRSM64ColliderStaticEditor : Editor {

    SerializedProperty terrainType;
    SerializedProperty surfaceType;

    void OnEnable() {
        terrainType = serializedObject.FindProperty("terrainType");
        surfaceType = serializedObject.FindProperty("surfaceType");
    }

    private Type[] AllowedColliderTypes = {
        typeof(BoxCollider),
        typeof(CapsuleCollider),
        typeof(MeshCollider),
        typeof(SphereCollider),
        typeof(TerrainCollider),
    };

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.PropertyField(terrainType);
        EditorGUILayout.PropertyField(surfaceType);
        serializedObject.ApplyModifiedProperties();

        var behavior = (CVRSM64ColliderStatic) target;

        var colliders = behavior.GetComponents<Collider>();
        var allowedCollidersString = string.Join(", ", AllowedColliderTypes.Select(i => i.ToString()));

        if (colliders.Length <= 0) {
            var err = $"The {nameof(CVRSM64ColliderStatic)} component requires a collider. Allowed types: {allowedCollidersString}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }
        else if (colliders.Length > 1) {
            var err = $"The {nameof(CVRSM64ColliderStatic)} component can only have a single collider. Allowed types: {allowedCollidersString}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        var collider = colliders[0];

        if (collider.isTrigger) {
            var err = $"The {nameof(CVRSM64ColliderStatic)}'s collider can NOT be set as Trigger!";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        EditorGUILayout.HelpBox(
            $"Only use this component on colliders that will not move, otherwise Marios will only use the " +
            $"collider initial position. For moving colliders use {nameof(CVRSM64ColliderDynamic)} component instead. " +
            $"Note that dynamic colliders have a heavier impact on performance.",
            MessageType.Info);
    }

}
