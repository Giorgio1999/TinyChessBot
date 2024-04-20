using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Linq;


public class MyBot : IChessBot
{
    int[] pieceValues = { 100, 300, 350, 500, 900, 0 };
    int[] mobilityBoni = { 0, 1, 1, 1, 1, -50 };
    int kingKillBoni = 20;
    int maxDepth = 4;
    int maxCaptureDepth = 3;
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        for (int i = 0; i < maxDepth; i++)
        {
            int[] scores = Search(moves, board, i);
            Array.Sort(scores,moves);
            Array.Reverse(moves);
            Console.WriteLine(scores[scores.Length-1]);
        }
        return moves[0];
    }

    int[] Search(Move[] moves, Board board, int maxDepth)
    {
        int[] scores = new int[moves.Length];
        int alpha = -100000000;
        int beta = -alpha;
        for (int i = 0; i < moves.Length; i++)
        {
            board.MakeMove(moves[i]);
            scores[i] = -RekursiveSearch(board, maxDepth, -alpha, -beta);
            board.UndoMove(moves[i]);
        }
        return scores;
    }

    int RekursiveSearch(Board board,int depth, int alpha, int beta)
    {
        if (board.IsDraw())
        {
            return 0;
        }
        if (depth == 0)
        {
            return Evaluate(board);
        }
        Move[] moves = board.GetLegalMoves();
        int score = -10000000;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int newScore = -RekursiveSearch(board, depth-1, -alpha, -beta);
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
        for (int i = 1; i < 7; i++)
        {
            PieceList myPieces = board.GetPieceList((PieceType)i, board.IsWhiteToMove);
            PieceList opponentPieces = board.GetPieceList((PieceType)i, !board.IsWhiteToMove);
            score += (myPieces.Count - opponentPieces.Count) * pieceValues[i-1];
            foreach (Piece piece in myPieces)
            {
                score += mobilityBoni[i-1] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, board.IsWhiteToMove));
                //score -= mobilityBoni[i] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, !board.IsWhiteToMove));
                //score += kingKillBoni * BitboardHelper.GetNumberOfSetBits(opponentKingAttacks & BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, board.IsWhiteToMove)); 
            }
        }
        //score += mobilityBoni[5] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks(PieceType.Queen, board.GetKingSquare(board.IsWhiteToMove),board,board.IsWhiteToMove));
        return score;
    }
}