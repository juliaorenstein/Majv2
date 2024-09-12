using System;
using UnityEngine;
using UnityEditor;

public class TileGenerator : EditorWindow
{
    private Transform TilePool;
    private GameObject TilePF;
    private int tileID = 0;

    [MenuItem("Tools/Tile Generator")]
    public static void ShowWindow()
    {
        GetWindow<TileGenerator>("GameObject Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Tiles", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate"))
        {
            GenerateTiles();
        }
    }

    private void GenerateTiles()
    {
        TilePool = GameObject.Find("Tile Pool").transform;
        TilePF = Resources.Load<GameObject>("Prefabs/Tile");
        CreateNumberDragons();
        CreateFlowerWinds();
        CreateJokers();
    }

    void CreateNumberDragons()
    {
        Suit[] suits = (Suit[])Enum.GetValues(typeof(Suit));

        foreach (Suit suit in suits)
        {
            if (suit == Suit.none) break;
            for (int num = 0; num < 10; num++)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject tileGO = Instantiate(TilePF, TilePool);
                    TileMono tileMono = tileGO.GetComponent<TileMono>();
                    tileMono.tile = new(tileMono, tileID++, num, suit);
                }
            }
        }
    }

    void CreateFlowerWinds()
    {
        Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));

        foreach (Direction dir in directions)
        {
            if (dir == Direction.none) break;
            for (int id = 0; id < 4; id++)
            {
                GameObject tileGO = Instantiate(TilePF, TilePool);
                TileMono tileMono = tileGO.GetComponent<TileMono>();
                tileMono.tile = new(tileMono, tileID++, -1, Suit.none, dir);
            }
        }

        // AND THE LAST FOUR FLOWERS
        for (int id = 0; id < 4; id++)
        {
            GameObject tileGO = Instantiate(TilePF, TilePool);
            TileMono tileMono = tileGO.GetComponent<TileMono>();
            tileMono.tile = new(tileMono, tileID++, -1, Suit.none, Direction.flower);
        }
    }

    void CreateJokers()
    {
        for (int id = 0; id < 8; id++)
        {
            GameObject tileGO = Instantiate(TilePF, TilePool);
            TileMono tileMono = tileGO.GetComponent<TileMono>();
            tileMono.tile = new(tileMono, tileID++);
        }
    }
}