using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

[RequireComponent(typeof(CVRSM64InputSpawnable))]
public class CVRSM64CMario : MonoBehaviour {
    [SerializeField] internal Material material;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64CMario))]
public class CVRSM64CMarioEditor : Editor {

    SerializedProperty material;

    void OnEnable() {
        material = serializedObject.FindProperty("material");
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.PropertyField(material);
        serializedObject.ApplyModifiedProperties();

        var behavior = (CVRSM64CMario) target;

        if (behavior.material != null && behavior.material.shader != null) {
            var hasMainTex = false;
            var propertyCount = ShaderUtil.GetPropertyCount(behavior.material.shader);
            for (var i = 0; i < propertyCount; i++) {
                if (ShaderUtil.GetPropertyName(behavior.material.shader, i) != "_MainTex") continue;
                hasMainTex = true;
                break;
            }

            if (hasMainTex) {
                EditorGUILayout.HelpBox("The material's _MainTex will be replaced by our own texture.", MessageType.Info);
            }
            else {
                var err = "The material's shader does not have a _MainTex property.";
                EditorGUILayout.HelpBox(err, MessageType.Error);
                if (Application.isPlaying) throw new Exception(err);
                return;
            }
        }
    }
}
