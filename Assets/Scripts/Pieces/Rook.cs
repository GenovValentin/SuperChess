using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        AddTopMoves(ref board, currentX, currentY, team, availableMoves);

        AddBottomMoves(ref board, currentX, currentY, team, availableMoves);

        AddLeftMoves(ref board, currentX, currentY, team, availableMoves);

        AddRightMoves(ref board, currentX, currentY, team, availableMoves);

        return availableMoves;
    }

    private static bool
    AddAvailableMove(
        ref ChessPiece[,] board,
        int x,
        int y,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        ChessPiece piece = board[x, y];
        bool isFieldEmpty = IsFieldEmpty(x, y, ref board);

        if (isFieldEmpty || piece.team != team)
        {
            availableMoves.Add(new Vector2Int(x, y));
        }

        return isFieldEmpty;
    }

    public static void AddTopMoves(
        ref ChessPiece[,] board,
        int currentX,
        int currentY,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        for (int i = currentY + 1; i < Board.TILE_COUNT_Y; i++)
        {
            bool shouldAddMoreMoves =
                AddAvailableMove(ref board, currentX, i, team, availableMoves);

            if (!shouldAddMoreMoves)
            {
                break;
            }
        }
    }

    public static void AddBottomMoves(
        ref ChessPiece[,] board,
        int currentX,
        int currentY,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        for (int i = currentY - 1; i >= 0; i--)
        {
            bool shouldAddMoreMoves =
                AddAvailableMove(ref board, currentX, i, team, availableMoves);

            if (!shouldAddMoreMoves)
            {
                break;
            }
        }
    }

    public static void AddLeftMoves(
        ref ChessPiece[,] board,
        int currentX,
        int currentY,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        for (int i = currentX - 1; i >= 0; i--)
        {
            bool shouldAddMoreMoves =
                AddAvailableMove(ref board, i, currentY, team, availableMoves);

            if (!shouldAddMoreMoves)
            {
                break;
            }
        }
    }

    public static void AddRightMoves(
        ref ChessPiece[,] board,
        int currentX,
        int currentY,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        for (int i = currentX + 1; i < Board.TILE_COUNT_Y; i++)
        {
            bool shouldAddMoreMoves =
                AddAvailableMove(ref board, i, currentY, team, availableMoves);

            if (!shouldAddMoreMoves)
            {
                break;
            }
        }
    }
}
