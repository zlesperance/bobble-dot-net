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
    class ColorCounter
    {
        private Dictionary<Bubble.Color, int> data;
        private List<Bubble.Color> remainingColors;
        private bool changed;

        public List<Bubble.Color> Remaining
        {
            get
            {
                if (changed)
                {
                    remainingColors = new List<Bubble.Color>();
                    foreach (Bubble.Color key in data.Keys)
                    {
                        if (data[key] > 0)
                            remainingColors.Add(key);
                    }
                    changed = false;
                }
                return remainingColors;
            }
        }

        public int DistinctColors
        {
            get
            {
                return remainingColors.Count;
            }
        }

        public ColorCounter()
        {
            data = new Dictionary<Bubble.Color, int>();
            Bubble.Color[] allColors = (Bubble.Color[])Enum.GetValues(typeof(Bubble.Color));
            foreach (Bubble.Color c in allColors)
            {
                data.Add(c, 0);
            }
            remainingColors = new List<Bubble.Color>();
            changed = false;
        }

        public void Reset()
        {
            foreach (Bubble.Color key in data.Keys.ToList())
            {
                data[key] = 0;
            }
            changed = true;
        }

        public void Tick(Bubble.Color color)
        {
            data[color] += 1;
            changed = true;
        }
        public void Untick(Bubble.Color color)
        {
            data[color] -= 1;
            changed = true;
        }

        public int Count(Bubble.Color color)
        {
            return data[color];
        }

        public int Total()
        {
            int total = 0;
            foreach (Bubble.Color key in data.Keys)
            {
                total += data[key];
            }
            return total;
        }
    }
}
