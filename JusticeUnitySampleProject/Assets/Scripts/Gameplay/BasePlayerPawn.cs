using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using Game.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    /// <summary>
    ///     The networked compoenent of the player pawn, 
    ///     handles all communications over the network and updateing the entity's state
    /// </summary>
    public class BasePlayerPawn : MovePlayerPawnBehavior
    {
        [Tooltip("The movement speed of the character.")]
        public float movementSpeed = 1.0f;
        [Tooltip("Representing the model view.")]
        public GameObject modelView;

        private Rigidbody pawnRigidbody;
        private CharacterController characterController;
        private bool isNetworkReady;
        private bool isInitialized;
        private bool isLocalOwner;

        private BaseInputFrame currentInput;
        private BaseInputListener inputListener;

        // last frame was processed locally
        private uint lastLocalFrame;
        // last frame that was sent by server / received by  client over the network
        private uint lastNetworkFrame;
        // calculates the current error between Simulation and modelView
        private Vector3 errorVector = Vector3.zero;
        // The interpolation timer for error interpolation
        private float errorTimer;

        private BaseHoveringText hoveringText;

        private const float MAGTITUDE_THRESHOLD = 0.00001f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            pawnRigidbody = GetComponentInChildren<Rigidbody>();
            hoveringText = GetComponent<BaseHoveringText>();
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            isNetworkReady = true;
        }

        /// <summary>
        ///     Initialization, sets all the initial parameters
        /// </summary>
        /// <returns></returns>
        private bool Initialize()
        {
            if (!networkObject.IsServer)
            {
                if (networkObject.OwnerNetId == 0)
                {
                    isInitialized = false;
                    return isInitialized;
                }
            }

            isLocalOwner = networkObject.MyPlayerId == networkObject.OwnerNetId;

            if (isLocalOwner || networkObject.IsServer)
            {
                networkObject.PositionInterpolation.Enabled = false;
                networkObject.RotationInterpolation.Enabled = false;
                if (inputListener == null)
                {
                    inputListener = FindObjectsOfType<BaseInputListener>()
                        .FirstOrDefault(x => x.networkObject.Owner.NetworkId == networkObject.OwnerNetId);
                    if (inputListener == null)
                    {
                        isInitialized = false;
                        return isInitialized;
                    }
                }
            }

            // TODO: Change this into username / character name
            hoveringText.ChangeTextLabel("Player " + networkObject.OwnerNetId.ToString());

            isInitialized = true;
            return isInitialized;
        }


        /// <summary>
        ///     Handles network sync and update of entityState
        /// </summary>
        // Update is called once per frame
        void Update()
        {
            if (!isNetworkReady || !isInitialized)
            {
                return;
            }

            // Set the networked fields in update so the pawn up to date the last physics update
            if (networkObject.IsServer)
            {
                if (lastNetworkFrame < lastLocalFrame)
                {
                    networkObject.Position = pawnRigidbody.position;
                    networkObject.Rotation = pawnRigidbody.rotation;

                    lastNetworkFrame = lastLocalFrame;
                    networkObject.Frame = lastLocalFrame;
                }
            }
            // The local player has to smooth away some errors
            else if (isLocalOwner)
            {
                CorrectError();
            }
            // Update frame nunbers and authoritatively set position if It's a remote player
            else
            {
                lastLocalFrame = lastNetworkFrame = networkObject.Frame;
                modelView.transform.position = new Vector3(pawnRigidbody.position.x,pawnRigidbody.position.y,modelView.transform.position.z);
            }
        }

        /// <summary>
        ///     Handles prediction, server processing , reconciliation & fixed udpate of state
        /// </summary>
        void FixedUpdate()
        {
            if (!isNetworkReady)
            {
                return;
            }
            if (!isInitialized && !Initialize())
            {
                return;
            }
            if ((networkObject.IsServer || isLocalOwner) && inputListener == null)
            {
                return;
            }

            if (!networkObject.IsServer)
            {
                pawnRigidbody.position = networkObject.Position;
                pawnRigidbody.rotation = networkObject.Rotation.normalized;
                if (isLocalOwner && networkObject.Frame != 0 && lastNetworkFrame <= networkObject.Frame)
                {
                    lastNetworkFrame = networkObject.Frame;
                    ValidateFrames();
                }
            }

            if (!isLocalOwner && !networkObject.IsServer)
            {
                return;
            }

            // Local client prediction and server auhoritative logic
            if (inputListener.framesToPlay.Count <= 0)
            {
                return;
            }
            currentInput = inputListener.framesToPlay.Pop();
            lastLocalFrame = currentInput.frameNumber;

            // Try to do a player update
            try
            {
                PlayerUpdate(currentInput);                
            }
            catch(Exception e)
            {
                Debug.LogError("BasePlayerPawn::FixedUpdate() Malformed input frame.");
                Debug.LogError(e);
            }

            inputListener.framesToValidate.Add(currentInput);
        }

        /// <summary>
        ///     Move the player's pawn simulation (rigid body)
        /// </summary>
        /// <param name="input"></param>
        private void Move(BaseInputFrame input)
        {
            // Move the player, clamping the movement so diagonals aren't faster
            //Vector3 movement = Vector3.ClampMagnitude(new Vector3(input.horizontal, input.vertical, 0) * movementSpeed * Time.fixedDeltaTime, movementSpeed);
            Vector3 movement = new Vector3(input.horizontal, 0, input.vertical) * movementSpeed * Time.fixedDeltaTime;

            pawnRigidbody.MovePosition(pawnRigidbody.position + movement);
        }

        private void Rotate(BaseInputFrame input)
        {
            // Determine the number of degrees to be turned based on the input, speed and time between frames.
            float turn = input.horizontal * 40 * Time.fixedDeltaTime;

            // Make this into a rotation in the y axis.
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

            // Apply this rotation to the rigidbody's rotation.
            pawnRigidbody.MoveRotation(pawnRigidbody.rotation * turnRotation);
        }

        /// <summary>
        ///     Detect and Resolve collisions with a simple on trigger overlap check
        /// </summary>
        private void PhysicsCollisions()
        {
            // If not moving then ignore
            if (pawnRigidbody.velocity == Vector3.zero)
            {
                return;
            }
        }

        /// <summary>
        ///     Player update composed of movement and collision processing
        /// </summary>
        /// <param name="input"></param>
        private void PlayerUpdate(BaseInputFrame input)
        {
            pawnRigidbody.velocity = Vector3.zero;
            if (input != null)// && input.hasInput) // since input only happens on key down, hasInput is not relevant
            {
                Move(input);
                PhysicsCollisions();
            }
        }

        /// <summary>
        ///     Validate inputs that haven't yet been authoritaatively processed by server
        /// </summary>
        private void ValidateFrames()
        {
            // Remove any inputs up to and including the last input processed by the server
            inputListener.framesToValidate.RemoveAll(f => f.frameNumber < networkObject.Frame);

            // Replay them all back to the last input processed by client prediction
            if(inputListener.framesToValidate.Count > 0)
            {
                for (Int32 i = 0; i  < inputListener.framesToValidate.Count; ++i)
                {
                    currentInput = inputListener.framesToValidate[i];
                    PlayerUpdate(currentInput);
                }
            }

            // The error vector measures the diff between the predicted and server position
            // and the view position (renderer view)
            errorVector = pawnRigidbody.position - (Vector3)modelView.transform.position;
            errorTimer = 0f;
        }

        private void CorrectError()
        {
            if (errorVector.magnitude >= MAGTITUDE_THRESHOLD)
            {
                float weight = Math.Max(0.0f, .75f - errorTimer);

                Vector3 newViewPosition = (Vector3)modelView.transform.position * weight + pawnRigidbody.position * (1.0f - weight);
                modelView.transform.position = new Vector3(newViewPosition.x,newViewPosition.y,modelView.transform.position.z);

                errorTimer += Time.fixedDeltaTime;

                errorVector = pawnRigidbody.position - (Vector3)modelView.transform.position;

                if (!(errorVector.magnitude < MAGTITUDE_THRESHOLD))
                {
                    return;
                }

                errorVector = Vector3.zero;
                errorTimer = 0.0f;
            }
            else
            {
                modelView.transform.position = new Vector3(pawnRigidbody.position.x,pawnRigidbody.position.y,modelView.transform.position.z);
            }
        }
    }
}
