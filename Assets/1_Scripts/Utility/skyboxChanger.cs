using UnityEngine;

namespace RedGaint.Utility
{
    public class SkyboxChanger : MonoBehaviour
    {
        public Material newSkybox;

        void Start()
        {
            RenderSettings.skybox = newSkybox;
        }
    }
}