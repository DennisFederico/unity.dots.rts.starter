using System;
using UnityEngine;

namespace rts.mono {
    public class MouseWorldPosition : MonoBehaviour {
        
        //TODO THIS IS CRAP, I DONT WANT TO USE MONOBEHAVIOUR
        //EVEN LESS A SINGLETON
        
        public static MouseWorldPosition Instance { get; private set; }

        private void Awake() {
            Instance = this;
        }


        public Vector3 GetPosition() {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out RaycastHit hit) ? hit.point : Vector3.zero;
        }
        
        //Cheap method for FLAT surfaces
        public Vector3 GetPositionSimple() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance)) {
                return ray.GetPoint(distance);
            } else {
                return Vector3.zero;
            }
        }
    }
}