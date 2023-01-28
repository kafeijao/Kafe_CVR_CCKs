using ABI.CCK.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace EyeMovementFix.CCK {

    public class EyeRotationLimits : MonoBehaviour {

        // Actual Rotation Limits (present in-game)
        public float LeftEyeMinX = -25;
        public float LeftEyeMaxX = 25;
        public float LeftEyeMinY = -25;
        public float LeftEyeMaxY = 25;
        
        public float RightEyeMinX = -25;
        public float RightEyeMaxX = 25;
        public float RightEyeMinY = -25;
        public float RightEyeMaxY = 25;


        // Inspector Saved Fields (not present in-game)
        public float lMinY = 25f;
        public float lMaxX = 25f;
        public float lMinX = 25f;
        public float lMaxY = 25f;

        public float rMinY = 25f;
        public float rMaxX = 25f;
        public float rMinX = 25f;
        public float rMaxY = 25f;

        public bool individualEyeRotations;
    }

    [CustomEditor(typeof(EyeRotationLimits))]
    public class EyeRotationLimitsEditor : Editor {

        private float _xValue, _yValue;
        private bool _isPreviewing;

        // Eye angle values
        private Vector2 _leftEyeAngle, _rightEyeAngle;

        private static BetterEye _leftEye = new() { IsLeft = true };
        private static BetterEye _rightEye = new() { IsLeft = false };

        // Eye Game Object
        private static GameObject _eyesContainer;
        private static Quaternion _leftEyeOriginalLocalRotation;
        private static bool _setLeftEyeOriginalLocalRotation;
        private static Quaternion _rightEyeOriginalLocalRotation;
        private static bool _setRightEyeOriginalLocalRotation;

        private class BetterEye {
            public bool IsLeft;
            public Transform RealEye;
            public Transform FakeEye;
            public Transform FakeEyeWrapper;
            public Transform FakeEyeViewpointOffset;
        }

        private static void CreateFake(BetterEye eye) {

            // Create the viewpoint offset parent, this is needed when the parent of the real eye is not aligned with the viewpoint
            // This way we keep the rotation offset, allowing the looking forward of the eye wrapper to be local rotation = Quaternion.identity
            var fakeEyeBallViewpointOffset = new GameObject($"[EyeMovementFix] Fake{(eye.IsLeft ? "Left" : "Right")}EyeViewpointOffset");
            var viewpointOffsetEye = fakeEyeBallViewpointOffset.transform;

            // Create a parent constraint (we're not gonna parent the eye)
            //var rotationOffset = Quaternion.Inverse(viewpoint.rotation) * eye.RealEye.parent.rotation;
            var source = new ConstraintSource();
            viewpointOffsetEye.SetParent(_eyesContainer.transform, true);
            viewpointOffsetEye.position = eye.RealEye.position;
            source.sourceTransform = eye.RealEye.parent;
            source.weight = 1f;

            var constraint = viewpointOffsetEye.gameObject.AddComponent<ParentConstraint>();
            constraint.AddSource(source);
            //constraint.SetRotationOffset(0, rotationOffset.eulerAngles);
            constraint.constraintActive = true;

            // Create the in-between fake eye ball wrapper
            var fakeEyeBallWrapper = new GameObject($"[EyeMovementFix] Fake{(eye.IsLeft ? "Left" : "Right")}EyeWrapper");
            var wrapperEye = fakeEyeBallWrapper.transform;

            wrapperEye.SetParent(viewpointOffsetEye, true);
            wrapperEye.localScale = Vector3.one;
            wrapperEye.localPosition = Vector3.zero;
            wrapperEye.localRotation = Quaternion.identity;

            // Create the in-between fake eye ball, copying the eye initial local rotation
            var fakeEyeBall = new GameObject($"[EyeMovementFix] Fake{(eye.IsLeft ? "Left" : "Right")}Eye");
            var fakeEye = fakeEyeBall.transform;

            fakeEye.SetParent(wrapperEye, true);
            fakeEye.localScale = Vector3.one;
            fakeEye.localPosition = Vector3.zero;

            // Default to the current real eye rotation
            fakeEye.rotation = eye.RealEye.rotation;

            eye.FakeEyeViewpointOffset = viewpointOffsetEye;
            eye.FakeEyeWrapper = wrapperEye;
            eye.FakeEye = fakeEye;
        }


        private const string EyesContainerGameObjectName = "[EyeMovementFix] [SafeToDelete] Previewing...";
        private static bool _destroyedContainer;

        private static void DestroyPreview() {
            // Get Previewing container
            if (_eyesContainer != null) {
                DestroyImmediate(_eyesContainer);
                _eyesContainer = null;
            }
            else {
                var leftovers= GameObject.Find(EyesContainerGameObjectName);
                if (leftovers) {
                    DestroyImmediate(leftovers);
                }
            }
        }

        private static void Initialize(CVRAvatar avatar, Transform leftRealEye, Transform rightRealEye) {

            // Get Previewing container
            if (_eyesContainer == null) {
                _eyesContainer = GameObject.Find(EyesContainerGameObjectName);
                DestroyImmediate(_eyesContainer);
                _eyesContainer = null;
            }

            // Create Previewing container
            if (_eyesContainer == null) {
                // Reset the position to the origin, and rotate the game object the same as the avatar
                _eyesContainer = new GameObject(EyesContainerGameObjectName) { transform = {
                    position = Vector3.zero,
                    rotation = avatar.transform.rotation,
                } };

                // Create the fake left eye
                if (leftRealEye != null) {
                    _leftEyeOriginalLocalRotation = leftRealEye.localRotation;
                    _setLeftEyeOriginalLocalRotation = true;
                    _leftEye = new BetterEye { IsLeft = true, RealEye = leftRealEye };
                    CreateFake(_leftEye);
                }

                // Create the fake right eye
                if (rightRealEye != null) {
                    _rightEyeOriginalLocalRotation = rightRealEye.localRotation;
                    _setRightEyeOriginalLocalRotation = true;
                    _rightEye = new BetterEye { IsLeft = false, RealEye = rightRealEye };
                    CreateFake(_rightEye);
                }
            }

            _destroyedContainer = false;
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }
        private void OnDisable() {
            _isPreviewing = false;
            StopPreview();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                // Perform actions before exiting play mode
            } else if (state == PlayModeStateChange.ExitingEditMode) {
                _isPreviewing = false;
                StopPreview();
                // Perform actions before entering play mode
            }
        }

        public void StopPreview() {

            _xValue = 0f;
            _yValue = 0f;
            _leftEyeAngle.x = _leftEyeAngle.y = _rightEyeAngle.x = _rightEyeAngle.y = 0f;

            if (!_destroyedContainer) {

                // Reset the eyes pre-preview rotation
                if (_setLeftEyeOriginalLocalRotation) {
                    _leftEye.RealEye.localRotation = _leftEyeOriginalLocalRotation;
                    _setLeftEyeOriginalLocalRotation = false;
                }
                if (_setRightEyeOriginalLocalRotation) {
                    _rightEye.RealEye.localRotation = _rightEyeOriginalLocalRotation;
                    _setRightEyeOriginalLocalRotation = false;
                }

                DestroyPreview();
                _destroyedContainer = true;
            }
        }

        public override void OnInspectorGUI() {

            if (Application.isPlaying) {
                EditorGUILayout.HelpBox("This script doesn't work in play mode!", MessageType.Warning);
                return;
            }

            var eyeRotationLimits = (EyeRotationLimits) target;

            // Get the CVR Avatar component on the same GameObject
            var avatar = eyeRotationLimits.GetComponent<CVRAvatar>();

            // Get the Animator component on the same GameObject
            var animator = eyeRotationLimits.GetComponent<Animator>();

            // Perform validations

            if (avatar == null) {
                EditorGUILayout.HelpBox("CVR Avatar component is missing on the GameObject. Make sure you're " +
                                        "attaching this script to the root of the avatar!", MessageType.Warning);
                return;
            }
            if (animator == null) {
                EditorGUILayout.HelpBox("Animator component is missing on the GameObject. Make sure you're " +
                                        "attaching this script to the root of the avatar!", MessageType.Warning);
                return;
            }

            if (!animator.isHuman) {
                EditorGUILayout.HelpBox("The Animator component is not set up with a Humanoid avatar. This " +
                                        "script only works for humanoid rigs!", MessageType.Warning);
                return;
            }

            // Get the eye bones from the Animator component
            var realLeftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            var realRightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);

            // check if the eyes bones are not set properly in the FBX
            if (realLeftEye == null || realRightEye == null) {
                EditorGUILayout.HelpBox("The left or right Eye bones are not set properly in FBX's rig. CVR " +
                                        "uses the FBX's rig to get the eye bones!", MessageType.Warning);
                return;
            }

            // We're all good, let's go to business!

            GUILayoutOption[] eyeAngleBoxOptions = new GUILayoutOption[] {
                //GUILayout.ExpandHeight(false),
                //GUILayout.ExpandWidth(false),
                GUILayout.Width(75),
                //GUILayout.Height(20),
            };

            EditorGUILayout.Separator();

            eyeRotationLimits.individualEyeRotations = EditorGUILayout.Toggle("Different Limits for each eye", eyeRotationLimits.individualEyeRotations);

            EditorGUILayout.LabelField($"{(eyeRotationLimits.individualEyeRotations ? "Left" : "")}Eye Angle Limits:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            eyeRotationLimits.lMinY = Mathf.Abs(EditorGUILayout.FloatField(eyeRotationLimits.lMinY, eyeAngleBoxOptions));
            eyeRotationLimits.LeftEyeMinY = -eyeRotationLimits.lMinY;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            eyeRotationLimits.lMaxX = Mathf.Abs(EditorGUILayout.FloatField(eyeRotationLimits.lMaxX, eyeAngleBoxOptions));
            eyeRotationLimits.LeftEyeMaxX = eyeRotationLimits.lMaxX;

            GUILayout.FlexibleSpace();

            eyeRotationLimits.lMinX = Mathf.Abs(EditorGUILayout.FloatField(eyeRotationLimits.lMinX, eyeAngleBoxOptions));
            eyeRotationLimits.LeftEyeMinX = -eyeRotationLimits.lMinX;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            eyeRotationLimits.lMaxY = Mathf.Abs(EditorGUILayout.FloatField(eyeRotationLimits.lMaxY, eyeAngleBoxOptions));
            eyeRotationLimits.LeftEyeMaxY = eyeRotationLimits.lMaxY;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (eyeRotationLimits.individualEyeRotations) {

                EditorGUILayout.Separator();

                EditorGUILayout.LabelField($"Right Eye Angle Limits:", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                eyeRotationLimits.rMinY = Mathf.Abs(EditorGUILayout.FloatField(eyeRotationLimits.rMinY, eyeAngleBoxOptions));
                eyeRotationLimits.RightEyeMinY = -eyeRotationLimits.rMinY;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                eyeRotationLimits.rMaxX = Mathf.Abs(EditorGUILayout.FloatField(eyeRotationLimits.rMaxX, eyeAngleBoxOptions));
                eyeRotationLimits.RightEyeMaxX = eyeRotationLimits.rMaxX;

                GUILayout.FlexibleSpace();

                eyeRotationLimits.rMinX = Mathf.Abs(EditorGUILayout.FloatField(eyeRotationLimits.rMinX, eyeAngleBoxOptions));
                eyeRotationLimits.RightEyeMinX = -eyeRotationLimits.rMinX;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                eyeRotationLimits.rMaxY = Mathf.Abs(EditorGUILayout.FloatField(eyeRotationLimits.rMaxY, eyeAngleBoxOptions));
                eyeRotationLimits.RightEyeMaxY = eyeRotationLimits.rMaxY;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }
            else {
                eyeRotationLimits.RightEyeMinX = -eyeRotationLimits.lMaxX;
                eyeRotationLimits.RightEyeMaxX = eyeRotationLimits.lMinX;
                eyeRotationLimits.RightEyeMinY = -eyeRotationLimits.lMinY;
                eyeRotationLimits.RightEyeMaxY = eyeRotationLimits.lMaxY;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("The values are relative to the avatar looking forward in unity, not the actual rotation of the eyes.", MessageType.Info);

            EditorGUILayout.Separator();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            _isPreviewing = EditorGUILayout.Toggle("Preview Eye Rotations", _isPreviewing);

            if (_isPreviewing) {
                //_color = EditorGUILayout.ColorField(new GUIContent("Preview Controller"), _color, false, false, false);

                var sliderHOptions = new [] { GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false), GUILayout.Width(200), GUILayout.Height(200) };
                var sliderVOptions = new [] { GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false), GUILayout.Width(200), GUILayout.Height(180) };

                EditorGUILayout.BeginHorizontal();
                _xValue = GUILayout.HorizontalSlider(_xValue, 1, -1, sliderHOptions);

                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                _yValue = GUILayout.VerticalSlider(_yValue, -1, 1, sliderVOptions);
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // Multiply with the selected limits
                _leftEyeAngle.x = _xValue < 0 ? eyeRotationLimits.LeftEyeMinX * -_xValue : eyeRotationLimits.LeftEyeMaxX * _xValue;
                _leftEyeAngle.y = _yValue < 0 ? eyeRotationLimits.LeftEyeMinY * -_yValue : eyeRotationLimits.LeftEyeMaxY * _yValue;
                _rightEyeAngle.x = _xValue < 0 ? eyeRotationLimits.RightEyeMinX * -_xValue : eyeRotationLimits.RightEyeMaxX * _xValue;
                _rightEyeAngle.y = _yValue < 0 ? eyeRotationLimits.RightEyeMinY * -_yValue : eyeRotationLimits.RightEyeMaxY * _yValue;

                #if DEBUG

                // Format the float values to display with 2 decimal points
                EditorGUILayout.LabelField("X Value: " + _xValue.ToString("F2"));
                EditorGUILayout.LabelField("Y Value: " + _yValue.ToString("F2"));

                EditorGUILayout.Space();

                // Use the bold label style for the section titles

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Left Eye Angles:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(_rightEyeAngle.ToString("F2"));
                EditorGUILayout.EndHorizontal();

                // Add some spacing between the sections
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Right Eye Angles:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(_leftEyeAngle.ToString("F2"));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                #endif

                Initialize(avatar, realLeftEye, realRightEye);


                // Rotate the fake eye wrapper
                _leftEye.FakeEyeWrapper.localRotation = Quaternion.Euler(_leftEyeAngle.y, _leftEyeAngle.x, 0f);
                _rightEye.FakeEyeWrapper.localRotation = Quaternion.Euler(_rightEyeAngle.y, _rightEyeAngle.x, 0f);

                // Query the fake eye position that has the initial offset and apply to the real eyes
                _leftEye.RealEye.rotation = _leftEye.FakeEye.rotation;
                _rightEye.RealEye.rotation = _rightEye.FakeEye.rotation;
            }
            else {
                StopPreview();
            }
        }
    }
}
