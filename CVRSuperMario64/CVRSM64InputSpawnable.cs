using ABI.CCK.Components;
using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

public class CVRSM64InputSpawnable : MonoBehaviour {
    [SerializeField] public CVRSpawnable Spawnable;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64InputSpawnable))]
public class CVRSM64InputSpawnableEditor : Editor {

    SerializedProperty Spawnable;

    void OnEnable() {
        Spawnable = serializedObject.FindProperty("Spawnable");
    }

    // Inputs
    private CVRSpawnableValue _inputHorizontal;
    private CVRSpawnableValue _inputVertical;
    private CVRSpawnableValue _inputJump;
    private CVRSpawnableValue _inputKick;
    private CVRSpawnableValue _inputStomp;

    private void LoadInput(CVRSM64InputSpawnable behavior, ref CVRSpawnableValue parameter, string inputName) {
        parameter = behavior.Spawnable.syncValues.FirstOrDefault(value => value.name == inputName);
        if (parameter == null) {
            var err = $"{nameof(CVRSM64InputSpawnable)} requires a {nameof(CVRSpawnable)} with a synced value named: {inputName}";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
        }
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.PropertyField(Spawnable);
        serializedObject.ApplyModifiedProperties();

        var behavior = (CVRSM64InputSpawnable) target;

        if (behavior.Spawnable == null) {
            var err = $"{nameof(CVRSM64InputSpawnable)} requires a {nameof(CVRSpawnable)} defined!";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        var rootSpawnable = behavior.GetComponent<CVRSpawnable>();

        var hasMarioSubSync = false;

        if (rootSpawnable == null) {
            foreach (var subSync in behavior.Spawnable.subSyncs) {
                if (subSync.transform == behavior.transform) {
                    if (subSync.precision != CVRSpawnableSubSync.SyncPrecision.Full) {
                        var err = $"We recommend setting the {nameof(CVRSpawnable)} sub-sync for the mario " +
                                  $"transform to Full. Currently set to: " +
                                  $"{Enum.GetName(typeof(CVRSpawnableSubSync.SyncPrecision), subSync.precision)}...";
                        EditorGUILayout.HelpBox(err, MessageType.Warning);
                    }

                    foreach (var syncFlag in (CVRSpawnableSubSync.SyncFlags[])Enum.GetValues(typeof(CVRSpawnableSubSync.SyncFlags))) {
                        if (!subSync.syncedValues.HasFlag(syncFlag)) {
                            var err = $"We need the sub-sync for the mario transform to sync EVERYTHING. Currently missing: {Enum.GetName(typeof(CVRSpawnableSubSync.SyncFlags), syncFlag)}";
                            EditorGUILayout.HelpBox(err, MessageType.Error);
                            if (Application.isPlaying) throw new Exception(err);
                            return;
                        }
                    }

                    hasMarioSubSync = true;
                }
            }
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
            var err = $"{nameof(CVRSM64InputSpawnable)} requires to be placed either on the game object that has" +
                      $" the {nameof(CVRSpawnable)} or on a transform that is set as a sub-sync.";
            EditorGUILayout.HelpBox(err, MessageType.Error);
            if (Application.isPlaying) throw new Exception(err);
            return;
        }

        // Load the spawnable inputs
        LoadInput(behavior, ref _inputHorizontal, "Horizontal");
        LoadInput(behavior, ref _inputVertical, "Vertical");
        LoadInput(behavior, ref _inputJump, "Jump");
        LoadInput(behavior, ref _inputKick, "Kick");
        LoadInput(behavior, ref _inputStomp, "Stomp");
    }
}
