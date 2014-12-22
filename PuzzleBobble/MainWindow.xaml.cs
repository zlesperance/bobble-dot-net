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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PuzzleBobble
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double ROTATION_STEP = 5;
        private const double BUBBLE_STEP = 1;
        private const double FULL_TRAVEL_TIME = .8;
        private const int STAGE_BUBBLE_WIDTH = 8;
        private const int STAGE_BUBBLE_HEIGHT = 13;
        private const int BUBBLE_POP_POINTS = 10;
        private const int CEIL_TIMER_MAX = 11;
        private double wallWidth;
        private double ceilHeight;
        private double ceilStartHeight;
        private double maxRotation;
        private double minRotation;
        private double curRotation;
        private int overflowRow;
        private int ceilCounter;
        private int currentLevel;
        private long score;
        private GameState state;
        private PointerState pointerState;
        private Bubble bubbleReady;
        private Bubble bubbleNext;
        private Bubble[][] bubbleGrid;
        private Queue<Point> bubbleAnimationStops;
        private BubbleHitData bubbleAnimationHitData;
        private ColorCounter bubbleCounter;
        private Point readyPoint;
        private Point loadPoint;
        private Point pointerRotatePoint;
        private BitmapImage popSpritesheet;
        private ObjectAnimationUsingKeyFrames[] popAnimations;
        
        private enum GameState { Initialized, Ready, Active, RoundOver, Paused, Stopped, GameOver }
        private enum PointerState { Ready, Firing, Reloading }
        private struct BubbleHitData
        {
            public Bubble Target { get; set; }
            public int Row { get; set; }
            public int Col { get; set; }
        }
        private long Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value;
                labelScore.Content = score;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            SetupStage();

            maxRotation = 75;
            minRotation = 0 - maxRotation;

            /* Setup Pop Animations */
            popAnimations = new ObjectAnimationUsingKeyFrames[8];
            int step = 56;
            for (int i = 0, x = 0; i < 8; i++, x += step)
            {
                popAnimations[i] = new ObjectAnimationUsingKeyFrames();
                popAnimations[i].Duration = TimeSpan.FromMilliseconds(250);
                popAnimations[i].FillBehavior = FillBehavior.Stop;

                for (int j = 0; j < 6; j++)
                {
                    popAnimations[i].KeyFrames.Add(
                    new DiscreteObjectKeyFrame(
                        new CroppedBitmap(popSpritesheet, new Int32Rect(x, step * j, step, step)),
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(40 * j)))
                    );
                }
            }

            curRotation = 0;
            Point origin = gamePointer.RenderTransformOrigin;
            pointerRotatePoint = new Point(Canvas.GetLeft(gamePointer) + (gamePointer.Width * origin.X), Canvas.GetTop(gamePointer) + (gamePointer.Height * origin.Y));

            overflowRow = STAGE_BUBBLE_HEIGHT - 1;

            wallWidth = leftWall.Width;
            ceilStartHeight = ceiling.Height + Canvas.GetTop(ceiling);
            ceilHeight = ceilStartHeight;

            bubbleCounter = new ColorCounter();
            gridRandomizer = new Random();

            bubbleAnimationStops = new Queue<Point>();

            readyPoint = new Point(
                Canvas.GetLeft(gamePointer) + (gamePointer.Width / 2.0) - (Bubble.SPRITE_EDGE / 2.0),
                Canvas.GetTop(gamePointer) + 62 - (Bubble.SPRITE_EDGE / 2.0)
                );
            loadPoint = new Point(Canvas.GetLeft(nextBox), Canvas.GetTop(nextBox));
            
            state = GameState.Initialized;
            pointerState = PointerState.Ready;

            LoadLevel(0);
        }

        private void SetupStage()
        {
            // Pointer
            try
            {
                BitmapImage pointerBmp = new BitmapImage(new Uri(@"/Images/pointer.png", UriKind.Relative));

                gamePointer.Source = pointerBmp;
                gamePointer.RenderTransform = new RotateTransform(0);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to Load Resource");
            }

            BitmapImage bubbleSpritesheet = new BitmapImage();
            bubbleSpritesheet.BeginInit();
            bubbleSpritesheet.UriSource = new Uri(@"/Images/bubbles.png", UriKind.Relative);
            bubbleSpritesheet.BaseUri = BaseUriHelper.GetBaseUri(this);
            bubbleSpritesheet.CacheOption = BitmapCacheOption.OnLoad;
            bubbleSpritesheet.EndInit();

            Bubble.bubbleSpritesheet = bubbleSpritesheet;

            popSpritesheet = new BitmapImage();
            popSpritesheet.BeginInit();
            popSpritesheet.UriSource = new Uri(@"/Images/bubblePop.png", UriKind.Relative);
            popSpritesheet.BaseUri = BaseUriHelper.GetBaseUri(this);
            popSpritesheet.CacheOption = BitmapCacheOption.OnLoad;
            popSpritesheet.EndInit();

            canvasPlayArea.Cursor = Cursors.Cross;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(state == GameState.Active || state == GameState.Ready))
                return;
            if (e.Key == Key.Right)
            {
                //gamePointer.Tra
                curRotation = Math.Min(maxRotation, curRotation + ROTATION_STEP);
                gamePointer.RenderTransform = new RotateTransform(curRotation);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                curRotation = Math.Max(minRotation, curRotation - ROTATION_STEP);
                gamePointer.RenderTransform = new RotateTransform(curRotation);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                curRotation = 0;
                gamePointer.RenderTransform = new RotateTransform(curRotation);
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                if (state == GameState.Active)
                    FireBubble();
            }
            else if (e.Key == Key.D1)
            {
                // TODO: Move this and following to Cheat Code system
                LoadLevel(0);
            }
            else if (e.Key == Key.D2)
            {
                LoadLevel(1);
            }
            else if (e.Key == Key.D3)
            {
                LoadLevel(2);
            }
            else if (e.Key == Key.D4)
            {
                LoadLevel(3);
            }
            else if (e.Key == Key.D5)
            {
                LoadLevel(4);
            }
            else if (e.Key == Key.D6)
            {
                LoadLevel(5);
            }
            else if (e.Key == Key.D7)
            {
                LoadLevel(6);
            }
            else if (e.Key == Key.D8)
            {
                LoadLevel(7);
            }
            else if (e.Key == Key.D9)
            {
                LoadLevel(8);
            }
            else if (e.Key == Key.D0)
            {
                LoadLevel(9);
            }
            else if (e.Key == Key.OemTilde)
            {
                LoadLevel(-1);
            }
        }

        #region Test Functions
        private void bubbleTester()
        {
            Bubble.Color[] bubbleColors = (Bubble.Color[]) Enum.GetValues(typeof(Bubble.Color));
            int topSet = 0;
            foreach (Bubble.Color c in bubbleColors)
            {
                Bubble b = new Bubble(c);
                Canvas.SetTop(b.Element, topSet);
                canvasPlayArea.Children.Add(b.Element);
                topSet += 38;
            }
        }
        #endregion

        private void menuItemNewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void menuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void canvasPlayArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(state == GameState.Active || state == GameState.Ready))
                return;
            //labelCoords.Text = e.GetPosition(canvasPlayArea).X + "\n" + e.GetPosition(canvasPlayArea).Y;
            Point coords = e.GetPosition(canvasPlayArea);
            double deltaX = pointerRotatePoint.Y - coords.Y;
            double deltaY = coords.X - pointerRotatePoint.X;
            double hypotenuse = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
            double rotation = Math.Asin(deltaY / hypotenuse) * (180 / Math.PI);
            /*if (coords.X < pointerRotatePoint.X)
                rotation = -rotation;*/
            curRotation = Math.Max(minRotation, Math.Min(maxRotation, rotation));
            gamePointer.RenderTransform = new RotateTransform(curRotation);
            e.Handled = true;
            //labelRotation.Content = curRotation;
        }

        private void canvasPlayArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (state == GameState.Active && e.ChangedButton == MouseButton.Left)
                FireBubble();
        }

        private void menuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Owner = this;
            about.Show();
        }

        private void menuItemRules_Click(object sender, RoutedEventArgs e)
        {
            RulesWindow rules = new RulesWindow();
            rules.Owner = this;
            rules.Show();
        }
    }
}
