using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

using ABI.CCK.Components;
using RetroCVR.Properties;

namespace Kafe.RetroCVR.CCK;

[InitializeOnLoad]
public static class RetroCVRCoreInitializer {
    static RetroCVRCoreInitializer() {
        const string symbol = "KAFE_RETRO_CVR_EXISTS";
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (defines.Contains(symbol)) return;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, $"{defines};{symbol}");
        Debug.Log($"Added {symbol} Scripting Symbol.");
    }
}

[DisallowMultipleComponent]
public class RetroCVRCore : MonoBehaviour {

    [SerializeField] public string version = AssemblyInfoParams.Version;

    [SerializeField] public string coreName;
    
}

[CanEditMultipleObjects]
[CustomEditor(typeof(RetroCVRCore))]
public class RetroCVRCoreEditor : Editor {

    SerializedProperty coreName;

    private void OnEnable() {
        coreName = serializedObject.FindProperty("coreName");
    }

    public override void OnInspectorGUI() {

        if (Application.isPlaying) {
            EditorGUILayout.HelpBox("You can't edit this script play mode!", MessageType.Warning);
            return;
        }

        serializedObject.Update();

        // var behavior = (RetroCVRCore) target;
        // EditorGUILayout.LabelField("Base Setup", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(coreName);
        var coreNameObject = coreName.stringValue;
        if (string.IsNullOrEmpty(coreNameObject)) {
            EditorGUILayout.HelpBox("The Core Name needs to be set!", MessageType.Error);
            return;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
