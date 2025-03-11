using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    public int numberOfRooms = 15;
    public float doubleRoomChance = 0.5f;
    public GameObject roomPrefab;
    public GameObject doubleRoomPrefab;
    public GameObject startRoomPrefab;
    public GameObject itemRoomPrefab;

    private GameObject BossRoomPrefab;

    private List<Room> rooms = new List<Room>();


    struct Room
    {
        public Vector2 position;
        public GameObject roomObject;

        public bool isDoubleRoom;

        public Vector2 secondPosition;
    }

    void createRoom(Vector2 position, GameObject roomPrefab, int direction = -1, bool isDoubleRoom = false, Vector2 secondPosition = new Vector2())
    {
        Vector3 position3 = new Vector3(position.x, 0, position.y);
        if (direction == -1)
            direction = Random.Range(0, 4);
        Quaternion rotation = intToQuaternion(direction);
        rooms.Add(new Room { position = position, roomObject = Instantiate(roomPrefab, position3 * 9, rotation), isDoubleRoom = isDoubleRoom, secondPosition = secondPosition });
    }

    public bool isAvailable(Vector2 position)
    {
        foreach (Room room in rooms)
        {
            if (room.position == position || room.secondPosition == position)
            {
                return false;
            }
        }
        return true;
    }

    public int placeForDoubleRoom(Vector2 position)
    {
        int valid = 0b1111;
        if (!isAvailable(position))
        {
            return 0;
        }
        for (int i = 0; i < 4; i++)
        {
            Vector2 direction = new Vector2(Mathf.Cos(i * Mathf.PI / 2), Mathf.Sin(i * Mathf.PI / 2));
            if (numberOfNeighbours(position + direction) != 0)
            {
                valid &= ~(1 << i);
            }
        }
        return valid;
    }

    void createDoobleRoom(Vector2 position, int valid = -1)
    {
        if (valid == -1)
            valid = placeForDoubleRoom(position);
        if (valid == 0)
            return;
        int direction = 0;
        while ((valid & (1 << direction)) == 0)
        {
            direction = Random.Range(0, 4);
        }
        createRoom(position, doubleRoomPrefab, direction, true, position + intToVector2(direction));
    }

    Vector2 getRandomDirection()
    {
        int i = Random.Range(0, 4);
        return new Vector2(Mathf.Cos(i * Mathf.PI / 2), Mathf.Sin(i * Mathf.PI / 2));
    }

    Vector2 intToVector2(int i)
    {
        return new Vector2(Mathf.Cos(i * Mathf.PI / 2), Mathf.Sin(i * Mathf.PI / 2));
    }

    Quaternion intToQuaternion(int i)
    {
        Vector2 dir = intToVector2(i);
        return Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y));
    }


    int numberOfNeighbours(Vector2 position)
    {
        int count = 0;
        foreach (Room room in rooms)
        {
            if (room.position == position || room.secondPosition == position)
            {
                count++;
            }
        }
        return count;
    }

    int numberOfNeighbours(int valid) {
        int count = 0;
        for (int i = 0; i < 4; i++) {
            if ((valid & (1 << i)) == 0) {
                count++;
            }
        }
        return count;
    }

    string QuaternionToDir(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        Vector2 dir = new Vector2(Mathf.Cos(euler.y * Mathf.PI / 180), Mathf.Sin(euler.y * Mathf.PI / 180));
        if (dir.x > 0.5)
            return "rig";
        if (dir.x < -0.5)
            return "lef";
        if (dir.y < -0.5)
            return "top";
        return "bot";
    }

    void placeDoor(Transform doors, Vector2 position, Quaternion roomRotation)
    {
        for (int i = 0; i < 4; i++) {
            Vector2 direction = intToVector2(i);
            Quaternion dir = intToQuaternion(i);
            dir = roomRotation * dir;
            string doorDir = QuaternionToDir(dir);
            if (doors == null) {
                continue;
            }
            GameObject door = doors.Find("wall-" + doorDir)?.gameObject;
            if (door == null) {
                continue;
            }
            if (isAvailable(position + direction)) {
                door.SetActive(true);
            } else {
                door.SetActive(false);
            }
        }
    }

    void placeDoors() {
        foreach (Room room in rooms) {
            Vector2 position = room.position;
            Quaternion roomRotation = room.roomObject.transform.rotation;
            roomRotation = Quaternion.Euler(0, 360 - (roomRotation.eulerAngles.y + 180), 0);
            if (room.isDoubleRoom) {
                Transform doors = room.roomObject.transform.Find("WALLDOORS");
                placeDoor(doors, position, roomRotation);
                Transform secondDoors = room.roomObject.transform.Find("WALLDOORS2");
                placeDoor(secondDoors, room.secondPosition, roomRotation);
            } else {
                Transform doors = room.roomObject.transform.Find("WALLDOORS");
                placeDoor(doors, position, roomRotation);
            }
        }
    }

    void Start()
    {
        createRoom(Vector2.zero, startRoomPrefab);

        while (rooms.Count < numberOfRooms) {
            Room room = rooms[Random.Range(0, rooms.Count)];
            Vector2 direction = getRandomDirection();
            Vector2 position = (room.isDoubleRoom ? room.secondPosition : room.position) + direction;
            if (isAvailable(position)) {
                int valid = placeForDoubleRoom(position);
                int neighbours = numberOfNeighbours(valid);
                if (neighbours != 1)
                    continue;
                if (Random.value < doubleRoomChance && valid != 0) {
                    createDoobleRoom(position, valid);
                } else {
                    createRoom(position, roomPrefab);
                }
            }
        }
        placeDoors();
    }
}
