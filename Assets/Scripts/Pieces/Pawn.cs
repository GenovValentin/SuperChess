using System.Collections.Generic;
using UnityEngine;

public enum PawnStartLines
{
    White = 1,
    Black = 6
}

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        int direction = GetDirection();

        AddOneFieldMove(currentX,
        currentY,
        direction,
        ref board,
        availableMoves);

        AddTwoFieldsMove(currentX,
        currentY,
        direction,
        ref board,
        availableMoves);

        AddTakingMove(true,
        currentX,
        currentY,
        direction,
        ref board,
        availableMoves);

        AddTakingMove(false,
        currentX,
        currentY,
        direction,
        ref board,
        availableMoves);

        return availableMoves;
    }

    public override SpecialMove
    GetSpecialMoves(
        ref ChessPiece[,] board,
        ref List<Vector2Int[]> moveList,
        ref List<Vector2Int> availableMoves
    )
    {
        int direction = (team == Team.White) ? 1 : -1;
        if (
            (team == Team.White && currentY == 6) ||
            (team == Team.Black && currentY == 1)
        )
        {
            return SpecialMove.Promotion;
        }

        // En Passant
        if (moveList.Count <= 0)
        {
            return SpecialMove.None;
        }

        Vector2Int[] lastMove = moveList[moveList.Count - 1];
        Vector2Int endField = lastMove[1];
        int endFieldX = endField.x;
        int endFieldY = endField.y;
        int startFieldY = lastMove[0].y;

        if (
            !WasLastMovedPawn(endFieldX, endFieldY, ref board) ||
            !WasLastMoveTwoFieldMove(startFieldY, endFieldY) ||
            !WasLastMoveFromOpponent(endFieldX, endFieldY, ref board) ||
            (endFieldY != currentY)
        )
        {
            return SpecialMove.None;
        }

        bool addedTakingEnPassentMoveToTheRight =
            AddTakingEnPassentMove(true,
            currentX,
            currentY,
            direction,
            endFieldX,
            ref board,
            availableMoves);

        bool addedTakingEnPassentMoveToTheLeft =
            AddTakingEnPassentMove(false,
            currentX,
            currentY,
            direction,
            endFieldX,
            ref board,
            availableMoves);

        if (
            addedTakingEnPassentMoveToTheLeft ||
            addedTakingEnPassentMoveToTheRight
        )
        {
            return SpecialMove.EnPassant;
        }

        return SpecialMove.None;
    }

    private bool WasLastMovedPawn(int x, int y, ref ChessPiece[,] board)
    {
        ChessPiece piece = board[x, y];
        return piece.type == ChessPieceType.Pawn;
    }

    private bool WasLastMoveTwoFieldMove(int startFieldY, int endFieldY)
    {
        return (Mathf.Abs(startFieldY - endFieldY) == 2);
    }

    private bool
    WasLastMoveFromOpponent(
        int endFieldX,
        int endFieldY,
        ref ChessPiece[,] board
    )
    {
        return (board[endFieldX, endFieldY].team != team);
    }

    private int GetDirection()
    {
        return (team == Team.White) ? 1 : -1;
    }

    private void AddAvailableMove(int x, int y, List<Vector2Int> availableMoves)
    {
        availableMoves.Add(new Vector2Int(x, y));
    }

    private void AddTwoFieldsMove(
        int currentX,
        int currentY,
        int direction,
        ref ChessPiece[,] board,
        List<Vector2Int> availableMoves
    )
    {
        bool isWhite = team == Team.White;
        PawnStartLines startLine =
            isWhite ? PawnStartLines.White : PawnStartLines.Black;
        int firstFieldY = currentY + direction;
        int secondFieldY = currentY + (direction * 2);

        if (
            !IsFieldEmpty(currentX, firstFieldY, ref board) ||
            !IsFieldEmpty(currentX, secondFieldY, ref board) ||
            currentY != (int) startLine
        )
        {
            return;
        }

        AddAvailableMove (currentX, secondFieldY, availableMoves);
    }

    private void AddOneFieldMove(
        int currentX,
        int currentY,
        int direction,
        ref ChessPiece[,] board,
        List<Vector2Int> availableMoves
    )
    {
        int firstFieldY = currentY + direction;
        if (!IsFieldEmpty(currentX, firstFieldY, ref board))
        {
            return;
        }

        AddAvailableMove (currentX, firstFieldY, availableMoves);
    }

    private void AddTakingMove(
        bool toTheRight,
        int currentX,
        int currentY,
        int direction,
        ref ChessPiece[,] board,
        List<Vector2Int> availableMoves
    )
    {
        int directionToTake = toTheRight ? 1 : -1;
        if (
            (!toTheRight || currentX == Chessboard.TILE_COUNT_X - 1) &&
            (toTheRight || currentX == 0)
        )
        {
            return;
        }

        int newX = currentX + directionToTake;
        int newY = currentY + direction;
        ChessPiece piece = board[newX, newY];
        if (IsFieldEmpty(newX, newY, ref board) || piece.team == team)
        {
            return;
        }

        AddAvailableMove (newX, newY, availableMoves);
    }

    private bool
    AddTakingEnPassentMove(
        bool toTheRight,
        int currentX,
        int currentY,
        int direction,
        int endFieldX,
        ref ChessPiece[,] board,
        List<Vector2Int> availableMoves
    )
    {
        int directionToTake = toTheRight ? 1 : -1;
        if (endFieldX != currentX + directionToTake)
        {
            return false;
        }

        AddAvailableMove(currentX + directionToTake,
        currentY + direction,
        availableMoves);

        return true;
    }
}
