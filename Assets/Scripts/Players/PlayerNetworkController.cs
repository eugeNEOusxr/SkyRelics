using Unity.Netcode;
using UnityEngine;

namespace SkyRelics.Players
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerNetworkController : NetworkBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float jumpForce = 8f;
        public float gravity = -20f;

        [Header("Stats")]
        public NetworkVariable<float> health = new NetworkVariable<float>(100f);
        public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100f);

        private CharacterController controller;
        private Vector3 velocity;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (!IsOwner) return;

            // Simple WASD movement
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * moveSpeed * Time.deltaTime);

            // Gravity
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Jump
            if (Input.GetButtonDown("Jump") && controller.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        [ServerRpc]
        public void TakeDamageServerRpc(float damage)
        {
            health.Value = Mathf.Max(0, health.Value - damage);
            
            if (health.Value <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            Debug.Log($"Player {OwnerClientId} died!");
            // Respawn logic here
        }
    }
}
