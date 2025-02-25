// /*                                                                                       *\
//     This program has been developed by students from the bachelor Computer Science at
//     Utrecht University within the Software Project course.
//
//     (c) Copyright Utrecht University (Department of Information and Computing Sciences)
// \*                                                                                       */

using System.Collections.Generic;
using System.Linq;
using AwARe.RoomScan.Path;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using AwARe.DevTools.ObjectGeneration;
#endif
namespace AwARe.ObjectGeneration
{
    public class ObjectPlacer
    {
        /// <summary>
        /// Tries to place a <paramref name="renderable"/> at <paramref name="position"/>.
        /// </summary>
        /// <param name="renderable">Renderable object to place in the scene.</param>
        /// <param name="position">Exact position at which we will try to place the renderable.</param>
        /// <param name="room">The room containing the polygons in which we will try to place.</param>
        /// <returns>Whether the object has been placed.</returns>
        private bool TryPlaceObject(
            Renderable renderable,
            Vector3 position,
            Data.Logic.Room room,
            PathData path)
        {
            // Check if the box of the new object will overlap with any other colliders
            Vector3 boxCenter = position;
            boxCenter.y += renderable.HalfExtents.y;
            if (Physics.CheckBox(
                boxCenter,
                renderable.HalfExtents,
                Quaternion.identity,
                LayerMask.GetMask("Placed Objects"))) //only check collisions with other materials.
                return false;

            // Check if the collider doesn't cross the positive Polygon's border
            List<Vector3> objectCorners = Renderable.CalculateColliderCorners(renderable, position);
            if (!PolygonHelper.ObjectColliderAllInPolygon(objectCorners, room.PositivePolygon))
                return false;

            // check if any of the objects corners lie on the path
            if (path != null && objectCorners.Any(corner => path.PointLiesOnPath(corner)))
                return false;

            // If the point does not lie on a negative polygon
            if (position.y == room.PositivePolygon.points[0].y)
                // Check if the collider isn't inside a Negative polygon
                foreach (Data.Logic.Polygon negativePolygon in room.NegativePolygons)
                    if (PolygonHelper.ObjectColliderAnyInPolygon(objectCorners, negativePolygon))
                        return false;

            // Adjust object size according to scalar
            GameObject newObject = Object.Instantiate(renderable.Prefab, position, Quaternion.identity);
            newObject.layer = LayerMask.NameToLayer("Placed Objects");
            newObject.transform.localScale =
                new Vector3(renderable.Scaling, renderable.Scaling, renderable.Scaling);

            if (renderable.ResourceType == ResourcePipeline.Logic.ResourceType.Water)
            {
                //The pivot point of water is in the center of the cube. Move it down.
                Vector3 trsf = new(0, renderable.HalfExtents.y, 0);
                newObject.transform.Translate(trsf); //move half extent up, otherwise its inside the world.

                //Set text to the watercube labels
                string newLabelText = "Water\n" + ((float)renderable.Quantity).ToString() + "L";
                TextMeshPro[] textMeshProComponents = newObject.GetComponentsInChildren<TextMeshPro>();
                foreach (TextMeshPro textMeshPro in textMeshProComponents)
                    textMeshPro.text = newLabelText;
                renderable.Quantity = 0;
            }

            // Add collider after changing object size
            BoxCollider bc = newObject.AddComponent<BoxCollider>();

            #if UNITY_EDITOR
                BoxColliderVisualizer visualBox = new(bc);
            #endif

            return true;
        }

        /// <summary>
        /// Automatically place a list of renderables by first initializing a clulster for each renderable. 
        /// After the cluster have been initialized
        /// each 'round' one object of the renderable with the lowest area usage will be generated. 
        /// </summary>
        /// <param name="renderables">All items that we are going to place.</param>
        /// <param name="room">Room to place the renderables in.</param>
        /// <param name="path">Path in the room.</param>
        public void PlaceRenderables(
            List<Renderable> renderables, 
            Data.Logic.Room room, 
            PathData path)
        {
            if (renderables.Count == 0)
                return;

            // 1. Get valid spawnpoints
            PolygonSpawnPointHandler spawnPointHandler = new();
            List<Vector3> validSpawnPoints = spawnPointHandler.GetValidSpawnPoints(room, path);

            // 2. Initialize clusters where one object of each group is placed
            Dictionary<Renderable, Vector3> initialSpawnsDictionary = InitializeClusters(validSpawnPoints, renderables, room, path);

            // 3. Each 'round' spawn or stack one of the renderables with the lowest area usage.
            bool allQuantitiesZero = false;
            while (!allQuantitiesZero)
            {
                allQuantitiesZero = true;
                Renderable renderableToPlace = GetRenderableWithLowestAreaUsage(initialSpawnsDictionary.Keys.ToList());

                if (renderableToPlace != null && renderableToPlace.Quantity > 0)
                {
                    float availableSurfaceArea = EstimateAvailableSurfaceArea(validSpawnPoints.Count);
                    Vector3 initialSpawnPoint = initialSpawnsDictionary[renderableToPlace];

                    bool placed = TrySpawnOrStackRenderable(
                        renderableToPlace,
                        initialSpawnPoint,
                        validSpawnPoints,
                        availableSurfaceArea,
                        room,
                        path);

                    renderableToPlace.Quantity -= 1;
                    if (!placed) Debug.Log("Could not place this object");

                    // Check if there are any renderables left to spawn
                    if (initialSpawnsDictionary.Keys.Any(r => r.Quantity > 0))
                        allQuantitiesZero = false;
                }
            }
        }

