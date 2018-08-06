using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenerateLevel : MonoBehaviour {

    public int columns;
    public int rows;
    public float doorHeight = 1.5f;
    public float blockScale;
    public float blockHeight;
    public GameObject wall;
    public GameObject door;
    public GameObject floor;
    public GameObject guard;
    public GameObject obstacle1x1;
    public GameObject obstacle2x1;
    public GameObject obstacle2x2;
    public GameObject start;
    public GameObject objective;
    enum Direction { none, up, down, left, right };
    enum Obstacle { o1x1, o2x1, o2x2, start, objective };
    GameObject[,] map;
    List<Vector2Int> targets = new List<Vector2Int>();
    List<Vector3> guardWaypoints = new List<Vector3>();
    private GameObject startInstance;

    //roomList is full of arrays of rooms in the format [x, y, w, h, i] where x, y are top-left and o is # of objectives
    List<int[]> roomList = new List<int[]>();

    private List<Vector3> gizmoPos = new List<Vector3>();

	// Use this for initialization
	void Start () {
        map = new GameObject[columns, rows];
        var surface = FillMap(columns, rows);
        PlaceRandomRoom(5, 20, 5, 20);
        for (var i = 0; i < 20; i++)
        {
            if (Random.Range(0, 100) < 80)
                PlaceHallway(4, 10, true);
            PlaceRoom(3, 8, 3, 8);
        }
        surface.BuildNavMesh();
        PlaceObjective(Obstacle.start, 0);
        PlaceObjective(Obstacle.objective, 1);
        PlaceObjective(Obstacle.objective, 1);
        PlaceObjective(Obstacle.objective, 1);
        PlaceObstaclesInRooms(0.1f, 0.25f);
        PlaceGuard(7, 10, 4, 10, 7, 30);
        PlaceGuard(7, 10, 4, 10, 7, 30);
        PlaceGuard(7, 10, 4, 10, 7, 30);
        PlaceGuard(7, 10, 4, 10, 7, 30);
        PlaceGuard(7, 10, 4, 10, 7, 30);
    }

    // Create a wall at a given position using the block scale vars, returns the created object
    GameObject CreateWall(float x, float y, float z)
    {
        Vector3 position = new Vector3(x, y, z);
        GameObject created = GameObject.Instantiate(wall, position, Quaternion.identity);
        created.transform.localScale = new Vector3(blockScale, blockHeight, blockScale);
        created.name = "Wall";
        return created;
    }

    // Creates a door
    GameObject CreateDoor(float x, float y, float z)
    {
        Vector3 position = new Vector3(x, y, z);
        GameObject created = GameObject.Instantiate(door, position, Quaternion.identity);
        created.transform.localScale = new Vector3(blockScale, blockScale*doorHeight, blockScale);
        created.name = "Door";
        CreateWall(x, y + (blockScale * doorHeight), z);
        return created;
    }

    // Creates an obstacle
    GameObject CreateObstacle(float x, float y, float z, float yRotate, GameObject obj)
    {
        Vector3 position = new Vector3(x, y, z);
        Quaternion rotate = Quaternion.Euler(0, yRotate, 0);
        GameObject created = GameObject.Instantiate(obj, position, Quaternion.identity);
        created.transform.localScale *= blockScale;
        created.name = "Obstacle";
        created.transform.rotation = rotate;
        return created;
    }

    // Creates an object and gives it a name using the block scale vars, returns the created object
    GameObject CreateObject(Vector3 position, GameObject obj, string name)
    {
        GameObject created = GameObject.Instantiate(obj, position, Quaternion.identity);
        created.transform.localScale = new Vector3(blockScale, blockScale, blockScale);
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
                map[i, j] = CreateWall(j * blockScale, 0, i * blockScale);
            }
        }
        var createdCeiling = Instantiate(floor, new Vector3(0, blockHeight, 0), Quaternion.identity);
        createdCeiling.transform.localScale = new Vector3(rows * blockScale, 1, columns * blockScale);

        var createdFloor = Instantiate(floor, new Vector3(0, 0, 0), Quaternion.identity);
        createdFloor.transform.localScale = new Vector3(rows * blockScale, 1, columns * blockScale);

        return createdFloor.GetComponentInChildren<NavMeshSurface>();
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

    // Places the given objective in a room, ignoring all rooms with o value above maxObjectives
    void PlaceObjective(Obstacle obs, int maxObjectives)
    {
        while (true)
        {
            var room = roomList[Random.Range(0, roomList.Count)];
            if (room[4] > maxObjectives)
                continue;
            if (RandomlyPlaceObstacle(obs, room, 30))
            {
                if (obs == Obstacle.start)
                    room[4] += 4;
                else
                    room[4] += 1;
                return;
            }
        }
    }

    //Given the coordinates for a grid position and a distance, returns all empty squares that distance away
    HashSet<Vector2Int> GetEmptySquaresAtDistance(int x, int y, int distance)
    {
        HashSet<Vector2Int> squaresSet = new HashSet<Vector2Int>();
        int yamt;

        for (int xdiff = -distance; xdiff <= distance; xdiff ++)
        {
            yamt = distance - Mathf.Abs(xdiff);
            foreach (int ydiff in new List<int> { yamt, -yamt })
            {
                //Is the square in the map?
                if (x + xdiff > 0 && x + xdiff < columns - 1 && y + ydiff > 0 && y + ydiff < columns - 1)
                    //Is the square empty?
                    if (map[x + xdiff, y + ydiff] == null)
                        squaresSet.Add(new Vector2Int(x + xdiff, y + ydiff));
            }

        }

        return squaresSet;
    }

    //Checks if a tile has an object with a given name adjacent to it - true means it does, false means it doesn't
    bool HasAdjacentObject(int x, int y, string name, bool includeDiagonals)
    {
        if (GetGridName(x - 1, y) == name || GetGridName(x + 1, y) == name ||
            GetGridName(x, y - 1) == name || GetGridName(x, y + 1) == name)
            return true;
        else
        {
            if (includeDiagonals && (GetGridName(x - 1, y - 1) == name || GetGridName(x + 1, y - 1) == name ||
                GetGridName(x - 1, y + 1) == name || GetGridName(x + 1, y + 1) == name))
                return true;
            return false;
        }
    }

    //Tests if a tile is empty, is not bordered by a door, and is not bordered by an obstacle
    bool TilePlaceable(int x, int y)
    {
        if (HasAdjacentObject(x, y, "Door", false) || HasAdjacentObject(x, y, "Obstacle", true) || GetGridName(x, y) != null)
            return false;
        return true;
    }

    //Try placing a given obstacle at a given location, returns t/f
    bool TryPlacingObstacle(Obstacle obs, int x, int y)
    {
       switch (obs)
       {
            case Obstacle.o1x1:
                if (TilePlaceable(x, y))
                {
                    map[x,y] = CreateObstacle(y * blockScale, 0, x * blockScale, 0, obstacle1x1);
                    return true;
                }
                break;

            case Obstacle.objective:
                if (TilePlaceable(x, y))
                {
                    map[x, y] = CreateObstacle(y * blockScale, 0, x * blockScale, 0, objective);
                    return true;
                }
                break;

            case Obstacle.o2x1:
                if (TilePlaceable(x, y) && TilePlaceable(x + 1, y))
                {
                    var created = CreateObstacle(y * blockScale, 0, x * blockScale, 0, obstacle2x1);
                    map[x, y] = created;
                    map[x + 1, y] = created;
                    return true;
                }
                else if (TilePlaceable(x - 1, y) && TilePlaceable(x - 1, y + 1))
                {
                    var created = CreateObstacle(y * blockScale, 0, x * blockScale, 90, obstacle2x1);
                    map[x - 1, y] = created;
                    map[x - 1, y + 1] = created;
                    return true;
                }
                break;

            case Obstacle.o2x2:
                if (TilePlaceable(x, y) && TilePlaceable(x + 1, y) && TilePlaceable(x, y + 1) && TilePlaceable(x + 1, y + 1))
                {
                    var created = CreateObstacle(y * blockScale, 0, x * blockScale, 0, obstacle2x2);
                    map[x, y] = created;
                    map[x + 1, y] = created;
                    map[x, y + 1] = created;
                    map[x + 1, y + 1] = created;
                    return true;
                }
                break;

            case Obstacle.start:
                if (TilePlaceable(x, y) && TilePlaceable(x + 1, y) && TilePlaceable(x, y + 1) && TilePlaceable(x + 1, y + 1))
                {
                    var created = CreateObstacle(y * blockScale, 0, x * blockScale, 0, start);
                    startInstance = created;
                    map[x, y] = created;
                    map[x + 1, y] = created;
                    map[x, y + 1] = created;
                    map[x + 1, y + 1] = created;
                    return true;
                }
                break;
        }
        return false;
    }

    //Finds a random empty space in a room without an adjacent door and places the given obstacle there
    bool RandomlyPlaceObstacle(Obstacle obstacle, int[] room, int maxTries)
    {
        int currentTries = 0;
        while (currentTries < maxTries)
        {
            int chosenx = room[0] + Random.Range(0, room[2]);
            int choseny = room[1] + Random.Range(0, room[3]);

            if (TryPlacingObstacle(obstacle, chosenx, choseny))
                return true;

            currentTries += 1;
        }
        return false;
    }

    //Given minDensity and maxDensity (obstacles to tiles ratio), fills up each room
    void PlaceObstaclesInRooms(float minDensity, float maxDensity)
    {
        foreach (int[] room in roomList)
        {
            float density = Random.Range(minDensity, maxDensity);
            int area = room[2] * room[3];
            int already_covered = room[4];
            int coveredTarget = Mathf.FloorToInt(area * density);
            int currentCovered = already_covered;
            List<Obstacle> potentialObstacles = new List<Obstacle> {Obstacle.o1x1, Obstacle.o2x1, Obstacle.o2x2};

            while (currentCovered < coveredTarget)
            {
                int remainder = coveredTarget - currentCovered;
                if (remainder <= 2)
                    potentialObstacles.Remove(Obstacle.o2x1);
                else if (remainder <= 4)
                    potentialObstacles.Remove(Obstacle.o2x2);
                Obstacle obs = potentialObstacles[Random.Range(0, potentialObstacles.Count)];
                bool success = RandomlyPlaceObstacle(obs, room, 30);
                if (success)
                {
                    if (obs == Obstacle.o1x1)
                        currentCovered += 1;
                    if (obs == Obstacle.o2x1)
                        currentCovered += 2;
                    if (obs == Obstacle.o2x2)
                        currentCovered += 4;
                }
                else break;
            }
        }
    }

    //Tests that pos is at least minGuardSpacing away from all other waypoints
    bool CheckGuardDistance(Vector3 pos, float minGuardSpacing)
    {
        foreach (Vector3 waypoint in guardWaypoints)
        {
            if (!CheckDistance(pos, waypoint, minGuardSpacing))
                return false;
        }
        return true;
    }

    //Returns t/f based on whether or not pathfinding considers pos at least minGuardSpacing away from waypoint
    bool CheckDistance(Vector3 pos, Vector3 waypoint, float minSpacing)
    {
        NavMeshPath testPath = new NavMeshPath();
        NavMesh.CalculatePath(pos, waypoint, NavMesh.AllAreas, testPath);
        if (GetPathLength(testPath) < minSpacing)
            return false;

        return true;
    }

    //

    /*private void OnDrawGizmos()
    {
        foreach (Vector3 pos in gizmoPos)
            Gizmos.DrawSphere(pos, 4);
    }*/

    //Given a guard object, a set from GetEmptySquaresAtDistance(), an idealDistance and an epsilon, returns the
    //Vector3 position closest to idealDistance in the hashSet within epsilon or a (-1, -1, -1) Vector3 if impossible
    Vector3 GetIdealPosition(GameObject guardObject, HashSet<Vector2Int> potentialSet, float idealDistance, float epsilon, float minGuardSpacing)
    {
        Vector3 destinationPos = new Vector3();
        NavMeshAgent guardAgent = guardObject.GetComponent<NavMeshAgent>();
        float bestDistance = Mathf.Infinity;

        foreach (Vector2Int pos in potentialSet)
        {
            Vector3 testPos = new Vector3(pos[1] * blockScale + blockScale * .5f, guardObject.transform.position.y, pos[0] * blockScale + blockScale * .5f);

            if (!CheckGuardDistance(testPos, minGuardSpacing) || !CheckDistance(testPos, startInstance.transform.position, minGuardSpacing * blockScale))
                continue;

            NavMeshPath testPath = new NavMeshPath();
            NavMesh.CalculatePath(guardAgent.transform.position, testPos, NavMesh.AllAreas, testPath);
            var currentDistance = GetPathLength(testPath);
            
            if (Mathf.Abs(idealDistance - currentDistance) < Mathf.Abs(idealDistance - bestDistance))
            {
                bestDistance = currentDistance;
                destinationPos = testPos;
            }
        }

        if (bestDistance > idealDistance - epsilon && bestDistance < idealDistance + epsilon)
            return destinationPos;
        else
            return new Vector3(-1, -1, -1);
    }

    //Code I stole to get path length :D
    public static float GetPathLength(NavMeshPath path)
    {
        float lng = 0.0f;

        if ((path.status != NavMeshPathStatus.PathInvalid))
        {
            for (int i = 1; i < path.corners.Length; ++i)
            {
                lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }

        return lng;
    }

    //Creates a guard at a random empty square and gives it two patrol points, its spawn and another using the arguments
    //Note that all params are given in terms of grid spaces, not actual Unity distances!
    void PlaceGuard(float gridDistance, float idealDistance, float epsilon, float minGuardSpacing, float minStartSpacing, int maxTries)
    {
        int currentTries = 0;
        //Place a guard
        while (true && currentTries < maxTries)
        {
            currentTries++;
            Vector2Int square = GetEmptySquare(columns, rows);
            Vector3 position = new Vector3(square[1] * blockScale + blockScale * .5f, 1, square[0] * blockScale + blockScale * .5f);
            if (!CheckGuardDistance(position, minGuardSpacing * blockScale) || !CheckDistance(position, startInstance.transform.position, minGuardSpacing * blockScale))
                continue;

            GameObject testGuard = Instantiate(guard, position, Quaternion.identity);

            HashSet<Vector2Int> squaresToCheck = GetEmptySquaresAtDistance(square[0], square[1], Mathf.RoundToInt(gridDistance * blockScale));
            Vector3 destinationPosition = GetIdealPosition(testGuard, squaresToCheck, idealDistance*blockScale, epsilon*blockScale, minGuardSpacing*blockScale);

            if (destinationPosition == new Vector3(-1, -1, -1))
            {
                GameObject.Destroy(testGuard);
                continue;
            }

            var empty = new GameObject();
            var add1 = Instantiate(empty, position, Quaternion.identity).GetComponent<Transform>();
            var add2 = Instantiate(empty, destinationPosition, Quaternion.identity).GetComponent<Transform>();
            testGuard.GetComponent<GuardPatrol>().waypoints.Add(add1);
            testGuard.GetComponent<GuardPatrol>().waypoints.Add(add2);
            guardWaypoints.Add(add1.position);
            guardWaypoints.Add(add2.position);
            gizmoPos.Add(destinationPosition);
            return;
        }
        print("Reached maximum tries! Guard not placed.");
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
            if (result)
                roomList.Add(new int[5] { x, y, w, h, 0 });
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
            int w = Random.Range(minWidth, maxWidth+1);
            int h = Random.Range(minHeight, maxHeight+1);
            var limiter = MaxHallwayLength(x, y, dir);
            int xoff = Random.Range(0, w - 1);
            int yoff = Random.Range(0, h - 1);
            if (limiter < minWidth || limiter < minHeight)
                continue;
            switch (dir)
            {
                case Direction.up:
                    if (h > limiter)
                        h = limiter;
                    if (TryMakingRectangle(x - xoff, y - h, w, h))
                    {
                        ClearRectangle(x, y, 1, 1, false);
                        PlaceDoor(x, y);
                        roomList.Add(new int[5] { x - xoff, y - h, w, h, 0 });
                        return;
                    }
                    break;
                case Direction.right:
                    if (w > limiter)
                        w = limiter;
                    if (TryMakingRectangle(x + 1, y - yoff, w, h))
                    {
                        ClearRectangle(x, y, 1, 1, false);
                        PlaceDoor(x, y);
                        roomList.Add(new int[5] { x + 1, y - yoff, w, h, 0 });
                        return;
                    }
                    break;
                case Direction.down:
                    if (h > limiter)
                        h = limiter;
                    if (TryMakingRectangle(x - xoff, y + 1, w, h))
                    {
                        ClearRectangle(x, y, 1, 1, false);
                        PlaceDoor(x, y);
                        roomList.Add(new int[5] { x - xoff, y + 1, w, h, 0 });
                        return;
                    }
                    break;
                case Direction.left:
                    if (w > limiter)
                        w = limiter;
                    if (TryMakingRectangle(x - w, y - yoff, w, h))
                    {
                        ClearRectangle(x, y, 1, 1, false);
                        PlaceDoor(x, y);
                        roomList.Add(new int[5] { x - w, y - yoff, w, h, 0 });
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

    // Returns what's at a grid position without errors: null if nothing or oob, the object's name if otherwise
    string GetGridName(int x, int y)
    {
        if (x >= columns || y >= rows || x < 0 || y < 0 || map[x, y] == null)
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
        if (x <= 0 || x + w >= columns || y <= 0 || y + h >= rows)
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
        map[x, y] = CreateDoor(y * blockScale, 0, x * blockScale);
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
