using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts;
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
    private float deathSize = 0.4f;

    [SerializeField]
    private float dragOffset = 0.5f;

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
    private IEnumerator coroutine;

    private ChessPiece[,] chessPieces;

    private ChessPiece currentlyDragging;

    private ChessPiece selectedPiece;

    private List<Vector2Int> availableMoves = new List<Vector2Int>();

    private List<ChessPiece> deadWhites = new List<ChessPiece>();

    private List<ChessPiece> deadBlacks = new List<ChessPiece>();

    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    private Board board;

    private Vector2Int currentHover;

    public bool isWhiteTurn;

    public bool setInGameButtons = false;

    public bool areTilesHighlighted = false;

    public bool isPieceSelected = false;

    public bool isPieceDeselected;

    private SpecialMove specialMove;

    public SoundController soundController;

    private bool isWhitePOV = true;

    private bool wasMenuButtonPressed = false;

    private bool isPromoted = false;

    private bool lastSelectedState = false;

    private ChessPiece lastSelectedPiece = null;

    private Vector2Int currentHitPosition;

    private Vector2Int prevPosition;

    public PromotionPieceController promotionPieceController;

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

    AccountHandler accountHandler;

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
        accountHandler = AccountHandler.GetInstance();
        InitControllers();
        SetIsWhiteTurn(true);

        GenerateAllTiles();
        SpawnAllPieces();
        PositionAllPieces();

        SetPromotionPiecesObjects();
        SetPromotionPiecesImagesPaths();
        SetRematchObjects();
        SetDrawObject();
        SetColors();
        RegisterEvents();
        ToggleObject(inGame, false);
        ResetVictoryScreen();
        ResetDrawIndicator();
        ResetTMPs();
        ToggleObject(promotionPieces, false);
        SetSounds();
        AddInputFieldListener();
    }

    private void SetColors()
    {
        promotionPieceController.SetColors();
    }

    private void InitControllers()
    {
        promotionPieceController = new PromotionPieceController();
        soundController = new SoundController();
        board = new Board(tileMaterial);
    }

    private void SetPromotionPiecesImagesPaths()
    {
        promotionPieceController.SetPromotionPiecesImagesPaths();
    }

    private void AddInputFieldListener()
    {
        playerNameInput.onEndEdit.AddListener (SetPlayerName);
    }

    private void SetPlayerName(string playerNameInput)
    {
        playerName = playerNameInput;
    }

    private void HandleSignIn()
    {
        playerName = accountHandler.ReturnUsername();
        playerNameInput.text = accountHandler.ReturnUsername();
    }

    private void HandleSignOut()
    {
        playerName = "";
        playerNameInput.text = "";
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
        ToggleObject(declinedTMP, false);
        ToggleObject(offeredDraw, false);
        ToggleObject(offeredRematch, false);
    }

    private void SetSounds()
    {
        soundController.SetSounds();
    }

    private void PlaySwooshSound()
    {
        soundController.PlaySwooshSound();
    }

    private void SetRematchObjects()
    {
        oppWantsRematchObj = rematchIndicator.transform.GetChild(0).gameObject;
        oppLeftObj = rematchIndicator.transform.GetChild(1).gameObject;
    }

    private void SetPromotionPiecesObjects()
    {
        promotionPieceController.SetPromotionPiecesObjects (promotionPieces);
    }

    private void SetPromotionPiecesColor(Team team)
    {
        promotionPieceController.SetPromotionPiecesColor (team);
    }

    private void SetPromotionPiecesImage(Team team)
    {
        team = GetOppositeTeam(team);
        promotionPieceController.SetPromotionPiecesImage (team);
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

            if (IsLeftMouseButtonDown())
            {
                HandleLeftMouseButtonDown (hitPosition);
            }

            if (IsLeftMouseButtonUp())
            {
                HandleLeftMouseButtonUp (hitPosition);
            }

            if (IsRightMouseButtonDown())
            {
                HandleRightMouseButtonDown();
            }
        }
        else
        {
            HandleLeftMouseButtonUpOutsideTile();
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

    private void HandleRightMouseButtonDown()
    {
        lastSelectedState = false;
        Vector2Int previousPiece = CloneChessPiece(selectedPiece);
        selectedPiece
            .SetPosition(GetTileCenter(previousPiece.x, previousPiece.y));
        DeselectPiece();
    }

    private void LiftPiece(Ray ray)
    {
        Plane horizontalPlane = board.GetNewHorizontalPlane();

        float distance = 0.0f;
        if (horizontalPlane.Raycast(ray, out distance))
        {
            currentlyDragging
                .SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }

    private void HandleLeftMouseButtonUpOutsideTile()
    {
        if (!HasHoveredTileBefore())
        {
            SetLayerOnMove();

            currentHover = -Vector2Int.one;
        }

        if (
            currentlyDragging &&
            (Input.GetMouseButtonUp(0) || IsRightMouseButtonUp())
        )
        {
            SetLastSelectedPieceAndState(true);
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
        return board.IsMouseOverTile(ray, out info, isReachable);
    }

    private bool IsMouseOverModal(Ray ray, out RaycastHit info)
    {
        return board.IsMouseOverModal(ray, out info);
    }

    private void HandleLeftMouseButtonUp(Vector2Int hitPosition)
    {
        if (selectedPiece == null)
        {
            return;
        }
        Vector2Int previousPiece = CloneChessPiece(selectedPiece);
        if (ContainsValidMove(ref availableMoves, ClonePosition(hitPosition)))
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

            SetLastSelectedPieceAndState();
            selectedPiece = null;
            return;
        }

        if (hitPosition == previousPiece)
        {
            selectedPiece
                .SetPosition(GetTileCenter(hitPosition.x, hitPosition.y));
            isPieceSelected = true;
            if (isPieceDeselected)
            {
                DeselectPiece();
            }
            return;
        }

        selectedPiece
            .SetPosition(GetTileCenter(previousPiece.x, previousPiece.y));

        SetLastSelectedPieceAndState();

        DeselectPiece();
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
        return board.CreatePosition(x, y);
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

        // Without this log the client's name disappears after rematching in a certain combination
        Debug.Log("SendGetOpponentNameToServer " + getOpponentName);
    }

    private void HandleLeftMouseButtonDown(Vector2Int hitPosition)
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

            SetLastSelectedPieceAndState(true, piece);
        }

        isPieceSelected = false;
        if (!lastSelectedState)
        {
            isPieceDeselected = true;
            return;
        }
        currentlyDragging = piece;

        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces);

        specialMove =
            currentlyDragging
                .GetSpecialMoves(ref chessPieces,
                ref moveList,
                ref availableMoves);

        PreventCheck();

        HighlightTiles();

        selectedPiece = piece;
        isPieceDeselected = false;
    }

    private void SetLastSelectedPieceAndState(
        bool isLastSelectedState = false,
        ChessPiece latestSelectedPiece = null
    )
    {
        lastSelectedState = isLastSelectedState;
        lastSelectedPiece = latestSelectedPiece;
    }

    private void DeselectPiece()
    {
        currentlyDragging = null;
        selectedPiece = null;
        RemoveHighlightTiles();
        ClearAvailableMoves();
    }

    private bool IsRightMouseButtonDown()
    {
        return currentlyDragging != null && Input.GetMouseButtonDown(1);
    }

    private bool IsRightMouseButtonUp()
    {
        return Input.GetMouseButtonUp(1);
    }

    private bool IsLeftMouseButtonDown()
    {
        return Input.GetMouseButtonDown(0);
    }

    private bool IsLeftMouseButtonUp()
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

            SetTileLayer(hitPosition, "Hover");
        }

        if (currentHover == hitPosition)
        {
            return;
        }

        // If we were already hovering a tile, change the previous one
        SetLayerOnMove();

        currentHover = hitPosition;

        SetTileLayer(hitPosition, "Hover");
    }

    private void SetLayerOnMove()
    {
        if (ContainsValidMove(ref availableMoves, currentHover))
        {
            SetTileLayer(currentHover, "Highlight");
            return;
        }

        SetTileLayer(currentHover, "Tile");
    }

    private bool HasHoveredTileBefore()
    {
        return currentHover == -Vector2Int.one;
    }

    private void SetTileLayer(Vector2Int hitPosition, string layerName)
    {
        SetLayer (hitPosition, layerName);
    }

    private void SetObjectLayer(GameObject gameObject, string layerName)
    {
        gameObject.layer = GetLayer(layerName);
    }

    private void SetLayer(Vector2Int position, string layerName)
    {
        board.SetLayer (position, layerName);
    }

    private int GetLayer(string layerName)
    {
        return board.GetLayer(layerName);
    }

    private void GenerateAllTiles()
    {
        board.GenerateAllTiles (transform);
    }

    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[Board.TILE_COUNT_X, Board.TILE_COUNT_Y];

        SpawnPieces(Team.White);
        SpawnPieces(Team.Black);

        SpawnAllPawns(true, chessPieces);
        SpawnAllPawns(false, chessPieces);
    }

    private void SpawnAllPawns(bool isWhite, ChessPiece[,] chessPieces)
    {
        Team team = isWhite ? Team.White : Team.Black;
        for (int i = 0; i < Board.TILE_COUNT_X; i++)
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
        for (int x = 0; x < Board.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < Board.TILE_COUNT_Y; y++)
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
        return board.GetTileCenter(x, y);
    }

    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            SetTileLayer(availableMoves[i], "Highlight");
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            SetTileLayer(availableMoves[i], "Tile");
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
        ToggleObject(inGame, false);
    }

    private void DisplayVictory(Team winningTeam)
    {
        ToggleObject(victoryScreen, true);

        if (!localGame && ((int) winningTeam == 0 || (int) winningTeam == 1))
        {
            TMP_Text winnerTMP =
                victoryScreen
                    .transform
                    .GetChild((int) winningTeam)
                    .gameObject
                    .GetComponent<TMP_Text>();
            SetTextComponentOfTMP_Text(winnerTMP,
            GetWinnerTMP((int) winningTeam),
            " Won!");
        }

        victoryScreen
            .transform
            .GetChild((int) winningTeam)
            .gameObject
            .SetActive(true);
        SetObjectLayer(victoryScreen, "Modal");

        isReachable = false;
    }

    private TMP_Text GetWinnerTMP(int winner)
    {
        if (winner == 0) return whitePlayerNameTMP;
        return blackPlayerNameTMP;
    }

    private void SetTextComponentOfTMP_Text(
        TMP_Text newTMP_Text,
        TMP_Text oldTMP_Text,
        string text
    )
    {
        newTMP_Text.text = oldTMP_Text.text + text;
    }

    public void OnRematchButton()
    {
        SendRematchToServer (currentTeam);
        if (!localGame)
        {
            rematchButton.interactable = false;
            return;
        }

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
        ToggleObject(offeredRematch, false);

        ResetInGamePlayerName();
        SetLocalGameCurrentTeam(Team.White);
        if (!localGame)
        {
            IsDrawButtonActive(!IsMyTurn());
            ChangeTeam();
            SetInGamePlayerName(GetOppositeTeam(myTeam));
        }
        else if (localGame)
        {
            SetInGamePlayerName(Team.White);
            SetInGamePlayerName(Team.Black);
        }

        ResetFields();
        DestroyPieces();

        SpawnAllPieces();
        PositionAllPieces();
        SetIsWhiteTurn(true);
        ResetVictoryScreen();
        ResetPlayerDraw();
        ActivateButtons(true, true);
        if (wasMenuButtonPressed == false)
        {
            soundController.PlayBoardSound();
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
        for (int x = 0; x < Board.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < Board.TILE_COUNT_Y; y++)
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
        if (!localGame)
        {
            SendRematchToServer(currentTeam, 0);
        }

        GameReset();
        ResetVictoryScreen();
        ToggleObject(inGame, false);
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
        ToggleObject(inGame, false);
        AreInGameButtonsActive(false);
    }

    public void OnDrawButton()
    {
        if (!localGame)
        {
            SendDrawToServer (currentTeam);
            IsDrawButtonActive(false);

            ToggleObject(declinedTMP, false);

            ToggleObject(offeredDraw, true);

            coroutine = WaitAndExecute(3.0f, offeredDraw, false);
            StartCoroutine (coroutine);
            return;
        }

        SendDrawToServer (currentTeam);
        AreInGameButtonsActive(false);
    }

    private void ToggleObject(GameObject gameObject, bool isActive)
    {
        gameObject.SetActive (isActive);
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
            AreInGameButtonsActive(false);
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
        ToggleObject(promotionPieces, true);

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
        promotionPieceController.SetPromotionPiecesObjectPosition (
            team,
            lastMove
        );
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

            ToggleObject(promotionPieces, false);
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
        AreInGameButtonsActive(true);

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
        return board.GetDeadPiecePosition(team, GetDeads(team));
    }

    private List<ChessPiece> GetDeads(Team team)
    {
        return team == Team.White ? deadWhites : deadBlacks;
    }

    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < Board.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < Board.TILE_COUNT_Y; y++)
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
        ChessPiece[,] simulation =
            new ChessPiece[Board.TILE_COUNT_X, Board.TILE_COUNT_Y];
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
        for (int x = 0; x < Board.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < Board.TILE_COUNT_Y; y++)
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
        for (int x = 0; x < Board.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < Board.TILE_COUNT_Y; y++)
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
        for (int x = 0; x < Board.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < Board.TILE_COUNT_Y; y++)
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

        for (int x = 0; x < Board.TILE_COUNT_X; x++)
        {
            for (int y = 0; y < Board.TILE_COUNT_Y; y++)
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

        soundController.PlaySpecialMoveSound (specialMove);

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
            ToggleObject(inGame, false);
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
        return board.LookupTileIndex(hitInfo);
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

        EventBus.SIGN_IN += HandleSignIn;
        EventBus.SIGN_OUT += HandleSignOut;
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

        EventBus.SIGN_IN -= HandleSignIn;
        EventBus.SIGN_OUT -= HandleSignOut;
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

        coroutine = WaitAndExecute(2.0f, inGame, true);
        StartCoroutine (coroutine);
        AreInGameButtonsActive(true);
        soundController.PlayBoardSound(2);
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
            soundController.PlayPiecesSound();
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
        ToggleObject(oppWantsRematchObj, false);
        ToggleObject(oppLeftObj, false);
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
        SetObjectLayer(victoryScreen, "Default");

        isReachable = true;

        ResetRematchIndicator();
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

        SetObjectLayer(drawIndicator, "Modal");

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
            ToggleObject(offeredRematch, true);

            coroutine = WaitAndExecute(3.0f, offeredRematch, false);
            StartCoroutine (coroutine);
        }

        // If both want to rematch
        if (
            (playerRematch[0] && playerRematch[1]) ||
            (localGame && (playerRematch[0] || playerRematch[1]))
        )
        {
            GameReset();

            ToggleObject(inGame, true);
            if (!localGame)
            {
                myTeam = GetOppositeTeam(myTeam);
                IsDrawButtonActive(!IsMyTurn());
            }
            AreInGameButtonsActive(true);
        }
        if (rm.wantRematch == 0)
        {
            ToggleObject(oppWantsRematchObj, false);
            ToggleObject(offeredRematch, false);
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

        ToggleObject(declinedTMP, false);
        ToggleObject(offeredDraw, false);
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
            ToggleObject(inGame, false);
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
            ToggleObject(declinedTMP, true);

            coroutine = WaitAndExecute(3.0f, declinedTMP, false);
            StartCoroutine (coroutine);
        }

        ToggleObject(offeredDraw, false);
        IsDrawButtonActive(!IsMyTurn());
        ResetPlayerDraw();
    }

    private IEnumerator
    WaitAndExecute(float waitTime, GameObject gameObject, bool isActive)
    {
        yield return new WaitForSeconds(waitTime);
        ToggleObject (gameObject, isActive);
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
