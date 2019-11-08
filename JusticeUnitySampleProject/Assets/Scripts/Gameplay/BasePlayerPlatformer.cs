using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

namespace Game
{
    public class BasePlayerPlatformer : MovePlayerPawnBehavior
    {
        private CharacterController playerController;
        [SerializeField]
        private float speed;
        [SerializeField]
        private float gravity;
        [SerializeField]
        private float jumpHeight;
        private float currentUpVelocity;
        private bool isDoubleJumpEnabled = false;

        // Start is called before the first frame update
        void Start()
        {
            playerController = GetComponent<CharacterController>();
            currentUpVelocity = 0.0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (!networkObject.IsOwner) //change it to IsOwner
            {
                transform.position = networkObject.Position;
                transform.rotation = networkObject.Rotation;
                return;
            }

            float horizontalInput = Input.GetAxis("Horizontal");
            Vector3 direction = new Vector3(horizontalInput, 0, 0);
            Vector3 velocity = direction * speed;

            if (playerController.isGrounded)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    currentUpVelocity = jumpHeight;
                    isDoubleJumpEnabled = true;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (isDoubleJumpEnabled)
                    {
                        currentUpVelocity += jumpHeight;                        
                        isDoubleJumpEnabled = false;
                    }
                }
                currentUpVelocity -= gravity;
            }

            velocity.y = currentUpVelocity;
            playerController.Move(velocity * Time.deltaTime);
            networkObject.Position = transform.position;
            networkObject.Rotation = transform.rotation;
        }
    }
}
