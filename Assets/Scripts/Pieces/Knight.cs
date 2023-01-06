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
