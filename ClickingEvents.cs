using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickingEvents : MonoBehaviour
{
    private Vector3 ClickMoveDistance = new Vector3(10f, 10f, 0f);
    private Vector3 ClickInitialDistance;
    private Vector3 ClickIntervalsAddition;
    public Camera cam;
    private int Clicking = 0; //0 is Not Clicking, 1 is Click, 2 is Drag
    private PlayerGrid Grid;
    private GameManager GameManager;
    public GameObject gridGO;
    public bool draggingObjectTF; //If true, it is dragging an object. If false, dragging moves the screen.
    public int draggingObjectNum; //1 = initial, 2 = get rid of.

    private void Start()
    {
        GameManager = this.GetComponent<GameManager>();
        Grid = gridGO.GetComponent<PlayerGrid>();
        draggingObjectTF = false;
        draggingObjectNum = 0;
    }


    void Update()//This does effect the Camera Movement (Script CameraMovement)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Clicking = 1;
            ClickInitialDistance = Input.mousePosition;
            StartCoroutine(ClickWaitTime());

            if (draggingObjectNum == 1) // Part of the Building Scroll Bar Dragging System
            {
                draggingObjectNum = 2;
            }
        }
        if (Input.GetMouseButtonUp(0) && draggingObjectTF && draggingObjectNum == 2) //If the player has released the drag, finalize the drag position.
        {// Part of the Building Scroll Bar Dragging System
            draggingObjectTF = false;
            this.GetComponent<UIManager>().FinalizeBuildingPosition();
        }
    }

    IEnumerator ClickWaitTime()
    {
        float counter = 0;
        Vector3 MousePos = GetMouseWorldPosition();

        while (counter < .1f)
        {
            //Increment Timer until counter >= waitTime
            counter += Time.deltaTime;
            yield return null;
        }
        Vector3 mousePos = Input.mousePosition;
        ClickIntervalsAddition = mousePos - ClickInitialDistance;
        if ((Mathf.Abs(ClickIntervalsAddition.x) >= ClickMoveDistance.x || Mathf.Abs(ClickIntervalsAddition.y) >= ClickMoveDistance.y))//Dragging
        {
            Clicking = 2;
        }
        var clickPosition = cam.ScreenToWorldPoint(ClickInitialDistance);
        if (Grid.MovementTF == 0)
        {
            Grid.ClickAction(Clicking, clickPosition);
        }
        else if (Grid.MovementTF == 1)
        {
            if (Clicking == 1)
            {
                Grid.ClickAction(Clicking, clickPosition);
            }
        }
        else if (Grid.MovementTF == 2)
        {
            Grid.ClickAction(Clicking, clickPosition);
        }
    }

    //Get Mouse Position in world with Z = 0f (2d)
    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }
    public static Vector3 GetMouseWorldPositionWithZ()
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    }
    public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
    }
    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }
}
