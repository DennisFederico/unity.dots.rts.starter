using Unity.Cinemachine;
using UnityEngine;

namespace rts.mono {
    public class CameraController : MonoBehaviour {
        //TODO Refactor to use InputSystem

        [SerializeField] private float moveSpeed = 30f;
        [SerializeField] private float rotationSpeed = 200f;
        [SerializeField] private float zoomStepAmount = 4f;
        [SerializeField] private float zoomSmoothing = 10f;
        [SerializeField] private float minZoom = 20f;
        [SerializeField] private float maxZoom = 60f;
        [SerializeField] private CinemachineCamera cineMachineCamera;

        private Transform cameraTransform;
        
        //For smoothing zoom
        [SerializeField] private float targetFieldOfView;

        private void Awake() {
            cameraTransform = Camera.main.transform;
            targetFieldOfView = cineMachineCamera.Lens.FieldOfView;
        }

        private void Update() {
            HandleMovement();
            HandleRotation();
            HandleZoom();
        }
        
        private void HandleMovement() {
            Vector3 moveDirection = Vector3.zero;
            
            if (Input.GetKey(KeyCode.W)) {
                moveDirection += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S)) {
                moveDirection += Vector3.back;
            }
            if (Input.GetKey(KeyCode.A)) {
                moveDirection += Vector3.left;
            }
            if (Input.GetKey(KeyCode.D)) {
                moveDirection += Vector3.right;
            }
            
            //moveDirection = cameraTransform.TransformDirection(moveDirection);
            moveDirection = cameraTransform.forward * moveDirection.z + cameraTransform.right * moveDirection.x;
            moveDirection.y = 0;
            
            transform.position += moveDirection.normalized * (moveSpeed * Time.deltaTime);
        }

        private void HandleRotation() {
            float rotationAmount = 0f;
            if (Input.GetKey(KeyCode.Q)) {
                rotationAmount = -1f;
            }
            if (Input.GetKey(KeyCode.E)) {
                rotationAmount = 1f;
            }
            transform.Rotate(Vector3.up, rotationAmount * (rotationSpeed * Time.deltaTime));
        }
        
        private void HandleZoom() {
            switch (Input.mouseScrollDelta.y) {
                case > 0:
                    targetFieldOfView -= zoomStepAmount;
                    break;
                case < 0:
                    targetFieldOfView += zoomStepAmount;
                    break;
            }
            targetFieldOfView = Mathf.Clamp(targetFieldOfView, minZoom, maxZoom);
            cineMachineCamera.Lens.FieldOfView = Mathf.Lerp(cineMachineCamera.Lens.FieldOfView, targetFieldOfView, Time.deltaTime * zoomSmoothing);
        }
    }
}