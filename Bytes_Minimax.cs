using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UvsChess;
namespace ShallowRed
{
    public static class Minimax
    {
        public static AILoggerCallback Log { get; set; }
        /// <summary>
        /// Purpose: To perform the minimax algorithm to determine chess move
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="color"></param>
        /// <returns>char[] board to represent the move</returns>
        ///
        public static byte[] miniMax(byte[] SRFen, ChessColor color)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
            byte[] initialBoard = SRFen;
            bool white;
            int alpha = -10000;
            int beta = 10000;
            int cutoff = 4;
            if (color == ChessColor.White) white = true;
            else white = false;
            int depth = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int h;
            h = minValue(ref SRFen, depth + 1, white, alpha, beta, cutoff, ref timer);
            byte[] bestSoFar = SRFen;
            while (timer.Elapsed < maxTime && h != -9999)
            {
                cutoff += 2;
                byte[] temp = initialBoard;
                h = minValue(ref temp, depth + 1, white, alpha, beta, cutoff, bestSoFar, ref timer);
                if (h != -9999) bestSoFar = temp;
            }
            //this.Log("cutoff" + cutoff);
            return bestSoFar;
        }
        public static int maxValue(ref byte[] board, int depth, bool white, int alpha, int beta, int cutoff, ref Stopwatch timer)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
            ChessColor color = (white ? ChessColor.White : ChessColor.Black);
            int hValue;
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);
            int count = childrenBoard.Count;
            if (count == 0)
            { //no moves available
                return -1000;
            }
            //sort array of moves
            int[] Hchildren = new int[count];
            for (int idx = 0; idx < count; ++idx)
                Hchildren[idx] = Heuristic.GetHeuristicValue(childrenBoard[idx], color);
            if (depth != cutoff)
                sort(ref childrenBoard, ref Hchildren);
            int maximumValue = -10000;
            int i = 0;
            byte[] tempBoard = null;
            while (i < count && timer.Elapsed < maxTime)
            { //process all the children move
                if (depth != cutoff)
                {
                    tempBoard = childrenBoard[i];
                    if (timer.Elapsed < maxTime)
                        hValue = minValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                    else
                        return -9999;
                }
                else
                    hValue = Hchildren[i];
                if (depth == 1)
                {
                    if (maximumValue == hValue)
                    {
                        //choose randomly between current choice and this move
                        Random rnd = new Random();
                        int value = rnd.Next(0, 1);
                        if (value == 0)
                            board = tempBoard;
                    }
                    else if (hValue > maximumValue)
                    {
                        maximumValue = hValue;
                        board = childrenBoard[i];
                    }
                    if (maximumValue >= beta)
                    {
                        hValue = maximumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    alpha = Math.Max(alpha, maximumValue);
                }
                else
                {
                    maximumValue = Math.Max(maximumValue, hValue);
                    if (maximumValue >= beta)
                    {
                        hValue = maximumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    alpha = Math.Max(alpha, maximumValue);
                }
                ++i;
            }
            childrenBoard = null;
            if (timer.Elapsed > maxTime) return -9999;
            hValue = maximumValue;
            return hValue;
        }
        public static int minValue(ref byte[] board, int depth, bool white, int alpha, int beta, int cutoff, ref Stopwatch timer)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(5500);
            int hValue;
            LightList equalMoves = new LightList();
            ChessColor color = (white ? ChessColor.White : ChessColor.Black);
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);
            int count = childrenBoard.Count;
            if (count == 0)
            { //no moves available
                return -1000;
            }
            //sort array of moves
            int[] Hchildren = new int[count];
            for (int idx = 0; idx < count; ++idx)
            {
                Hchildren[idx] = Heuristic.GetHeuristicValue(childrenBoard[idx], color);
            }
            if (depth != cutoff)
                sort(ref childrenBoard, ref Hchildren);
            int minimumValue = 10000;
            int i = 0;
            byte[] tempBoard = null;
            while (i < count)
            { //process all the children move
                if (depth != cutoff)
                {
                    tempBoard = childrenBoard[i];
                    if (timer.Elapsed < maxTime)
                        hValue = maxValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                    else
                        return -9999;
                }
                else
                    //get heuristics value
                    hValue = Hchildren[i];
                if (depth == 1)
                {
                    if (minimumValue == hValue)
                    {//store move into list of moves of same value
                        equalMoves.Add(tempBoard);
                    }
                    else if (hValue < minimumValue)
                    {//new minimum value, reset the move list
                        minimumValue = hValue;
                        board = childrenBoard[i];
                        equalMoves.Empty();
                        equalMoves.Add(board);
                    }
                    if (minimumValue <= alpha)
                    {
                        board = getRandomMove(equalMoves);
                        childrenBoard = null;
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                else
                {
                    minimumValue = Math.Min(minimumValue, hValue);
                    if (minimumValue <= alpha)
                    {
                        hValue = minimumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                ++i;
            }
            hValue = minimumValue;
            if (depth == 1)
            {
                board = getRandomMove(equalMoves);
            }
            childrenBoard = null;
            if (timer.Elapsed > maxTime) return -9999;
            hValue = minimumValue;
            return hValue;
        }
        //This min value version is only called at depth 1 when a previously found best move is used to inform the ordering of moves
        public static int minValue(ref byte[] board, int depth, bool white, int alpha, int beta, int cutoff, byte[] best, ref Stopwatch timer)
        {
            ChessColor color = (white ? ChessColor.White : ChessColor.Black);
            int hValue;
            LightList equalMoves = new LightList();
            LightList childrenBoard = FEN.GetAvailableMoves(board, color, false);
            int count = childrenBoard.Count;
            if (count == 0)
            { //no moves available
                return -1000;
            }
            //sort array of moves
            int[] Hchildren = new int[count];
            for (int idx = 0; idx < count; ++idx)
                Hchildren[idx] = Heuristic.GetHeuristicValue(childrenBoard[idx], color);
            sort(ref childrenBoard, ref Hchildren);
            bool found = false;
            int indexFound = 0;
            for (int idx = 0; idx < count; ++idx)
            {
                found = true;
                for (int index = 0; index < FEN.OUTOFBOUNDSHIGH; ++index)
                {
                    if (best[index] != childrenBoard[idx][index])
                    {
                        found = false;
                        break;
                    }
                }
                if (found == true)
                {
                    indexFound = idx;
                    break;
                }
            }
            //prioritize previously found best move
            if (indexFound != 0)
            {//swap
                byte[] temp;
                temp = childrenBoard[0];
                childrenBoard.Replace(childrenBoard[indexFound], 0);
                childrenBoard.Replace(temp, indexFound);
            }
            int minimumValue = 10000;
            int i = 0;
            byte[] tempBoard = null;
            while (i < count)
            { //process all the children moves
                if (depth != cutoff)
                {
                    tempBoard = childrenBoard[i];
                    hValue = maxValue(ref tempBoard, depth + 1, !white, alpha, beta, cutoff, ref timer);
                }
                else //get heuristics value
                    hValue = Hchildren[i];
                if (depth == 1)
                {
                    if (minimumValue == hValue)
                    {
                        equalMoves.Add(tempBoard);
                    }
                    else if (hValue < minimumValue)
                    {
                        minimumValue = hValue;
                        board = childrenBoard[i];
                        equalMoves.Empty();
                        equalMoves.Add(board);
                    }
                    if (minimumValue <= alpha)
                    {
                        board = getRandomMove(equalMoves);
                        childrenBoard = null;
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                else
                {
                    minimumValue = Math.Min(minimumValue, hValue);
                    if (minimumValue <= alpha)
                    {
                        hValue = minimumValue;
                        childrenBoard = null;
                        return hValue;
                    }
                    beta = Math.Min(beta, minimumValue);
                }
                ++i;
            }
            hValue = minimumValue;
            if (depth == 1)
                board = getRandomMove(equalMoves);
            childrenBoard = null;
            return hValue;
        }
        private static byte[] getRandomMove(LightList equalMoves)
        {
            if (equalMoves.Count == 1)
                return equalMoves[0];
            else
            {
                Random rnd = new Random();
                int value = rnd.Next(0, equalMoves.Count - 1);
                return equalMoves[value];
            }
        }
        private static void sort(ref LightList children, ref int[] h)
        {
            int count = children.Count;
            bool swapped;
            do
            {
                swapped = false;
                for (int j = 1; j < count; ++j)
                {
                    if (h[j - 1] < h[j])
                    {
                        int temp;
                        byte[] tempBoard;
                        temp = h[j];
                        tempBoard = children[j];
                        h[j] = h[j - 1];
                        h[j - 1] = temp;
                        children.Replace(children[j - 1], j);
                        children.Replace(tempBoard, j - 1);
                        swapped = true;
                    }
                }
                count--;
            } while (swapped);
        }
    }
}
