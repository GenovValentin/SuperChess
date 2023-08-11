using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

using Random = UnityEngine.Random;

public enum SpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3,
    Capture = 4
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
    private float yOffset = 0.101f;

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
    private GameObject inGame;

    [SerializeField]
    private Transform rematchIndicator;

    [SerializeField]
    private GameObject drawIndicator;

    [SerializeField]
    private GameObject offeredDraw;

    [SerializeField]
    private GameObject offeredRematch;

    [SerializeField]
    private GameObject declinedTMP;

    [SerializeField]
    private GameObject promotionPieces;

    [SerializeField]
    private TMP_InputField playerNameInput;

    [SerializeField]
    public TMP_Text whitePlayerNameTMP;

    [SerializeField]
    public TMP_Text blackPlayerNameTMP;

    [SerializeField]
    private Button rematchButton;

    [SerializeField]
    private Button resignButton;

    [SerializeField]
    private Button drawButton;

    [SerializeField]
    private Button whiteButton;

    [SerializeField]
    private Button blackButton;

    [SerializeField]
    private Button queenButton;

    [SerializeField]
    private Button rookButton;

    [SerializeField]
    private Button bishopButton;

    [SerializeField]
    private Button knightButton;

    [SerializeField]
    private Button acceptButton;

    [SerializeField]
    private Button declineButton;

    [SerializeField]
    private Button exitButton;

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

    private ChessPiece selectedPiece;

    private List<Vector2Int> availableMoves = new List<Vector2Int>();

    private List<ChessPiece> deadWhites = new List<ChessPiece>();

    private List<ChessPiece> deadBlacks = new List<ChessPiece>();

    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    private GameObject[,] tiles;

    private Vector2Int currentHover;

    private Vector3 bounds;

    public bool isWhiteTurn;

    public bool setInGameButtons = false;

    public bool areTilesHighlighted = false;

    public bool isPieceSelected = false;

    public bool isPieceDeselected;

    private SpecialMove specialMove;

    public AudioSource Board;

    public AudioSource Pieces;

    public AudioSource Castle;

    public AudioSource Capture;

    public AudioSource Promote;

    public AudioSource Swoosh1;

    public AudioSource Swoosh2;

    public AudioSource Swoosh3;

    private bool isWhitePOV = true;

    private bool wasMenuButtonPressed = false;

    private bool isPromoted = false;

    private bool lastSelectedState = false;

    private ChessPiece lastSelectedPiece = null;

    private Vector2Int currentHitPosition;

    private Vector2Int prevPosition;

    public Texture whiteQueenImage;

    public Texture blackQueenImage;

    public Texture currentQueenImage;

    public Texture whiteRookImage;

    public Texture blackRookImage;

    public Texture currentRookImage;

    public Texture whiteBishopImage;

    public Texture blackBishopImage;

    public Texture currentBishopImage;

    public Texture whiteKnightImage;

    public Texture blackKnightImage;

    public Texture currentKnightImage;

    Color whiteColor;

    Color blackColor;

    Color currentColor;

    Color whiteHighlightedColor;

    Color blackHighlightedColor;

    Color currentHighlightedColor;

    Color whitePressedColor;

    Color blackPressedColor;

    Color currentPressedColor;

    GameObject promotionPiecesObject;

    GameObject promotionQueen;

    GameObject promotionRook;

    GameObject promotionBishop;

    GameObject promotionKnight;

    GameObject promotionQueenImage;

    GameObject promotionRookImage;

    GameObject promotionBishopImage;

    GameObject promotionKnightImage;

    // Multiplayer logic
    private int playerCount = -1;

    private string playerName = "NN";

    private Team currentTeam = Team.None;

    private Team myTeam = Team.None;

    private bool localGame = true;

    private bool isReachable = true;

    private bool[] playerRematch = new bool[2];

    private bool[] playerDraw = new bool[2];

    GameObject oppWantsRematchObj;

    GameObject oppLeftObj;

    GameObject oppOfferedObj;

    private static ChessPieceType[]
        PIECES_ORDERED =
            new ChessPieceType[8]
            {
                ChessPieceType.Rook,
                ChessPieceType.Knight,
                ChessPieceType.Bishop,
                ChessPieceType.Queen,
                ChessPieceType.King,
                ChessPieceType.Bishop,
                ChessPieceType.Knight,
                ChessPieceType.Rook
            };


