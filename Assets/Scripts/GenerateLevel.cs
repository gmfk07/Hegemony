using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenerateLevel : MonoBehaviour {

    public int columns;
    public int rows;
    public float blockXScale;
    public float blockYScale;
    public float blockZScale;
    public GameObject wall;
    public GameObject door;
    public GameObject floor;
    enum Direction { none, up, down, left, right };
    GameObject[,] map;
    List<Vector2Int> targets = new List<Vector2Int>();

	// Use this for initialization
	void Start () {
        map = new GameObject[columns, rows];
        var surface = FillMap(columns, rows);
        PlaceRandomRoom(5, 20, 5, 20);
        for (var i = 0; i < 15; i++)
        {
            PlaceHallway(4, 10, true);
            PlaceRoom(3, 8, 3, 8);
        }
        surface.BuildNavMesh();
    }

    // Create a wall at a given position using the block scale vars, returns the created object
    GameObject CreateWall(float x, float y, float z)
    {
        Vector3 position = new Vector3(x, y, z);
        return CreateObject(position, wall, "Wall");
    }

    // Creates a door
    GameObject CreateDoor(float x, float y, float z)
    {
        Vector3 position = new Vector3(x, y, z);
        return CreateObject(position, door, "Door");
    }

    // Creates an object and gives it a name using the block scale vars, returns the created object
    GameObject CreateObject(Vector3 position, GameObject obj, string name)
    {
        GameObject created = GameObject.Instantiate(obj, position, Quaternion.identity);
        created.transform.localScale = new Vector3(blockXScale, blockYScale, blockZScale);
        created.name = name;
        return created;
    }

    // Fills up the map and 2d array, along with floor
    NavMeshSurface FillMap(float columns, float rows)
    {
        for (var i = 0; i < columns; i++)
        {
            for (var j = 0; j < rows; j++)
            {
                map[i, j] = CreateWall(j * blockYScale, 0, i * blockXScale);
            }
        }
        var created = Instantiate(floor, new Vector3(0, 0, 0), Quaternion.identity);
        created.transform.localScale = new Vector3(rows * blockYScale, 1, columns * blockXScale);
        return created.GetComponentInChildren<NavMeshSurface>();
    }

    // Randomly looks for an empty square
    Vector2Int GetEmptySquare(int columns, int rows)
    {
        while (true)
        {
            int x = Random.Range(1, columns - 2);
            int y = Random.Range(1, rows - 2);
            if (map[x,y] == null)
                return new Vector2Int(x, y);
        }
    }

    // Keeps trying to place a random room somewhere on the level
    void PlaceRandomRoom(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        bool result = false;
        while (!result)
        {
            int w = Random.Range(minWidth, maxWidth);
            int h = Random.Range(minHeight, maxHeight);
            int x = Random.Range(1, columns - 1 - w);
            int y = Random.Range(1, rows - 1 - h);
            result = TryMakingRectangle(x, y, w, h);
        }
    }

    // Keeps trying to place a room on some target
    void PlaceRoom(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        while (true)
        {
            Vector2Int coords = GetRandomInList(targets);
            var x = coords.x; var y = coords.y;
            Direction dir = GetBuildDirection(x, y);
            var w = Random.Range(minWidth, maxWidth);
            var h = Random.Range(minHeight, maxHeight);
            var limiter = MaxHallwayLength(x, y, dir);
            int xoff = Random.Range(0, w - 1);
            int yoff = Random.Range(0, h - 1);
            if (limiter < minWidth || limiter < minHeight)
                continue;
            switch (dir)
            {
                case Direction.up:
                    if (h > limiter - 1)
                        h = limiter - 1;
                    if (TryMakingRectangle(x - xoff, y - h, w, h))
                    {
                        ClearRectangle(x, y, 1, 1, false);
                        PlaceDoor(x, y);
                        return;
                    }
                    break;
                case Direction.right:
                    if (w > limiter - 1)
                        w = limiter - 1;
                    if (TryMakingRectangle(x + 1, y - yoff, w, h))
                    {
                        ClearRectangle(x, y, 1, 1, false);
                        PlaceDoor(x, y);
                        return;
                    }
                    break;
                case Direction.down:
                    if (h > limiter - 1)
                        h = limiter - 1;
                    if (TryMakingRectangle(x - xoff, y + 1, w, h))
                    {
                        ClearRectangle(x, y, 1, 1, false);
                        PlaceDoor(x, y);
                        return;
                    }
                    break;
                case Direction.left:
                    if (w > limiter - 1)
                        w = limiter - 1;
                    if (TryMakingRectangle(x - w, y - yoff, w, h))
                    {
                        ClearRectangle(x, y, 1, 1, false);
                        PlaceDoor(x, y);
                        return;
                    }
                    break;
            }
        }
    }

    // Keeps trying to place a hallway on some target
    void PlaceHallway(int minLength, int maxLength, bool allowFusing)
    {
        while (true)
        {
            Vector2Int coords = GetRandomInList(targets);
            if (TryMakingHallway(coords.x, coords.y, minLength, maxLength, allowFusing))
            {
                targets.Remove(coords);
                return;
            }
        }
    }

    // Tries to make a hallway given a block as the door/starting point. Returns true/false based on success.
    bool TryMakingHallway(int x, int y, int minLength, int maxLength, bool allowFusing = false)
    {
        var dir = GetBuildDirection(x, y);
        var limit = MaxHallwayLength(x, y, dir);
        if (limit < minLength)
            return false;
        var length = Random.Range(minLength, maxLength);
        if (length > limit)
            length = limit;
        bool result = PlaceHallwayWithDirection(x, y, length, dir);

        //If result is false, it failed because of an attempted fusing
        if (!result)
        {
            if (!allowFusing)
                return false;
            else
            {
                if (!PlaceHallwayWithDirection(x, y, length - 1, dir, true))
                    return false;
            }
        }

        ClearRectangle(x, y, 1, 1, false);
        PlaceDoor(x, y);

        return true;
    }

    // Try to place a hallway of a given length at a given position with a given direction.
    // The bool doorAfter determines whether or not to add a door after the hallway.
    bool PlaceHallwayWithDirection(int x, int y, int length, Direction dir, bool doorAfter=false)
    {
        bool result;
        if (dir == Direction.right)
        {
            result = TryMakingRectangle(x + 1, y, length, 1);
            if (doorAfter && result)
            {
                ClearRectangle(x + length + 1, y, 1, 1, false);
                PlaceDoor(x + length + 1, y);
            }
            return result;
        }
        if (dir == Direction.left)
        {
            result = TryMakingRectangle(x - length, y, length, 1);
            if (doorAfter && result)
            {
                ClearRectangle(x - length - 1, y, 1, 1, false);
                PlaceDoor(x - length - 1, y);
            }
            return result;
        }
        if (dir == Direction.up)
        {
            result = TryMakingRectangle(x, y - length, 1, length);
            if (doorAfter && result)
            {
                ClearRectangle(x, y - length - 1, 1, 1, false);
                PlaceDoor(x, y - length - 1);
            }
            return result;
        }
        if (dir == Direction.down)
        {
            result = TryMakingRectangle(x, y + 1, 1, length);
            if (doorAfter && result)
            {
                ClearRectangle(x, y + length + 1, 1, 1, false);
                PlaceDoor(x, y + length + 1);
            }
            return result;
        }
        return false;
    }

    // Checks if a rectangle is valid, and if so, carves it into the level
    bool TryMakingRectangle(int x, int y, int w, int h, bool addTargets=true)
    {
        if (CheckInBounds(x,y,w,h) && CheckFull(x - 1, y, w + 2, h) && CheckFull(x, y - 1, w, 1) && CheckFull(x, y + h, w, 1))
        {
            ClearRectangle(x, y, w, h, addTargets);
            return true;
        }
        return false;
    }

    // Returns what's at a grid position without errors: null if nothing, the object's name if otherwise
    string GetGridName(int x, int y)
    {
        if (map[x, y] == null)
            return null;
        else
            return map[x, y].name;
    }

    // Checks if a rectangle is both in bounds and full of walls
    bool CheckValid(int x, int y, int w, int h)
    {
        return (CheckInBounds(x, y, w, h) && CheckFull(x, y, w, h));
    }

    // Check if a rectangle is within the bounds of the stage and doesn't include border blocks
    bool CheckInBounds(int x, int y, int w, int h)
    {
        if (x <= 0 || x + w >= rows || y <= 0 || y + h >= columns)
            return false;
        return true;
    }

    // Check if a rectangle is full of walls, where x and y are the top-left corner
    bool CheckFull(int x, int y, int w, int h)
    {
        for (var i = x; i < x + w; i++)
        {
            for (var j = y; j < y + h; j++)
            {
                if (GetGridName(i,j) != "Wall")
                    return false;
            }
        }
        return true;
    }

    // Clear a rectangular area, provided the area is full
    void ClearRectangle(int x, int y, int w, int h, bool addTargets=true)
    {
        for (var i = x; i < x + w; i++)
        {
            //Add top and bottom to targets
            AddTarget(i, y - 1, addTargets);
            AddTarget(i, y + h, addTargets);

            for (var j = y; j < y + h; j++)
            {
                GameObject.Destroy(map[i, j]);
                map[i, j] = null;

                //Add left and right to targets
                AddTarget(x - 1, j, addTargets);
                AddTarget(x + w, j, addTargets);
            }
        }
    }

    // Adds a tile to targets unless that tile is out of bounds or on the border, and removes tiles from targets that were already there
    // Note: the addTargets bool determines whether targets are actually added or just removing duplicates.
    void AddTarget(int x, int y, bool addTargets)
    {
        if (y > 0 && y < rows - 1 && x > 0 && x < columns - 1)
        {
            var to_add = new Vector2Int(x, y);
            if (targets.Contains(to_add))
                targets.Remove(to_add);
            else if (addTargets)
                targets.Add(to_add);
        }
    }

    // Gets a random Vector2Int from a list
    Vector2Int GetRandomInList(List<Vector2Int> list)
    {
        int val = (int) Mathf.Floor(Random.Range(0, list.Count));
        return list[val];
    }

    // Places a door and updates the map
    void PlaceDoor(int x, int y)
    {
        map[x, y] = CreateDoor(y * blockYScale, 0, x * blockXScale);
    }

    // Given a grid position not on a border, returns which direction expansion should take place in or null
    Direction GetBuildDirection(int x, int y)
    {
        if (GetGridName(x - 1, y) == null && GetGridName(x + 1, y) == "Wall")
            return Direction.right;
        if (GetGridName(x + 1, y) == null && GetGridName(x - 1, y) == "Wall")
            return Direction.left;
        if (GetGridName(x, y - 1) == null && GetGridName(x, y + 1) == "Wall")
            return Direction.down;
        if (GetGridName(x, y + 1) == null && GetGridName(x, y - 1) == "Wall")
            return Direction.up;
        return Direction.none;
    }

    // Given a grid position for a door and a direction, returns how many squares a potential hallway could be.
    // Hallways are stopped by anything that isn't a wall or the border of the map.
    int MaxHallwayLength(int x, int y, Direction dir)
    {
        if (dir == Direction.none)
            return 0;
        var i = 1;
        while (true)
        {
            if ((dir == Direction.right && !CheckValid(x + 1, y, i, 1)) ||
                 (dir == Direction.left && !CheckValid(x - i, y, i, 1)) ||
                 (dir == Direction.down && !CheckValid(x, y + 1, 1, i)) ||
                 (dir == Direction.up && !CheckValid(x, y - i, 1, i)))
                return i - 1;
            i++;
        }
    }
}
