using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        int[] pieceValues = { 100, 300, 350, 500, 900, 0 };
        int[] mobilityBoni = { 0, 50, 50, 50, 50, -50 };
        int kingKillBoni = 20;
        int maxDepth = 2;
        int maxCaptureDepth = 3;
        public Move Think(Board board, Timer timer)
        {
            Move[] moves = board.GetLegalMoves();
            int score = -10000000;
            Move bestMove = moves[0];
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int newScore = -RekursiveSearch(board, 0);
                board.UndoMove(move);
                bestMove = newScore > score ? move : bestMove;
                score = newScore > score ? newScore : score;
            }
            Console.WriteLine(score);
            return bestMove;
        }

        int RekursiveSearch(Board board, int depth)
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
                int newScore = -RekursiveSearch(board, depth + 1);
                board.UndoMove(move);
                score = newScore > score ? newScore : score;
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
                score += (myPieces.Count - opponentPieces.Count) * pieceValues[i - 1];
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


        /*public Move Think(Board board, Timer timer)
        {
            int width = 100;
            int maxDepth = 0;

            bool isWhite = board.IsWhiteToMove;
            int[] pieceValues = { 0, 1, 3, 3, 5, 9, 100 };
            Random randi = new Random();
            Move[] legalMoves = board.GetLegalMoves();
            float[] scores = new float[legalMoves.Length];

            int moveIdx = 0;
            foreach (Move currMove in legalMoves)
            {
                board.MakeMove(currMove);

                Move[] opMoves = board.GetLegalMoves();
                foreach (Move opMove in opMoves)
                {
                    board.MakeMove(opMove);
                    if (board.IsInCheckmate())
                    {
                        scores[moveIdx] -= 1000 / (float)width / (float)opMoves.Length;
                    }
                    board.UndoMove(opMove);
                }
                opMoves = board.GetLegalMoves(true);
                if (opMoves.Length == 0)
                {
                    opMoves = board.GetLegalMoves();
                }
                int opMovesLength = opMoves.Length;
                if (opMoves.Length > 0)
                {
                    for (int w = 0; w < width; w++)
                    {
                        foreach (Move currOpMove in opMoves)
                        {
                            List<Move> movesMade = new List<Move>();
                            board.MakeMove(currOpMove);
                            movesMade.Add(currOpMove);

                            int depth = 0;
                            while (depth <= maxDepth)
                            {
                                Move[] tempMoves = board.GetLegalMoves();
                                foreach (Move move in tempMoves)
                                {
                                    board.MakeMove(move);
                                    if (board.IsInCheckmate())
                                    {
                                        scores[moveIdx] += 1000 / (float)width / (float)opMovesLength;
                                    }
                                    board.UndoMove(move);
                                }
                                tempMoves = board.GetLegalMoves(true);
                                if (tempMoves.Length == 0)
                                {
                                    tempMoves = board.GetLegalMoves();
                                }
                                if (tempMoves.Length > 0)
                                {
                                    int rIdx = randi.Next(tempMoves.Length);
                                    board.MakeMove(tempMoves[rIdx]);
                                    movesMade.Add(tempMoves[rIdx]);

                                    tempMoves = board.GetLegalMoves();
                                    foreach (Move move in tempMoves)
                                    {
                                        board.MakeMove(move);
                                        if (board.IsInCheckmate())
                                        {
                                            scores[moveIdx] -= 1000 / (float)width / (float)opMovesLength;
                                        }
                                        board.UndoMove(move);
                                    }
                                    tempMoves = board.GetLegalMoves(true);
                                    if (tempMoves.Length == 0)
                                    {
                                        tempMoves = board.GetLegalMoves();
                                    }
                                    if (tempMoves.Length > 0)
                                    {
                                        rIdx = randi.Next(tempMoves.Length);
                                        board.MakeMove(tempMoves[rIdx]);
                                        movesMade.Add(tempMoves[rIdx]);
                                        depth++;
                                    }
                                    else if (board.IsInCheckmate())
                                    {
                                        scores[moveIdx] += 1000 / (float)width / (float)opMovesLength;
                                        depth++;
                                    }
                                    else
                                    {
                                        depth++;
                                    }
                                }
                                else if (board.IsInCheckmate())
                                {
                                    scores[moveIdx] -= 1000 / (float)width / (float)opMovesLength;
                                    depth++;
                                }
                                else
                                {
                                    depth++;
                                }
                            }

                            for (int i = 1; i < 7; i++)
                            {
                                ulong pieces = board.GetPieceBitboard((PieceType)i, isWhite);
                                scores[moveIdx] += (float)(pieceValues[i] * BitboardHelper.GetNumberOfSetBits(pieces)) / (float)width / (float)opMovesLength;
                                pieces = board.GetPieceBitboard((PieceType)i, !isWhite);
                                scores[moveIdx] -= (float)(pieceValues[i] * BitboardHelper.GetNumberOfSetBits(pieces)) / (float)width / (float)opMovesLength;
                            }



                            for (int i = movesMade.Count - 1; i >= 0; i--)
                            {
                                board.UndoMove(movesMade[i]);
                            }
                        }
                    }

                }
                else if (board.IsDraw())
                {
                    scores[moveIdx] = 0;
                }
                else if (board.IsInCheckmate())
                {
                    return currMove;
                }
                board.UndoMove(currMove);
                moveIdx++;
            }
            return legalMoves[Array.IndexOf(scores, scores.Max())];
        }
    }*/
    }
}