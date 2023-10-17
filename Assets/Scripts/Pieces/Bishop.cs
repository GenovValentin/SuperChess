using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        AddTopRightMoves(ref board, currentX, currentY, team, availableMoves);

        AddTopLeftMoves(ref board, currentX, currentY, team, availableMoves);

        AddBottomRightMoves(ref board,
        currentX,
        currentY,
        team,
        availableMoves);

        AddBottomLeftMoves(ref board, currentX, currentY, team, availableMoves);

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
        bool isFieldEmpty = piece == null;

        if (isFieldEmpty || piece.team != team)
        {
            availableMoves.Add(new Vector2Int(x, y));
        }

        return isFieldEmpty;
    }

    public static void AddBottomLeftMoves(
        ref ChessPiece[,] board,
        int currentX,
        int currentY,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        for (
            int
                x = currentX - 1,
                y = currentY - 1;
            x >= 0 && y >= 0;
            x--, y--
        )
        {
            bool shouldAddMoreMoves =
                AddAvailableMove(ref board, x, y, team, availableMoves);

            if (!shouldAddMoreMoves)
            {
                break;
            }
        }
    }

    public static void AddBottomRightMoves(
        ref ChessPiece[,] board,
        int currentX,
        int currentY,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        for (
            int
                x = currentX + 1,
                y = currentY - 1;
            x < Board.TILE_COUNT_X && y >= 0;
            x++, y--
        )
        {
            bool shouldAddMoreMoves =
                AddAvailableMove(ref board, x, y, team, availableMoves);

            if (!shouldAddMoreMoves)
            {
                break;
            }
        }
    }

    public static void AddTopLeftMoves(
        ref ChessPiece[,] board,
        int currentX,
        int currentY,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        for (
            int
                x = currentX - 1,
                y = currentY + 1;
            x >= 0 && y < Board.TILE_COUNT_Y;
            x--, y++
        )
        {
            bool shouldAddMoreMoves =
                AddAvailableMove(ref board, x, y, team, availableMoves);

            if (!shouldAddMoreMoves)
            {
                break;
            }
        }
    }

    public static void AddTopRightMoves(
        ref ChessPiece[,] board,
        int currentX,
        int currentY,
        Team team,
        List<Vector2Int> availableMoves
    )
    {
        for (
            int
                x = currentX + 1,
                y = currentY + 1;
            x < Board.TILE_COUNT_X && y < Board.TILE_COUNT_Y;
            x++, y++
        )
        {
            bool shouldAddMoreMoves =
                AddAvailableMove(ref board, x, y, team, availableMoves);

            if (!shouldAddMoreMoves)
            {
                break;
            }
        }
    }
}
