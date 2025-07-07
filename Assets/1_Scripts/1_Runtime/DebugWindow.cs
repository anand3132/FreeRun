using System;
using System.Threading.Tasks;
using RedGaint.Network.Runtime.UserData;
using UnityEngine;
using UnityEngine.UI;
namespace RedGaint.Utility
{
    public class DebugWindow : MonoBehaviour
    {
        public Button TestAlloactionButton;

        void OnEnable()
        {
            TestAlloactionButton.onClick.AddListener(HandleButtonClicked);
        }

        private void HandleButtonClicked()
        {
            // Call async logic without awaiting (Unity doesn't support async void in UI events)
            _ = OnTestAlloactionButtonClicked(); // fire-and-forget
        }

        private async Task OnTestAlloactionButtonClicked()
        {
            try
            {
                string result = await CloudModule.Instance.TestAllocation();
                Debug.Log($"Allocation Result: {result}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in allocation: {ex.Message} :Authenticate before calling TestAlloaction");
            }
        }

        void OnDisable()
        {
            TestAlloactionButton.onClick.RemoveListener(HandleButtonClicked);
        }
    }

}
