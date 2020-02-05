using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileAutomaton : MonoBehaviour
{
    [SerializeField] int wallThresholdSize = 10;
    [SerializeField] int emptySpaceThresholdSize = 15;
    [SerializeField] ChunkGenerator worldGenerator;
    System.Random PseudoRandom;

    [Range(0, 100)]
    public int iniChance;
    [Range(1, 8)]
    public int birthLimit;
    [Range(1, 8)]
    public int deathLimit;
    [Range(1, 10)]
    public int repNum;
    private int count = 0;

    private int[,] terrainMap;

    public Vector3Int chuckSize;
    int width;
    int height;

    public Tilemap interactableMap;
    public Tilemap iceMap;
    public Tilemap snowMap;
    public Tile topTile;
    public Tile bottomTile;

    void Start()
    {
        worldGenerator = GetComponentInParent<ChunkGenerator>();
        PseudoRandom = new System.Random(worldGenerator.GetRandomSeed().GetHashCode());
        chuckSize = new Vector3Int((int)worldGenerator.PassChuckSize().x, (int)worldGenerator.PassChuckSize().y,0);
    }
    // Update is called once per frame
    void Update()
    {
        //redo how the simulation gets called get simulated.

        if (Input.GetMouseButtonDown(0))
        {
            doSimulation(repNum);
        }
        if (Input.GetMouseButtonDown(1))
        {
            ClearMap(true);
        }
        if (Input.GetMouseButtonDown(2))
        {
            SaveAssetMap();
        }
    }

    public void doSimulation(int repNum) // runs cellular automata simulation
    {
        ClearMap(false);
        width = chuckSize.x;
        height = chuckSize.y;

        //if terain map is null then generate a new one
        if (terrainMap == null)
        {
            terrainMap = new int[width, height];
            InitPos();
        }

        for (int i = 0; i < repNum; i++)
        {
            terrainMap = GenTilePos(terrainMap);
        }

        RefineMap();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrainMap[x, y] == 1)
                    iceMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), topTile);
                else
                {
                    snowMap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), bottomTile);
                } 
            }
        }
    }

    public int[,] GenTilePos(int[,] oldMap)
    {
        int[,] newMap = new int[width, height];
        int neighb;

        BoundsInt myB = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                neighb = 0;
                foreach (var b in myB.allPositionsWithin)
                {
                    if (b.x == 0 && b.y == 0) continue; // exclude this position
                    if (x + b.x >= 0 && x + b.x < width && y + b.y >= 0 && y + b.y < height) // exclude the borders
                    {
                        neighb += oldMap[x + b.x, y + b.y];
                    }
                    else
                    {
                        neighb++;
                    }

                }

                if (oldMap[x, y] == 1)
                {
                    if (neighb < deathLimit) newMap[x, y] = 0;
                    else
                    {
                        newMap[x, y] = 1;
                    }
                }

                if (oldMap[x, y] == 0)
                {
                    if (neighb > birthLimit) newMap[x, y] = 1;
                    else
                    {
                        newMap[x, y] = 0;
                    }
                }
            }
        }

        return newMap;
    }

    // initial position of the simulation
    public void InitPos()
    {
         for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                terrainMap[x, y] = PseudoRandom.Next(1, 100) < iniChance ? 1 : 0;
                //terrainMap[x, y] = Random.Range(1, 101) < iniChance ? 1 : 0; // do a random check against the initialize chance. if its greater then give a value of 0 , which means its an empty tile
            }
        }
    }

    void RefineMap()
    {
        List<List<Vector2>> wallRegions = GetRegions(1);

        foreach (List<Vector2> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallThresholdSize)
            {
                foreach(Vector2 tile in wallRegion)
                {
                    terrainMap[(int)tile.x, (int)tile.y] = 0;
                }
            }
        }

        List<List<Vector2>> emptyRegions = GetRegions(0);
        Debug.Log(emptyRegions.Count);

        foreach (List<Vector2> emptyRegion in emptyRegions)
        {
            if (emptyRegion.Count < emptySpaceThresholdSize)
            {
                foreach (Vector2 tile in emptyRegion)
                {
                    terrainMap[(int)tile.x, (int)tile.y] = 1;
                }
            }
        }
    }

    #region Map Refinement

    // gets regions in a form of coordinate lists of tile type
    private List<List<Vector2>> GetRegions(int tileType)
    {
        List<List<Vector2>> regions = new List<List<Vector2>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(mapFlags[x,y] == 0 && terrainMap[x,y] == tileType)
                {
                    List<Vector2> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);
                    foreach(Vector2 tile in newRegion)
                    {
                        mapFlags[(int)tile.x, (int)tile.y] = 1;
                    }
                }
            }
        }
        return regions;
    }

    // gets coordinates of all tile in a region
    private List<Vector2> GetRegionTiles(int startX, int startY)
    {
        List<Vector2> tiles = new List<Vector2>();
        int[,] mapFlags = new int[width, height];
        int tileType = terrainMap[startX, startY];

        Queue<Vector2> queue = new Queue<Vector2>();
        queue.Enqueue(new Vector2(startX, startY));
        mapFlags[startX, startY] = 1;

        while(queue.Count > 0)
        {
            Vector2 tilePos = queue.Dequeue();
            tiles.Add(tilePos);
            for (int x = (int)tilePos.x - 1; x <= (int)tilePos.x + 1; x++)
            {
                for (int y = (int)tilePos.y - 1; y <= (int)tilePos.y + 1; y++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height && (y == (int)tilePos.y || x == (int)tilePos.x)) // if in map range
                    {
                        if(mapFlags[x,y] == 0 && terrainMap[x,y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Vector2(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    #endregion

    //saves as a prefab in the assetmenu
    public void SaveAssetMap()
    {
        string saveName = "tmapXY" + count;
        bool success;
        var mf = GameObject.Find("Grid");
        if (mf)
        {
            var savePath = "Assets/World/Maps/" + saveName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(mf, savePath, out success);

            if(success)
            {
                EditorUtility.DisplayDialog("Tilemap Saved", "Your Tilemap was saved " + savePath, "Continue");
            }
            else
            {
                EditorUtility.DisplayDialog("Tilemap not saved", "Error occured", "Continue");
            }
        }
    }
    // note can't be used in runtimeBuild. to get around it maybe it can store values of what the cave/dungeon is
    // and then when player enters again it passes the info to the grid system. to recreate the same dungeon.
    // needs testing.

    //wipes the current map
    public void ClearMap(bool complete)
    {
        iceMap.ClearAllTiles();
        snowMap.ClearAllTiles();
        if (complete)
        {
            terrainMap = null;
        }
    }

}
