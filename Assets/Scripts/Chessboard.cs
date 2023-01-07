using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

public enum SpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3
}

public class Chessboard : MonoBehaviour
{
#region  Initialization
    [Header("Art")]
    [SerializeField]
    private Material tileMaterial;

    [SerializeField]
    private float tileSize = 1.0f;

    [SerializeField]
    private float yOffset = 0.11f;

    [SerializeField]
    private float deathSize = 0.4f;

    [SerializeField]
    private float deathSpacing = 0.5f;

    [SerializeField]
    private float dragOffset = 0.5f;

    [SerializeField]
    private Vector3 boardCenter = Vector3.zero;

    [SerializeField]
    private GameObject victoryScreen;

    [SerializeField]
    private Transform rematchIndicator;

    [SerializeField]
    private Button rematchButton;

    [Header("Prefabs & Materials")]
    [SerializeField]
    private GameObject[] prefabs;

    [SerializeField]
    private Material[] teamMaterials;

    // Logic
    public const int TILE_COUNT_X = 8;

    public const int TILE_COUNT_Y = 8;

    private ChessPiece[,] chessPieces;

    private ChessPiece currentlyDragging;

    private List<Vector2Int> availableMoves = new List<Vector2Int>();

    private List<ChessPiece> deadWhites = new List<ChessPiece>();

    private List<ChessPiece> deadBlacks = new List<ChessPiece>();

    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    private GameObject[,] tiles;

    private Vector2Int currentHover;

    private Vector3 bounds;

    public bool isWhiteTurn;

    private SpecialMove specialMove;

    // Multiplayer logic
    private int playerCount = -1;

    private Team currentTeam = Team.None;

    private bool localGame = true;

    private bool[] playerRematch = new bool[2];

    GameObject oppWantsRematchObj;

    GameObject oppLeftObj;


#endregion


