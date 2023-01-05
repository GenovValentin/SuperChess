using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        Rook.AddTopMoves(ref board, currentX, currentY, team, availableMoves);

        Rook
            .AddBottomMoves(ref board,
            currentX,
            currentY,
            team,
            availableMoves);

        Rook.AddLeftMoves(ref board, currentX, currentY, team, availableMoves);

        Rook.AddRightMoves(ref board, currentX, currentY, team, availableMoves);

        Bishop
            .AddTopRightMoves(ref board,
            currentX,
            currentY,
            team,
            availableMoves);

        Bishop
            .AddTopLeftMoves(ref board,
            currentX,
            currentY,
            team,
            availableMoves);

        Bishop
            .AddBottomRightMoves(ref board,
            currentX,
            currentY,
            team,
            availableMoves);

        Bishop
            .AddBottomLeftMoves(ref board,
            currentX,
            currentY,
            team,
            availableMoves);

        return availableMoves;
    }
}