        /// <summary>
        /// Tries to spawn one object of a given renderable. If it could not be placed on the ground it will try to stack. 
        /// </summary>
        /// <param name="renderable">The specific renderable we are going to place.</param>
        /// <param name="initialSpawnPoint">The initial spawn point for the cluster of this renderable.</param>
        /// <param name="validSpawnPoints">All spawnPoints at which it could be placed.</param>
        /// <param name="availableSurfaceArea">The percentage of surface area that this renderable is allowed to use.</param>
        /// <param name="room">Room to place the renderables in.</param>
        /// <returns>Whether the object could either be placed on the ground or stacked.</returns>
        private bool TrySpawnOrStackRenderable(
            Renderable renderable, 
            Vector3 initialSpawnPoint, 
            List<Vector3> validSpawnPoints, 
            float availableSurfaceArea, 
            Data.Logic.Room room,
            PathData path)
        {
            // sort available spawn points by closest distance to initial spawn point
            validSpawnPoints = SortClosestSpawnPointsByDistance(initialSpawnPoint, validSpawnPoints);

            foreach (var point in validSpawnPoints)
            {
                // check if area-ratio is allowed 
                float ratioUsage = renderable.HalfExtents.x * renderable.HalfExtents.z * 4 / availableSurfaceArea;
                if (renderable.CurrentRatioUsage + ratioUsage < renderable.AllowedSurfaceUsage)
                {
                    // try placing
                    bool hasPlaced = TryPlaceObject(renderable, point, room, path);
                    if (hasPlaced)
                    {
                        float height = point.y + 2 * renderable.HalfExtents.y;
                        renderable.CurrentRatioUsage += ratioUsage;
                        renderable.ObjStacks.Add(point, height);
                        return true;
                    }
                    else { } // try again at next closest spawn point
                }
            }

            return TryStack(renderable, room, path);
        }




        /// <summary>
        /// Try to stack the object on one of the objStacks in the given renderable until successful or out of stacks.
        /// </summary>
        /// <param name="renderable">Renderable to place.</param>
        /// <param name="room">Room to place the renderables in.</param>
        /// <returns>Whether the stacking was successful.</returns>
        private bool TryStack(
            Renderable renderable,
            Data.Logic.Room room,
            PathData path)
        {
            while (renderable.ObjStacks.Keys.ToList().Count > 0)
            {
                // 1. find the smallest stack.
                float smallestStackHeight = float.MaxValue;
                Vector3 smallestStackPos = Vector3.zero;
                foreach (KeyValuePair<Vector3, float> kvp in renderable.ObjStacks)
                {
                    if (kvp.Value < smallestStackHeight)
                    {
                        smallestStackHeight = kvp.Value;
                        smallestStackPos = kvp.Key;
                    }
                }
                if (smallestStackHeight == float.MaxValue) //error scenario
                {
                    return false;
                }

                // 2. check if it doesnt reach through the roof.
                float stackHeight = renderable.ObjStacks[smallestStackPos];
                float newHeight = stackHeight + renderable.HalfExtents.y * 2 + 0.05f;
                float maxHeight = room.PositivePolygon.height;
                //prevent placement if this stack will reach higher than 'x' meters with the additional current object on top
                if (newHeight > maxHeight)
                {
                    renderable.ObjStacks.Remove(smallestStackPos);
                    return false;
                }

                // 3. Test placement on new spot, check for collisions.
                Vector3 newPos = smallestStackPos;
                newPos.y = stackHeight + 0.05f;
                bool hasPlaced = TryPlaceObject(renderable, newPos, room, path);
                if (hasPlaced)
                {
                    renderable.ObjStacks[smallestStackPos] = newHeight;
                    return true;
                }
                else
                    renderable.ObjStacks.Remove(smallestStackPos);
            }


            //Out of available stacks for this gameObject:
            return false;
        }

        /// <summary>
        /// Estimate the surface area of the spawn Polygon by squaring the distance between the points
        /// and multiplying this by a factor (not all space is usable on a sloped line).
        /// </summary>
        /// <param name="spawnPointCount">The amount of spawnPoints.</param>
        /// <returns>Estimated surface area.</returns>
        private float EstimateAvailableSurfaceArea(int spawnPointCount) =>
            spawnPointCount * 0.1f * 0.1f * 0.9f;

