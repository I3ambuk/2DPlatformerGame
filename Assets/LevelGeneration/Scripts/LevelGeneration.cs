using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    private struct Room
    {
        public GameObject prefab;
        public List<Vector2Int> neighbours;

        public Room(GameObject pref, List<Vector2Int> neighb)
        {
            prefab = pref;
            neighbours = neighb;
        }
    }
    private struct Cell
    {
        public Vector2 pos;
        public Room? room;
        public bool isEmpty()
        {
            return room == null;
        }
        public Cell(Vector2 pos)
        {
            this.pos = pos;
            this.room = null;
        }
    }
    public GameObject RoomLR;
    public GameObject RoomLRB;
    public GameObject RoomLRT;
    public GameObject RoomLRBT;
    private List<Room> rooms = new List<Room>();

    public Vector2Int[] startingPositions = new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0)};
    public float step;

    public Transform upperLeftCorner;
    private Cell[,] positionArray = new Cell[4,4];
    private Vector2Int currentPos;
    //TODO: Schleife und Abbruchbedingung
    //TODO: Prüfen der Ausrichtung der Räume

    // Start is called before the first frame update
    void Start()
    {
        rooms.Add(new Room(RoomLR, new List<Vector2Int>() { new Vector2Int(-1, 0), new Vector2Int(1, 0) }));
        rooms.Add(new Room(RoomLRB, new List<Vector2Int>() { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0,-1) }));
        rooms.Add(new Room(RoomLRT, new List<Vector2Int>() { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0,1) }));
        rooms.Add(new Room(RoomLRBT, new List<Vector2Int>() { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0,-1), new Vector2Int(0,1) }));


        for (int x = 0; x < positionArray.GetLength(0); x++) {
            for (int y = 0; y < positionArray.GetLength(1); y++) {
                Vector2 position = upperLeftCorner.position + new Vector3(x * step, -y * step, 0f);
                positionArray[x, y] = new Cell(position);
            }
        }

        int randStartingPos_idx = Random.Range(0, startingPositions.Length);
        currentPos = startingPositions[randStartingPos_idx];
        InstanstiateRoom(rooms[0], currentPos);

        NextRoom();
        NextRoom();
        NextRoom();
        NextRoom();
        NextRoom();
        NextRoom();
        NextRoom();
        NextRoom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void InstanstiateRoom(Room room, Vector2Int pos_idx)
    {

        Vector2 position = positionArray[pos_idx.x, pos_idx.y].pos;
        positionArray[pos_idx.x, pos_idx.y].room = room;
        Instantiate(room.prefab, position, Quaternion.identity);
    }
    private bool testRoom(Vector2Int neighbour)
    {
        Room currentRoom = (Room) positionArray[currentPos.x, currentPos.y].room;
        Vector2Int neighbourPos = currentPos + neighbour;

        bool neighbourAcess = currentRoom.neighbours.Contains(-neighbour);
        bool neighbourIsEmpty = positionArray[neighbourPos.x, neighbourPos.y].isEmpty();
        //currentRoom.neighbours.ForEach(p => Debug.Log(p));

        return neighbourAcess && neighbourIsEmpty;
    }

    private void NextRoom()
    {
        List<Vector2Int> possibleRoomIdx = new List<Vector2Int>();
        
        //Test Left:
        if (currentPos.x > 0 && testRoom(new Vector2Int(-1,0)))
        {
            possibleRoomIdx.Add(new Vector2Int(currentPos.x -1, currentPos.y));
        }
        //Test Right:
        if (currentPos.x + 1 < positionArray.GetLength(0) && testRoom(new Vector2Int(1, 0)))
        {
            possibleRoomIdx.Add(new Vector2Int(currentPos.x + 1, currentPos.y));
        }
        //Test UP:
        if (currentPos.y > 0 && testRoom(new Vector2Int(0, -1)))
        {
            possibleRoomIdx.Add(new Vector2Int(currentPos.x, currentPos.y - 1));
        }
        //Test Down:
        if (currentPos.y + 1 < positionArray.GetLength(1) && testRoom(new Vector2Int(0, 1)))
        {
            possibleRoomIdx.Add(new Vector2Int(currentPos.x, currentPos.y + 1));
        }
        Debug.Log(currentPos.ToString());
        //possibleRoomIdx.ForEach(p => Debug.Log(p));

        //TODO: if (possibleDirs.Count == 0 || Weg lang genug). Abbruch Der Generation des Hauptpfades
        if (possibleRoomIdx.Count == 0)
        {
            return;
        }
        int rand_idx_dir = Random.Range(0, possibleRoomIdx.Count);;//TODO: Next Room random with right door
        List<Room> roomsFiltered = rooms.FindAll(room => room.neighbours.Contains(-(currentPos - possibleRoomIdx[rand_idx_dir])));

        int rand_idx_room = Random.Range(0, roomsFiltered.Count);

        InstanstiateRoom(roomsFiltered[rand_idx_room], possibleRoomIdx[rand_idx_dir]);
        currentPos = possibleRoomIdx[rand_idx_dir];
    }
}
