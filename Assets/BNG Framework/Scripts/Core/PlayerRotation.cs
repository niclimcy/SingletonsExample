using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BNG {

    public enum RotationMechanic {
        Snap,
        Smooth
    }
    public class PlayerRotation : MonoBehaviour {

        [Header("Input")]
        [Tooltip("Set to false to skip Update")]
        public bool AllowInput = true;

        [Tooltip("Used to determine whether to turn left / right. This can be an X Axis on the thumbstick, for example. -1 to snap left, 1 to snap right.")]
        public List<InputAxis> inputAxis = new List<InputAxis>() { InputAxis.RightThumbStickAxis };

        [Tooltip("Unity Input Action used to rotate the player")]
        public InputActionReference RotateAction;

        [Header("Smooth / Snap Turning")]
        [Tooltip("Snap rotation will rotate a fixed amount of degrees on turn. Smooth will linearly rotate the player.")]
        public RotationMechanic RotationType = RotationMechanic.Snap;

        [Header("Snap Turn Settings")]
        [Tooltip("How many degrees to rotate if RotationType is set to 'Snap'")]
        public float SnapRotationAmount = 45f;

        [Tooltip("Thumbstick X axis must be >= this amount to be considered an input event")]
        public float SnapInputAmount = 0.75f;

        [Header("Smooth Turn Settings")]
        [Tooltip("How fast to rotate the player if RotationType is set to 'Smooth'")]
        public float SmoothTurnSpeed = 40f;

        [Tooltip("Thumbstick X axis must be >= this amount to be considered an input event")]
        public float SmoothTurnMinInput = 0.1f;

        float recentSnapTurnTime;        

        /// <summary>
        /// How much to rotate this frame
        /// </summary>
        float rotationAmount = 0;

        float xAxis;
        float previousXInput;

        //added
        [Header("Screen Fade")]
        [Tooltip("Use ScreenFader on teleportation if true.")]
        public bool FadeScreenOnTeleport = true;

        [Tooltip("If FadeScreenOnTeleport = true, fade the screen at this speed.")]
        public float TeleportFadeSpeed = 10f;

        [Tooltip("Seconds to wait before initiating teleport. Useful if you want to fade the screen  before teleporting.")]
        public float TeleportDelay = 0.2f;

        private ScreenFader fader; //added

        #region Events
        public delegate void OnBeforeRotateAction();
        public static event OnBeforeRotateAction OnBeforeRotate;

        public delegate void OnAfterRotateAction();
        public static event OnAfterRotateAction OnAfterRotate;
        #endregion


        void Start() { //added
            BNGPlayerController playerController = GetComponent<BNGPlayerController>();
            Transform cameraRig = playerController.CameraRig;
            fader = cameraRig.GetComponentInChildren<ScreenFader>();
        }
        void Update() {

            if(!AllowInput) {
                return;
            }

            xAxis = GetAxisInput();

            if (RotationType == RotationMechanic.Snap) {
                DoSnapRotation(xAxis);
            }

            else if (RotationType == RotationMechanic.Smooth) {
                DoSmoothRotation(xAxis);
            }

            // Store input for future checks
            previousXInput = xAxis;
        }

        /// <summary>
        /// Return a float between -1 and 1 to determine which direction to turn the character
        /// </summary>
        /// <returns></returns>
        public virtual float GetAxisInput() {

            // Use the largest, non-zero value we find in our input list
            float lastVal = 0;

            // Check Raw Input
            if(inputAxis != null) {
                for (int i = 0; i < inputAxis.Count; i++) {
                    float axisVal = InputBridge.Instance.GetInputAxisValue(inputAxis[i]).x;

                    // Always take this value if our last entry was 0. 
                    if (lastVal == 0) {
                        lastVal = axisVal;
                    }
                    else if (axisVal != 0 && axisVal > lastVal) {
                        lastVal = axisVal;
                    }
                }
            }

            // Check Unity Input Action
            if(RotateAction != null) {
                float axisVal = RotateAction.action.ReadValue<Vector2>().x;
                // Always take this value if our last entry was 0. 
                if (lastVal == 0) {
                    lastVal = axisVal;
                }
                else if (axisVal != 0 && axisVal > lastVal) {
                    lastVal = axisVal;
                }
            }

            return lastVal;
        }

        public virtual void DoSnapRotation(float xInput) {

            // Reset rotation amount before retrieving inputs
            rotationAmount = 0;

            // Snap Right
            if (xInput >= 0.1f && previousXInput < 0.1f) {
                rotationAmount += SnapRotationAmount;
            }
            // Snap Left
            else if (xInput <= -0.1f && previousXInput > -0.1f) {
                rotationAmount -= SnapRotationAmount;
            }

            if (Math.Abs(rotationAmount) > 0) {
                //added
                recentSnapTurnTime = Time.time;
                StartCoroutine(DoRotate(Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationAmount, transform.eulerAngles.z))));
                /* //removed
                // Call any Before Rotation Events
                OnBeforeRotate?.Invoke();

                // Apply rotation
                transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationAmount, transform.eulerAngles.z));

                recentSnapTurnTime = Time.time;

                // Call any After Rotation Events
                OnAfterRotate?.Invoke();
                */
            }
        }

        IEnumerator DoRotate(Quaternion newRotation) { //added
            if (FadeScreenOnTeleport && fader) {
                fader.FadeInSpeed = TeleportFadeSpeed;
                fader.DoFadeIn();
            }

            if (TeleportDelay > 0) {
                yield return new WaitForSeconds(TeleportDelay);
            }

            // Call any Before Rotation Events
            OnBeforeRotate?.Invoke();

            // Apply rotation
            transform.rotation = newRotation;

            recentSnapTurnTime = Time.time;

            // Call any After Rotation Events
            OnAfterRotate?.Invoke();

            if (FadeScreenOnTeleport && fader) {
                fader.DoFadeOut();
            }
        }

        public virtual bool RecentlySnapTurned() {
            return Time.time - recentSnapTurnTime <= 0.1f;
        }

        public virtual void DoSmoothRotation(float xInput) {

            // Reset rotation amount before retrieving inputs
            rotationAmount = 0;

            // Smooth Rotate Right
            if (xInput >= SmoothTurnMinInput) {
                rotationAmount += xInput * SmoothTurnSpeed * Time.deltaTime;
            }
            // Smooth Rotate Left
            else if (xInput <= -SmoothTurnMinInput) {
                rotationAmount += xInput * SmoothTurnSpeed * Time.deltaTime;
            }

            // Apply rotation
            transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationAmount, transform.eulerAngles.z));
        }
    }
}

