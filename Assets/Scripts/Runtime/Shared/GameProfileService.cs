// using System.Threading.Tasks;
//
// namespace RedGaint.Network.Runtime
// {
//     public static class GameProfileService
//     {
//         public static UserData.PlayerProfileData CurrentProfileData { get; private set; }
//
//         public static void LoadFromLocal()
//         {
//             CurrentProfileData = UserData.LocalPlayerProfileHandler.Load() ?? new UserData.PlayerProfileData();
//         }
//
//         public static async Task LoadFromCloudAsync()
//         {
//             CurrentProfileData = await UserData.CloudPlayerProfileHandler.LoadAsync() ?? new UserData.PlayerProfileData();
//         }
//
//         public static void SaveLocal() => UserData.LocalPlayerProfileHandler.Save(CurrentProfileData);
//
//         public static async Task SaveCloudAsync() => await UserData.CloudPlayerProfileHandler.SaveAsync(CurrentProfileData);
//     }
//
// }