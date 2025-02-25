// /*                                                                                       *\
//     This program has been developed by students from the bachelor Computer Science at
//     Utrecht University within the Software Project course.
//
//     (c) Copyright Utrecht University (Department of Information and Computing Sciences)
// \*                                                                                       */

using System.Collections;
using AwARe.Objects;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;

namespace AwARe.InterScenes.Objects
{
    /// <summary>
    /// Secretary class providing safe access to AR Objects, Components and Managers.
    /// </summary>
    public class ARSecretary : MonoBehaviour
    {
        // Singleton instance
        private static ARSecretary instance;

        // AR Support members
        [FormerlySerializedAs("ARSession")][SerializeField] private ARSession session;
        [FormerlySerializedAs("XROrigin")][SerializeField] private XROrigin origin;
        [FormerlySerializedAs("Camera")][SerializeField] private Camera cam;
        [FormerlySerializedAs("EventSystem")][SerializeField] private EventSystem eventSystem;
        
        /// <summary>
        /// Gets the current AR Session.
        /// </summary>
        /// <value>The current AR Session.</value>
        public ARSession Session
        {
            get => session != null ? session : FindObjectOfType<ARSession>();
            private set => session = value;
        }
        
        /// <summary>
        /// Gets the current AR Session Origin.
        /// </summary>
        /// <value>The current AR Session Origin.</value>
        public XROrigin Origin
        {
            get => origin != null ? origin : FindObjectOfType<XROrigin>();
            private set => origin = value;
        }

        /// <summary>
        /// Gets the current AR Camera.
        /// </summary>
        /// <value>The current AR Camera.</value>
        public Camera Camera
        {
            get => cam != null ? cam : FindObjectOfType<Camera>();
            private set => cam = value;
        }

        /// <summary>
        /// Gets the current AR Camera.
        /// </summary>
        /// <value>The current AR Camera.</value>
        public EventSystem EventSystem
        {
            get => eventSystem != null ? eventSystem : FindObjectOfType<EventSystem>();
            private set => eventSystem = value;
        }

        /// <summary>
        /// Get the component of type T from Origin, Session, Camera or itself, if present.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <returns>The component, if present.</returns>
        public new T GetComponent<T>()
            where T : Component =>
            gameObject.GetComponent<T>()
            ?? (Origin ? Origin.GetComponent<T>() : null)
            ?? (Session ? Session.GetComponent<T>() : null)
            ?? (Camera ? Camera.GetComponent<T>() : null)
            ?? (EventSystem ? EventSystem.GetComponent<T>() : null);

        /// <summary>
        /// Initialize this singleton component.
        /// </summary>
        /// <param name="session">The ARSession in the ARSupport scene.</param>
        /// <param name="origin">The AR Session Origin in the ARSupport scene.</param>
        /// <param name="camera">The Camera under the AR Session Origin.</param>
        /// <param name="eventSystem">The EventSystem in the ARSupport scene.</param>
        /// <returns>The initialized component.</returns>
        public static ARSecretary SetComponent(ARSession session, XROrigin origin, Camera camera, EventSystem eventSystem)
        {
            var secretary = Get();
            secretary.Session = session;
            secretary.Origin = origin;
            secretary.Camera = camera;
            secretary.EventSystem = eventSystem;
            return secretary;
        }

        private void Awake()
        {
            // Singleton behaviour
            Singleton.Awake(ref instance, this);
            // Do not unload its scene
            SceneSwitcher sceneSwitcher = SceneSwitcher.Get();
            sceneSwitcher.Keepers.Add(gameObject.scene);
            // Keep alive between scenes
            DontDestroyOnLoad(this.gameObject);

#if UNITY_EDITOR
            // Find Simulation Scene and keep it alive aswell
            StartCoroutine(ProtectSimulationScene());
#endif
        }

        private void OnDestroy() =>
            Singleton.OnDestroy(ref instance, this);

#if UNITY_EDITOR
        /// <summary>
        /// Finds the XR simulation environment scene and keep it alive during the application.
        /// </summary>
        /// <returns>The coroutine that seeks out the environment scene.</returns>
        private IEnumerator ProtectSimulationScene()
        {
            bool searching = true;
            Scene scene;
            while(searching)
            {
                for (int i = 0; searching && i < SceneManager.sceneCount; i++)
                {
                    scene = SceneManager.GetSceneAt(i);
                    if (!scene.name.Contains("Simulated Environment Scene"))
                        continue;

                    SceneSwitcher.Get().Keepers.Add(scene);
                    searching = false;
                }
                yield return null;
            }
            yield return null;
        }
#endif

        /// <summary>
        /// Get its current instance.
        /// Instantiate a new instance if necessary.
        /// </summary>
        /// <returns>An instance of itself.</returns>
        public static ARSecretary Get() =>
            Singleton.Get(ref instance, Instantiate);

        /// <summary>
        /// Instantiate a new instance of itself.
        /// </summary>
        /// <returns>An instance of itself.</returns>
        private static ARSecretary Instantiate() =>
            new GameObject("ARSecretary").AddComponent<ARSecretary>();
    }
}