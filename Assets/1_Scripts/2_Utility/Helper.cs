using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
//using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace RedGaint.Utility
{

    public static class Helper
    {

        public static string GetCurrentAppPlatform()
        {
            string platform = "ios";
#if UNITY_IOS
            platform = "ios";
#elif UNITY_STANDALONE_LINUX
            platform = "linux";
#elif UNITY_STANDALONE_OSX
            platform = "macos";
#elif UNITY_STANDALONE_WIN
            platform = "windows";
#elif UNITY_ANDROID
        platform = "android";
#endif
            return platform;
        }


        public static bool IsPhone()
        {
            return (GetAspectRatio() > 1.65f);
        }

        public static bool ISaKindleDevice()
        {
            List<string> deviceNames = new List<string>
            {
                "KFMUWI", "KFKAWI", "KFONWI",
            };

            string deviceModel = SystemInfo.deviceModel;
            foreach (string name in deviceNames)
            {
                if (deviceModel.Contains(name))
                    return true;
            }

            return false;
        }

        public static bool IsRamLessThan2GB()
        {
            return SystemInfo.systemMemorySize < 2100;
        }

        public static bool IsIpad3or4()
        {
            if (SystemInfo.deviceModel.Contains("iPad3") || SystemInfo.deviceModel.Contains("iPad4"))
                return true;

            return false;
        }

        public static bool ISaCenterCamDevice()
        {
            List<string> deviceNames = new List<string>
            {
                "KFONWI",
            };

            string deviceModel = SystemInfo.deviceModel;
            foreach (string name in deviceNames)
            {
                if (deviceModel.Contains(name))
                    return true;
            }

            return false;

        }

        public static T FindDeepChild<T>(Transform aParent, string aName, bool exactMatch = true) where T : Component
        {
            var transform = FindDeepChild(aParent, aName, exactMatch);
            if (transform != null)
            {
                return transform.GetComponent<T>();
            }

            return null;
        }

        public static GameObject FindDeepChildObject(Transform aParent, string aName, bool exactMatch = true)
        {
            FindDeepChild(aParent, aName, exactMatch);
            return aParent.gameObject;
        }

        public static Transform FindDeepChild(Transform aParent, string aName, bool exactMatch = true)
        {
            if (aParent == null)
            {
                return null;
            }

            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c == null) continue;
                if (exactMatch)
                {
                    if (c.name == aName)
                    {
                        return c;
                    }
                }
                else
                {
                    if (c.name.Contains(aName))
                    {
                        return c;
                    }
                }

                foreach (Transform t in c)
                    queue.Enqueue(t);
            }

            return null;
        }

        public static List<Transform> FindDeepChildsWithName(Transform aParent, string aName, bool exactMatch)
        {
            List<Transform> allT = new List<Transform>();

            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (exactMatch)
                {
                    if (c.name == aName)
                    {
                        allT.Add(c);
                    }
                }
                else
                {
                    if (c.name.Contains(aName))
                    {
                        allT.Add(c);
                    }
                }

                foreach (Transform t in c)
                    queue.Enqueue(t);
            }

            return allT;
        }

        public static float GetAspectRatio()
        {
            return (float)((float)Screen.width / (float)Screen.height);
        }

        public static Color GetColorFromHex(string a_strHexValue)
        {
            Color colorToReturn = Color.black;
            if (!a_strHexValue.StartsWith("#"))
                a_strHexValue = "#" + a_strHexValue;
            ColorUtility.TryParseHtmlString(a_strHexValue, out colorToReturn);
            return colorToReturn;
        }

        public static string GetColorHexFromColor(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static string GetColorHexFromColorWithAlpha(Color32 color)
        {
            return string.Format("{0}/{1}", ColorUtility.ToHtmlStringRGB(color), color.a);
        }

        public static Color GetColorFromHexWithAlpha(string a_strHexValueWithAlpha)
        {
            Color32 colorToReturn = Color.black;
            string[] cols = a_strHexValueWithAlpha.Split("/");
            if (cols.Length > 0)
            {
                colorToReturn = GetColorFromHex(cols[0]);
            }

            if (cols.Length == 2)
            {
                byte.TryParse(cols[1], out colorToReturn.a);
            }

            return colorToReturn;
        }

        public static float NormalizedValue(float value, float min, float max)
        {
            float normalized = (value - min) / (max - min);
            return normalized;
        }

        public static bool Approximately(this Quaternion quatA, Quaternion value, float precision)
        {
            if (quatA.Equals(value))
            {
                return true;
            }

            float dotVal = Mathf.Abs(Quaternion.Dot(quatA, value));
            return dotVal >= 1f - precision;
        }

        public static bool IsPointerOverUIElement(Vector2 mousePosition, out string name, bool checkUILayerOnly = true)
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults(mousePosition), out name, checkUILayerOnly);
        }

        ///Returns 'true' if we touched or hovering on Unity UI element.
        private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, out string name,
            bool checkUILayerOnly = true)
        {
            name = string.Empty;
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];
                // Debug.LogFormat("Raycast result : {0} -- {1}", curRaysastResult.gameObject.name, curRaysastResult.gameObject.layer);
                if (checkUILayerOnly && curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    name = curRaysastResult.gameObject.name;
                    return true;
                }
                else if (!checkUILayerOnly)
                {
                    name = curRaysastResult.gameObject.name;
                    return true;
                }
            }

            return false;
        }

        ///Gets all event systen raycast results of current mouse or touch position.
        private static List<RaycastResult> GetEventSystemRaycastResults(Vector2 position)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = position;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            if (EventSystem.current != null)
            {
                EventSystem.current.RaycastAll(eventData, raysastResults);
            }

            return raysastResults;
        }

        public static float troubleShootHelpDelayCount = 0;

        public static void ResetTroubleshootHelper()
        {
            troubleShootHelpDelayCount = 0;
        }

        public static bool IncreaseTroubleshootHelper(float _count = 1f)
        {
            troubleShootHelpDelayCount += _count;
            if (troubleShootHelpDelayCount > 5)
                return true;
            else
                return false;
        }

        public static GenericItemCount<T> FindMostPopular<T>(List<T> list)
        {
            GenericItemCount<T> itemCount = new GenericItemCount<T>();

            var query = from i in list
                group i by i
                into g
                select new { g.Key, Count = g.Count() };

            int frequency = query.Max(g => g.Count);

            // find the values with that frequency
            List<T> modes = query.Where(g => g.Count == frequency).Select(g => g.Key).ToList();
            if (modes.Count > 0)
            {
                itemCount.Item = modes.First();
                itemCount.Count = frequency;
            }

            return itemCount;
        }

        public class GenericItemCount<T>
        {
            public T Item;
            public int Count;
        }

        public static bool IsBetween(float value, float min, float max)
        {
            return (value >= min && value <= max);
        }

        public static bool IsDeviceRamHigherThan2GB()
        {
            Debug.Log($"System Memory = {SystemInfo.systemMemorySize}");
            if (SystemInfo.systemMemorySize >= 2048)
                return true;
            return false;
        }

        public static IEnumerator FadeUIElement(CanvasGroup canvasGroup,
            bool enabled, float duration = 0.5f, float delayBeforeFade = 0)
        {
            return FadeUIElements(new List<CanvasGroup>() { canvasGroup },
                enabled, duration, delayBeforeFade);
        }

        public static IEnumerator FadeUIElements(List<CanvasGroup> canvasGroups,
            bool enabled, float duration = 0.5f, float delayBeforeFade = 0)
        {
            float targetAlpha;
            if (enabled)
            {
                targetAlpha = 1;
            }
            else
            {
                targetAlpha = 0;
            }

            List<float> currAlphas = new List<float>();
            for (int i = 0; i < canvasGroups.Count; i++)
            {
                if (enabled)
                {
                    if (!canvasGroups[i].gameObject.activeSelf)
                    {
                        canvasGroups[i].gameObject.SetActive(true);
                        canvasGroups[i].alpha = 0;
                    }
                }

                currAlphas.Add(canvasGroups[i].alpha);
            }

            if (delayBeforeFade != 0)
            {
                yield return new WaitForSeconds(delayBeforeFade);
            }

            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                for (int i = 0; i < canvasGroups.Count; i++)
                {
                    canvasGroups[i].alpha = Mathf.Lerp(currAlphas[i],
                        targetAlpha, t / duration);
                }

                yield return new WaitForEndOfFrame();
            }

            for (int i = 0; i < canvasGroups.Count; i++)
            {
                canvasGroups[i].alpha = targetAlpha;
                if (!enabled)
                {
                    canvasGroups[i].gameObject.SetActive(false);
                }
            }
        }

