using System.Collections.Generic;
using UnityEngine;

public enum StartRank
{
    White = 0,
    Black = 7
}

public enum StartFileRook
{
    QueenSide = 0,
    KingSide = 7
}

public enum EndFileKing
{
    QueenSide = 2,
    KingSide = 6
}

public class King : ChessPiece
{
    public const int StartFileKing = 4;

    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        AddAvailableMove(ref board, currentX - 1, currentY, availableMoves);
        AddAvailableMove(ref board, currentX - 1, currentY - 1, availableMoves);
        AddAvailableMove(ref board, currentX - 1, currentY + 1, availableMoves);
        AddAvailableMove(ref board, currentX, currentY - 1, availableMoves);
        AddAvailableMove(ref board, currentX, currentY + 1, availableMoves);
        AddAvailableMove(ref board, currentX + 1, currentY, availableMoves);
        AddAvailableMove(ref board, currentX + 1, currentY - 1, availableMoves);
        AddAvailableMove(ref board, currentX + 1, currentY + 1, availableMoves);

        return availableMoves;
    }

    public override SpecialMove
    GetSpecialMoves(
        ref ChessPiece[,] board,
        ref List<Vector2Int[]> moveList,
        ref List<Vector2Int> availableMoves
    )
    {
        bool wasKingMoved = WasKingMoved(ref moveList);
        bool wasQueenSideRookMoved = WasRookMoved(ref moveList, true);
        bool wasKingSideRookMoved = WasRookMoved(ref moveList, false);

        if (wasKingMoved || currentX != StartFileKing)
        {
            return SpecialMove.None;
        }

        if (
            IsCorrectRookOnStartSquare(wasQueenSideRookMoved,
            true,
            Team.White,
            ref board) &&
            AreQueenSideCastlingFieldsEmpty(StartRank.White, ref board)
        )
        {
            AddCastlingMove(true, Team.White, availableMoves);
            return SpecialMove.Castling;
        }

        if (
            IsCorrectRookOnStartSquare(wasKingSideRookMoved,
            false,
            Team.White,
            ref board) &&
            AreKingSideCastlingFieldsEmpty(StartRank.White, ref board)
        )
        {
            AddCastlingMove(false, Team.White, availableMoves);
            return SpecialMove.Castling;
        }

        if (
            IsCorrectRookOnStartSquare(wasQueenSideRookMoved,
            true,
            Team.Black,
            ref board) &&
            AreQueenSideCastlingFieldsEmpty(StartRank.Black, ref board)
        )
        {
            AddCastlingMove(true, Team.Black, availableMoves);
            return SpecialMove.Castling;
        }

        if (
            IsCorrectRookOnStartSquare(wasQueenSideRookMoved,
            false,
            Team.Black,
            ref board) &&
            AreKingSideCastlingFieldsEmpty(StartRank.Black, ref board)
        )
        {
            AddCastlingMove(false, Team.Black, availableMoves);
            return SpecialMove.Castling;
        }

        return SpecialMove.None;
    }

    private bool WasKingMoved(ref List<Vector2Int[]> moveList)
    {
        var kingMove =
            moveList
                .Find(move =>
                {
                    Vector2Int startField = move[0];
                    StartRank startingRankKing = GetStartRankKing(team);

                    return startField.x == StartFileKing &&
                    startField.y == (int) startingRankKing;
                });

        return kingMove != null;
    }

    private bool WasRookMoved(ref List<Vector2Int[]> moveList, bool isQueenSide)
    {
        var moveWithRook =
            moveList
                .Find(move =>
                {
                    Vector2Int startField = move[0];
                    StartRank startingRankKing = GetStartRankKing(team);
                    StartFileRook startingFileRook =
                        GetStartFileRook(isQueenSide);

                    return startField.x == (int) startingFileRook &&
                    startField.y == (int) startingRankKing;
                });

        return moveWithRook != null;
    }

    private StartRank GetStartRankKing(Team teamToCheck)
    {
        return teamToCheck == Team.White ? StartRank.White : StartRank.Black;
    }

    private StartFileRook GetStartFileRook(bool isQueenSide)
    {
        return isQueenSide ? StartFileRook.QueenSide : StartFileRook.KingSide;
    }

    private bool
    AreFieldsEmpty(
        int startX,
        int endX,
        StartRank startingRank,
        ref ChessPiece[,] board
    )
    {
        for (int i = startX; i <= endX; i++)
        {
            if (!IsFieldEmpty(i, (int) startingRank, ref board))
            {
                return false;
            }
        }

        return true;
    }

    private bool
    AreKingSideCastlingFieldsEmpty(
        StartRank startingRank,
        ref ChessPiece[,] board
    )
    {
        return AreFieldsEmpty(5, 6, startingRank, ref board);
    }

    private bool
    AreQueenSideCastlingFieldsEmpty(
        StartRank startingRank,
        ref ChessPiece[,] board
    )
    {
        return AreFieldsEmpty(1, 3, startingRank, ref board);
    }

    private bool
    IsCorrectRookOnStartSquare(
        bool wasRookMoved,
        bool isQueenSide,
        Team rookTeam,
        ref ChessPiece[,] board
    )
    {
        ChessPiece startingPiece =
            board[(int) GetStartFileRook(isQueenSide),
            (int) GetStartRankKing(rookTeam)];

        return !wasRookMoved &&
        team == rookTeam &&
        startingPiece.type == ChessPieceType.Rook &&
        startingPiece.team == rookTeam;
    }

    private void AddCastlingMove(
        bool isQueenSide,
        Team currentTeam,
        List<Vector2Int> availableMoves
    )
    {
        availableMoves
            .Add(new Vector2Int((int) GetEndFileKing(isQueenSide),
                (int) GetStartRankKing(currentTeam)));
    }

    private EndFileKing GetEndFileKing(bool isQueenSide)
    {
        return isQueenSide ? EndFileKing.QueenSide : EndFileKing.KingSide;
    }
}
