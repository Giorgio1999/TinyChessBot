using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Linq;


public class MyBot : IChessBot
{
    int[] pieceValues = { 100, 300, 350, 500, 900, 0 };
    int[] mobilityBoni = { 0, 50, 50, 50, 50, -50 };
    int kingKillBoni = 20;
    int maxDepth = 3;
    int maxCaptureDepth = 3;
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        int score = -10000000;
        int alpha = -100000000;
        int beta = -alpha;
        Move bestMove = moves[0];
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int newScore = -RekursiveSearch(board, 0, -alpha, -beta);
            board.UndoMove(move);
            bestMove = newScore > score ? move : bestMove;
            score = newScore > score ? newScore : score;
            if (board.IsWhiteToMove)
            {
                alpha = alpha > newScore ? alpha : newScore;
                if (beta <= alpha)
                {
                    break;
                }
            }
            else
            {
                beta = beta > newScore ? beta : newScore;
                if (beta >= alpha)
                {
                    break;
                }
            }
        }
        Console.WriteLine(score);
        return bestMove;
    }

    int RekursiveSearch(Board board,int depth, int alpha, int beta)
    {
        if (board.IsDraw())
        {
            return 0;
        }
        if (depth == maxDepth)
        {
            return Evaluate(board);
        }
        Move[] moves = board.GetLegalMoves();
        int score = -10000000;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int newScore = -RekursiveSearch(board, depth+1, -alpha, -beta);
            board.UndoMove(move);
            score = newScore > score ? newScore : score;
            if (board.IsWhiteToMove)
            {
                alpha = alpha > newScore ? alpha : newScore;
                if (beta <= alpha)
                {
                    break;
                }
            }
            else
            {
                beta = beta > newScore ? beta : newScore;
                if (beta >= alpha) 
                {
                    break;
                }
            }
        }
        return score;
    }

    int Evaluate(Board board)
    {
        int score = 0;
        ulong opponentKingAttacks = BitboardHelper.GetKingAttacks(board.GetKingSquare(!board.IsWhiteToMove));
        for (int i = 1; i < 6; i++)
        {
            PieceList myPieces = board.GetPieceList((PieceType)i, board.IsWhiteToMove);
            PieceList opponentPieces = board.GetPieceList((PieceType)i, !board.IsWhiteToMove);
            score += (myPieces.Count - opponentPieces.Count) * pieceValues[i-1];
            foreach (Piece piece in myPieces)
            {
                score += mobilityBoni[i] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, board.IsWhiteToMove));
                //score -= mobilityBoni[i] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, !board.IsWhiteToMove));
                //score += kingKillBoni * BitboardHelper.GetNumberOfSetBits(opponentKingAttacks & BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, board.IsWhiteToMove)); 
            }
        }
        //score += mobilityBoni[5] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks(PieceType.Queen, board.GetKingSquare(board.IsWhiteToMove),board,board.IsWhiteToMove));
        return score;
    }
}