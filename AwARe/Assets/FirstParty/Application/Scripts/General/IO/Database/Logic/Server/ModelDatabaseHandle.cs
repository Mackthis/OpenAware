// /*                                                                                       *\
//     This program has been developed by students from the bachelor Computer Science at
//     Utrecht University within the Software Project course.
//
//     (c) Copyright Utrecht University (Department of Information and Computing Sciences)
// \*                                                                                       */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwARe.ResourcePipeline.Logic;
using AwARe.Server.Logic;
using UnityEngine;

namespace AwARe.Database.Logic
{
    /// <summary>
    /// Implementation of the Model Database interface, that uses the remote database.
    /// Currently unused.
    /// </summary>
    public class ModelDatabaseHandle : IModelDatabase
    {
        [Serializable]
        struct GetModelRequestBody
        {
            public int id;
        }

        [Serializable]
        struct ModelResponse
        {
        }

        /// <inheritdoc/>
        public Task<Model> GetModel(int id)
        {
            var model = Client.GetInstance().Get<GetModelRequestBody, Model>("ingr/getModel", new GetModelRequestBody
            {
                id = id
            }).Then((res) =>
            {
                Debug.Log(res);

                ModelResponse response = JsonUtility.FromJson<ModelResponse>(res);

                return null;

            }).Catch((err) =>
                {
                    if (err.StatusCode == 403)
                    {
                        // Unauthorized. User must login.
                        Debug.LogError("Failed to get model from the server. You're not logged in.");
                    }
                    else
                    {
                        Debug.LogError("Failed to get model from the server.: [" + err.StatusCode + "] " + err.ServerMessage);
                    }
                }).Send();

            return model;
        }

        /// <inheritdoc/>
        public Task<Model[]> GetModels(IEnumerable<int> ids)
        {
            List<Task<Model>> tasks = new();
            foreach (int id in ids)
            {
                tasks.Add(this.GetModel(id));
            }
            return Task.WhenAll(tasks);
        }
    }
}

