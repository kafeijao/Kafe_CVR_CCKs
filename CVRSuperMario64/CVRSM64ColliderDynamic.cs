using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

public class CVRSM64ColliderDynamic : MonoBehaviour {
    [SerializeField] SM64TerrainType terrainType = SM64TerrainType.Grass;
    [SerializeField] SM64SurfaceType surfaceType = SM64SurfaceType.Default;

    public SM64TerrainType TerrainType => terrainType;
    public SM64SurfaceType SurfaceType => surfaceType;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64ColliderDynamic))]
public class CVRSM64ColliderDynamicEditor : Editor {

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

        var behavior = (CVRSM64ColliderDynamic) target;

        var colliders = behavior.GetComponents<Collider>();
        var allowedCollidersString = string.Join(", ", AllowedColliderTypes.Select(i => i.ToString()));

        if (colliders.Length <= 0) {
            var err = $"The {nameof(CVRSM64ColliderDynamic)} component requires a collider. Allowed types: {allowedCollidersString}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }
        else if (colliders.Length > 1) {
            var err = $"The {nameof(CVRSM64ColliderDynamic)} component can only have a single collider. Allowed types: {allowedCollidersString}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        var collider = colliders[0];

        if (collider.isTrigger) {
            var err = $"The {nameof(CVRSM64ColliderDynamic)}'s collider can NOT be set as Trigger!";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        EditorGUILayout.HelpBox(
            $"Only use this component on colliders where the game object will move. " +
            $"Otherwise use the {nameof(CVRSM64ColliderStatic)} component instead. A dynamic collider way heavier " +
            $"than a static collider performance wise.",
            MessageType.Info);
    }
}
