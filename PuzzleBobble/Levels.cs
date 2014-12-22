/*
 *  Zach Lesperance, CSCD371
 * 
 *  Puzzle Bobble (A.K.A. Bust-a-Move)
 *  
 *  Bubble-bursting game based on the 1994 arcade game by the same name
 * 
 */ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBobble
{
    partial class MainWindow
    {
        private static Random gridRandomizer;
        public void LoadLevel(int n)
        {
            currentLevel = n;
            if (n == 0)
            {
                /* Level 1 */
                /*  a a b b c c d d
                 *   a a b b c c d
                 *  c c d d a a b b
                 *   c d d a a b b
                 */
                string buildInstructions = "1 1 2 2 3 3 4 4 b 1 1 2 2 3 3 4 b 3 3 4 4 1 1 2 2 b 3 4 4 1 1 2 2";
                BuildLevel(buildInstructions);
            }
            else if (n == 1)
            {
                /* Level 2 */
                /*  _ _ _ a a _ _ _
                 *   _ _ _ b _ _ _
                 *  _ _ _ b _ _ _ _
                 *   _ _ _ x _ _ _
                 *  _ _ _ x _ _ _ _
                 *   _ _ _ x _ _ _
                 *  _ _ _ x _ _ _ _
                 *   _ _ _ x _ _ _
                 */
                string buildInstructions = "0 0 0 1 1 b 0 0 0 2 b 0 0 0 2 b 0 0 0 3 b 0 0 0 3 b 0 0 0 x b 0 0 0 x b 0 0 0 x b";
                BuildLevel(buildInstructions, 6);
            }
            else if (n == 2)
            {
                /* Level 3 */
                /*
                 *  a _ _ _ _ _ _ a
                 *   b a c d b a c
                 *  d _ _ _ _ _ _ d
                 *   c d b a c d b
                 *  _ _ _ b _ _ _ _
                 *   _ _ _ a _ _ _
                 *  _ _ _ b _ _ _ _
                 * 
                 */
                string buildInstructions = "1 0 0 0 0 0 0 1 b 2 1 3 4 2 1 3 b 4 0 0 0 0 0 0 4 b 3 4 2 1 3 4 2 b 0 0 0 2 b 0 0 0 1 b 0 0 0 2";
                BuildLevel(buildInstructions);
            }
            else if (n == 3)
            {
                /* Level 4 */
                /*
                 *  _ a a _ _ b b _
                 *   _ x _ _ _ x _
                 *  _ x _ _ _ x _ _
                 *   _ x _ _ _ x _
                 *  _ x _ _ _ x _ _
                 *   _ x _ _ _ x _
                 *  _ x _ _ _ x _ _
                 *   _ x _ _ _ x _
                 * 
                 */
                string buildInstructions = "0 1 1 0 0 2 2 b";
                for (int i = 0; i < 7; i++)
                {
                    buildInstructions += " 0 x 0 0 0 x b";
                }
                BuildLevel(buildInstructions, 6);
            }
            else if (n == 4)
            {
                /* Level 5 */
                /*
                 *  _ x _ x _ x _ x
                 *   x _ x _ x _ x
                 *  x _ x _ x _ x _
                 *   _ x _ x _ x _
                 *  _ x _ x _ x _ _
                 *   x _ x _ x _ _
                 *  _ _ x _ x _ _ _
                 *   _ _ _ x _ _ _
                 * 
                 */
                string buildInstructions = "0 x 0 x 0 x 0 x b x 0 x 0 x 0 x b x 0 x 0 x 0 x b 0 x 0 x 0 x b 0 x 0 x 0 x b x 0 x 0 x b 0 0 x 0 x b 0 0 0 x";
                BuildLevel(buildInstructions, 6);
            }
            else if (n == 5)
            {
                /* Level 6 */
                /*
                 *  x x x x x x x x
                 *   x _ x _ x _ x
                 *  _ x x x x x x _
                 *   _ x _ x _ x _
                 *  _ x x x x x x _
                 *   x _ x _ x _ x
                 *  x x x x x x x x
                 * 
                 */
                string buildInstructions = "x x x x x x x x b x 0 x 0 x 0 x b 0 x x x x x x b 0 x 0 x 0 x b 0 x x x x x x b x 0 x 0 x 0 x b x x x x x x x x";
                BuildLevel(buildInstructions, 6);
            }
            else if (n == 6)
            {
                /* Level 7 */
                /*
                 *  _ _ _ x x _ _ _
                 *   _ _ x x x _ _
                 *  _ _ _ x x _ _ _
                 *   _ x x _ x x _
                 *  _ x x x x x x _
                 *   _ x x _ x x _
                 * 
                 */
                string buildInstructions = "0 0 0 x x b 0 0 x x x b 0 0 0 x x b 0 x x 0 x x b 0 x x x x x x b 0 x x 0 x x";
                BuildLevel(buildInstructions, 6);
            }
            else if (n == 7)
            {
                /* Level 8 */
                /*
                 *  a b c d e f a b
                 *   e f a b c d e
                 *  a b c d e f a b
                 *   e f a b c d e
                 */
                string buildInstructions = "1 2 3 4 5 6 1 2 b 5 6 1 2 3 4 5 b 1 2 3 4 5 6 1 2 b 5 6 1 2 3 4 5";
                BuildLevel(buildInstructions);
            }
            else if (n == 8)
            {
                /* Level 9 */
                /*
                 *  a a a a a a a a
                 *   a _ _ _ _ _ a
                 *  a _ _ x x x x x
                 *   a _ _ _ x x x
                 *  a _ _ _ _ _ x x
                 *   x x _ _ _ _ _
                 *  x x x x _ _ _ _
                 *   x x x x x _ _
                 */
                string buildInstructions = "1 1 1 1 1 1 1 1 b 1 0 0 0 0 0 1 b 1 0 0 x x x x x b 1 0 0 0 x x x b 1 0 0 0 0 0 x x b x x b x x x x b x x x x x";
                BuildLevel(buildInstructions, 6);                
            }
            else if (n == 9)
            {
                /* Level 10 */
                /*
                 *  a _ _ _ _ _ _ b
                 *   b c d e f g a
                 *  a _ _ _ _ _ _ b
                 *   b c d e f g a
                 *  a _ _ _ _ _ _ b
                 *   b c d e f g a
                 */
                string odd = "1 0 0 0 0 0 0 2 b";
                string even = "2 3 4 5 6 7 1 b";
                string buildInstructions = odd + " " + even + " " + odd + " " + even + " " + odd + " " + even;
                BuildLevel(buildInstructions);
            }
            else if (n == -1)
            {
                // Debug Level
                string buildInstructions = "0 0 0 0 0 0 0 x b 0 0 0 0 0 0 x b 0 0 0 0 0 0 0 x b 0 0 0 0 0 0 x b 0 0 0 0 0 0 0 x b 0 0 0 0 0 0 x b 0 0 0 0 0 0 0 x";
                BuildLevel(buildInstructions, 6);
            }
        }
        /*  Build Instructions
         * 
         *  0 for empty space
         *  1-8 for bubble colors (1-8 are randomly assigned but all 1s are the same)
         *  x for random
         *  b for line break
         *  
         */ 
        private void BuildLevel(string instructions, int maxColors = 0)
        {
            ClearGrid();
            string[] input = instructions.ToLower().Split(new char [] {' '});
            Bubble.Color?[] colorAssignments = new Bubble.Color?[8]; // TODO: Change 8 to the dynamic number of colors
            List<Bubble.Color> remainingAssignments = new List<Bubble.Color>((Bubble.Color[])Enum.GetValues(typeof(Bubble.Color)));
            List<Tuple<int, int>> rands = new List<Tuple<int, int>>();
            int row = 0;
            int col = 0;
            int thisVal;
            bubbleGrid[0] = new Bubble[STAGE_BUBBLE_WIDTH];
            foreach (string s in input)
            {
                if (s != "0")
                {
                    if (s == "b")
                    {
                        row++;
                        if (row % 2 == 0)
                            bubbleGrid[row] = new Bubble[STAGE_BUBBLE_WIDTH];
                        else
                            bubbleGrid[row] = new Bubble[STAGE_BUBBLE_WIDTH - 1];
                        col = -1; // Will become 0 at the end of the loop
                    }
                    else if (s == "x")
                    {
                        rands.Add(new Tuple<int, int>(row, col));
                    }
                    else if (Int32.TryParse(s, out thisVal) == true)
                    {
                        if (colorAssignments[thisVal] == null)
                        {
                            Bubble.Color pulled = remainingAssignments[gridRandomizer.Next(remainingAssignments.Count)];
                            remainingAssignments.Remove(pulled);
                            colorAssignments[thisVal] = pulled;
                        }
                        Bubble.Color thisColor = (Bubble.Color)colorAssignments[thisVal];
                        bubbleGrid[row][col] = new Bubble(thisColor);
                        bubbleCounter.Tick(thisColor);
                    }
                }
                col++;
            } // end foreach
            // Now that all colors that are in this grid are loaded, figure out the randoms
            List<Bubble.Color> randColors = new List<Bubble.Color>();
            foreach (Bubble.Color? c in colorAssignments)
            {
                if (c != null)
                    randColors.Add((Bubble.Color)c);
            }
            for (int i = 0; randColors.Count < maxColors && i < remainingAssignments.Count; i++)
            {
                randColors.Add(remainingAssignments[i]);
            }
            foreach (Tuple<int, int> coord in rands)
            {
                // Issue: Below has possiblity of grabbing null
                Bubble.Color thisColor = randColors[gridRandomizer.Next(randColors.Count)];
                bubbleGrid[coord.Item1][coord.Item2] = new Bubble(thisColor);
                bubbleCounter.Tick(thisColor);
            }
            ResetCeiling();
            ResetCeilCounter();
            ConstructGrid();
            LevelReady();
        } // end BuildLevel
    }
}