    private void Start()
    {
        SetIsWhiteTurn(true);

        GenerateAllTiles (tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();

        SetRematchObjects();
        RegisterEvents();
    }

    private void SetRematchObjects()
    {
        oppLeftObj = rematchIndicator.transform.GetChild(1).gameObject;
        oppWantsRematchObj = rematchIndicator.transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        RaycastHit info;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (IsMouseOverTile(ray, out info))
        {
            // Get the indexes of the tile I've hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            HandleHovering (hitPosition);

            if (IsMouseButtonDown())
            {
                HandleMouseButtonDown (hitPosition);
            }

            if (IsMouseButtonUp())
            {
                HandleMouseButtonUp (hitPosition);
            }
        }
        else
        {
            HandleMouseButtonUpOutsideTile();
        }

        if (currentlyDragging)
        {
            LiftPiece (ray);
        }
    }

    private void LiftPiece(Ray ray)
    {
        Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
        float distance = 0.0f;
        if (horizontalPlane.Raycast(ray, out distance))
        {
            currentlyDragging
                .SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }

    private void HandleMouseButtonUpOutsideTile()
    {
        if (!HasHoveredTileBefore())
        {
            SetLayerOnMove();

            currentHover = -Vector2Int.one;
        }

        if (currentlyDragging && Input.GetMouseButtonUp(0))
        {
            ResetPiecePosition();
        }
    }

    private void ResetPiecePosition()
    {
        currentlyDragging
            .SetPosition(GetTileCenter(currentlyDragging.currentX,
            currentlyDragging.currentY));
        currentlyDragging = null;
        RemoveHighlightTiles();
    }

    private bool IsMouseOverTile(Ray ray, out RaycastHit info)
    {
        return Physics
            .Raycast(ray,
            out info,
            100,
            LayerMask.GetMask("Tile", "Hover", "Highlight"));
    }

    private void HandleMouseButtonUp(Vector2Int hitPosition)
    {
        Vector2Int previousPiece = CloneChessPiece(currentlyDragging);

        if (ContainsValidMove(ref availableMoves, ClonePosition(hitPosition)))
        {
            MoveTo(previousPiece.x,
            previousPiece.y,
            hitPosition.x,
            hitPosition.y);

            SendMoveToServer (previousPiece, hitPosition);
            return;
        }

        currentlyDragging
            .SetPosition(GetTileCenter(previousPiece.x, previousPiece.y));
        currentlyDragging = null;
        RemoveHighlightTiles();
    }

    private Vector2Int CloneChessPiece(ChessPiece position)
    {
        return CreatePosition(position.currentX, position.currentY);
    }

    private Vector2Int ClonePosition(Vector2Int position)
    {
        return CreatePosition(position.x, position.y);
    }

    private Vector2Int CreatePosition(int x, int y)
    {
        return new Vector2Int(x, y);
    }

    private void SendMoveToServer(
        Vector2Int previousPosition,
        Vector2Int hitPosition
    )
    {
        NetMakeMove move = new NetMakeMove();
        move.originalX = previousPosition.x;
        move.originalY = previousPosition.y;
        move.destinationX = hitPosition.x;
        move.destinationY = hitPosition.y;
        move.teamId = (int) currentTeam;

        Client.Instance.SendToServer (move);
    }

    private void SendRematchToServer(Team team)
    {
        Debug.Log("Sending rematch for team " + team);
        NetRematch rematch = new NetRematch();
        rematch.teamId = (int) team;
        rematch.wantRematch = 1;
        Client.Instance.SendToServer (rematch);
    }

    private void HandleMouseButtonDown(Vector2Int hitPosition)
    {
        ChessPiece piece = GetChessPiece(hitPosition);
        if (
            piece == null ||
            (
            !IsMyTurn(Team.White, hitPosition) &&
            !IsMyTurn(Team.Black, hitPosition)
            )
        )
        {
            return;
        }

        currentlyDragging = GetChessPiece(hitPosition);

        // Get a list of where I can go, highlight tiles as well
        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces);

        // Get a list of special moves as well
        specialMove =
            currentlyDragging
                .GetSpecialMoves(ref chessPieces,
                ref moveList,
                ref availableMoves);

        PreventCheck();

        HighlightTiles();
    }

    private bool IsMouseButtonDown()
    {
        return Input.GetMouseButtonDown(0);
    }

    private bool IsMouseButtonUp()
    {
        return currentlyDragging != null && Input.GetMouseButtonUp(0);
    }

    private ChessPiece GetChessPiece(Vector2Int position)
    {
        return chessPieces[position.x, position.y];
    }

    private bool IsMyTurn(Team team, Vector2Int hitPosition)
    {
        bool isMyTurn = team == Team.White ? isWhiteTurn : !isWhiteTurn;

        return GetChessPiece(hitPosition).team == team &&
        isMyTurn &&
        currentTeam == team;
    }

    private void HandleHovering(Vector2Int hitPosition)
    {
        if (HasHoveredTileBefore())
        {
            currentHover = hitPosition;
            SetHoveringLayer (hitPosition);
        }

        if (currentHover == hitPosition)
        {
            return;
        }

        // If we were already hovering a tile, change the previous one
        SetLayerOnMove();

        currentHover = hitPosition;
        SetHoveringLayer (hitPosition);
    }

    private void SetLayerOnMove()
    {
        if (ContainsValidMove(ref availableMoves, currentHover))
        {
            SetHighlightLayer (currentHover);
            return;
        }

        SetTileLayer (currentHover);
    }

    private bool HasHoveredTileBefore()
    {
        return currentHover == -Vector2Int.one;
    }

    private void SetHoveringLayer(Vector2Int hitPosition)
    {
        SetLayer(hitPosition, "Hover");
    }

    private void SetHighlightLayer(Vector2Int hitPosition)
    {
        SetLayer(hitPosition, "Highlight");
    }

    private void SetTileLayer(Vector2Int hitPosition)
    {
        SetLayer(hitPosition, "Tile");
    }

    private void SetLayer(Vector2Int position, string layerName)
    {
        tiles[position.x, position.y].layer = GetLayer(layerName);
    }

    private int GetLayer(string layerName)
    {
        return LayerMask.NameToLayer(layerName);
    }

    private void GenerateAllTiles(
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
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format($"X:{x}, Y:{y}"));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] =
            new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] =
            new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] =
            new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) -
            bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        // Generate the board
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        Team
            whiteTeam = Team.White,
            blackTeam = Team.Black;

        // White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);

        SpawnAllPawns(true, chessPieces);

        // Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        SpawnAllPawns(false, chessPieces);
    }

    private void SpawnAllPawns(bool isWhite, ChessPiece[,] chessPieces)
    {
        Team team = isWhite ? Team.White : Team.Black;
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, (int) Pawn.GetPawnStartLine(isWhite)] =
                SpawnSinglePiece(ChessPieceType.Pawn, team);
        }
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, Team team)
    {
        ChessPiece chessPiece =
            Instantiate(prefabs[(int) type - 1], transform)
                .GetComponent<ChessPiece>();

        chessPiece.type = type;
        chessPiece.team = team;
        chessPiece.GetComponent<MeshRenderer>().material =
            teamMaterials[(int) team];

        return chessPiece;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        ChessPiece piece = chessPieces[x, y];
        piece.currentX = x;
        piece.currentY = y;
        piece.SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) -
        bounds +
        new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            SetHighlightLayer(availableMoves[i]);
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            SetTileLayer(availableMoves[i]);
        }

        availableMoves.Clear();
    }

    // Checkmate
    private void CheckMate(Team team)
    {
        DisplayVictory (team);
    }

    private void DisplayVictory(Team winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen
            .transform
            .GetChild((int) winningTeam)
            .gameObject
            .SetActive(true);
    }

    public void OnRematchButton()
    {
        if (!localGame)
        {
            SendRematchToServer (currentTeam);
            return;
        }

        SendRematchToServer(Team.White);
        SendRematchToServer(Team.Black);
        SendRematchToServer(Team.Black);
    }

    public void GameReset()
    {
        ResetFields();
        DestroyPieces();

        SpawnAllPieces();
        PositionAllPieces();
        SetIsWhiteTurn(true);
        // ResetVictoryScreen();
        // if (localGame)
        // {
        //     currentTeam = 0;
        // }
    }

    private void DestroyPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    Destroy(chessPieces[x, y].gameObject);
                }
                chessPieces[x, y] = null;
            }
        }

        DestroyDeadPieces (deadWhites);
        DestroyDeadPieces (deadBlacks);
    }

    private void DestroyDeadPieces(List<ChessPiece> deadPieces)
    {
        for (int i = 0; i < deadPieces.Count; i++)
        {
            Destroy(deadPieces[i].gameObject);
        }
        deadPieces.Clear();
    }

    private void ResetFields()
    {
        currentlyDragging = null;
        availableMoves = new List<Vector2Int>();
        moveList.Clear();
        playerRematch[0] = playerRematch[1] = false;
    }

    public void OnMenuButton()
    {
        NetRematch rm = new NetRematch();
        rm.teamId = (int) currentTeam;
        rm.wantRematch = 0;
        Client.Instance.SendToServer (rm);

        GameReset();
        ResetVictoryScreen();
        GameUI.Instance.OnLeaveGameMenu();

        Invoke("ShutdownRelay", 1.0f);

        // Reset some values
        playerCount = -1;
        currentTeam = Team.None;
    }

    // Special moves
    private void ProcessSpecialMove()
    {
        // En Passant
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn =
                chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if (myPawn.currentX == enemyPawn.currentX)
            {
                if (
                    myPawn.currentY == enemyPawn.currentY - 1 ||
                    myPawn.currentY == enemyPawn.currentY + 1
                )
                {
                    if (enemyPawn.team == Team.White)
                    {
                        deadWhites.Add (enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn
                            .SetPosition(new Vector3(-1.4f * tileSize,
                                yOffset,
                                7.75f * tileSize) -
                            bounds +
                            new Vector3(tileSize / 2, 0, tileSize / 2) +
                            (Vector3.back * deathSpacing) * deadWhites.Count);
                    }
                    else
                    {
                        deadBlacks.Add (enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn
                            .SetPosition(new Vector3(8.4f * tileSize,
                                yOffset,
                                -0.75f * tileSize) -
                            bounds +
                            new Vector3(tileSize / 2, 0, tileSize / 2) +
                            (Vector3.forward * deathSpacing) *
                            deadBlacks.Count);
                    }

                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        // Promotion
        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                // White team
                if (targetPawn.team == Team.White && lastMove[1].y == 7)
                {
                    ChessPiece newQueen =
                        SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position =
                        chessPieces[lastMove[1].x, lastMove[1].y]
                            .transform
                            .position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y]
                        .gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }

                // Black team
                if (targetPawn.team == Team.Black && lastMove[1].y == 0)
                {
                    ChessPiece newQueen =
                        SpawnSinglePiece(ChessPieceType.Queen, Team.Black);
                    newQueen.transform.position =
                        chessPieces[lastMove[1].x, lastMove[1].y]
                            .transform
                            .position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y]
                        .gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        // Castling
        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            // Left rook
            if (lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0)
                {
                    // White side
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7)
                {
                    // Black side
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            } // Right rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0)
                {
                    // White side
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7)
                {
                    // Black side
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }

    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].type == ChessPieceType.King)
                    {
                        if (chessPieces[x, y].team == currentlyDragging.team)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                }
            }
        }

        // Since we're sending ref availableMoves, we will be deleting moves that are putting us in check
        SimulateMoveForSinglePiece(currentlyDragging,
        ref availableMoves,
        targetKing);
    }

    private void SimulateMoveForSinglePiece(
        ChessPiece cp,
        ref List<Vector2Int> moves,
        ChessPiece targetKing
    )
    {
        // Save the current values, to reset after the function call
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going through all the moves, simulate them and check if we're in check
        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = CloneChessPiece(targetKing);

            // Did we simulate the king's move
            if (cp.type == ChessPieceType.King)
            {
                kingPositionThisSim = CreatePosition(simX, simY);
            }

            // Copy the [,] and not a reference
            ChessPiece[,] simulation =
                new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != cp.team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }

            // Simulate that move
            simulation[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            // Did one of the pieces get taken down during our simulation
            var deadPiece =
                simAttackingPieces
                    .Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
            {
                simAttackingPieces.Remove (deadPiece);
            }

            // Get all the simulated attacking pieces' moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves =
                    simAttackingPieces[a].GetAvailableMoves(ref simulation);
                for (int b = 0; b < pieceMoves.Count; b++)
                {
                    simMoves.Add(pieceMoves[b]);
                }
            }

            // Is the king in trouble and if so, remove the move
            if (ContainsValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            // Restore the actual cp data
            cp.currentX = actualX;
            cp.currentY = actualY;
        }

        // Remove from the current available move list
        for (int i = 0; i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
    }

    private bool CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        Team targetTeam =
            (
            (chessPieces[lastMove[1].x, lastMove[1].y].team == (int) Team.White)
                ? Team.Black
                : Team.White
            );

        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }
            }
        }

        // Is the king attacked right now?
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves =
                attackingPieces[i].GetAvailableMoves(ref chessPieces);
            for (int b = 0; b < pieceMoves.Count; b++)
            {
                currentAvailableMoves.Add(pieceMoves[b]);
            }
        }

        // Are we in check right now?
        if (
            ContainsValidMove(ref currentAvailableMoves,
            CloneChessPiece(targetKing))
        )
        {
            // King is under attack, can we do something to help him?
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves =
                    defendingPieces[i].GetAvailableMoves(ref chessPieces);

                // Since we're sending ref availableMoves, we will be deleting moves that are putting us in check
                SimulateMoveForSinglePiece(defendingPieces[i],
                ref defendingMoves,
                targetKing);

                if (defendingMoves.Count != 0)
                {
                    return false;
                }
            }

            return true; // Checkmate exit
        }

        return false;
    }

    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }

    private void MoveTo(int originalX, int originalY, int x, int y)
    {
        ChessPiece cp = chessPieces[originalX, originalY];
        Vector2Int previousPosition = CreatePosition(originalX, originalY);

        // Is there a piece on the target position?
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];
            if (cp.team == ocp.team)
            {
                return;
            }

            // If it is the enemy team
            if (ocp.team == Team.White)
            {
                if (ocp.type == ChessPieceType.King)
                {
                    CheckMate(Team.Black);
                }

                deadWhites.Add (ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp
                    .SetPosition(new Vector3(-1.4f * tileSize,
                        yOffset,
                        7.75f * tileSize) -
                    bounds +
                    new Vector3(tileSize / 2, 0, tileSize / 2) +
                    (Vector3.back * deathSpacing) * deadWhites.Count);
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                {
                    CheckMate(0);
                }

                deadBlacks.Add (ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp
                    .SetPosition(new Vector3(8.4f * tileSize,
                        yOffset,
                        -0.75f * tileSize) -
                    bounds +
                    new Vector3(tileSize / 2, 0, tileSize / 2) +
                    (Vector3.forward * deathSpacing) * deadBlacks.Count);
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece (x, y);
        SetIsWhiteTurn(!isWhiteTurn);
        if (localGame)
        {
            currentTeam =
                ((currentTeam == Team.White) ? Team.Black : Team.White);
        }
        moveList
            .Add(new Vector2Int[] { previousPosition, CreatePosition(x, y) });
        ProcessSpecialMove();
        if (currentlyDragging)
        {
            currentlyDragging = null;
        }
        RemoveHighlightTiles();

        if (CheckForCheckmate())
        {
            CheckMate(cp.team);
        }

        return;
    }

    private void SetIsWhiteTurn(bool newIsWhiteTurn)
    {
        isWhiteTurn = newIsWhiteTurn;
        ChangeRimsColour (newIsWhiteTurn);
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return CreatePosition(x, y);
                }
            }
        }
        return -Vector2Int.one; //Invalid
    }

    private void ChangeRimsColour(bool isWhiteTurn)
    {
        GameObject rims = GameObject.Find("Chess/Board/Rims");
        for (int i = 0; i < rims.transform.childCount; i++)
        {
            GameObject child = rims.transform.GetChild(i).gameObject;
            ChangeRimColour (child, isWhiteTurn);
        }
    }

    private void ChangeRimColour(GameObject rim, bool isWhiteTurn)
    {
        string layerName = "Rim_White";
        if (isWhiteTurn == false)
        {
            layerName = "Rim_Black";
        }

        rim.layer = LayerMask.NameToLayer(layerName);
    }