#endregion


    private void Start()
    {
        SetIsWhiteTurn(true);

        GenerateAllTiles (tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();

        SetPromotionPiecesObjects();
        SetPromotionPiecesImagesPaths();
        SetRematchObjects();
        SetDrawObject();
        SetColors();
        RegisterEvents();
        ResetInGame();
        ResetVictoryScreen();
        ResetDrawIndicator();
        ResetTMPs();
        ResetPromotion();
        SetSounds();
        AddInputFieldListener();
    }

    private void SetColors()
    {
        ColorUtility.TryParseHtmlString("#A19984", out whiteColor);
        ColorUtility.TryParseHtmlString("#323232", out blackColor);
        ColorUtility.TryParseHtmlString("#9C9191", out whiteHighlightedColor);
        ColorUtility.TryParseHtmlString("#B09B9B", out blackHighlightedColor);
        ColorUtility.TryParseHtmlString("#666666", out whitePressedColor);
        ColorUtility.TryParseHtmlString("#202020", out blackPressedColor);
    }

    private void SetPromotionPiecesImagesPaths()
    {
        whiteQueenImage = Resources.Load<Texture>("Queen");
        blackQueenImage = Resources.Load<Texture>("Queen_B");
        whiteRookImage = Resources.Load<Texture>("Rook");
        blackRookImage = Resources.Load<Texture>("Rook_B");
        whiteBishopImage = Resources.Load<Texture>("Bishop");
        blackBishopImage = Resources.Load<Texture>("Bishop_B");
        whiteKnightImage = Resources.Load<Texture>("Knight");
        blackKnightImage = Resources.Load<Texture>("Knight_B");
    }

    private void ResetPromotion()
    {
        promotionPieces.SetActive(false);
    }

    private void AddInputFieldListener()
    {
        playerNameInput.onEndEdit.AddListener (SetPlayerName);
    }

    private void SetPlayerName(string playerNameInput)
    {
        playerName = playerNameInput;
    }

    private void SetInGamePlayerName(Team thisTeam)
    {
        if (thisTeam == Team.White)
        {
            whitePlayerNameTMP.text = playerName;
        }
        else if (thisTeam == Team.Black)
        {
            blackPlayerNameTMP.text = playerName;
        }
        SendGetOpponentNameToServer (currentTeam, playerName);
    }

    private void ResetTMPs()
    {
        declinedTMP.SetActive(false);
        offeredDraw.SetActive(false);
        offeredRematch.SetActive(false);
    }

    private void SetSounds()
    {
        GameObject boardSound = GameObject.Find("BoardSound");
        Board = boardSound.GetComponent<AudioSource>();
        GameObject piecesSound = GameObject.Find("PiecesSound");
        Pieces = piecesSound.GetComponent<AudioSource>();
        GameObject castleSound = GameObject.Find("CastleSound");
        Castle = castleSound.GetComponent<AudioSource>();
        GameObject captureSound = GameObject.Find("CaptureSound");
        Capture = captureSound.GetComponent<AudioSource>();
        GameObject promoteSound = GameObject.Find("PromoteSound");
        Promote = promoteSound.GetComponent<AudioSource>();
        GameObject swooshSound1 = GameObject.Find("SwooshSound1");
        Swoosh1 = swooshSound1.GetComponent<AudioSource>();
        GameObject swooshSound2 = GameObject.Find("SwooshSound2");
        Swoosh1 = swooshSound2.GetComponent<AudioSource>();
        GameObject swooshSound3 = GameObject.Find("SwooshSound3");
        Swoosh1 = swooshSound3.GetComponent<AudioSource>();
    }

    private void PlaySwooshSound()
    {
        int swooshSoundToBePlayed = Random.Range(1, 4);
        switch (swooshSoundToBePlayed)
        {
            case 1:
                Swoosh1.Play();
                break;
            case 2:
                Swoosh2.Play();
                break;
            case 3:
                Swoosh3.Play();
                break;
        }
    }

    private void SetRematchObjects()
    {
        oppWantsRematchObj = rematchIndicator.transform.GetChild(0).gameObject;
        oppLeftObj = rematchIndicator.transform.GetChild(1).gameObject;
    }

    private void SetPromotionPiecesObjects()
    {
        promotionPiecesObject =
            promotionPieces.transform.GetChild(0).gameObject;

        SetPromotionPiecesObject(ref promotionQueen,
        ref promotionQueenImage,
        0);
        SetPromotionPiecesObject(ref promotionRook, ref promotionRookImage, 1);
        SetPromotionPiecesObject(ref promotionBishop,
        ref promotionBishopImage,
        2);
        SetPromotionPiecesObject(ref promotionKnight,
        ref promotionKnightImage,
        3);
    }

    private void SetPromotionPiecesObject(
        ref GameObject promotionPiece,
        ref GameObject promotionPieceImage,
        int x = 0
    )
    {
        promotionPiece =
            promotionPieces.transform.GetChild(0).GetChild(x).gameObject;
        promotionPieceImage = promotionPiece.transform.GetChild(0).gameObject;
    }

    private void SetPromotionPiecesColor(Team team)
    {
        Button promotionQueenButton = promotionQueen.GetComponent<Button>();
        Button promotionRookButton = promotionRook.GetComponent<Button>();
        Button promotionBishopButton = promotionBishop.GetComponent<Button>();
        Button promotionKnightButton = promotionKnight.GetComponent<Button>();
        ColorBlock colorBlock = promotionQueenButton.colors;

        SetPromotionPiecesWantedColors (team, colorBlock);

        SetPromotionPiecesColors(ref promotionQueenButton,
        ref promotionQueen,
        colorBlock);
        SetPromotionPiecesColors(ref promotionRookButton,
        ref promotionRook,
        colorBlock);
        SetPromotionPiecesColors(ref promotionBishopButton,
        ref promotionBishop,
        colorBlock);
        SetPromotionPiecesColors(ref promotionKnightButton,
        ref promotionKnight,
        colorBlock);
    }

    private void SetPromotionPiecesColors(
        ref Button promotionButton,
        ref GameObject promotionPiece,
        ColorBlock colorBlock
    )
    {
        promotionButton.colors = colorBlock;
        promotionPiece.GetComponent<Image>().color = currentColor;
    }

    private void SetPromotionPiecesWantedColors(
        Team team,
        ColorBlock colorBlock
    )
    {
        if (team == Team.White)
        {
            SetPromotionPiecesCurrentColors (
                whiteColor,
                whiteHighlightedColor,
                whitePressedColor
            );
        }
        else if (team == Team.Black)
        {
            SetPromotionPiecesCurrentColors (
                blackColor,
                blackHighlightedColor,
                blackPressedColor
            );
        }
        colorBlock.highlightedColor = currentHighlightedColor;
        colorBlock.pressedColor = currentPressedColor;
    }

    private void SetPromotionPiecesCurrentColors(
        Color wantedColor,
        Color wantedHighlightedColor,
        Color wantedPressedColor
    )
    {
        currentColor = wantedColor;
        currentHighlightedColor = wantedHighlightedColor;
        currentPressedColor = wantedPressedColor;
    }

    private void SetPromotionPiecesImage(Team team)
    {
        team = GetOppositeTeam(team);
        SetPromotionPiecesWantedImages (team);

        SetPromotionPiecesImages (promotionQueenImage, currentQueenImage);
        SetPromotionPiecesImages (promotionRookImage, currentRookImage);
        SetPromotionPiecesImages (promotionBishopImage, currentBishopImage);
        SetPromotionPiecesImages (promotionKnightImage, currentKnightImage);
    }

    private void SetPromotionPiecesImages(
        GameObject pieceImage,
        Texture currentPieceImage
    )
    {
        pieceImage.GetComponent<RawImage>().texture = currentPieceImage;
    }

    private void SetPromotionPiecesWantedImages(Team team)
    {
        if (team == Team.White)
        {
            SetPromotionPiecesCurrentImages (
                whiteQueenImage,
                whiteRookImage,
                whiteBishopImage,
                whiteKnightImage
            );
        }
        else if (team == Team.Black)
        {
            SetPromotionPiecesCurrentImages (
                blackQueenImage,
                blackRookImage,
                blackBishopImage,
                blackKnightImage
            );
        }
    }

    private void SetPromotionPiecesCurrentImages(
        Texture wantedQueenImage,
        Texture wantedRookImage,
        Texture wantedBishopImage,
        Texture wantedKnightImage
    )
    {
        currentQueenImage = wantedQueenImage;
        currentRookImage = wantedRookImage;
        currentBishopImage = wantedBishopImage;
        currentKnightImage = wantedKnightImage;
    }

    private void SetDrawObject()
    {
        oppOfferedObj = drawIndicator.transform.GetChild(0).gameObject;
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

        if (currentlyDragging && isPieceSelected == false)
        {
            LiftPiece (ray);
        }

        if (setInGameButtons == true)
        {
            if (Input.GetKey(KeyCode.W))
            {
                OnWhiteButton();
            }

            if (Input.GetKey(KeyCode.B))
            {
                OnBlackButton();
            }
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
        ClearAvailableMoves();
    }

    private bool IsMouseOverTile(Ray ray, out RaycastHit info)
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

    private bool IsMouseOverModal(Ray ray, out RaycastHit info)
    {
        return Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Modal"));
    }

    private void HandleMouseButtonUp(Vector2Int hitPosition)
    {
        if (selectedPiece != null)
        {
            Vector2Int previousPiece = CloneChessPiece(selectedPiece);
            if (
                ContainsValidMove(ref availableMoves,
                ClonePosition(hitPosition))
            )
            {
                currentHitPosition = hitPosition;

                MoveTo(previousPiece.x,
                previousPiece.y,
                hitPosition.x,
                hitPosition.y);

                if (specialMove != SpecialMove.Promotion)
                {
                    SendMoveToServer (previousPiece, hitPosition);
                }

                lastSelectedState = false;
                lastSelectedPiece = null;
                selectedPiece = null;
                return;
            }

            if (hitPosition == previousPiece)
            {
                selectedPiece
                    .SetPosition(GetTileCenter(hitPosition.x, hitPosition.y));
                isPieceSelected = true;
            }
            else
            {
                selectedPiece
                    .SetPosition(GetTileCenter(previousPiece.x,
                    previousPiece.y));
                currentlyDragging = null;
                lastSelectedState = false;
                lastSelectedPiece = null;
                selectedPiece = null;

                RemoveHighlightTiles();
                ClearAvailableMoves();
            }
        }
    }

    private bool ToggleBool(ref bool exampleBool)
    {
        return !exampleBool;
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
        Vector2Int hitPosition,
        ChessPieceType promotionPiece = ChessPieceType.None
    )
    {
        NetMakeMove move = new NetMakeMove();
        move.originalX = previousPosition.x;
        move.originalY = previousPosition.y;
        move.destinationX = hitPosition.x;
        move.destinationY = hitPosition.y;
        move.teamId = (int) currentTeam;
        if (promotionPiece != ChessPieceType.None)
        {
            move.promotionPieceType = GetSocketChessPieceType(promotionPiece);
        }

        Client.Instance.SendToServer (move);
    }

    private ChessPieceType GetChessPieceType(int socketChessPieceType)
    {
        switch (socketChessPieceType)
        {
            case 1:
                return ChessPieceType.Queen;
            case 2:
                return ChessPieceType.Rook;
            case 3:
                return ChessPieceType.Bishop;
            case 4:
                return ChessPieceType.Knight;
            default:
                return ChessPieceType.None;
        }
    }

    private int GetSocketChessPieceType(ChessPieceType promotedPiece)
    {
        switch (promotedPiece)
        {
            case ChessPieceType.Queen:
                return 1;
            case ChessPieceType.Rook:
                return 2;
            case ChessPieceType.Bishop:
                return 3;
            case ChessPieceType.Knight:
                return 4;
            default:
                return 0;
        }
    }

    private void SendRematchToServer(Team team, byte wantRematch = 1)
    {
        NetRematch rematch = new NetRematch();
        rematch.teamId = (int) team;
        rematch.wantRematch = wantRematch;
        Client.Instance.SendToServer (rematch);
    }

    private void SendResignToServer(Team team, byte hasResigned = 1)
    {
        NetResign resign = new NetResign();
        resign.teamID = (int) team;
        resign.hasResigned = hasResigned;
        Client.Instance.SendToServer (resign);
    }

    private void SendDrawToServer(Team team, byte wantDraw = 1)
    {
        NetDraw draw = new NetDraw();
        draw.teamNumber = (int) team;
        draw.wantDraw = wantDraw;
        Client.Instance.SendToServer (draw);
    }

    private void SendDeclineToServer(Team team, byte wantDecline = 1)
    {
        NetDecline decline = new NetDecline();
        decline.teamNr = (int) team;
        decline.wantDecline = wantDecline;
        Client.Instance.SendToServer (decline);
    }

    private void SendGetOpponentNameToServer(Team team, string opponentsName)
    {
        NetGetOpponentName getOpponentName = new NetGetOpponentName();
        getOpponentName.teamNUMBER = (int) team;
        getOpponentName.opponentName = opponentsName;
        Client.Instance.SendToServer (getOpponentName);
    }

    private void HandleMouseButtonDown(Vector2Int hitPosition)
    {
        ChessPiece piece = GetChessPiece(hitPosition);

        if (
            piece == null ||
            (
            !IsTeamsTurn(Team.White, hitPosition) &&
            !IsTeamsTurn(Team.Black, hitPosition)
            )
        )
        {
            return;
        }

        if (piece == lastSelectedPiece)
        {
            lastSelectedState = ToggleBool(ref lastSelectedState);
        }
        else
        {
            if (lastSelectedPiece != null)
            {
                RemoveHighlightTiles();
                ClearAvailableMoves();
            }

            lastSelectedPiece = piece;
            lastSelectedState = true;
        }

        isPieceSelected = false;
        if (lastSelectedState)
        {
            currentlyDragging = GetChessPiece(hitPosition);

            availableMoves =
                currentlyDragging.GetAvailableMoves(ref chessPieces);

            specialMove =
                currentlyDragging
                    .GetSpecialMoves(ref chessPieces,
                    ref moveList,
                    ref availableMoves);

            PreventCheck();

            HighlightTiles();

            selectedPiece = piece;
        }
        else
        {
            isPieceDeselected = true;
            // currentlyDragging = null;
            // selectedPiece = null;
            // RemoveHighlightTiles();
            // ClearAvailableMoves();
        }
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

    private bool IsTeamsTurn(Team team, Vector2Int hitPosition)
    {
        bool isMyTurn = team == Team.White ? isWhiteTurn : !isWhiteTurn;

        return GetChessPiece(hitPosition).team == team &&
        isMyTurn &&
        currentTeam == team;
    }

    private bool IsMyTurn()
    {
        return myTeam == Team.White ? isWhiteTurn : !isWhiteTurn;
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

    private void SetLayerVictoryScreen(string layerName)
    {
        victoryScreen.layer = GetLayer(layerName);
    }

    private void SetLayerDrawIndicator(string layerName)
    {
        drawIndicator.layer = GetLayer(layerName);
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
        return CreateTileObject(CreateTileMesh(tileSize, x, y), x, y);
    }

    private GameObject CreateTileObject(Mesh mesh, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format($"X:{x}, Y:{y}"));
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
        vertices[0] = CreateTileVertice(x * tileSize, yOffset, y * tileSize);
        vertices[1] =
            CreateTileVertice(x * tileSize, yOffset, (y + 1) * tileSize);
        vertices[2] =
            CreateTileVertice((x + 1) * tileSize, yOffset, y * tileSize);
        vertices[3] =
            CreateTileVertice((x + 1) * tileSize, yOffset, (y + 1) * tileSize);

        return vertices;
    }

    private Vector3 CreateTileVertice(float x, float y, float z)
    {
        return new Vector3(x, y, z) - bounds;
    }

    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        SpawnPieces(Team.White);
        SpawnPieces(Team.Black);

        SpawnAllPawns(true, chessPieces);
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

    private void SpawnPieces(Team team)
    {
        int startRank = (int) King.GetStartRankKing(team);
        for (int i = 0; i < 8; i++)
        {
            chessPieces[i, startRank] =
                SpawnSinglePiece(PIECES_ORDERED[i], team);
        }
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
    }

    private void ClearAvailableMoves()
    {
        availableMoves.Clear();
    }

    // Checkmate
    private void CheckMate(Team team)
    {
        DisplayVictory (team);
        ResetInGame();
    }

    private void DisplayVictory(Team winningTeam)
    {
        victoryScreen.SetActive(true);

        victoryScreen
            .transform
            .GetChild((int) winningTeam)
            .gameObject
            .SetActive(true);
        SetLayerVictoryScreen("Modal");
        isReachable = false;
    }

    public void DisplayInGame()
    {
        inGame.SetActive(true);
    }

    public void OnRematchButton()
    {
        if (!localGame)
        {
            SendRematchToServer (currentTeam);
            rematchButton.interactable = false;

            return;
        }

        SendRematchToServer(Team.White);
        SendRematchToServer(Team.Black);
        GameUI.Instance.ChangeCamera(CameraAngle.whiteTeam);
        AreInGameButtonsActive(true);
    }

    public void OnWhiteButton()
    {
        GameUI.Instance.ChangeCamera(CameraAngle.whiteTeam);
        if (!isWhitePOV)
        {
            PlaySwooshSound();
        }
        isWhitePOV = true;
    }

    public void OnBlackButton()
    {
        GameUI.Instance.ChangeCamera(CameraAngle.blackTeam);
        if (isWhitePOV)
        {
            PlaySwooshSound();
        }
        isWhitePOV = false;
    }

    public void OnAcceptButton()
    {
        playerDraw[0] = true;
        playerDraw[1] = true;
        SendDrawToServer (currentTeam);
    }

    public void OnDeclineButton()
    {
        ResetDrawIndicator();
        ActivateButtons(true);
        ResetPlayerDraw();
        SendDeclineToServer (currentTeam);
    }

    public void OnExitBtton()
    {
        ShutdownRelay();
        Application.Quit();
    }

    public void GameReset()
    {
        HideOfferRematch();
        ResetInGamePlayerName();
        if (!localGame)
        {
            IsDrawButtonActive(!IsMyTurn());
            ChangeTeam();
            SetInGamePlayerName(GetOppositeTeam(myTeam));
        }
        else if (localGame)
        {
            SetInGamePlayerName(Team.White);
        }

        ResetFields();
        DestroyPieces();

        SetLocalGameCurrentTeam(Team.White);
        SpawnAllPieces();
        PositionAllPieces();
        SetIsWhiteTurn(true);
        ResetVictoryScreen();
        ResetPlayerDraw();
        ActivateButtons(true, true);
        if (wasMenuButtonPressed == false)
        {
            Board.Play();
        }
        wasMenuButtonPressed = false;
    }

    private void ResetInGamePlayerName()
    {
        whitePlayerNameTMP.text = "";
        blackPlayerNameTMP.text = "";
    }

    private void ResetPlayerDraw()
    {
        playerDraw[0] = false;
        playerDraw[1] = false;
    }

    private void SetLocalGameCurrentTeam(Team team)
    {
        if (localGame)
        {
            currentTeam = team;
        }
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
        wasMenuButtonPressed = true;
        SendRematchToServer(currentTeam, 0);

        GameReset();
        ResetVictoryScreen();
        ResetInGame();
        GameUI.Instance.OnLeaveGameMenu();

        Invoke("ShutdownRelay", 0.5f);

        // Reset some values
        playerCount = -1;
        currentTeam = Team.None;
        myTeam = Team.None;
        IsDrawButtonActive(true);
    }

    public void OnResignButton()
    {
        if (!localGame)
        {
            SendResignToServer (currentTeam);
            return;
        }

        CheckMate(GetOppositeTeam(currentTeam));
        ResetInGame();
        AreInGameButtonsActive(false);
    }

    public void OnDrawButton()
    {
        if (!localGame)
        {
            SendDrawToServer (currentTeam);
            IsDrawButtonActive(false);
            HideDeclined();
            ShowOfferDraw();
            Invoke("HideOfferDraw", 3.0f);
            return;
        }

        SendDrawToServer(Team.White);
        SendDrawToServer(Team.Black);
        AreInGameButtonsActive(false);
    }

    private void ShowOfferDraw()
    {
        offeredDraw.transform.gameObject.SetActive(true);
    }

    private void HideOfferDraw()
    {
        offeredDraw.transform.gameObject.SetActive(false);
    }

    private void ShowOfferRematch()
    {
        offeredRematch.transform.gameObject.SetActive(true);
    }

    private void HideOfferRematch()
    {
        offeredRematch.transform.gameObject.SetActive(false);
    }

    // Special moves
    private void ProcessSpecialMove(
        ChessPieceType promotionPieceType = ChessPieceType.None
    )
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            ProcessEnPassant();
        }

        if (specialMove == SpecialMove.Promotion)
        {
            if (promotionPieceType == ChessPieceType.None)
            {
                ActivatePromotion (currentTeam);
            }
            else
            {
                ProcessPromoting (promotionPieceType);
            }
        }

        if (specialMove == SpecialMove.Castling)
        {
            ProcessCastling();
        }
    }

    private void ProcessCastling()
    {
        Vector2Int lastMove = GetLastMove();

        MakeCastlingToSide(true, lastMove);
        MakeCastlingToSide(false, lastMove);
    }

    private void MakeCastlingToSide(bool isQueenSide, Vector2Int lastMove)
    {
        if (lastMove.x != (int) King.GetEndFileKing(isQueenSide))
        {
            return;
        }

        MakeCastling(Team.White, isQueenSide, lastMove);
        MakeCastling(Team.Black, isQueenSide, lastMove);
    }

    private void MakeCastling(Team team, bool isQueenSide, Vector2Int lastMove)
    {
        int startRank = (int) King.GetStartRankKing(team);
        if (lastMove.y != startRank)
        {
            return;
        }

        int startFileRook = (int) King.GetStartFileRook(isQueenSide);
        int endFileRook = (int) King.GetEndFileRook(isQueenSide);
        ChessPiece rook = chessPieces[startFileRook, startRank];

        chessPieces[endFileRook, startRank] = rook;
        PositionSinglePiece (endFileRook, startRank);
        chessPieces[startFileRook, startRank] = null;
    }

    private void ActivatePromotion(Team team)
    {
        promotionPieces.SetActive(true);

        if (!localGame)
        {
            team = GetOppositeTeam(currentTeam);
        }
        SetPromotionPiecesColor (team);
        SetPromotionPiecesImage (team);
        SetPromotionPiecesObjectPosition (team);
        SetLayerPromotion("Modal");
        isReachable = false;
    }

    private void SetPromotionPiecesObjectPosition(Team team)
    {
        var lastMove = GetLastMove();
        int x = 0;
        if (team == Team.White)
        {
            switch (lastMove.x)
            {
                case 7:
                    x = -200;
                    break;
                case 6:
                    x = -120;
                    break;
                case 5:
                    x = -30;
                    break;
                case 4:
                    x = 50;
                    break;
                case 3:
                    x = 140;
                    break;
                case 2:
                    x = 220;
                    break;
                case 1:
                    x = 310;
                    break;
                case 0:
                    x = 400;
                    break;
                default:
                    x = 0;
                    break;
            }
        }
        else if (team == Team.Black)
        {
            switch (lastMove.x)
            {
                case 0:
                    x = -200;
                    break;
                case 1:
                    x = -120;
                    break;
                case 2:
                    x = -30;
                    break;
                case 3:
                    x = 50;
                    break;
                case 4:
                    x = 140;
                    break;
                case 5:
                    x = 220;
                    break;
                case 6:
                    x = 310;
                    break;
                case 7:
                    x = 400;
                    break;
                default:
                    x = 0;
                    break;
            }
        }

        Vector3 newPosition = new Vector3(x, 500f, 0f);
        promotionPiecesObject.transform.position =
            promotionPiecesObject.transform.parent.TransformPoint(newPosition);
    }

    private void ProcessPromotion(ChessPieceType promotionPieceType)
    {
        Vector2Int lastMove = GetLastMove();
        ChessPiece targetPiece = GetLastPiece();

        if (targetPiece.type != ChessPieceType.Pawn)
        {
            return;
        }

        if (isPromoted)
        {
            PromotePawn(Team.White, targetPiece, lastMove, promotionPieceType);
            PromotePawn(Team.Black, targetPiece, lastMove, promotionPieceType);

            ResetPromotion();
        }
    }

    private void PromotePawn(
        Team team,
        ChessPiece targetPiece,
        Vector2Int lastMove,
        ChessPieceType promotionPieceType
    )
    {
        if (
            targetPiece.team != team ||
            lastMove.y != (int) King.GetStartRankKing(GetOppositeTeam(team))
        )
        {
            return;
        }

        ChessPiece lastPiece = GetLastPiece();

        ChessPiece newPiece = SpawnSinglePiece(promotionPieceType, team);

        newPiece.transform.position = lastPiece.transform.position;
        Destroy(lastPiece.gameObject);

        chessPieces[lastMove.x, lastMove.y] = newPiece;
        PositionSinglePiece(lastMove.x, lastMove.y);

        isPromoted = false;
        isReachable = true;

        CheckForGameEnd (lastPiece);
    }

    private void ProcessEnPassant()
    {
        ChessPiece myPawn = GetLastPiece();
        ChessPiece enemyPawn = GetEnemyPawnForEnPassant();

        if (ArePawnsOnEnPassantFields(myPawn, enemyPawn))
        {
            TakeEnemyPawn (enemyPawn);
        }
    }

    private void SetLayerPromotion(string layerName)
    {
        promotionPieces.layer = GetLayer(layerName);
    }

    private Team GetOppositeTeam(Team team)
    {
        return team == Team.White ? Team.Black : Team.White;
    }

    public void OnQueenButton()
    {
        ProcessPromoting(ChessPieceType.Queen);
    }

    public void OnRookButton()
    {
        ProcessPromoting(ChessPieceType.Rook);
    }

    public void OnBishopButton()
    {
        ProcessPromoting(ChessPieceType.Bishop);
    }

    public void OnKnightButton()
    {
        ProcessPromoting(ChessPieceType.Knight);
    }

    private void ProcessPromoting(ChessPieceType promotionPieceType)
    {
        isPromoted = true;
        ProcessPromotion (promotionPieceType);
        SetLayerPromotion("Default");
        ChessPiece currentPiece = GetLastPiece();

        SendMoveToServer (prevPosition, currentHitPosition, promotionPieceType);
    }

    private Vector2Int GetLastMove()
    {
        return moveList[moveList.Count - 1][1];
    }

    private ChessPiece GetLastPiece()
    {
        var lastMove = GetLastMove();
        return chessPieces[lastMove.x, lastMove.y];
    }

    private ChessPiece GetEnemyPawnForEnPassant()
    {
        var targetPawnPosition = moveList[moveList.Count - 2][1];
        return chessPieces[targetPawnPosition.x, targetPawnPosition.y];
    }

    private bool
    ArePawnsOnEnPassantFields(ChessPiece myPawn, ChessPiece enemyPawn)
    {
        return myPawn.currentX == enemyPawn.currentX &&
        (
        myPawn.currentY == enemyPawn.currentY - 1 ||
        myPawn.currentY == enemyPawn.currentY + 1
        );
    }

    private void TakeEnemyPawn(ChessPiece enemyPawn)
    {
        List<ChessPiece> deads = GetDeads(enemyPawn.team);
        deads.Add (enemyPawn);

        enemyPawn.SetScale(Vector3.one * deathSize);
        enemyPawn.SetPosition(GetDeadPiecePosition(enemyPawn.team));

        chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
    }

    private Vector3 GetDeadPiecePosition(Team team)
    {
        float xCoef = team == Team.White ? -1.4f : 8.4f;
        float zCoef = team == Team.White ? 7.75f : -0.75f;
        Vector3 direction = team == Team.White ? Vector3.back : Vector3.forward;
        List<ChessPiece> deads = GetDeads(team);

        return new Vector3(xCoef * tileSize, yOffset, zCoef * tileSize) -
        bounds +
        new Vector3(tileSize / 2, 0, tileSize / 2) +
        (direction * deathSpacing) * deads.Count;
    }

    private List<ChessPiece> GetDeads(Team team)
    {
        return team == Team.White ? deadWhites : deadBlacks;
    }

    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                ChessPiece piece = chessPieces[x, y];
                if (
                    piece != null &&
                    piece.type == ChessPieceType.King &&
                    piece.team == currentlyDragging.team
                )
                {
                    targetKing = piece;
                }
            }
        }

        // Since we're sending ref availableMoves, we will be deleting moves that are putting us in check
        SimulateMoveForSinglePiece(currentlyDragging,
        ref availableMoves,
        targetKing);
    }

    private void SimulateMoveForSinglePiece(
        ChessPiece chessPiece,
        ref List<Vector2Int> moves,
        ChessPiece targetKing
    )
    {
        // Save the current values, to reset after the function call
        int actualX = chessPiece.currentX;
        int actualY = chessPiece.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going through all the moves, simulate them and check if we're in check
        for (int i = 0; i < moves.Count; i++)
        {
            Vector2Int move = moves[i];

            SimulateMove(chessPiece, move, targetKing, ref movesToRemove, null);

            List<Vector2Int> additionalMovesForCastling =
                GetAdditionalMovesForCastling(move, targetKing);

            foreach (var additionalMove in additionalMovesForCastling)
            {
                SimulateMove(chessPiece,
                move,
                targetKing,
                ref movesToRemove,
                additionalMove);
            }
        }

        RemoveMoves (moves, movesToRemove);
    }

    private List<Vector2Int>
    GetAdditionalMovesForCastling(Vector2Int move, ChessPiece targetKing)
    {
        List<Vector2Int> additionalMoves = new List<Vector2Int>();
        if (specialMove != SpecialMove.Castling)
        {
            return additionalMoves;
        }

        if (move.x == targetKing.currentX - 2)
        {
            additionalMoves
                .Add(new Vector2Int(targetKing.currentX - 1,
                    targetKing.currentY));
            additionalMoves
                .Add(new Vector2Int(targetKing.currentX, targetKing.currentY));
        }

        if (move.x == targetKing.currentX + 2)
        {
            additionalMoves
                .Add(new Vector2Int(targetKing.currentX + 1,
                    targetKing.currentY));
            additionalMoves
                .Add(new Vector2Int(targetKing.currentX, targetKing.currentY));
        }

        return additionalMoves;
    }

    private void SimulateMove(
        ChessPiece chessPiece,
        Vector2Int move,
        ChessPiece targetKing,
        ref List<Vector2Int> movesToRemove,
        Nullable<Vector2Int> additionalMove
    )
    {
        int actualX = chessPiece.currentX;
        int actualY = chessPiece.currentY;

        int simX = move.x;
        int simY = move.y;
        if (additionalMove.HasValue)
        {
            simX = additionalMove.Value.x;
            simY = additionalMove.Value.y;
        }

        Vector2Int kingPositionThisSim = CloneChessPiece(targetKing);

        // Did we simulate the king's move
        if (chessPiece.type == ChessPieceType.King)
        {
            kingPositionThisSim = CreatePosition(simX, simY);
        }

        // Copy the [,] and not a reference
        ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
        List<ChessPiece> simAttackingPieces =
            CreateSimAttackingPieces(simulation, chessPiece.team);

        // Simulate that move
        simulation[actualX, actualY] = null;
        chessPiece.currentX = simX;
        chessPiece.currentY = simY;
        simulation[simX, simY] = chessPiece;

        RemoveSimDeadPiece (simAttackingPieces, simX, simY);

        List<Vector2Int> simMoves =
            GetSimAttackingMoves(simAttackingPieces, simulation);

        // Is the king in trouble and if so, remove the move
        if (ContainsValidMove(ref simMoves, kingPositionThisSim))
        {
            movesToRemove.Add (move);
        }

        // Restore the actual cp data
        chessPiece.currentX = actualX;
        chessPiece.currentY = actualY;
    }

    private void RemoveMoves(
        List<Vector2Int> moves,
        List<Vector2Int> movesToRemove
    )
    {
        for (int i = 0; i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
    }

    private List<Vector2Int>
    GetSimAttackingMoves(
        List<ChessPiece> simAttackingPieces,
        ChessPiece[,] simulation
    )
    {
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

        return simMoves;
    }

    private void RemoveSimDeadPiece(
        List<ChessPiece> simAttackingPieces,
        int simX,
        int simY
    )
    {
        var deadPiece =
            simAttackingPieces
                .Find(piece =>
                    piece.currentX == simX && piece.currentY == simY);
        if (deadPiece != null)
        {
            simAttackingPieces.Remove (deadPiece);
        }
    }

    private List<ChessPiece>
    CreateSimAttackingPieces(ChessPiece[,] simulation, Team team)
    {
        List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                ChessPiece piece = chessPieces[x, y];
                if (piece != null)
                {
                    simulation[x, y] = piece;
                    if (simulation[x, y].team != team)
                    {
                        simAttackingPieces.Add(simulation[x, y]);
                    }
                }
            }
        }

        return simAttackingPieces;
    }

    private List<ChessPiece>
    GetAttackingPieces(ChessPiece[,] chessPieces, Team attackingTeam)
    {
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                ChessPiece piece = chessPieces[x, y];
                if (piece != null && piece.team == attackingTeam)
                {
                    attackingPieces.Add (piece);
                }
            }
        }

        return attackingPieces;
    }

    private ChessPiece GetTargetKing(ChessPiece[,] chessPieces, Team targetTeam)
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                ChessPiece piece = chessPieces[x, y];
                if (
                    piece != null &&
                    piece.team == targetTeam &&
                    piece.type == ChessPieceType.King
                )
                {
                    return piece;
                }
            }
        }

        return targetKing;
    }

    private bool CheckForCheckOrStaleMate(bool checkForCheckmate)
    {
        Vector2Int lastMove = GetLastMove();
        Team attackingTeam = chessPieces[lastMove.x, lastMove.y].team;
        Team defendingTeam = GetOppositeTeam(attackingTeam);

        List<ChessPiece> attackingPieces =
            GetAttackingPieces(chessPieces, attackingTeam);

        List<ChessPiece> defendingPieces =
            GetAttackingPieces(chessPieces, defendingTeam);
        ChessPiece targetKing = GetTargetKing(chessPieces, defendingTeam);

        List<Vector2Int> currentAvailableMoves =
            GetCurrentAvailableMoves(attackingPieces);

        bool hasValidMove =
            ContainsValidMove(ref currentAvailableMoves,
            CloneChessPiece(targetKing));

        if (checkForCheckmate)
        {
            return hasValidMove && IsCheckMate(defendingPieces, targetKing);
        }
        else
        {
            return !hasValidMove && IsCheckMate(defendingPieces, targetKing);
        }
    }

    private bool CheckForInsufficientMaterial()
    {
        int kingCount = 0;
        int knightCount = 0;
        int bishopCount = 0;
        int otherPieceCount = 0;

        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    ChessPiece chessPiece = GetChessPiece(new Vector2Int(x, y));
                    if (chessPiece.type == ChessPieceType.King)
                    {
                        kingCount++;
                    }
                    else if (chessPiece.type == ChessPieceType.Knight)
                    {
                        knightCount++;
                    }
                    else if (chessPiece.type == ChessPieceType.Bishop)
                    {
                        knightCount++;
                    }
                    else
                    {
                        otherPieceCount++;
                    }
                }
            }
        }

        return (
        kingCount == 2 &&
        knightCount == 0 &&
        bishopCount == 0 &&
        otherPieceCount == 0
        ) ||
        (
        kingCount == 2 &&
        knightCount == 1 &&
        bishopCount == 0 &&
        otherPieceCount == 0
        ) ||
        (
        kingCount == 2 &&
        knightCount == 0 &&
        bishopCount == 1 &&
        otherPieceCount == 0
        );
    }

    private bool
    IsCheckMate(List<ChessPiece> defendingPieces, ChessPiece targetKing)
    {
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

    private List<Vector2Int>
    GetCurrentAvailableMoves(List<ChessPiece> attackingPieces)
    {
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves =
                attackingPieces[i].GetAvailableMoves(ref chessPieces);

            for (int k = 0; k < pieceMoves.Count; k++)
            {
                currentAvailableMoves.Add(pieceMoves[k]);
            }
        }

        return currentAvailableMoves;
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

    private void MoveTo(
        int originalX,
        int originalY,
        int x,
        int y,
        ChessPieceType promotionPieceType = ChessPieceType.None
    )
    {
        ChessPiece piece = chessPieces[x, y];
        ChessPiece originalPiece = chessPieces[originalX, originalY];
        Vector2Int previousPosition = CreatePosition(originalX, originalY);

        // Is there a piece on the target position?
        if (piece != null)
        {
            if (originalPiece.team == piece.team)
            {
                return;
            }

            if (piece.type == ChessPieceType.King)
            {
                CheckMate(GetOppositeTeam(piece.team));
                return;
            }

            List<ChessPiece> deads = GetDeads(piece.team);
            deads.Add (piece);
            piece.SetScale(Vector3.one * deathSize);
            piece.SetPosition(GetDeadPiecePosition(piece.team));
            if (specialMove == SpecialMove.None)
            {
                specialMove = SpecialMove.Capture;
            }
        }

        if (specialMove == SpecialMove.None)
        {
            Pieces.Play();
        }
        else if (specialMove == SpecialMove.Castling)
        {
            Castle.Play();
        }
        else if (specialMove == SpecialMove.EnPassant)
        {
            Capture.Play();
        }
        else if (specialMove == SpecialMove.Promotion)
        {
            Promote.Play();
        }
        else if (specialMove == SpecialMove.Capture)
        {
            Capture.Play();
        }

        chessPieces[x, y] = originalPiece;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        prevPosition = previousPosition;

        PositionSinglePiece (x, y);
        SetIsWhiteTurn(!isWhiteTurn);
        if (localGame)
        {
            currentTeam = (GetOppositeTeam(currentTeam));
        }
        moveList
            .Add(new Vector2Int[] { previousPosition, CreatePosition(x, y) });
        ProcessSpecialMove (promotionPieceType);

        if (currentlyDragging)
        {
            currentlyDragging = null;
        }
        RemoveHighlightTiles();
        ClearAvailableMoves();

        CheckForGameEnd (originalPiece);

        return;
    }

    private void CheckForGameEnd(ChessPiece originalPiece)
    {
        if (CheckForCheckOrStaleMate(true))
        {
            CheckMate(originalPiece.team);
        }
        if (CheckForCheckOrStaleMate(false) || CheckForInsufficientMaterial())
        {
            ResetInGame();
            DisplayVictory(Team.Draw);
        }

        if (!localGame)
        {
            IsDrawButtonActive(!IsMyTurn());
        }
    }

    private bool IsDrawButtonActive(bool active)
    {
        return drawButton.interactable = active;
    }

    private void AreInGameButtonsActive(bool areButtonsActive)
    {
        setInGameButtons = areButtonsActive;
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
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;
        NetUtility.S_RESIGN += OnResignServer;
        NetUtility.S_DRAW += OnDrawServer;
        NetUtility.S_DECLINE += OnDeclineServer;
        NetUtility.S_GET_OPPONENT_NAME += OnGetOpponentNameServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;
        NetUtility.C_RESIGN += OnResignClient;
        NetUtility.C_DRAW += OnDrawClient;
        NetUtility.C_DECLINE += OnDeclineClient;
        NetUtility.C_GET_OPPONENT_NAME += OnGetOpponentNameClient;

        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }

    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;
        NetUtility.S_RESIGN -= OnResignServer;
        NetUtility.S_DRAW -= OnDrawServer;
        NetUtility.S_DECLINE -= OnDeclineServer;
        NetUtility.S_GET_OPPONENT_NAME -= OnGetOpponentNameServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE -= OnMakeMoveClient;
        NetUtility.C_REMATCH -= OnRematchClient;
        NetUtility.C_RESIGN -= OnResignClient;
        NetUtility.C_DRAW -= OnDrawClient;
        NetUtility.C_DECLINE -= OnDeclineClient;
        NetUtility.C_GET_OPPONENT_NAME -= OnGetOpponentNameClient;

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
        Server.Instance.Broadcast (msg);
    }

    private void OnResignServer(NetMessage msg, NetworkConnection cnn)
    {
        NetResign rg = msg as NetResign;
        Server.Instance.Broadcast (rg);
    }

    private void OnDrawServer(NetMessage msg, NetworkConnection cnn)
    {
        NetDraw dw = msg as NetDraw;
        Server.Instance.Broadcast (dw);
    }

    private void OnDeclineServer(NetMessage msg, NetworkConnection cnn)
    {
        NetDecline dc = msg as NetDecline;
        Server.Instance.Broadcast (dc);
    }

    private void OnGetOpponentNameServer(NetMessage msg, NetworkConnection cnn)
    {
        NetGetOpponentName gon = msg as NetGetOpponentName;
        Server.Instance.Broadcast (gon);
    }

    // Client
    private void OnWelcomeClient(NetMessage msg)
    {
        // Receive the connection message
        NetWelcome nw = msg as NetWelcome;

        // Assign the team
        currentTeam = (Team) nw.AssignedTeam;
        myTeam = currentTeam;

        if (localGame && currentTeam == Team.White)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    private void OnStartGameClient(NetMessage msg)
    {
        SetInGamePlayerName (myTeam);
        ChangeCameraAngles (currentTeam);
        ResetVictoryScreen();
        if (!localGame)
        {
            IsDrawButtonActive(!IsMyTurn());
        }
        Invoke("DisplayInGame", 2);
        AreInGameButtonsActive(true);
        Board.PlayDelayed(2);
    }

    private void ChangeTeam()
    {
        currentTeam = GetOppositeTeam(currentTeam);
        ChangeCameraAngles (currentTeam);
    }

    private void ChangeCameraAngles(Team team)
    {
        GameUI
            .Instance
            .ChangeCamera((team == Team.White)
                ? CameraAngle.whiteTeam
                : CameraAngle.blackTeam);
    }

    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;

        if (mm.teamId != (int) currentTeam)
        {
            Pieces.Play();
            ChessPiece target = chessPieces[mm.originalX, mm.originalY];

            if (target == null)
            {
                return;
            }
            availableMoves = target.GetAvailableMoves(ref chessPieces);
            specialMove =
                target
                    .GetSpecialMoves(ref chessPieces,
                    ref moveList,
                    ref availableMoves);

            ChessPieceType promotionPieceType =
                GetChessPieceType(mm.promotionPieceType);

            MoveTo(mm.originalX,
            mm.originalY,
            mm.destinationX,
            mm.destinationY,
            promotionPieceType);
        }
    }

    private void OnGetOpponentNameClient(NetMessage msg)
    {
        NetGetOpponentName gon = msg as NetGetOpponentName;
        if (gon.teamNUMBER != (int) currentTeam)
        {
            if (gon.teamNUMBER == 0)
            {
                whitePlayerNameTMP.text = gon.opponentName;
            }
            else if (gon.teamNUMBER == 1)
            {
                blackPlayerNameTMP.text = gon.opponentName;
            }
        }
    }

    private void ResetRematchIndicator()
    {
        rematchButton.interactable = true;
        oppWantsRematchObj.SetActive(false);
        oppLeftObj.SetActive(false);
    }

    private void ResetDrawIndicator()
    {
        drawIndicator.transform.gameObject.SetActive(false);
        oppOfferedObj.SetActive(false);

        drawIndicator.transform.GetChild(1).gameObject.SetActive(false);
        drawIndicator.transform.GetChild(2).gameObject.SetActive(false);
        isReachable = true;
    }

    private void ResetVictoryScreen()
    {
        victoryScreen.SetActive(false);
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(2).gameObject.SetActive(false);
        SetLayerVictoryScreen("Default");
        isReachable = true;

        ResetRematchIndicator();
    }

    public void ResetInGame()
    {
        inGame.SetActive(false);
    }

    private void ActivateRematchIndicatorChildren(bool oppWantsRematch)
    {
        if (localGame)
        {
            return;
        }

        GameObject objToActivate =
            oppWantsRematch ? oppWantsRematchObj : oppLeftObj;
        rematchButton.interactable = oppWantsRematch;
        objToActivate.SetActive(true);
    }

    private void ActivateDrawIndicator(bool oppWantsDraw)
    {
        if (localGame)
        {
            return;
        }

        SetLayerDrawIndicator("Modal");
        drawIndicator.transform.gameObject.SetActive(true);

        oppOfferedObj.SetActive(true);
        drawIndicator.transform.GetChild(1).gameObject.SetActive(true);
        drawIndicator.transform.GetChild(2).gameObject.SetActive(true);

        ActivateButtons(false);

        acceptButton.interactable = true;
        declineButton.interactable = true;
        isReachable = false;
    }

    private void ActivateButtons(
        bool buttonsActive,
        bool drawButtonActive = false
    )
    {
        IsDrawButtonActive (drawButtonActive);
        resignButton.interactable = buttonsActive;
        whiteButton.interactable = buttonsActive;
        blackButton.interactable = buttonsActive;
    }

    private void OnRematchClient(NetMessage msg)
    {
        // Receive the connection message
        NetRematch rm = msg as NetRematch;
        bool oppWantsRematch = rm.wantRematch == 1;

        // Set the boolean for rematch
        playerRematch[rm.teamId] = oppWantsRematch;

        // Activate the piece of UI
        if (rm.teamId != (int) currentTeam)
        {
            ActivateRematchIndicatorChildren (oppWantsRematch);
        }
        else
        {
            ShowOfferRematch();
            Invoke("HideOfferRematch", 3.0f);
        }

        // If both want to rematch
        if (
            (playerRematch[0] && playerRematch[1]) ||
            (localGame && (playerRematch[0] || playerRematch[1]))
        )
        {
            GameReset();
            DisplayInGame();
            if (!localGame)
            {
                myTeam = GetOppositeTeam(myTeam);
                IsDrawButtonActive(!IsMyTurn());
            }
            AreInGameButtonsActive(true);
        }
        if (rm.wantRematch == 0)
        {
            oppWantsRematchObj.SetActive(false);
            HideOfferRematch();
            ResetInGamePlayerName();
        }
    }

    private void OnResignClient(NetMessage msg)
    {
        NetResign rs = msg as NetResign;

        Team winning = currentTeam;
        if (rs.teamID == (int) winning)
        {
            winning = GetOppositeTeam(winning);
        }
        HideDeclined();
        HideOfferDraw();
        ResetDrawIndicator();
        CheckMate (winning);
        AreInGameButtonsActive(false);
    }

    private void OnDrawClient(NetMessage msg)
    {
        NetDraw dw = msg as NetDraw;
        bool oppWantsDraw = dw.wantDraw == 1;

        playerDraw[dw.teamNumber] = oppWantsDraw;

        if (dw.teamNumber != (int) currentTeam)
        {
            ActivateDrawIndicator (oppWantsDraw);
        }

        if (
            (playerDraw[0] && playerDraw[1]) ||
            (localGame && (playerDraw[0] || playerDraw[1]))
        )
        {
            ResetInGame();
            ResetDrawIndicator();
            DisplayVictory(Team.Draw);
        }
        AreInGameButtonsActive(false);
    }

    private void OnDeclineClient(NetMessage msg)
    {
        NetDecline dc = msg as NetDecline;
        if (dc.teamNr != (int) currentTeam)
        {
            ShowDeclined();
            Invoke("HideDeclined", 3.0f);
        }
        HideOfferDraw();

        IsDrawButtonActive(!IsMyTurn());

        ResetPlayerDraw();
    }

    private void ShowDeclined()
    {
        declinedTMP.SetActive(true);
    }

    private void HideDeclined()
    {
        declinedTMP.SetActive(false);
    }

    public void ShutdownRelay()
    {
        Debug.Log("Shutdown");
        try
        {
            Client.Instance.Shutdown();
            Server.Instance.Shutdown();
        }
        catch (Exception e)
        {
            Debug.Log("Error shutdown relay" + e);
        }
    }

    // Local game
    private void OnSetLocalGame(bool v)
    {
        playerCount = -1;
        currentTeam = Team.None;
        myTeam = Team.None;
        localGame = v;
    }


#endregion
}
