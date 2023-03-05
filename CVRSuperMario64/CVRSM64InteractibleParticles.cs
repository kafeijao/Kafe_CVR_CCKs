using UnityEditor;
using UnityEngine;

namespace Kafe.CVRSuperMario64;

[RequireComponent(typeof(ParticleSystem))]
public class CVRSM64InteractableParticles : MonoBehaviour {

    public enum ParticleType {
        GoldCoin,
        BlueCoin,
        RedCoin,
    }

    [SerializeField] private ParticleType particleType = ParticleType.GoldCoin;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(CVRSM64InteractableParticles))]
public class CVRSM64InteractableParticlesEditor : Editor {

    private const string MarioParticleTargetName = "[MarioParticleTargetName]";

    SerializedProperty particleType;

    private void OnEnable() {
        particleType = serializedObject.FindProperty("particleType");
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.PropertyField(particleType);
        serializedObject.ApplyModifiedProperties();

        var behavior = (CVRSM64InteractableParticles)target;

        // Todo: Validate the particle system ?

        EditorGUILayout.HelpBox($"This component needs to be on the same game object as a particle system!", MessageType.Info);
        EditorGUILayout.HelpBox($"The particles need to collide with the mario, so make sure you enable collision.", MessageType.Info);
        EditorGUILayout.HelpBox($"The Coin effects will only trigger when colliding with a game object named {MarioParticleTargetName}", MessageType.Info);
    }
}