#region

    private void RegisterEvents()
    {
        Debug.Log("Register events chessboard ");
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;

        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }

    private void UnRegisterEvents()
    {
        Debug.Log("Deregister events chessboard ");
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_REMATCH -= OnRematchClient;

        GameUI.Instance.SetLocalGame -= OnSetLocalGame;
    }

    // Server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        // Client has connected, assign a team and send the message back to him
        NetWelcome nw = msg as NetWelcome;

        // Assign a team
        nw.AssignedTeam = ++playerCount;

        // Return back to the client
        Server.Instance.SendToClient (cnn, nw);

        Debug.Log("On welcome server " + playerCount);

        // If full, start the game
        if (playerCount == 1)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        // Receive the message, broadcast it back
        NetMakeMove mm = msg as NetMakeMove;

        // This is where you could do some validation checks!
        // Receive and just broadcast it back
        Server.Instance.Broadcast (mm);
    }

    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {
        Debug.Log("OnRematchServer ");
        Server.Instance.Broadcast (msg);
    }

    // Client
    private void OnWelcomeClient(NetMessage msg)
    {
        // Receive the connection message
        NetWelcome nw = msg as NetWelcome;

        // Assign the team
        currentTeam = (Team) nw.AssignedTeam;

        Debug.Log($"My assigned team is {nw.AssignedTeam}");

        if (localGame && currentTeam == Team.White)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    private void OnStartGameClient(NetMessage msg)
    {
        GameUI
            .Instance
            .ChangeCamera((currentTeam == Team.White)
                ? CameraAngle.whiteTeam
                : CameraAngle.blackTeam);
        ResetVictoryScreen();
    }

    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;

        Debug.Log($"MM : {mm.teamId} : {mm.originalX} {mm.originalY}");
        Debug.Log($"-> {mm.destinationX} {mm.destinationY}");

        if (mm.teamId != (int) currentTeam)
        {
            ChessPiece target = chessPieces[mm.originalX, mm.originalY];

            availableMoves = target.GetAvailableMoves(ref chessPieces);
            specialMove =
                target
                    .GetSpecialMoves(ref chessPieces,
                    ref moveList,
                    ref availableMoves);

            MoveTo(mm.originalX,
            mm.originalY,
            mm.destinationX,
            mm.destinationY);
        }
    }

    private void ResetRematchIndicator()
    {
        rematchButton.interactable = true;
        oppWantsRematchObj.SetActive(false);
        oppLeftObj.SetActive(false);
    }

    private void ResetVictoryScreen()
    {
        victoryScreen.SetActive(false);
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);

        ResetRematchIndicator();
    }

    private void ActivateRematchIndicatorChildren(bool oppWantsRematch)
    {
        // ResetVictoryScreen();
        if (localGame)
        {
            return;
        }

        GameObject objToActivate =
            oppWantsRematch ? oppWantsRematchObj : oppLeftObj;
        rematchButton.interactable = oppWantsRematch;
        objToActivate.SetActive(true);
        Debug.Log("Am i active " + objToActivate);
    }

    private void OnRematchClient(NetMessage msg)
    {
        // Receive the connection message
        NetRematch rm = msg as NetRematch;
        bool oppWantsRematch = rm.wantRematch == 1;

        // Set the boolean for rematch
        playerRematch[rm.teamId] = oppWantsRematch;
        Debug.Log("OnRematchClient " + rm.teamId);

        // Activate the piece of UI
        if (rm.teamId != (int) currentTeam)
        {
            Debug.Log("ActivateRematchIndicatorChildren " + currentTeam);
            ActivateRematchIndicatorChildren (oppWantsRematch);
        }

        Debug
            .Log("Who wants rematch " +
            playerRematch[0] +
            " " +
            playerRematch[1]);

        // If both want to rematch
        if (playerRematch[0] && playerRematch[1])
        {
            Debug.Log("Rematch received");
            // GameReset();
        }
    }

    private void ShutdownRelay()
    {
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();
        // UnRegisterEvents();
    }

    // Local game
    private void OnSetLocalGame(bool v)
    {
        playerCount = -1;
        currentTeam = Team.None;
        localGame = v;
    }


#endregion
}
