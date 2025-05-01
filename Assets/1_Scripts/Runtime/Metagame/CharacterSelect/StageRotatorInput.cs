using UnityEngine;
using UnityEngine.EventSystems;

namespace RedGaint.Network.Runtime
{
    public class StageRotatorInput : MonoBehaviour
    {
        private bool isDragging = false;
        private Table selectedTable = null;
        private Vector3 lastMousePosition;

        public LayerMask tableLayerMask; // Assign this to the "TableModel" layer

        void Update()
        {
            HandleInput();
        }

        void HandleInput()
        {
            // Ignore if clicking UI (to prevent interaction with UI elements)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Mouse button down (click)
            if (Input.GetMouseButtonDown(0))
            {
                // Raycast to detect the clicked table
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, tableLayerMask))
                {
                    selectedTable = FindTableFromHit(hit.transform); // Find the table the player clicked on
                    if (selectedTable != null)
                    {
                        isDragging = true;
                        lastMousePosition = Input.mousePosition;
                    }
                }
            }

            // Mouse button drag (rotate the selected character model)
            if (Input.GetMouseButton(0) && isDragging && selectedTable != null)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition; // Get the movement delta of the mouse
                lastMousePosition = Input.mousePosition;

                // Rotate the model on the selected table
                if (selectedTable.currentCharacter != null)
                {
                    // Rotate around the Y-axis (you can adjust this for different rotation)
                    selectedTable.currentCharacter.transform.Rotate(Vector3.up, -delta.x * 0.5f, Space.World);
                }
            }

            // Mouse button up (stop dragging)
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                selectedTable = null;
            }
        }

        // Find the table that corresponds to the clicked transform (hit object)
        private Table FindTableFromHit(Transform hitTransform)
        {
            // Search through all tables in the Stage instance to find which one the user clicked on
            foreach (var table in Stage.Instance.tables)
            {
                // Check if the hit object is a child of this table's model hook
                if (hitTransform.IsChildOf(table.modelHook))
                    return table;
            }
            return null;
        }
    }
}
