using RedGaint;
using UnityEngine;

namespace RedGaint.Utility
{
    public class ModelTurntable : MonoBehaviour
    {
        public float rotationSpeed = 5f;

        private bool isDragging = false;
        private float lastMouseX;

        void Update()
        {
            HandleMouseInput();
        }

        void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMouseX = Input.mousePosition.x;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                float deltaX = Input.mousePosition.x - lastMouseX;
                transform.Rotate(Vector3.up, -deltaX * rotationSpeed * Time.deltaTime);
                lastMouseX = Input.mousePosition.x;
            }
        }

        public void ShowNextModel()
        {
            
        }

        public void ShowPreviousModel()
        {
            
        }
        public bool LogThisClass { get; } = false;
    }
}