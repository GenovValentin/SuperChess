using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour{
    [Header("Art")]
    [SerializeField] private Material tileMaterial;

    // Logic
    private const int TILE_Count_X = 8;
    private const int TILE_Count_Y = 8;
    private GameObject[,] tiles;
    private Vector2Int currentHover;

    private void Awake(){
        GenerateAllTiles(1, TILE_Count_X, TILE_Count_Y);
    }
    private void Update(){
        RaycastHit info;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover"))){
            // Get the indexes of the tile I've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // If we're hovering a tile after not hovering any tiles
            if (currentHover == -Vector2Int.one){
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we were already hovering a tile, change the previous one
            if (currentHover != hitPosition){
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }
        else{
            if (currentHover != -Vector2Int.one){
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }
    }

    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY){
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++){
            for (int y = 0; y < tileCountY; y++){
                tiles[x,y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y){
        GameObject tileObject = new GameObject(string.Format("X:[0], Y:[0]", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0, (y+1) * tileSize);
        vertices[2] = new Vector3((x+1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x+1) * tileSize, 0, (y+1) * tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2};

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        // Generate the board
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }
    // Operations
    private Vector2Int LookupTileIndex(GameObject hitInfo){
        for (int x = 0; x < TILE_Count_X; x++){
            for (int y = 0; y< TILE_Count_Y; y++){
                if (tiles[x, y] == hitInfo){
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one; //Invalid
    }
}