        /// <summary>
        /// Extract all renderables of <see cref="ResourceType"> water, and place those at the front of the list.
        /// This is done to ensure that it will be placed at the same location for the plant and the animal room.
        /// </summary>
        /// <param name="renderables">Renderables to be sorted.</param>
        private void SortWaterRenderables(ref List<Renderable> renderables)
        {
            List<Renderable> waterRenderables = renderables.Where(
                x => x.ResourceType == ResourcePipeline.Logic.ResourceType.Water).ToList();

            renderables.RemoveAll(x => x.ResourceType == ResourcePipeline.Logic.ResourceType.Water);

            renderables.InsertRange(0, waterRenderables);
        }

        /// <summary>
        /// Spawns one object of each renderable at a valid spawnoint. The initial objects are spawned as far apart from eachother as possible.
        /// </summary>
        /// <param name="spawnPoints">All the allowed spawn points in the Polygon.</param>
        /// <param name="renderables">All of the renderables that need to be spawned.</param>
        /// <param name="room">Room to place the renderables in.</param>
        /// <returns>A dictionary of the initial cluster spawnpoint for each renderable (Renderable, InitialSpawnPoint).</returns>
        private Dictionary<Renderable, Vector3> InitializeClusters(
            List<Vector3> spawnPoints, 
            List<Renderable> renderables, 
            Data.Logic.Room room,
            PathData path)
        {
            Dictionary<Renderable, Vector3> initialSpawns = new();

            SortWaterRenderables(ref renderables);

            foreach (var renderable in renderables)
            {
                // sort all spawnpoint with furthest distance to other initial spawnpoints first 
                List<Vector3> sortedSpawnPoints = SortFurthestSpawnPointsByDistance(
                    spawnPoints, initialSpawns.Values.ToList(), renderable.ComputeSpaceNeeded());
                foreach (var point in sortedSpawnPoints)
                {
                    if (TryPlaceObject(renderable, point, room, path))
                    {
                        if (renderable.ResourceType == ResourcePipeline.Logic.ResourceType.Water)
                            renderable.Quantity = 0;
                        else
                            renderable.Quantity -= 1; // remove one renderable if the initial object is spawned
                        initialSpawns.Add(renderable, point);
                        break; // Exit the loop once a valid point is found
                    }
                }

                if (!initialSpawns.ContainsKey(renderable))
                {
                    // Handle the case where no valid initial spawn point could be found
                    Debug.Log("NO AVAILABLE INITIAL SPAWN POINT!!!");
                }
            }

            return initialSpawns;
        }

        /// <summary>
        /// Sorts the list of available spawnpoints by furthest distance from the already occupied spawnpoints. 
        /// </summary>
        /// <param name="validSpawnPoints">All the allowed spawnpoints in the Polygon.</param>
        /// <param name="occupiedPoints">All of the already occupied spawn points.</param>
        /// <returns>The list of spawnpoints sorted by furthest distance from the occupied points.</returns>
        private List<Vector3> SortFurthestSpawnPointsByDistance(
            List<Vector3> validSpawnPoints,
            List<Vector3> occupiedPoints,
            float totalAreaRequired
            )
        {
            return validSpawnPoints.OrderByDescending(
                 point => CalculateWeightedDistance(point, occupiedPoints, totalAreaRequired)).ToList();
        }

        private float CalculateWeightedDistance(Vector3 point, List<Vector3> occupiedPoints, float totalAreaRequired)
        {
            float nearestDistance = occupiedPoints.Count > 0 ?
                                    occupiedPoints.Min(occupied => Vector3.Distance(point, occupied)) :
                                    float.MaxValue;

            float weight = 1 / totalAreaRequired;
            return nearestDistance * weight;
        }

        /// <summary>
        /// Sorts the list of available spawnpoints by shortest distance from the initial spawnpoint. 
        /// </summary>
        /// <param name="initialSpawnPoint">The initial spawnpoint.</param>
        /// <param name="validSpawnPoints">All the allowed spawnpoints in the Polygon.</param>
        /// <returns>The list of spawnpoints sorted by shortest distance from the initial spawnpoint.</returns>
        private List<Vector3> SortClosestSpawnPointsByDistance(
            Vector3 initialSpawnPoint,
            List<Vector3> validSpawnPoints)
        {
            return validSpawnPoints.OrderBy(point => Vector3.Distance(initialSpawnPoint, point)).ToList();
        }

        /// <summary>
        /// Calculates the renderable with the lowest area in use from the given list of renderables. 
        /// </summary>
        /// <param name="renderables">All the renderables that will be taken into the calculation.</param>
        /// <returns>The renderable with the lowest area usage.</returns>
        private Renderable GetRenderableWithLowestAreaUsage(List<Renderable> renderables)
        {
            return renderables
                .Where(r => r.Quantity > 0)
                .OrderBy(r => r.CurrentRatioUsage)
                .FirstOrDefault();
        }
    }
}