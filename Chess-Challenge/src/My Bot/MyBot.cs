using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Linq;


public class MyBot : IChessBot
{
    int[] pieceValues = { 100, 300, 350, 500, 900, 0 };
    int[] mobilityBoni = { 0, 20, 20, 20, 20, -50 };
    int kingKillBoni = 1;
    int maxDepth = 5;
    int maxCaptureDepth = 3;
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        int[] scores = new int[moves.Length];
        //Iterative deepening loop
        for (int i = 1; i < maxDepth; i++)
        {
            scores = Search(moves, board, i);
            Array.Sort(scores,moves);
            //Always sort in acsending order, therefore best move is last or first move depending on color
            if (board.IsWhiteToMove)
            {
                Array.Reverse(moves);
                Array.Reverse(scores);
            }
        }
        Console.WriteLine(scores[0]);
        return moves[0];
    }

    //Search function for a single iteration of iterative deepening loop
    int[] Search(Move[] moves, Board board, int maxDepth)
    {
        int[] scores = new int[moves.Length];
        int alpha = -100000000;
        int beta = -alpha;
        for (int i = 0; i < moves.Length; i++)
        {
            board.MakeMove(moves[i]);
            scores[i] = RekursiveSearch(board, maxDepth-1, alpha, beta,board.IsWhiteToMove);
            board.UndoMove(moves[i]);
        }
        return scores;
    }

    //Rekursive search with alphe, beta pruning
    int RekursiveSearch(Board board,int depth, int alpha, int beta, bool maximizingPlayer)
    {
        //Check for draws...might needs improvement
        if (board.IsDraw())
        {
            return 0;
        }
        //Static eval if depth==0
        if (depth == 0)
        {
            return Evaluate(board);
        }
        Move[] moves = board.GetLegalMoves();
        int score = -10000000;
        //not very elegant difference in function depending on which person is evaluating
        if (maximizingPlayer)
        {
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int newScore = RekursiveSearch(board, depth - 1, alpha, beta, false);
                board.UndoMove(move);
                score = newScore > score ? newScore : score;
                alpha = alpha > newScore ? alpha : newScore;
                if (beta <= alpha)
                {
                    break;
                }
            }
        }
        else
        {
            score *= -1;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int newScore = RekursiveSearch(board, depth - 1, alpha, beta, true);
                board.UndoMove(move);
                score = newScore < score ? newScore : score;
                beta = beta > newScore ? newScore : beta;
                if (beta <= alpha)
                {
                    break;
                }
            }

        }
        return score;
    }

    //Static evaluation function
    int Evaluate(Board board)
    {
        int score = 0;
        ulong whiteKingAttacks = BitboardHelper.GetKingAttacks(board.GetKingSquare(true));
        ulong blackKingAttacks = BitboardHelper.GetKingAttacks(board.GetKingSquare(false));
        for (int i = 1; i < 7; i++)
        {
            PieceList whitePieces = board.GetPieceList((PieceType)i,true);
            PieceList blackPieces = board.GetPieceList((PieceType)i, false);
            //Material difference
            score += (whitePieces.Count - blackPieces.Count) * pieceValues[i-1];
            foreach (Piece piece in whitePieces)
            {
                score +=
                        //White pieces mobility = number of pieces attacked by given piecetype which are not blocked by white pieces, multiplied by mobilityboni 
                        mobilityBoni[i - 1] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, true) & ~board.WhitePiecesBitboard)
                        //Black pieces mobility
                        - mobilityBoni[i - 1] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, false) & ~board.BlackPiecesBitboard)
                        //Black King unsafety = number of squares the black king can move to which are also attacked by white pieces of the given piece type and not occupied by black pieces
                        + kingKillBoni * BitboardHelper.GetNumberOfSetBits(blackKingAttacks & BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, true) & ~board.BlackPiecesBitboard)
                        //White King unsafety
                        - kingKillBoni * BitboardHelper.GetNumberOfSetBits(whiteKingAttacks & BitboardHelper.GetPieceAttacks((PieceType)i, piece.Square, board, false) & ~board.WhitePiecesBitboard)
                        //First move advantage to help with draws so that black chooses a draw over a slightly worse position while white chooses the worse position
//                          + 10
                        ;
            }
        }
        //score += mobilityBoni[5] * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks(PieceType.Queen, board.GetKingSquare(board.IsWhiteToMove),board,board.IsWhiteToMove));
        return score;
    }
}