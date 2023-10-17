using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Board
    {
        public const int TILE_COUNT_X = 8;

        public const int TILE_COUNT_Y = 8;

        private GameObject[,] tiles;

        private Vector3 bounds;

        private float tileSize = 1.0f;

        private float yOffset = 0.101f;

        private float deathSpacing = 0.5f;

        private Vector3 boardCenter = Vector3.zero;

        private Material tileMaterial;

        public Board(Material material)
        {
            tileMaterial = material;
        }

        public void GenerateAllTiles(Transform transform)
        {
            GenerateAllTiles (transform, tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        }

        public Vector3 GetDeadPiecePosition(Team team, List<ChessPiece> deads)
        {
            float xCoef = team == Team.White ? -1.4f : 8.4f;
            float zCoef = team == Team.White ? 7.75f : -0.75f;
            Vector3 direction =
                team == Team.White ? Vector3.back : Vector3.forward;

            // List<ChessPiece> deads = GetDeads(team);
            return new Vector3(xCoef * tileSize, yOffset, zCoef * tileSize) -
            bounds +
            new Vector3(tileSize / 2, 0, tileSize / 2) +
            (direction * deathSpacing) * deads.Count;
        }

        public Vector3 GetTileCenter(int x, int y)
        {
            return new Vector3(x * tileSize, yOffset, y * tileSize) -
            bounds +
            new Vector3(tileSize / 2, 0, tileSize / 2);
        }

        public Vector2Int LookupTileIndex(GameObject hitInfo)
        {
            for (int x = 0; x < Board.TILE_COUNT_X; x++)
            {
                for (int y = 0; y < Board.TILE_COUNT_Y; y++)
                {
                    if (tiles[x, y] == hitInfo)
                    {
                        return CreatePosition(x, y);
                    }
                }
            }
            return -Vector2Int.one; //Invalid
        }

        public Vector2Int CreatePosition(int x, int y)
        {
            return new Vector2Int(x, y);
        }

        public void SetLayer(Vector2Int position, string layerName)
        {
            tiles[position.x, position.y].layer = GetLayer(layerName);
        }

        public Plane GetNewHorizontalPlane()
        {
            return new Plane(Vector3.up, Vector3.up * yOffset);
        }

        public int GetLayer(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }

        public bool
        IsMouseOverTile(Ray ray, out RaycastHit info, bool isReachable)
        {
            if (isReachable)
            {
                return Physics
                    .Raycast(ray,
                    out info,
                    100,
                    LayerMask.GetMask("Tile", "Hover", "Highlight"));
            }

            return Physics
                .Raycast(ray, out info, 100, LayerMask.GetMask("invalid"));
        }

        public bool IsMouseOverModal(Ray ray, out RaycastHit info)
        {
            return Physics
                .Raycast(ray, out info, 100, LayerMask.GetMask("Modal"));
        }

        private void GenerateAllTiles(
            Transform transform,
            float tileSize,
            int tileCountX,
            int tileCountY
        )
        {
            yOffset += transform.position.y;
            float fieldCenter = (tileCountX / 2) * tileSize;
            bounds = new Vector3(fieldCenter, 0, fieldCenter) + boardCenter;
            tiles = new GameObject[tileCountX, tileCountY];

            for (int x = 0; x < tileCountX; x++)
            {
                for (int y = 0; y < tileCountY; y++)
                {
                    tiles[x, y] = GenerateSingleTile(tileSize, x, y, transform);
                }
            }
        }

        private GameObject
        GenerateSingleTile(float tileSize, int x, int y, Transform transform)
        {
            return CreateTileObject(CreateTileMesh(tileSize, x, y),
            x,
            y,
            transform);
        }

        private GameObject
        CreateTileObject(Mesh mesh, int x, int y, Transform transform)
        {
            GameObject tileObject =
                new GameObject(string.Format($"X:{x}, Y:{y}"));
            tileObject.transform.parent = transform;
            tileObject.AddComponent<MeshFilter>().mesh = mesh;
            tileObject.AddComponent<MeshRenderer>().material = tileMaterial;
            tileObject.layer = GetLayer("Tile");
            tileObject.AddComponent<BoxCollider>();

            return tileObject;
        }

        private Mesh CreateTileMesh(float tileSize, int x, int y)
        {
            Mesh mesh = new Mesh();

            mesh.vertices = CreateTileVertices(tileSize, x, y);
            mesh.triangles = CreateTriangles();
            mesh.RecalculateNormals();

            return mesh;
        }

        private int[] CreateTriangles()
        {
            return new int[] { 0, 1, 2, 1, 3, 2 };
        }

        private Vector3[] CreateTileVertices(float tileSize, int x, int y)
        {
            Vector3[] vertices = new Vector3[4];
            vertices[0] =
                CreateTileVertice(x * tileSize, yOffset, y * tileSize);
            vertices[1] =
                CreateTileVertice(x * tileSize, yOffset, (y + 1) * tileSize);
            vertices[2] =
                CreateTileVertice((x + 1) * tileSize, yOffset, y * tileSize);
            vertices[3] =
                CreateTileVertice((x + 1) * tileSize,
                yOffset,
                (y + 1) * tileSize);

            return vertices;
        }

        private Vector3 CreateTileVertice(float x, float y, float z)
        {
            return new Vector3(x, y, z) - bounds;
        }
    }
}
