using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelGeneration_Neu : MonoBehaviour
{
    //TEST
    public GameObject TestMain;
    public GameObject TestSide;
    public GameObject TestStart;
    public GameObject TestEnd;
    public GameObject Player;
    //
    public float roomSize = 20;
    public int LevelShape_x = 5;
    public int LevelShape_y = 5;
    public int maxlengthOfMainPath = 5;
    public Transform upperLeftCorner;
    public GameObject Empty;
    public GameObject RoomL;
    public GameObject RoomR;
    public GameObject RoomT;
    public GameObject RoomB;
    public GameObject RoomLR;
    public GameObject RoomLT;
    public GameObject RoomLB;
    public GameObject RoomRT;
    public GameObject RoomRB;
    public GameObject RoomTB;
    public GameObject RoomLRB;
    public GameObject RoomLRT;
    public GameObject RoomLTB;
    public GameObject RoomRTB;
    public GameObject RoomLRBT;

    private GenerateGrid Generator;
    private GenerateGrid.Cell[,] grid;
    private GameObject[,] roomGrid = new GameObject[4,4];
    private Dictionary<(bool l, bool r, bool t, bool b), GameObject> roomMap;
    void Start()
    {
        Generator = new GenerateGrid(LevelShape_x, LevelShape_y, maxlengthOfMainPath);

        Transform ROOMS = this.transform.Find("Rooms").transform;
        Transform RoomTypeTags = this.transform.Find("RoomTypeTags");
        //Setze welcher Eingänge die jeweiligen Räume haben
        roomMap = new Dictionary<(bool l, bool r, bool t, bool b), GameObject>()
        {
            { (false, false, false, false), Empty},
            { (true, false, false, false), RoomL},
            { (false, true, false, false), RoomR},
            { (false, false, true, false), RoomT},
            { (false, false, false, true), RoomB},
            { (true, true, false, false), RoomLR},
            { (true, false, true, false), RoomLT},
            { (true, false, false, true), RoomLB},
            { (false, true, true, false), RoomRT},
            { (false, true, false, true), RoomRB},
            { (false, false, true, true), RoomTB},
            { (true, true, false, true), RoomLRB},
            { (true, true, true, false), RoomLRT},
            { (true, false, true, true), RoomLTB},
            { (false, true, true, true), RoomRTB},
            { (true, true, true, true), RoomLRBT},

        };
        //Generiere den Grid für die Räume und instanziiere passende Prefabs
        grid = Generator.Generate();
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                GameObject room = PickRoom(grid[x, y]);
                Vector2 pos = (Vector2) upperLeftCorner.position + roomSize * new Vector2(x,-y);
                Instantiate(room, pos, Quaternion.identity).transform.parent = ROOMS;
                
                //Instanziiere den Player Spawn im Start Raum und für Testzwecke die die Test Objekte
                if (grid[x,y].type == GenerateGrid.CellType.Main)
                {
                    Instantiate(TestMain, pos, Quaternion.identity).transform.parent = RoomTypeTags; ;
                } else if(grid[x, y].type == GenerateGrid.CellType.Side)
                {
                    Instantiate(TestSide, pos, Quaternion.identity).transform.parent = RoomTypeTags;
                }
                else if (grid[x, y].type == GenerateGrid.CellType.Start)
                {
                    Instantiate(Player, pos, Quaternion.identity).transform.parent = this.transform.parent;
                }
                else if (grid[x, y].type == GenerateGrid.CellType.End)
                {
                    Instantiate(TestEnd, pos, Quaternion.identity).transform.parent = RoomTypeTags;
                }
            }
        }
    }

    private GameObject PickRoom(GenerateGrid.Cell cell)
    {
        List<Vector2Int> edges = cell.markedEdges;
        bool l = edges.Contains(Vector2Int.left);
        bool r = edges.Contains(Vector2Int.right);
        bool t = edges.Contains(Vector2Int.down); //grid Origin upper left corner
        bool b = edges.Contains(Vector2Int.up); //grid Origin upper left corner
        if (cell.type == GenerateGrid.CellType.EMPTY)
        {
            return Empty;
        }
        return roomMap[(l, r, t, b)];
    }
    
}
