// /*                                                                                       *\
//     This program has been developed by students from the bachelor Computer Science at
//     Utrecht University within the Software Project course.
//
//     (c) Copyright Utrecht University (Department of Information and Computing Sciences)
// \*                                                                                       */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AwARe.InterScenes.Objects
{
    /// <summary>
    /// An implementation of the Scene Secretary fit for AR.
    /// The Scene Secretary is an adapter on top of the Scene Manager.
    /// </summary>
    public class SceneSecretary : MonoBehaviour, ISceneSecretary
    {
        // Tracking
        private int isBusy;
        private bool IsBusy => isBusy > 0;

        /// <inheritdoc/>
        public HashSet<Scene> Keepers { get; private set; } = new();

        /// <inheritdoc/>
        public YieldInstruction LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            AsyncOperation Load() => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            void SetActive() => SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            return StartCoroutine(DoLoading(Load, SetActive, mode));
        }

        /// <inheritdoc/>
        public YieldInstruction LoadScene(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single)
        {
            AsyncOperation Load() => SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
            void SetActive() => SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex));

            return StartCoroutine(DoLoading(Load, SetActive, mode));
        }

        /// <summary>
        /// Helper method and body of the LoadScene methods.
        /// </summary>
        /// <param name="loadScene">The method of loading.</param>
        /// <param name="setActive">The method of setting the active scene.</param>
        /// <param name="mode">Specify whether to keep other scenes loaded.</param>
        /// <returns>An enumerator for the coroutine.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid LoadSceneMode is thrown.</exception>
        private IEnumerator DoLoading(Func<AsyncOperation> loadScene, Action setActive, LoadSceneMode mode)
        {
            // Atomic Lock
            if (BusyLock())
                yield return null;
            else
            {
                // Loading strategy
                IEnumerator loading =
                    mode switch
                    {
                        LoadSceneMode.Additive => DoLoading_Additive(loadScene),
                        LoadSceneMode.Single   => DoLoading_Single(loadScene, setActive),
                        _                      => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
                    };
                while (loading.MoveNext())
                    yield return loading.Current;

                // Unlock
                BusyUnlock();
                yield return null;
            }
        }

        private bool BusyLock() =>
            Interlocked.CompareExchange(ref isBusy, 1, 0) > 0;

        private void BusyUnlock() =>
            Interlocked.Exchange(ref isBusy, 0);

        /// <summary>
        /// Helper method and body of the LoadScene methods.
        /// The LoadSceneMode is set to Single.
        /// </summary>
        /// <param name="loadScene">The method of loading.</param>
        /// <param name="setActive">The method of setting the active scene.</param>
        /// <returns>An enumerator for the coroutine.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid LoadSceneMode is thrown.</exception>
        private IEnumerator DoLoading_Single(Func<AsyncOperation> loadScene, Action setActive)
        {
            // Get all scenes to Unload
            IEnumerable<Scene> toUnload = GetScenesWithoutKeepers();

            // Load new scene and set to active
            yield return loadScene();
            setActive();

            // Unload scenes to unload
            IEnumerator unloader = UnloadScenes(toUnload);
            while (unloader.MoveNext())
                yield return unloader.Current;
        }

        /// <summary>
        /// Helper method and body of the LoadScene methods.
        /// The LoadSceneMode is set to Single.
        /// </summary>
        /// <param name="loadScene">The method of loading.</param>
        /// <returns>An enumerator for the coroutine.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid LoadSceneMode is thrown.</exception>
        private IEnumerator DoLoading_Additive(Func<AsyncOperation> loadScene)
        { yield return loadScene(); }

        /// <summary>
        /// Helper method of the LoadScene methods.
        /// Gets all loaded scenes without scenes that should never be unloaded.
        /// </summary>
        /// <returns>A collection of all scenes to unload.</returns>
        private IEnumerable<Scene> GetScenesWithoutKeepers()
        {
            int nroScenes = SceneManager.sceneCount;
            IEnumerable<Scene> scenes = from i in Enumerable.Range(0, nroScenes) select SceneManager.GetSceneAt(i);
            HashSet<Scene> keepers = Keepers;
            return from scene in scenes where !keepers.Contains(scene) select scene;
        }

        /// <summary>
        /// Helper method of the LoadScene methods.
        /// Unload all given scenes.
        /// </summary>
        /// <param name="scenes">All scenes to unload.</param>
        /// <returns>An enumerator for a coroutine.</returns>
        private IEnumerator UnloadScenes(IEnumerable<Scene> scenes)
        {
            // Unload all scenes asynchronous
            var operations = new List<AsyncOperation>();
            foreach (Scene scene in scenes)
                operations.Add(SceneManager.UnloadSceneAsync(scene));

            // Wait till all finished
            foreach (AsyncOperation operation in operations)
                yield return new WaitUntil(() => operation.isDone);
        }
    }
}