using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pilot
{
    public class PilotMovement : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody2D rigidbody2D;

        [SerializeField]
        private ClientNetworkTransform clientNetworkTransform;

        [SerializeField]
        private float jumpForce;

        [SerializeField]
        private float parachuteDrag = 8;
        
        [SerializeField]
        private float parachuteSideSpeed;

        [SerializeField]
        private float runSpeed;
        
        private float edgeDistance;

        public void Init(float edgeDistance)
        {
            this.edgeDistance = edgeDistance;
        }
        
        private void Update()
        {
            if (Mathf.Abs(transform.position.x) < edgeDistance) {
                return;
            }

            MoveToOtherSide();
        }

        public void MoveWithParachute(int direction)
        {
            rigidbody2D.velocity = new Vector2(parachuteSideSpeed * direction, rigidbody2D.velocity.y);
        }

        public void MoveOnGround(int direction)
        {
            rigidbody2D.velocity = new Vector2(runSpeed * direction, rigidbody2D.velocity.y);
        }
        
        public void Jump(Vector2 direction)
        {
            rigidbody2D.AddForce(direction * jumpForce, ForceMode2D.Impulse);
        }

        public void SetDeadMovement()
        {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.gravityScale = -0.1f;
            rigidbody2D.drag = 0;
        }

        public void SetParachuteMovement()
        {
            rigidbody2D.drag = parachuteDrag;
        }

        public void SetFallingMovement()
        {
            rigidbody2D.drag = 0;
        }
        
        private void MoveToOtherSide()
        {
            var playerPos = transform.position;
            clientNetworkTransform.Teleport(
                new Vector3(playerPos.x > 0 ? -edgeDistance : edgeDistance, playerPos.y),
                transform.rotation,
                Vector3.one);
        }
    }
}