#if UNITY_EDITOR
    public static List<T> FindAssetsByTypeInProjectWindow<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = UnityEditor.AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }

        return assets;
    }
#endif

        public static string GetCoreDataSavePath()
        {
            return Application.persistentDataPath + "/data/";
        }

        public static void CopyStructFieldValues<T>(T source, ref T target) where T : struct
        {
            Type structType = typeof(T);
            var fields = structType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                object value = field.GetValue(source);
                field.SetValueDirect(__makeref(target), value);
            }

            object boxedTarget = target;
            var properties =
                structType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    object propertyValue = property.GetValue(source);
                    property.SetValue(boxedTarget, propertyValue);
                }
            }

            target = (T)boxedTarget;
        }
        // public static string[] GetSfxClipNames()
        // {
        //     var sfxClips = ResourceDB.GetAll<AudioClip>("sfx");
        //     string[] names = new string[sfxClips.Length];
        //     for (int i = 0; i < sfxClips.Length; i++)
        //     {
        //         names[i] = sfxClips[i].name;
        //     }
        //     return names;
        // }

        // public static string GetSfxClipNameByIndex(int _index)
        // {
        //     string[] names = GetSfxClipNames();
        //     if (_index < names.Length)
        //         return names[_index];
        //
        //     return null;
        // }

        // public static string[] GetVfxClipNames()
        // {
        //     var vfxClips = ResourceDB.GetAll<GameObject>("vfx");
        //     string[] names = new string[vfxClips.Length];
        //     for (int i = 0; i < vfxClips.Length; i++)
        //     {
        //         names[i] = vfxClips[i].name;
        //     }
        //     return names;
        // }

        // public static string GetVfxClipNameByIndex(int _index)
        // {
        //     string[] names = GetVfxClipNames();
        //     if (_index < names.Length)
        //         return names[_index];
        //     return null;
        // }

        public static float StringToFloat(string _value)
        {
            float.TryParse(_value, out float result);
            return result;
        }

        public static int StringInInt(string _val)
        {
            int.TryParse(_val, out int result);
            return result;
        }

        public static T DeepCopy<T>(T other)
        {
            var serializedData = JsonUtility.ToJson(other);
            return JsonUtility.FromJson<T>(serializedData);
        }

        public static string GetPresetName(this Enum value, string moduleName)
        {
            return string.Concat(moduleName, "/", moduleName, "_", value.ToString(), "_PresetSO");
        }

        public static bool IsChildOfSelectedObjects(GameObject obj, IEnumerable<GameObject> selectedObjects)
        {
            foreach (GameObject selectedObj in selectedObjects)
            {
                if (obj.transform.IsChildOf(selectedObj.transform) && obj != selectedObj)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Transform> GetChildren(Transform parent, bool recursive)
        {
            /** Get a list of children from a given parent, either the direct
                descendants or all recursively. **/

            List<Transform> children = new List<Transform>();

            foreach (Transform child in parent)
            {
                children.Add(child);
                if (recursive)
                {
                    children.AddRange(GetChildren(child, true));
                }
            }

            return children;
        }

        public static bool IsInUnityEditorMode()
        {
#if UNITY_EDITOR
        return !Application.isPlaying;
#else
            return false;
#endif
        }

        public static bool IsInUnityEditor()
        {
#if UNITY_EDITOR
        return true;
#else
            return false;
#endif
        }

        public static bool IsPlatformWebGL()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public static bool IsPrimitive(this GameObject go, out PrimitiveType type)
        {
            type = default;
            if (go.TryGetComponent(out MeshFilter mesh))
            {
                var name = mesh.sharedMesh.name;
                if (name.Contains(" "))
                {
                    name = name.Split()[0];
                }

                var isFound = true;
                switch (name)
                {
                    default:
                        isFound = false;
                        break;
                    case nameof(PrimitiveType.Cube):
                        type = PrimitiveType.Cube;
                        break;
                    case nameof(PrimitiveType.Capsule):
                        type = PrimitiveType.Capsule;
                        break;
                    case nameof(PrimitiveType.Sphere):
                        type = PrimitiveType.Sphere;
                        break;
                    case nameof(PrimitiveType.Plane):
                        type = PrimitiveType.Plane;
                        break;
                    case nameof(PrimitiveType.Quad):
                        type = PrimitiveType.Quad;
                        break;
                    case nameof(PrimitiveType.Cylinder):
                        type = PrimitiveType.Cylinder;
                        break;
                }

                return isFound;
            }

            return false;
        }

        public static void SetBoundsValue(this Collider collider, float multiplySizeBy = 0f)
        {
            if (collider.GetType() != typeof(BoxCollider)) return;
            var bx = (BoxCollider)collider;
            var tr = collider.gameObject.transform;
            var renderers = collider.gameObject.GetComponentsInChildren<Renderer>();
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var renderer in renderers)
            {
                var childBounds = renderer.bounds;
                childBounds.center = tr.InverseTransformPoint(childBounds.center);
                childBounds.size = Vector3.Scale(childBounds.size, renderer.transform.localScale);
                bounds.Encapsulate(childBounds);
            }

            if (multiplySizeBy != 0)
            {
                bounds.size *= multiplySizeBy;
            }

            bx.size = bounds.size;
            bx.center = bounds.center;
        }

        public static Bounds CalculateBounds<T>(this IEnumerable<T> objects) where T : Transform
        {
            Bounds bounds = default;
            bool firstBound = true;
            foreach (var transform in objects)
            {
                foreach (var renderer in transform.GetComponentsInChildren<Renderer>())
                {
                    if (firstBound)
                    {
                        firstBound = false;
                        bounds = renderer.bounds;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            return bounds;
        }

        public static Vector3 LocalToWorldScale(this Vector3 localScale, Transform parentTransform)
        {
            if (!parentTransform)
            {
                return localScale;
            }

            var worldScale = new Vector3(
                localScale.x * parentTransform.lossyScale.x,
                localScale.y * parentTransform.lossyScale.y,
                localScale.z * parentTransform.lossyScale.z
            );
            return worldScale;
        }

        public static Vector3 WorldToLocalScale(this Vector3 worldScale, Transform parentTransform)
        {
            if (!parentTransform)
            {
                return worldScale;
            }

            var localScale = new Vector3(
                worldScale.x / parentTransform.lossyScale.x,
                worldScale.y / parentTransform.lossyScale.y,
                worldScale.z / parentTransform.lossyScale.z
            );
            return localScale;
        }

        public static void SetTrigger(this Collider collider, bool isTrigger)
        {
            var type = collider.GetType();
            switch (type.Name)
            {
                case nameof(BoxCollider):
                case nameof(SphereCollider):
                case nameof(CapsuleCollider):
                    collider.isTrigger = isTrigger;
                    break;
                case nameof(MeshCollider):
                    var mc = (MeshCollider)collider;
                    mc.convex = isTrigger;
                    mc.isTrigger = isTrigger;
                    break;
            }
        }

        public static T GetEnumValueByIndex<T>(int index) where T : Enum
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Type T must be an Enum");
            }

            T[] enumValues = (T[])Enum.GetValues(typeof(T));

            if (index >= 0 && index < enumValues.Length)
            {
                return enumValues[index];
            }

            throw new ArgumentOutOfRangeException("Index is out of range");
        }

        public static int GetEnumIndexByString<TEnum>(string value) where TEnum : struct, Enum
        {
            if (Enum.TryParse(value, out TEnum enumValue))
            {
                return Convert.ToInt32(enumValue);
            }
            else
            {
                return 0;
            }
        }

        public static List<string> GetEnumValuesAsStrings<TEnum>() where TEnum : Enum
        {
            return new List<string>(Enum.GetNames(typeof(TEnum)));
        }

        // public static List<string> GetEnumWithAliasNames<TEnum>() where TEnum : Enum
        // {
        //     var enumType = typeof(TEnum);
        //     return GetEnumWithAliasNames(enumType);
        // }

        // public static List<string> GetEnumWithAliasNames(Type enumType)
        // {
        //     var enumNamesWithDisplayNames = new List<string>();
        //
        //     if (!enumType.IsEnum)
        //         return null;
        //     foreach (var enumValue in Enum.GetValues(enumType))
        //     {
        //         var fieldInfo = enumType.GetField(enumValue.ToString());
        //         var displayNameAttribute = fieldInfo.GetCustomAttribute<AliasDrawerAttribute>();
        //
        //         if (displayNameAttribute != null)
        //         {
        //             enumNamesWithDisplayNames.Add(displayNameAttribute.Alias);
        //         }
        //         else
        //         {
        //             enumNamesWithDisplayNames.Add(enumValue.ToString());
        //         }
        //     }
        //
        //     return enumNamesWithDisplayNames;
        // }

        public static void DeepCopy<T>(List<T> source, List<T> destination)
        {
            destination.Clear();
            foreach (var originalItem in source)
            {
                T newItem = originalItem;
                destination.Add(newItem);
            }
        }

        public static Rigidbody AddRigidbody(this GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(out Rigidbody rb))
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            if (gameObject.TryGetComponent(out MeshCollider meshCollider))
            {
                meshCollider.convex = meshCollider.isTrigger;
            }

            return rb;
        }

        public static void RemoveColliders(this GameObject gameObject)
        {
            Collider[] colliders = gameObject.GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                Object.Destroy(collider);
            }

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.RemoveColliders();
            }
        }

        public static void SetInteractive(this CanvasGroup canvasGroup, bool isInteractable)
        {
            canvasGroup.blocksRaycasts = isInteractable;
            canvasGroup.interactable = isInteractable;
            canvasGroup.ignoreParentGroups = isInteractable;
        }

        public static Vector3 GetAbsEulerAngle(this Transform transform)
        {
            var currentAngle = transform.eulerAngles;
            if (currentAngle.x < 0)
            {
                currentAngle.x = 360f + currentAngle.x;
                if (Mathf.Approximately(currentAngle.x, 360f))
                {
                    currentAngle.x = 0f;
                }
            }

            if (currentAngle.y < 0)
            {
                currentAngle.y = 360f + currentAngle.y;
                if (Mathf.Approximately(currentAngle.y, 360f))
                {
                    currentAngle.y = 0f;
                }
            }

            if (currentAngle.z < 0)
            {
                currentAngle.z = 360f + currentAngle.z;
                if (Mathf.Approximately(currentAngle.z, 360f))
                {
                    currentAngle.z = 0f;
                }
            }

            return currentAngle;
        }

        public static Vector3 GetAbsEulerAngle(this Vector3 currentAngle)
        {
            if (currentAngle.x < 0)
            {
                currentAngle.x = 360f + currentAngle.x;
            }

            if (Mathf.Abs(currentAngle.x - 360f) < 3)
            {
                currentAngle.x = 0f;
            }

            if (currentAngle.y < 0)
            {
                currentAngle.y = 360f + currentAngle.y;
            }

            if (Mathf.Abs(currentAngle.y - 360f) < 3)
            {
                currentAngle.y = 0f;
            }

            if (currentAngle.z < 0)
            {
                currentAngle.z = 360f + currentAngle.z;
            }

            if (Mathf.Abs(currentAngle.z - 360f) < 3)
            {
                currentAngle.z = 0f;
            }

            return currentAngle;
        }

        public static Quaternion Lerp(Quaternion p, Quaternion q, float t, bool shortWay)
        {
            if (shortWay)
            {
                float dot = Quaternion.Dot(p, q);
                if (dot < 0.0f)
                    return Lerp(ScalarMultiply(p, -1.0f), q, t, true);
            }

            Quaternion r = Quaternion.identity;
            r.x = p.x * (1f - t) + q.x * (t);
            r.y = p.y * (1f - t) + q.y * (t);
            r.z = p.z * (1f - t) + q.z * (t);
            r.w = p.w * (1f - t) + q.w * (t);
            return r;
        }

        public static Quaternion Slerp(Quaternion p, Quaternion q, float t, bool shortWay)
        {
            float dot = Quaternion.Dot(p, q);
            if (shortWay)
            {
                if (dot < 0.0f)
                {
                    q = ScalarMultiply(q, -1.0f);
                    dot = -dot;
                }
            }
            else
            {
                if (dot > 0.0f)
                {
                    q = ScalarMultiply(q, -1.0f);
                    dot = -dot;
                }
            }

            float angle = Mathf.Acos(dot);
            Quaternion first = ScalarMultiply(p, Mathf.Sin((1f - t) * angle));
            Quaternion second = ScalarMultiply(q, Mathf.Sin((t) * angle));
            float division = 1f / Mathf.Sin(angle);
            return ScalarMultiply(Add(first, second), division);
        }

        public static Quaternion ScalarMultiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }

        public static Quaternion Add(Quaternion p, Quaternion q)
        {
            return new Quaternion(p.x + q.x, p.y + q.y, p.z + q.z, p.w + q.w);
        }

        public static string ReplaceHttpsFromUrl(string url)
        {
            return url.Replace("https", "http");
        }

        public static string UnzipBase64String(string compressedText)
        {
            byte[] compressedBytes = Convert.FromBase64String(compressedText); // Assuming the string is Base64-encoded

            using (MemoryStream memoryStream = new MemoryStream(compressedBytes))
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(gzipStream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static T CopyComponent<T>(this T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }

            return copy as T;
        }

        // public static void GetAssetsOfTypeAll(Type type, Action<object[]> onComplete, TextureTypes textureType = TextureTypes.None)
        // {
        //     object[] allReferences = new object[1];
        //
        //     List<object> objects = new List<object>();
        //
        //     if (typeof(Texture).IsAssignableFrom(type) || typeof(Sprite).IsAssignableFrom(type))
        //     {
        //         new TextureAPI(textureType).DoRequest((status, response) =>
        //         {
        //             if (status)
        //             {
        //                 var data = JsonConvert.DeserializeObject<TextureAPI.TextureData[]>(response);
        //                 objects.AddRange(data);
        //                 allReferences = objects.ToArray();
        //             }
        //             else
        //             {
        //                 Debug.Log($"API Response failed for {textureType.ToString()}");
        //             }
        //             onComplete?.Invoke(allReferences);
        //         });
        //
        //     }
        //     else if (typeof(AudioClip).IsAssignableFrom(type))
        //     {
        //         objects.AddRange(SystemOp.Resolve<AudioManager>().GetAudioDatas());
        //         allReferences = objects.ToArray();
        //         onComplete?.Invoke(allReferences);
        //     }
        //     else
        //     {
        //         allReferences = Resources.FindObjectsOfTypeAll(type);
        //         onComplete?.Invoke(allReferences);
        //     }
        // }

        private const byte k_MaxByteForOverexposedColor = 191;

        public static void DecomposeHdrColor(this Color linearColorHdr, out Color32 baseLinearColor, out float exposure)
        {
            baseLinearColor = linearColorHdr;
            var maxColorComponent = linearColorHdr.maxColorComponent;
            // replicate Photoshops's decomposition behaviour
            if (maxColorComponent == 0f || maxColorComponent <= 1f && maxColorComponent >= 1 / 255f)
            {
                exposure = 0f;

                baseLinearColor.r = (byte)Mathf.RoundToInt(linearColorHdr.r * 255f);
                baseLinearColor.g = (byte)Mathf.RoundToInt(linearColorHdr.g * 255f);
                baseLinearColor.b = (byte)Mathf.RoundToInt(linearColorHdr.b * 255f);
            }
            else
            {
                // calibrate exposure to the max float color component
                var scaleFactor = k_MaxByteForOverexposedColor / maxColorComponent;
                exposure = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);

                // maintain maximal integrity of byte values to prevent off-by-one errors when scaling up a color one component at a time
                baseLinearColor.r = Math.Min(k_MaxByteForOverexposedColor,
                    (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.r));
                baseLinearColor.g = Math.Min(k_MaxByteForOverexposedColor,
                    (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.g));
                baseLinearColor.b = Math.Min(k_MaxByteForOverexposedColor,
                    (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.b));
            }
        }

        public static void RemoveCloneFromName(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            go.name = go.name.Replace("(Clone)", "");
        }

        public static bool IsDefault<T>(this T instance) where T : struct
        {
            return instance.Equals(default(T));
        }
    }
}//RedGaint.Utility