// /*                                                                                       *\
//     This program has been developed by students from the bachelor Computer Science at
//     Utrecht University within the Software Project course.
//
//     (c) Copyright Utrecht University (Department of Information and Computing Sciences)
// \*                                                                                       */

using System.IO;
using System.Linq;

using AwARe.IngredientList.Objects;
using AwARe.InterScenes.Objects;
using AwARe.Objects;
using AwARe.RoomScan.Polygons.Objects;
using AwARe.RoomScan.Path.Objects;

using UnityEngine;
using UnityEngine.TestTools;
using AwARe.UI;

using TMPro;
using System.Collections.Generic;

using AwARe.Data.Logic;

using Unity.IO.LowLevel.Unsafe;

using Room = AwARe.Data.Objects.Room;
using System.Collections;
using AwARe.Data.Objects;

namespace AwARe.RoomScan.Objects
{
    /// <summary>
    /// Contains the Room and handles the different states within the Polygon scanning.
    /// </summary>
    public class RoomManager : MonoBehaviour, IPointer
    {
        // Objects to control
        [SerializeField] private PolygonManager polygonManager;
        [SerializeField] private PathManager pathManager;
        [SerializeField] private RoomListOverviewScreen roomScreen;
        // [SerializeField] private VisualizePath pathVisualizer; //TODO: Get out of polygonScanning

        // The UI
        [SerializeField] private RoomUI ui;
        [SerializeField] private Transform canvas;
        [SerializeField] private Transform sceneCanvas;
        [SerializeField] private GameObject saveNameScreen;
        [SerializeField] private TMP_InputField inputName;

        // Templates
        [SerializeField] private GameObject roomBase;

        // Tracking
        private State stateBefore;

        /// <summary>
        /// Gets the current state of the room scanner.
        /// </summary>
        /// <value>
        /// The current state of the room scanner.
        /// </value>
        public State CurrentState { get; private set; } = State.Default;

        [ExcludeFromCoverage]
        private void Awake()
        {
            // Move all content prefab canvas to scene canvas.
            Mover.MoveAllChildren(canvas, sceneCanvas, true);

            // Instantiate a room to construct.
            Room = Instantiate(roomBase, transform).GetComponent<Room>();

            SwitchToState(State.Default);
        }

        /// <summary>
        /// Gets or sets the current room.
        /// </summary>
        /// <value>
        /// The current room.
        /// </value>
        public Room Room { get; set; }

        /// <summary>
        /// Gets the current position of the pointer.
        /// </summary>
        /// <value>
        /// The current position of the pointer.
        /// </value>
        public Vector3 PointedAt =>
            ui.PointedAt;

        /// <summary>
        /// Called when no UI element has been hit on click or press.
        /// </summary>
        public void OnUIMiss() =>
            polygonManager.OnUIMiss();

        /// <summary>
        /// Called on create button click.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnCreateButtonClick()
        {
            SwitchToState(State.Scanning);
            polygonManager.OnCreateButtonClick();
        }

        /// <summary>
        /// Called on reset button click.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnResetButtonClick() =>
            polygonManager.OnResetButtonClick();


        /// <summary>
        /// Called on apply button click.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnApplyButtonClick() =>
            polygonManager.OnApplyButtonClick();

        /// <summary>
        /// Called on confirm button click.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnConfirmButtonClick()
        {
            polygonManager.OnConfirmButtonClick();
            SwitchToState(State.Done);
        }

        /// <summary>
        /// Called on changing the slider.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnHeightSliderChanged(float value) =>
            polygonManager.OnHeightSliderChanged(value);

        /// <summary>
        /// Called on save button click; Stores the current room and switches to the home screen.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnSaveButtonClick()
        {
            SwitchToState(State.Saving);
            roomScreen.DisplayRoomLists();
        }


        public Data.Logic.Room ChooseRoom(string name)
        {
            List<Data.Logic.Room> listofrooms = LoadRoomList();
            // Now you can work with the list of Room objects
            return listofrooms.Where(obj => obj.RoomName == name).SingleOrDefault();
        }

        public void MakeRoom(Data.Logic.Room room)
        {
            Room.Data = room;
            Room.positivePolygon.GetComponent<Mesher>().UpdateMesh();
            Room.positivePolygon.GetComponent<Liner>().UpdateLine();
            foreach (var polygon in Room.negativePolygons)
            {
                polygon.GetComponent<Mesher>().UpdateMesh();
                polygon.GetComponent<Liner>().UpdateLine();
            }
            //pathManager.GenerateAndDrawPath();
        }

       
        public void OnConfirmSaveClick()
        {
            SaveLoadManager saveLoadManager = GetComponent<SaveLoadManager>();
            Debug.Log(Room.Data.RoomName);
            Storage.Get().ActiveRoom = Room.Data;
            Storage.Get().ActiveRoom.RoomName = inputName.text;

            stateBefore = CurrentState;

            // Load existing room list
            RoomListSerialization roomList = saveLoadManager.LoadRooms("rooms");

            if (roomList == null)
            {
                roomList = new RoomListSerialization();
            }

            // Add the current room to the list
            roomList.Rooms.Add(new RoomSerialization(Storage.Get().ActiveRoom));

            // Save the updated room list
            saveLoadManager.SaveRoomList("rooms", roomList);

            SwitchToState(State.Default);
        }

        public List<Data.Logic.Room> LoadRoomList()
        {
            
            SaveLoadManager saveLoadManager = GetComponent<SaveLoadManager>();

            // Ensure that saveLoadManager is not null before proceeding
            if (saveLoadManager == null)
            {
                Debug.LogError("SaveLoadManager is null.");
                return new List<Data.Logic.Room>();
            }
            

            RoomListSerialization roomListSerialization = saveLoadManager.LoadRooms("rooms");

            if (roomListSerialization == null)
            {
                //Debug.LogError("RoomListSerialization is null.");
                return new List<Data.Logic.Room>();
            }
            Debug.Log(roomListSerialization.Rooms?.Select(roomSerialization => roomSerialization.ToRoom()).ToList() ?? new List<Data.Logic.Room>());

            // Convert RoomListSerialization to a list of Room objects
            return roomListSerialization.Rooms?.Select(roomSerialization => roomSerialization.ToRoom()).ToList() ?? new List<Data.Logic.Room>();
        }
       
        /// <summary>
        /// Called on load button button click; changes state so user sees load slots.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnLoadButtonClick()
        {
            stateBefore = CurrentState; 
            SwitchToState(State.Loading);
        }

        /// <summary>
        /// Called on continue button click.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnContinueClick() =>
            SwitchToState(stateBefore);

        /// <summary>
        /// Called on path button click.
        /// </summary>
        [ExcludeFromCoverage]
        public void OnPathButtonClick() =>
            pathManager.OnPathButtonClick();

        /// <summary>
        /// Sets all Objects activities to match new state.
        /// </summary>
        /// <param name="state">The new state switched to.</param>
        private void SwitchToState(State state)
        {
            CurrentState = state;
            SetActive();
        }

        /// <summary>
        /// Sets activity of components.
        /// </summary>
        /// <param name="state">Current/new state.</param>
        [ExcludeFromCoverage]
        public void SetActive()
        {
            if (CurrentState == State.Scanning && !(pathManager.IsActive || polygonManager.IsActive))
                CurrentState = State.Done;
            
            // Set UI activity
            ui.SetActive(this.CurrentState, polygonManager.CurrentState, pathManager.CurrentState);
        }

    }

    /// <summary>
    /// The different states within the Room scanning process.
    /// </summary>
    public enum State
    {
        Default,
        Scanning,
        Done,
        Saving,
        Loading,
    }
}