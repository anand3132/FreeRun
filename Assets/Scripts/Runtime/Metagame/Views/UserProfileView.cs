using UnityEngine;

namespace RedGaint.Network.Runtime
{
    public class UserProfileView:View<MetagameApplication>
    {
        public UserData.PlayerProfileData userdata;
        private void OnClickSave()
        {
            //userdata = new UserProfile($"user_{Random.Range(0, 100)},");
        }
        
    }
}