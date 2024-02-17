using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerGrid : MonoBehaviour
{
    public int width; //Tile Coords
    public int height; //Tile Coords
    //Width and height are currently used as external fringes for drag and drop for the UI Buildings snap to grid.
    private int CL; //Current Length (So far)
    private int CH; //Current Height (So far)
    public float cellSize;
    public float xModifier; //Since the square is now diagonal, it has unequal proportions. This means it needs a specific multiplier for each side.
    public float yModifier; //The multiplier appears to be dispersed evenly, x subtracting and y adding.
    private Vector3 originPosition; //Not set up to use negative numbers
    public GameObject[,] TileArray;
    //public List<Tile> TileList = new List<Tile>();
    public int MovementTF; //For unit movement and airforce attack movement... doesn't allow ClickingEvents script to register drag clicks while this is true

    public GameObject tilePreset;
    public GameObject tileFolder;
    public GameObject GameManager;
    public GameObject tileGO; //Currently selected tile
    public Vector2 tileGOCoords;
    public bool TileClicked;//Used for the tile buttons to tell if a tile has been clicked or not... if clicked, then move the unit on the second click, if not, then select the tile and bring up it's tabs
    public Sprite TileIMG;

    public int MouseclickNumber;
    UnityEvent e_mouseClick; //e_ stands for an event... use with all future events
    //This activates events in the start function (Add Listener)
    public bool ClickActionTaken;
    public Vector2 currentlyClickedTile;
    private Vector3 MousePositionOnClickAction;

    void Start()
    {
        CH = 0;
        CL = 0;
        CreateGrid(width, height, cellSize, new Vector3(0, 0)); //Creating the grid

        MouseclickNumber = 0;
        if (e_mouseClick == null)
            e_mouseClick = new UnityEvent();
        e_mouseClick.AddListener(MouseClickEvent);
        currentlyClickedTile = new Vector2(0, 0);
        ClickActionTaken = false;
    }

    public void CreateGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        TileArray = new GameObject[width, height];

        for (int i = 0; i < width * height; i++)
        {
            GameObject newTile = (GameObject)Instantiate(tilePreset);
            newTile.SetActive(true);
            newTile.transform.SetParent(tileFolder.gameObject.transform);
            newTile.transform.rotation = Quaternion.Euler(0, 0, 45);
            TileArray[CL, CH] = newTile;
            //newTile.GetComponent<TileProperties>().tile = TileList[i];
            newTile.GetComponent<TileProperties>().spriteRenderer.sprite = TileIMG;//newTile.GetComponent<TileProperties>().tile.ReturnTileGraphics();
            newTile.GetComponent<TileProperties>().xCoord = CL;
            newTile.GetComponent<TileProperties>().yCoord = CH;

            //HPBar Material Creation
            //Material newRadialFill = (Material)Instantiate(GameManager.GetComponent<GameManager>().HPBarRadialMaterial);
            //newTile.GetComponent<TileProperties>().unitTileGO.GetComponent<UnitImageScript>().HPImage.GetComponent<SpriteRenderer>().material = newRadialFill;
            //This is how you change the hp of the health bar...newTile.GetComponent<TileProperties>().unitTileGO.GetComponent<UnitImageScript>().HPBarUpdate();

            var x = (CL * cellSize * xModifier); //Size of a 2.25 scale hexagon is 5 screen units long/tall
            var y = 1f;
            
            if (CL % 2 == 1) //Hexagonal grids have very similar modification properties as 45 degree square grids.
            {
                y = (CH * cellSize * yModifier) - (cellSize / 2 * yModifier);
            }
            else
            {
                y = (CH * cellSize * yModifier);
            }
            newTile.transform.position = new Vector3(x, y, 0f);

            if (CL == width - 1)
            {
                CH += 1;
                CL = 0;
            }
            else
            {
                CL += 1;
            }
        }
        //SetUpMapDetails();
    }


    //#####  Beginning of Click Events  #####
    public void ClickAction(int clickID, Vector3 MousePos) //Called from GameManager/Clicking Events on mouse down.
    {
        //print("Click Registered");
        MousePositionOnClickAction = MousePos;
        if (clickID == 1) //Click
        {
            ClickEvent();
        }
        else if (clickID == 2) //Drag
        {
            //Does nothing, because we don't want the board to be selected when dragging, only when clicking
        }
    }

    private void ClickEvent()
    {
        if (e_mouseClick != null)
        {
            MouseclickNumber += 1;
            //Broadcast Clicking Events
            if (MouseclickNumber == 1)
                e_mouseClick.Invoke();
            if (MouseclickNumber == 2)
            {
                MovementTF = 0;
                StartCoroutine(ButtonClickedCoroutine(.1f));
            }
        }
    }

    IEnumerator ButtonClickedCoroutine(float waitTime)
    {
        float counter = 0;

        while (counter < waitTime)
        {
            //Increment Timer until counter >= waitTime
            counter += Time.deltaTime;
            yield return null;
        }
        if (ClickActionTaken)
        {
            MouseclickNumber -= 1;
            ClickActionTaken = false;
        }
        else
        {
            e_mouseClick.Invoke();
            MouseclickNumber = 0;
        }
    }

    void MouseClickEvent()
    {
        if (MouseclickNumber == 1)
        {
            if (MovementTF == 0)
            {
                int x, y;
                GetXY(MousePositionOnClickAction, out x, out y);
                OpenTileMenu(x, y);
            }
        }
        if (MouseclickNumber == 2)
        {
            //print("Click Off");
            ActionTaken();
        }
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y) //out int allows for a return of multiple values from a single function
    {
        /*
        Summary: Since X and Y are vertical and horizontal, we analyze each verical and horizontal square (0,0),(2,0),(0,1),etc. (x goes by 2, y goes by 1)
        We take each of these squares, and determine if you are clicking inside or outside of them. 
        If outside, add or remove x/y values to correspond to the correct tile coordinate.
        */

        var cellSizeXMod = cellSize * xModifier * 2; //X Length between cells.
        var cellSizeYMod = cellSize * yModifier; //Y Length between cells.
        var cellSizeModifierMultiplier = (cellSizeXMod / cellSizeYMod); //Converts Y units to X units

        var oldX = (worldPosition.x + (cellSizeXMod / 2)); //Adjust Mouse X and Y to (0,0) starting at the bottom left of cell (0,0)
        var oldY = (worldPosition.y + (cellSizeYMod / 2));

        var baseX = Mathf.FloorToInt(oldX / cellSizeXMod); //Adjusts the Mouse X and Y to straight horizontal and vertical axis coordinates
        var baseY = Mathf.FloorToInt(oldY / cellSizeYMod);

        var newX = (oldX - (cellSizeXMod * baseX)); //These are the local (To the specific tile) x and y coordinates of your click. Remember this click is a square shape, not the final diamond.
        var newY = (oldY - (cellSizeYMod * baseY));

        var finalX = baseX * 2; //These are the final x and y values that will be returned back. 
        var finalY = baseY; //The x value is doubled because in one tile length, there are two tiles, one centered, and one to the right and below.


        if (newX <= 3.65)
        {
            var localX = newX; //This and localY are the x and y values local to the pos/neg sides of the tile graph, if the center is 0. 
            //However, the x here is simply subtracted, not mirrored (negatives being left and positives being right).
            if (newY <= 3.175) //Left Bottom
            {
                var localY = (newY * cellSizeModifierMultiplier);
                if (localX + localY < 3.65) //If Pos Ratio
                {//Converting Y to X units, then checking if the ratio is positive or negative. If positive, it's over the line and into the next tile.
                    finalX -= 1;
                    finalY += 0;
                }
            }
            else //Left Top
            {
                var localY = ((newY - (cellSizeYMod / 2)) * cellSizeModifierMultiplier);
                if (localX < localY) //If Pos Ratio
                {
                    finalX -= 1;
                    finalY += 1;
                }
            }
            
        }
        else
        {
            var localX = (newX - (cellSizeXMod / 2));
            if (newY <= 3.175) //Right Bottom
            {
                var localY = (newY * cellSizeModifierMultiplier);
                if (localX > localY)
                {
                    finalX += 1;
                    finalY += 0;
                }
            }
            else //Right Top
            {
                var localY = ((newY - (cellSizeYMod / 2)) * cellSizeModifierMultiplier);
                if (localX + localY > 3.65)
                {
                    finalX += 1;
                    finalY += 1;
                }
            }
            
        }

        x = finalX;
        y = finalY;
        //print("Final Pos: " + x + "," + y);

        /*
         * Old Function
        x = Mathf.FloorToInt((worldPosition.x + (cellSize / 2)) / cellSize);
        y = 1;
        if (x % 2 == 1)
        {
            y = Mathf.FloorToInt((worldPosition.y + cellSize) / cellSize);
        }
        else
        {
            y = Mathf.FloorToInt((worldPosition.y + (cellSize / 2)) / cellSize); //- cellSize
        }
        */
    }

    public void OpenTileMenu(int x, int y) //All the data and actions associated with clicking on that individual tile.
    {
        if (x <= width - 1 && x >= 0 && y <= height - 1 && y >= 0) //for borders
        {
            tileGO = TileArray[x, y];
            tileGOCoords = new Vector2(x, y);
            var tile = tileGO.GetComponent<TileProperties>();
            var startingTile = tileGO; //The tile you clicked on
            //print("Tile Sucessfully Clicked!");
            tileGO.GetComponent<TileProperties>().TileSelected(); //Starts selection Bars
        }
        else
        {
            MouseclickNumber = 0;
            if (tileGO != null)
                tileGO.GetComponent<TileProperties>().TileDeselected();
        }
    }

    public void ActionTaken()
    {
        MovementTF = 0;
        //HideAllButtons();
        tileGO.GetComponent<TileProperties>().TileDeselected();
    }
    //#####  End of Click Events  #####
}
