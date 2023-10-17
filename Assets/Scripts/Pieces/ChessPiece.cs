using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public enum Team
{
    White = 0,
    Black = 1,
    Draw = 2,
    None = -1
}

public class ChessPiece : MonoBehaviour
{
    public Team team;

    public int currentX;

    public int currentY;

    public ChessPieceType type;

    private Vector3 desiredPosition;

    private Vector3 desiredScale = Vector3.one * 0.7f;

    private void Start()
    {
        if (type == ChessPieceType.King)
        {
            return;
        }

        transform.rotation = Quaternion.Euler(GetRotationVector());
    }

    protected bool IsFieldOnBoard(int x, int y)
    {
        return x >= 0 &&
        y >= 0 &&
        x < Board.TILE_COUNT_X &&
        y < Board.TILE_COUNT_Y;
    }

    protected void AddAvailableMove(
        ref ChessPiece[,] board,
        int x,
        int y,
        List<Vector2Int> availableMoves
    )
    {
        if (!IsFieldOnBoard(x, y))
        {
            return;
        }

        ChessPiece piece = board[x, y];
        if (piece == null || piece.team != team)
        {
            availableMoves.Add(new Vector2Int(x, y));
        }
    }

    private Vector3 GetRotationVector()
    {
        bool isWhite = team == Team.White;
        return isWhite ? new Vector3(-90, 90, 0) : new Vector3(-90, -90, 0);
    }

    private void Update()
    {
        float delta = Time.deltaTime * 10;
        transform.position =
            Vector3.Lerp(transform.position, desiredPosition, delta);
        transform.localScale =
            Vector3.Lerp(transform.localScale, desiredScale, delta);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        availableMoves.Add(new Vector2Int(3, 3));
        availableMoves.Add(new Vector2Int(4, 3));
        availableMoves.Add(new Vector2Int(3, 4));
        availableMoves.Add(new Vector2Int(4, 4));

        return availableMoves;
    }

    public virtual SpecialMove
    GetSpecialMoves(
        ref ChessPiece[,] board,
        ref List<Vector2Int[]> moveList,
        ref List<Vector2Int> availableMoves
    )
    {
        return SpecialMove.None;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }

    protected static bool IsFieldEmpty(int x, int y, ref ChessPiece[,] board)
    {
        return board[x, y] == null;
    }
}
