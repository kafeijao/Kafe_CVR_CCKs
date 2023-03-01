using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

public class CVRSM64Interactable : MonoBehaviour {

    public enum InteractableType {
        VanishCap,
        MetalCap,
        WingCap,
    }

    [SerializeField] private InteractableType interactableType = InteractableType.MetalCap;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64Interactable))]
public class CVRSM64InteractableEditor : Editor {

    SerializedProperty interactableType;

    void OnEnable() {
        interactableType = serializedObject.FindProperty("interactableType");
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.PropertyField(interactableType);
        serializedObject.ApplyModifiedProperties();

        var behavior = (CVRSM64Interactable)target;
    }
}
