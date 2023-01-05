using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        AddTopRightMoves(ref board, availableMoves);

        AddTopLeftMoves(ref board, availableMoves);

        AddBottomRightMoves(ref board, availableMoves);

        AddBottomLeftMoves(ref board, availableMoves);

        return availableMoves;
    }

    private void AddAvailableMove(
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

    private bool IsFieldOnBoard(int x, int y)
    {
        return x >= 0 &&
        y >= 0 &&
        x < Chessboard.TILE_COUNT_X &&
        y < Chessboard.TILE_COUNT_Y;
    }

    private void AddTopRightMoves(
        ref ChessPiece[,] board,
        List<Vector2Int> availableMoves
    )
    {
        int x = currentX + 1;
        int y = currentY + 2;
        AddAvailableMove(ref board, x, y, availableMoves);

        x = currentX + 2;
        y = currentY + 1;
        AddAvailableMove(ref board, x, y, availableMoves);
    }

    private void AddTopLeftMoves(
        ref ChessPiece[,] board,
        List<Vector2Int> availableMoves
    )
    {
        int x = currentX - 1;
        int y = currentY + 2;
        AddAvailableMove(ref board, x, y, availableMoves);

        x = currentX - 2;
        y = currentY + 1;
        AddAvailableMove(ref board, x, y, availableMoves);
    }

    private void AddBottomLeftMoves(
        ref ChessPiece[,] board,
        List<Vector2Int> availableMoves
    )
    {
        int x = currentX - 1;
        int y = currentY - 2;
        AddAvailableMove(ref board, x, y, availableMoves);

        x = currentX - 2;
        y = currentY - 1;
        AddAvailableMove(ref board, x, y, availableMoves);
    }

    private void AddBottomRightMoves(
        ref ChessPiece[,] board,
        List<Vector2Int> availableMoves
    )
    {
        int x = currentX + 1;
        int y = currentY - 2;
        AddAvailableMove(ref board, x, y, availableMoves);

        x = currentX + 2;
        y = currentY - 1;
        AddAvailableMove(ref board, x, y, availableMoves);
    }
}
