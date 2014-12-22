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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBobble
{
    class Bubble
    {
        public enum Color { Red, Orange, Yellow, Green, Blue, Purple, Gray, Black }
        private static Random bubbleRandomizer = new Random();
        public static BitmapImage bubbleSpritesheet;
        public const double SPRITE_EDGE = 28.0;
        private Color color;
        private Set<Bubble> chain;
        // Bubble neighbors
        private Bubble nw;
        private Bubble ne;
        private Bubble e;
        private Bubble se;
        private Bubble sw;
        private Bubble w;
        public bool Destroyed { get; set; }
        #region Bubble Sibling Properties
        public Bubble NW {
            get
            {
                return this.nw;
            }
            set
            {
                if (value != null && value.Destroyed == true)
                    return;
                this.nw = value;
                if (value != null && this.color == value.color)
                {
                    foreach (Bubble b in value.GetChain())
                    {
                        this.chain.Add(b);
                        b.chain.Add(this);
                    }
                    this.chain.Add(value);
                    value.chain.Add(this);
                }
            }
        }
        public Bubble NE
        {
            get
            {
                return this.ne;
            }
            set
            {
                if (value != null && value.Destroyed == true)
                    return;
                this.ne = value;
                if (value != null && this.color == value.color)
                {
                    foreach (Bubble b in value.GetChain())
                    {
                        this.chain.Add(b);
                        b.chain.Add(this);
                    }
                    this.chain.Add(value);
                    value.chain.Add(this);
                }
            }
        }
        public Bubble E
        {
            get
            {
                return this.e;
            }
            set
            {
                if (value != null && value.Destroyed == true)
                    return;
                this.e = value;
                if (value != null && this.color == value.color)
                {
                    foreach (Bubble b in value.GetChain())
                    {
                        this.chain.Add(b);
                        b.chain.Add(this);
                    }
                    this.chain.Add(value);
                    value.chain.Add(this);
                }
            }
        }
        public Bubble SE
        {
            get
            {
                return this.se;
            }
            set
            {
                if (value != null && value.Destroyed == true)
                    return;
                this.se = value;
                if (value != null && this.color == value.color)
                {
                    foreach (Bubble b in value.GetChain())
                    {
                        this.chain.Add(b);
                        b.chain.Add(this);
                    }
                    this.chain.Add(value);
                    value.chain.Add(this);
                }
            }
        }
        public Bubble SW
        {
            get
            {
                return this.sw;
            }
            set
            {
                if (value != null && value.Destroyed == true)
                    return;
                this.sw = value;
                if (value != null && this.color == value.color)
                {
                    foreach (Bubble b in value.GetChain())
                    {
                        this.chain.Add(b);
                        b.chain.Add(this);
                    }
                    this.chain.Add(value);
                    value.chain.Add(this);
                }
            }
        }
        public Bubble W
        {
            get
            {
                return this.w;
            }
            set
            {
                if (value != null && value.Destroyed == true)
                    return;
                this.w = value;
                if (value != null && this.color == value.color)
                {
                    foreach (Bubble b in value.GetChain())
                    {
                        this.chain.Add(b);
                        b.chain.Add(this);
                    }
                    this.chain.Add(value);
                    value.chain.Add(this);
                }
            }
        }
        #endregion
        public Bubble ToCeil { get; set; }

        private Image sprite;

        public Image Element
        {
            get
            {
                return sprite;
            }
        }

        //private Storyboard shakeAnim;

        /*public static Bubble()
        {
            if (Bubble.bubbleRandomizer == null)
                Bubble.bubbleRandomizer = new Random();
        }*/

        public Bubble() : this((Color[])Enum.GetValues(typeof(Color)))
        {
        }
        /*public Bubble(Color[] remainingColors)
        {
            this.color = (Color)remainingColors[bubbleRandomizer.Next(remainingColors.Length)];
            List<Bubble> ex = new List<Bubble>();
            ex.Add(this);
            this.chain = new Set<Bubble>(ex);
            this.Destroyed = false;

            Draw();
        }*/
        // Note: Array inplements the IList interface
        public Bubble(IList<Bubble.Color> remainingColors) : this((Color)remainingColors[bubbleRandomizer.Next(remainingColors.Count)])
        {
            /*this.color = ;
            List<Bubble> ex = new List<Bubble>();
            ex.Add(this);
            this.chain = new Set<Bubble>(ex);
            this.Destroyed = false;

            Draw();*/
        }
        public Bubble(Color color)
        {
            this.color = color;
            List<Bubble> ex = new List<Bubble>();
            ex.Add(this);
            this.chain = new Set<Bubble>(ex);
            this.Destroyed = false;

            Draw();
        }
        public Color GetColor()
        {
            return this.color;
        }
        public void EnChain(Bubble b)
        {
            chain.Add(b);
        }
        public void ClearChain(Bubble b)
        {
            chain.Clear();
        }
        public Set<Bubble> GetChain()
        {
            return this.chain;
        }
        public bool IsSameColorAs(Bubble that)
        {
            return (this.color == that.color);
        }

        public void Draw()
        {
            int offset = 0;
            switch (this.color)
            {
                case Color.Red: offset = 0; break;
                case Color.Orange: offset = 1; break;
                case Color.Yellow: offset = 2; break;
                case Color.Green: offset = 3; break;
                case Color.Blue: offset = 4; break;
                case Color.Purple: offset = 5; break;
                case Color.Gray: offset = 6; break;
                case Color.Black: offset = 7; break;
            }

            sprite = new Image();
            sprite.Width = SPRITE_EDGE;
            sprite.Height = SPRITE_EDGE;

            CroppedBitmap spritesheet = new CroppedBitmap(bubbleSpritesheet, new Int32Rect((int)(offset * SPRITE_EDGE), 0, (int)SPRITE_EDGE, (int)SPRITE_EDGE));

            sprite.Source = spritesheet;
        }
    }
}
