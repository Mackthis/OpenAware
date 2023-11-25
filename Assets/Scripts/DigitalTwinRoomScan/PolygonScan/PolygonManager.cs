// /*                                                                                       *\
//     This program has been developed by students from the bachelor Computer Science at
//     Utrecht University within the Software Project course.
//
//     (c) Copyright Utrecht University (Department of Information and Computing Sciences)
// \*                                                                                       */

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AwARe;
using Unity.VisualScripting;
using UnityEngine;

namespace RoomScan
{
    public class PolygonManager : MonoBehaviour
    {
        [SerializeField] private PolygonDrawer polygonDrawer;
        [SerializeField] private PolygonMesh polygonMesh;
        [SerializeField] private PolygonScan scanner;

        [SerializeField] private GameObject createBtn;
        [SerializeField] private GameObject resetBtn;
        [SerializeField] private GameObject applyBtn;
        [SerializeField] private GameObject confirmBtn;
        [SerializeField] private GameObject endBtn;
        [SerializeField] private GameObject slider;
        [SerializeField] private GameObject pointerObj;

        /// <summary>
        /// All UI components of the polygon scan.
        /// </summary>
        List<GameObject> UIObjects;

        /// <summary>
        /// Gets the Room represented by the polygons.
        /// </summary>
        /// <value>A Room represented by the polygons.</value>
        public Room Room { get; private set; }

        /// <summary>
        /// Gets the polygon currently being drawn.
        /// </summary>
        public Polygon CurrentPolygon { get; private set; }

        public void Start()
        {
            CurrentPolygon = new Polygon();

            Room = new TestRoom();

            polygonDrawer.DrawRoomPolygons(Room);

            UIObjects = new()
            {
                createBtn, resetBtn, confirmBtn, slider, applyBtn, endBtn, pointerObj, scanner.gameObject, polygonMesh.gameObject
            };

            SwitchToState(State.Default);
        }

        /// <summary>
        /// Called on reset or create button click; starts a new polygon scan.
        /// </summary>
        public void StartScanning()
        {
            CurrentPolygon = new();
            polygonDrawer.Reset();
            SwitchToState(State.Scanning);
        }

        /// <summary>
        /// Called on apply button click; adds and draws the current polygon.
        /// </summary>
        public void OnApplyButtonClick()
        {
            polygonDrawer.ClearScanningLines();
            SwitchToState(State.SettingHeight);
            polygonMesh.SetPolygon(CurrentPolygon.GetPoints());

            polygonDrawer.DrawPolygon(CurrentPolygon, Room.PositivePolygon != null);
            Room.AddPolygon(CurrentPolygon);
        }

        /// <summary>
        /// Called on confirm button click; sets the height of the polygon.
        /// </summary>
        public void Confirm()
        {
            // TODO: set room height
            SwitchToState(State.Saving);
        }

        /// <summary>
        /// Called on changing the slider; sets the height of the polygon mesh.
        /// </summary>
        /// <param name="height">Height the slider is currently at.</param>
        public void OnSlider(System.Single height)
        {
            this.polygonMesh.SetHeight(height);
        }

        /// <summary>
        /// Sets all UIObjects to inactive, then activates all UIObjects of this state.
        /// </summary>
        /// <param name="toState">Which state the UI should switch to.</param>
        private void SwitchToState(State toState)
        {
            foreach (GameObject obj in UIObjects)
            {
                obj.SetActive(false);
            }

            foreach (GameObject obj in GetStateObjects(toState))
            {
                obj.SetActive(true);
            }
        }

        /// <summary>
        /// Gets the UI objects that need to be present in this state.
        /// </summary>
        /// <param name="state">A state of the scanning process.</param>
        /// <returns></returns>
        List<GameObject> GetStateObjects(State state)
        {
            List<GameObject> objects = new();
            switch (state)
            {
                case State.Default:
                    objects.Add(createBtn);
                    break;
                case State.Scanning:
                    objects.Add(applyBtn);
                    objects.Add(resetBtn);
                    objects.Add(scanner.gameObject);
                    objects.Add(pointerObj);
                    break;
                case State.SettingHeight:
                    objects.Add(confirmBtn);
                    objects.Add(slider);
                    objects.Add(polygonMesh.gameObject);
                    break;
                case State.Saving:
                    objects.Add(createBtn);
                    objects.Add(endBtn);
                    break;

            }
            return objects;
        }

        /// <summary>
        /// The different states within the polygon scanning process.
        /// </summary>
        enum State
        {
            Default,
            Scanning,
            SettingHeight,
            Saving
        }
    }
}