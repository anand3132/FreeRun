using RedGaint;
using UnityEngine;

namespace RedGaint.Utility
{

    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour, IBugsBunny
    {
        public static bool verbose = false;
        public static bool keepAlive = true;

        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        var singletonObj = new GameObject();
                        singletonObj.name = typeof(T).ToString();
                        _instance = singletonObj.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        public static bool IsInstanceAlive => _instance != null;

        public virtual void Awake()
        {
            if (_instance != null)
            {
                if (verbose)
                    Debug.Log($"[Singleton] Destroying duplicate instance of {typeof(T).Name}: {name}");
                Destroy(gameObject);
                return;
            }

            _instance = GetComponent<T>();

            if (keepAlive)
            {
                if (transform.parent != null)
                {
                    transform.SetParent(null); // Detach from parent to become a root GameObject
                }

                DontDestroyOnLoad(gameObject);
            }

            if (_instance == null)
            {
                if (verbose)
                    Debug.LogError($"[Singleton<{typeof(T).Name}>] Instance is null in Awake.");
                return;
            }

            if (verbose)
                Debug.Log($"[Singleton] Instance initialized for {typeof(T).Name}");
        }

    }

    public class SingletonSimple<T> : MonoBehaviour where T : Component, IBugsBunny
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var objs = FindObjectsOfType(typeof(T)) as T[];
                    if (objs.Length > 0)
                        _instance = objs[0];
                    if (objs.Length > 1)
                    {
                        Debug.LogError(
                            $"[SingletonSimple] More than one {typeof(T).Name} instance found in the scene.");
                    }

                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        _instance = obj.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }
    }

    public class SingletonPersistent<T> : MonoBehaviour where T : Component, IBugsBunny
    {
        public static T Instance { get; private set; }

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
                DontDestroyOnLoad(this);
            }
            else
            {
                Debug.Log($"[SingletonPersistent] Destroying duplicate instance of {typeof(T).Name}: {name}");
                Destroy(gameObject);
            }
        }
    }

    public class SingletonSafe<T> : MonoBehaviour where T : MonoBehaviour, IBugsBunny
    {
        private static T instance;

        // Lock object to ensure thread safety
        private static readonly object lockObject = new object();

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning(
                        $"[SingletonSafe] Instance of {typeof(T)} already destroyed (application is quitting). Returning null.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<T>();

                        if (instance == null)
                        {
                            GameObject singletonObject = new GameObject(typeof(T).Name);
                            instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return instance;
                }
            }
        }

        private static bool applicationIsQuitting = false;

        // This method is called when the application is about to quit
        private void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        // Optional: You can also reset the singleton instance during play mode (useful for testing)
        private void OnDestroy()
        {
            instance = null;
        }
    }
}