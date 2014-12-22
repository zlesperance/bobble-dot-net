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
    partial class MainWindow
    {
        #region Bubble Interactions
        private void FireBubble()
        {
            if (pointerState == PointerState.Ready)
            {                
                // Calculate Movement
                double angle = curRotation;
                double reverseAngle = (angle < 0) ? (90 + angle) : (angle - 90);
                double deltaX = BUBBLE_STEP * Math.Sin(angle * (Math.PI / 180));
                double deltaY = -(BUBBLE_STEP * Math.Sin((90 - Math.Abs(angle)) * (Math.PI / 180)));

                // Find lowest possible bubble
                double lowest = 0;
                int startRow = bubbleGrid.Length - 1;
                for (int row = startRow; row > -1 && lowest == 0; row--)
                {
                    if (bubbleGrid[row] != null && bubbleGrid[row].Length > 0)
                    {
                        for (int col = 0; col < bubbleGrid[row].Length && lowest == 0; col++)
                        {
                            if (bubbleGrid[row][col] != null && bubbleGrid[row][col].Destroyed == false)
                            {
                                lowest = Canvas.GetTop(bubbleGrid[row][col].Element) + bubbleGrid[row][col].Element.Height;
                                startRow = row;
                            }
                        }
                    }
                }

                bubbleAnimationStops.Clear();
                bubbleAnimationHitData = new BubbleHitData();
                bubbleAnimationHitData.Target = null;
                bubbleAnimationHitData.Row = -1;
                bubbleAnimationHitData.Col = -1;
                double bubY = Canvas.GetTop(bubbleReady.Element);
                double bubX = Canvas.GetLeft(bubbleReady.Element);
                double bubW = bubbleReady.Element.Width;
                double bubH = bubbleReady.Element.Height;
                bubbleAnimationStops.Enqueue(new Point(bubX, bubY));
                do
                {
                    if (WallCheck(bubX, bubW) == true)
                    {
                        bubbleAnimationStops.Enqueue(new Point(bubX, bubY));
                        deltaX = -deltaX;
                        double temp = angle;
                        angle = reverseAngle;
                        reverseAngle = temp;
                    }
                    bubY += deltaY;
                    bubX += deltaX;
                } while (HitCheck(bubX, bubY, bubW, bubH, lowest, startRow, angle) == false);

                bubbleAnimationStops.Enqueue(new Point(bubX, bubY));
                Point final = DetermineBubbleSpot(bubX, bubY, deltaY / deltaX);

                Storyboard bubbleStoryboard = new Storyboard();
                
                double fromY = bubbleAnimationStops.Peek().Y;
                TimeSpan vertDur = TimeSpan.FromSeconds(((fromY - bubY) / canvasPlayArea.Height) * FULL_TRAVEL_TIME);
                DoubleAnimation vertAnim = new DoubleAnimation(fromY, bubY, vertDur);
                Storyboard.SetTargetProperty(vertAnim, new PropertyPath(Canvas.TopProperty));
                bubbleStoryboard.Children.Add(vertAnim);

                TimeSpan startTime = TimeSpan.FromSeconds(0);

                while (bubbleAnimationStops.Count > 1)
                {
                    fromY = bubbleAnimationStops.Peek().Y;
                    DoubleAnimation horizAnim = new DoubleAnimation();
                    horizAnim.From = bubbleAnimationStops.Dequeue().X;
                    double stepY = bubbleAnimationStops.Peek().Y;
                    horizAnim.To = bubbleAnimationStops.Peek().X;
                    horizAnim.BeginTime = startTime;
                    TimeSpan thisDur = TimeSpan.FromSeconds(((fromY - stepY) / canvasPlayArea.Height) * FULL_TRAVEL_TIME);
                    horizAnim.Duration = thisDur;
                    startTime += thisDur;

                    Storyboard.SetTargetProperty(horizAnim, new PropertyPath(Canvas.LeftProperty));
                    bubbleStoryboard.Children.Add(horizAnim);
                }

                double finalDist = (Math.Sqrt(Math.Pow(final.X - bubX, 2) + Math.Pow(final.Y - bubY, 2)) / canvasPlayArea.Height) * FULL_TRAVEL_TIME;

                DoubleAnimation finalYAnim = new DoubleAnimation(bubY, final.Y, TimeSpan.FromSeconds(finalDist));
                finalYAnim.BeginTime = vertDur;
                Storyboard.SetTargetProperty(finalYAnim, new PropertyPath(Canvas.TopProperty));
                bubbleStoryboard.Children.Add(finalYAnim);

                DoubleAnimation finalXAnim = new DoubleAnimation(bubX, final.X, TimeSpan.FromSeconds(finalDist));
                finalXAnim.BeginTime = startTime;
                Storyboard.SetTargetProperty(finalXAnim, new PropertyPath(Canvas.LeftProperty));
                bubbleStoryboard.Children.Add(finalXAnim);
                bubbleStoryboard.FillBehavior = FillBehavior.Stop;

                bubbleStoryboard.Completed += (s, e) =>
                {
                    Canvas.SetTop(bubbleReady.Element, final.Y);
                    Canvas.SetLeft(bubbleReady.Element, final.X);
                    TryPopBubble(bubbleReady);
                    EndOfFireCheck();
                };

                pointerState = PointerState.Firing;

                bubbleStoryboard.Begin(bubbleReady.Element);
            }

        }
        private Point DetermineBubbleSpot(double bubX, double bubY, double m)
        {
            Bubble hit = bubbleAnimationHitData.Target;
            int hitRow = bubbleAnimationHitData.Row;
            int hitCol = bubbleAnimationHitData.Col;
            if (hit == null) // Ceiling Hit
            {
                int i;
                double offset;
                for (i = 0, offset = wallWidth; i < (STAGE_BUBBLE_WIDTH - 1) && (offset + (Bubble.SPRITE_EDGE / 2.0)) < bubX; i++, offset += Bubble.SPRITE_EDGE) ;
                AssimilateBubble(bubbleReady, 0, i);
                return new Point(offset, ceilHeight);
            }
            else
            {
                double r = hit.Element.Width / 2.0;
                double hitX = Canvas.GetLeft(hit.Element);
                double hitY = Canvas.GetTop(hit.Element);
                int bubRow;
                int bubCol;
                Point? intersect = Intersection(bubX + r, bubY + r, hitX + r, hitY + r, r, m);
                Point final;
                if (intersect == null)
                {
                    if (bubX < hitX)
                    {
                        // Approaching from West
                        if (m > 0 || (hit.W != null && hit.W.Destroyed == false) || hitCol == 0)
                        {
                            // South West
                            if (bubbleGrid[hitRow + 1] == null)
                            {
                                bubbleGrid[hitRow + 1] = new Bubble[((hitRow % 2 == 0) ? (bubbleGrid[hitRow].Length - 1) : (bubbleGrid[hitRow].Length + 1))];
                            }
                            bubRow = hitRow + 1;
                            if (hit.SW == null || hit.SW.Destroyed == true)
                            {
                                // South West
                                if (hitRow % 2 == 0)
                                {
                                    bubCol = Math.Max(0, hitCol - 1);
                                }
                                else
                                {
                                    bubCol = hitCol;
                                }
                                if (hitRow % 2 == 0 && hitCol == 0)
                                    final = new Point(hitX + r, hitY + hit.Element.Height);
                                else
                                    final = new Point(hitX - r, hitY + hit.Element.Height);

                            }
                            else if (hit.SE == null || hit.SE.Destroyed == true)
                            {
                                // There was already a South-West, put it in South-East
                                if (hitRow % 2 == 0)
                                {
                                    bubCol = Math.Min(bubbleGrid[hitRow].Length - 1, hitCol);
                                }
                                else
                                {
                                    bubCol = hitCol + 1;
                                }
                                final = new Point(hitX + r, hitY + hit.Element.Height);
                            }
                            else
                            {
                                // There is nowhere to put this bubble
                                // TODO: Work in logic to deal with this
                                throw new Exception("Invalid Landing Location");
                            }
                        }
                        else
                        {
                            // West
                            bubRow = hitRow;
                            bubCol = hitCol - 1;
                            final = new Point(hitX - hit.Element.Width, hitY);
                        }
                    }
                    else
                    {
                        // Approaching from East
                        if (m < 0 || (hit.E != null && hit.E.Destroyed == false) || hitCol == (bubbleGrid[hitRow].Length - 1))
                        {
                            // South East
                            if (bubbleGrid[hitRow + 1] == null)
                            {
                                bubbleGrid[hitRow + 1] = new Bubble[((hitRow % 2 == 0) ? (bubbleGrid[hitRow].Length - 1) : (bubbleGrid[hitRow].Length + 1))];
                            }
                            bubRow = hitRow + 1;
                            if (hit.SE == null || hit.SE.Destroyed == true)
                            {
                                if (hitRow % 2 == 0)
                                {
                                    bubCol = Math.Min(bubbleGrid[bubRow].Length - 1, hitCol);
                                }
                                else
                                {
                                    bubCol = hitCol + 1;
                                }
                                if (hitRow % 2 == 0 && hitCol == bubbleGrid[hitRow].Length - 1)
                                    final = new Point(hitX - r, hitY + hit.Element.Height);
                                else
                                    final = new Point(hitX + r, hitY + hit.Element.Height);
                            }
                            else if (hit.SW == null || hit.SW.Destroyed == true)
                            {
                                // There was already a South-East, put it in South-West
                                if (hitRow % 2 == 0)
                                {
                                    bubCol = Math.Max(0, hitCol - 1);
                                }
                                else
                                {
                                    bubCol = hitCol;
                                }
                                final = new Point(hitX - r, hitY + hit.Element.Height);
                            }
                            else
                            {
                                // There is nowhere to put this bubble
                                // TODO: Work in logic to deal with this
                                throw new Exception("Invalid Landing Location");
                            }
                        }
                        else
                        {
                            // East
                            bubRow = hitRow;
                            bubCol = hitCol + 1;
                            final = new Point(hitX + hit.Element.Width, hitY);
                        }
                    }
                }
                else
                {
                    double fence = hitY + r + (r / 5.0);
                    Point i = (Point)intersect;
                    if (i.X < (hitX + r))
                    {
                        // West
                        if (i.Y > fence || (hit.W != null && hit.W.Destroyed == false) || hitCol == 0)
                        {
                            if (bubbleGrid[hitRow + 1] == null)
                            {
                                bubbleGrid[hitRow + 1] = new Bubble[((hitRow % 2 == 0) ? (bubbleGrid[hitRow].Length - 1) : (bubbleGrid[hitRow].Length + 1))];
                            }
                            bubRow = hitRow + 1;
                            if (hit.SW == null || hit.SW.Destroyed == true)
                            {
                                // South West
                                if (hitRow % 2 == 0)
                                {
                                    bubCol = Math.Max(0, hitCol - 1);
                                }
                                else
                                {
                                    bubCol = hitCol;
                                }
                                if (hitRow % 2 == 0 && hitCol == 0)
                                    final = new Point(hitX + r, hitY + hit.Element.Height);
                                else
                                    final = new Point(hitX - r, hitY + hit.Element.Height);
                            }
                            else if (hit.SE == null || hit.SE.Destroyed == true)
                            {
                                // There was already a South-West, put it in South-East
                                if (hitRow % 2 == 0)
                                {
                                    bubCol = Math.Min(bubbleGrid[hitRow].Length - 1, hitCol);
                                }
                                else
                                {
                                    bubCol = hitCol + 1;
                                }
                                final = new Point(hitX + r, hitY + hit.Element.Height);
                            }
                            else
                            {
                                // There is nowhere to put this bubble
                                // TODO: Work in logic to deal with this
                                throw new Exception("Invalid Landing Location");
                            }
                        }
                        else
                        {
                            // West
                            bubRow = hitRow;
                            bubCol = hitCol - 1;
                            final = new Point(hitX - hit.Element.Width, hitY);
                        }
                    }
                    else
                    {
                        // East
                        if (i.Y > fence || (hit.E != null && hit.E.Destroyed == false) || hitCol == (bubbleGrid[hitRow].Length - 1))
                        {
                            // South-East
                            if (bubbleGrid[hitRow + 1] == null)
                            {
                                bubbleGrid[hitRow + 1] = new Bubble[((hitRow % 2 == 0) ? (bubbleGrid[hitRow].Length - 1) : (bubbleGrid[hitRow].Length + 1))];
                            }
                            bubRow = hitRow + 1;
                            if (hit.SE == null || hit.SE.Destroyed == true)
                            {
                                if (hitRow % 2 == 0)
                                {
                                    bubCol = Math.Min(bubbleGrid[bubRow].Length - 1, hitCol);
                                }
                                else
                                {
                                    bubCol = hitCol + 1;
                                }
                                if (hitRow % 2 == 0 && hitCol == bubbleGrid[hitRow].Length - 1)
                                    final = new Point(hitX - r, hitY + hit.Element.Height);
                                else
                                    final = new Point(hitX + r, hitY + hit.Element.Height);
                            }
                            else if (hit.SW == null || hit.SW.Destroyed == true)
                            {
                                // There was already a South-East, put it in South-West
                                if (hitRow % 2 == 0)
                                {
                                    bubCol = Math.Max(0, hitCol - 1);
                                }
                                else
                                {
                                    bubCol = hitCol;
                                }
                                final = new Point(hitX - r, hitY + hit.Element.Height);
                            }
                            else
                            {
                                // There is nowhere to put this bubble
                                // TODO: Work in logic to deal with this
                                throw new Exception("Invalid Landing Location");
                            }
                        }
                        else
                        {
                            // East
                            bubRow = hitRow;
                            bubCol = hitCol + 1;
                            final = new Point(hitX + hit.Element.Width, hitY);
                        }

                    }
                }
                //bubbleGrid[bubRow][bubCol] = bubbleReady;
                AssimilateBubble(bubbleReady, bubRow, bubCol);
                return final;
            }
        }

        private Point? Intersection(double x0, double y0, double x1, double y1, double r, double m)
        {
            if (double.IsInfinity(m))
            {

                return new Point(x0, y1 + r);
            }
            Point? p = null;
            double d = (-(m * x0)) + y0 - y1;
            double c = (x1 * x1) + (d * d) - (r * r);
            double b = 2 * ((m * d) - x1);
            double a = (m * m) + 1;
            double discriminant = Math.Sqrt((b * b) - (4 * a * c));
            
            double t1 = (-b - discriminant)/(2 * a);
            double t2 = (-b + discriminant)/(2 * a);

            if (!(double.IsNaN(t1) && double.IsNaN(t2)))
            {
                if (double.IsNaN(t1))
                {
                    // Use t2
                    double y = (m * (t2 - x0)) + y0;
                    p = new Point(t2, y);
                }
                else if (double.IsNaN(t2))
                {
                    // Use t1
                    double y = (m * (t1 - x0)) + y0;
                    p = new Point(t1, y);
                }
                else
                {
                    // Use whichever's closest to (x0, y0)
                    double yA = (m * (t1 - x0)) + y0;
                    double yB = (m * (t2 - x0)) + y0;
                    double dA = Math.Sqrt(Math.Pow(t1 - x0, 2) + Math.Pow(yA - y0, 2));
                    double dB = Math.Sqrt(Math.Pow(t2 - x0, 2) + Math.Pow(yB - y0, 2));
                    if (dA < dB)
                    {
                        // Use t1
                        p = new Point(t1, yA);
                    }
                    else
                    {
                        p = new Point(t2, yB);
                    }
                }
            }

            /*if ( t1 >= 0 && t1 <= 1)
            {
                // use x1\
                double y = (m * (t1 - x0)) + y0;
                p = new Point(t1, y);
                
            }*/


             // Else, Assume no hit, so go either north-east or north-west based on angle, figured out outside of this method

            return p;
        }

        //delegate 
        private void LoadBubble()
        {
            if (bubbleCounter.Total() > 0)
            {
                bubbleReady = bubbleNext;
                bubbleNext = new Bubble(bubbleCounter.Remaining);
                bubbleNext.Draw();
                canvasPlayArea.Children.Add(bubbleNext.Element);
                Canvas.SetLeft(bubbleReady.Element, readyPoint.X);
                Canvas.SetTop(bubbleReady.Element, readyPoint.Y);
                Canvas.SetLeft(bubbleNext.Element, loadPoint.X);
                Canvas.SetTop(bubbleNext.Element, loadPoint.Y);
            }
            else
            {
                canvasPlayArea.Children.Remove(bubbleNext.Element);
                bubbleNext = null;
            }
            pointerState = PointerState.Ready;
        }

        private void AssimilateBubble(Bubble b, int row, int col)
        {
            bubbleGrid[row][col] = b;
            if (row > 0)
            {
                if (row % 2 == 0)
                {
                    if (col > 0 && bubbleGrid[row - 1][col - 1] != null && bubbleGrid[row - 1][col - 1].Destroyed == false)
                        b.NW = bubbleGrid[row - 1][col - 1];
                    if (col < (bubbleGrid[row].Length - 1) && bubbleGrid[row - 1][col] != null && bubbleGrid[row - 1][col].Destroyed == false)
                        b.NE = bubbleGrid[row - 1][col];
                }
                else
                {
                    if (bubbleGrid[row - 1][col] != null && bubbleGrid[row - 1][col].Destroyed == false)
                        b.NW = bubbleGrid[row - 1][col];
                    if (bubbleGrid[row - 1][col + 1] != null && bubbleGrid[row - 1][col + 1].Destroyed == false)
                        b.NE = bubbleGrid[row - 1][col + 1];
                }
            }
            if (row < STAGE_BUBBLE_HEIGHT - 1 && bubbleGrid[row + 1] != null)
            {
                if (row % 2 == 0)
                {
                    if (col > 0 && bubbleGrid[row + 1][col - 1] != null && bubbleGrid[row + 1][col - 1].Destroyed == false)
                        b.SW = bubbleGrid[row + 1][col - 1];
                    if (col < (bubbleGrid[row].Length - 1) && bubbleGrid[row + 1][col] != null && bubbleGrid[row + 1][col].Destroyed == false)
                        b.SE = bubbleGrid[row + 1][col];
                }
                else
                {
                    if (bubbleGrid[row + 1][col] != null && bubbleGrid[row + 1][col].Destroyed == false)
                        b.SW = bubbleGrid[row + 1][col];
                    if (bubbleGrid[row + 1][col + 1] != null && bubbleGrid[row + 1][col + 1].Destroyed == false)
                        b.SE = bubbleGrid[row + 1][col + 1];
                }
            }
            if (col > 0 && bubbleGrid[row][col - 1] != null && bubbleGrid[row][col - 1].Destroyed == false)
                b.W = bubbleGrid[row][col - 1];
            if (col < (bubbleGrid[row].Length - 1) && bubbleGrid[row][col + 1] != null && bubbleGrid[row][col + 1].Destroyed == false)
                b.E = bubbleGrid[row][col + 1];

            if (b.NW != null)
                b.NW.SE = b;

            if (b.NE != null)
                b.NE.SW = b;

            if (b.W != null)
                b.W.E = b;

            if (b.SE != null)
                b.SE.NW = b;

            if (b.SW != null)
                b.SW.NE = b;

            if (b.E != null)
                b.E.W = b;

            if (b.NW != null)
                b.ToCeil = b.NW;
            else if (b.NE != null)
                b.ToCeil = b.NE;
            else if (b.W != null)
                b.ToCeil = b.W;
            else
                b.ToCeil = b.E;

            bubbleCounter.Tick(b.GetColor());
        }

        private void SeparateBubble(Bubble b)
        {
            if (b.NW != null)
                b.NW.SE = null;
            if (b.NE != null)
                b.NE.SW = null;
            if (b.E != null)
                b.E.W = null;
            if (b.SE != null)
                b.SE.NW = null;
            if (b.SW != null)
                b.SW.NE = null;
            if (b.W != null)
                b.W.E = null;

            b.NW = null;
            b.NE = null;
            b.E = null;
            b.SE = null;
            b.SW = null;
            b.W = null;

            b.Destroyed = true;

            bubbleCounter.Untick(b.GetColor());
            //canvasPlayArea.Children.Remove(b.Element);
        }

        private void TryPopBubble(Bubble bubble)
        {
            List<Bubble> toPop = bubble.GetChain().ToList();
            toPop.Add(bubble);
            List<Bubble> ceilBubs = new List<Bubble>();
            foreach (Bubble cB in bubbleGrid[0])
            {
                if (cB != null && cB.Destroyed == false)
                    ceilBubs.Add(cB);
            }
            Set<Bubble> disturbed = new Set<Bubble>(toPop, ceilBubs);
            if (toPop.Count > 2) // Including self, that would mean we have 3 or more in chain
            {
                foreach (Bubble b in toPop)
                {
                    if (b.E != null && b.E.Destroyed == false)
                        disturbed.Add(b.E);
                    if (b.SE != null && b.SE.Destroyed == false)
                        disturbed.Add(b.SE);
                    if (b.SW != null && b.SW.Destroyed == false)
                        disturbed.Add(b.SW);
                    if (b.W != null && b.W.Destroyed == false)
                        disturbed.Add(b.W);
                    //score += POP_SCORE
                    Score += BUBBLE_POP_POINTS;
                    SeparateBubble(b);
                    AnimatePop(b);
                    //canvasPlayArea.Children.Remove(b.Element);
                }
                TryDropBubbles(disturbed);
            }
            
        }
        private void TryDropBubbles(Set<Bubble> bubbles)
        {
            int points = 0;
            while (bubbles.Count > 0)
            {
                if (bubbles[0].ToCeil == null || bubbles[0].ToCeil.Destroyed == true)
                {
                    Bubble thisBub = bubbles[0];
                    if (thisBub.NW != null && thisBub.NW.Destroyed == false)
                    {
                        thisBub.ToCeil = thisBub.NW;
                        if (thisBub.NW.ToCeil == null || thisBub.NW.ToCeil.Destroyed == true)
                            bubbles.Add(thisBub.NW);
                    }
                    else if (thisBub.NE != null && thisBub.NE.Destroyed == false)
                    {
                        thisBub.ToCeil = thisBub.NE;
                        if (thisBub.NE.ToCeil == null || thisBub.NE.ToCeil.Destroyed == true)
                            bubbles.Add(thisBub.NE);
                    }
                    else if (thisBub.W != null && thisBub.W.Destroyed == false && thisBub.W.ToCeil != thisBub)
                    {
                        thisBub.ToCeil = thisBub.W;
                        if (thisBub.W.ToCeil == null || thisBub.W.ToCeil.Destroyed == true || thisBub.W.ToCeil == thisBub.W.W)
                        {
                            bubbles.Add(thisBub.W);
                        }
                    }
                    else if (thisBub.E != null && thisBub.E.Destroyed == false)
                    {
                        thisBub.ToCeil = thisBub.E;
                        thisBub.E.ToCeil = null;
                        bubbles.Add(thisBub.E); // Prevents Horizontals from being marked as stable
                    }
                    else
                    {
                        // Drop and add disturbed
                        if (thisBub.SW != null && thisBub.SW.Destroyed == false)
                            bubbles.Add(thisBub.SW);
                        if (thisBub.SE != null && thisBub.SE.Destroyed == false)
                            bubbles.Add(thisBub.SE);
                        if (thisBub.E != null && thisBub.E.Destroyed == false)
                            bubbles.Add(thisBub.E);
                        if (thisBub.W != null && thisBub.W.Destroyed == false)
                            bubbles.Add(thisBub.W);

                        SeparateBubble(thisBub);
                        AnimateDrop(thisBub);
                        if (points == 0)
                            points = BUBBLE_POP_POINTS * 2;
                        else
                            points = Math.Min(points * 2, 1310720);
                    }
                }
                bubbles.Remove(bubbles[0]);
            }
            // Do something with points
            Score += points;
        }

        private void AnimatePop(Bubble b)
        {
            /* Setup Pop Animation */
            Storyboard thisAnim = new Storyboard();
            ObjectAnimationUsingKeyFrames popAnimation;

            switch(b.GetColor())
            {
                case Bubble.Color.Red:      popAnimation = popAnimations[0]; break;
                case Bubble.Color.Orange:   popAnimation = popAnimations[1]; break;
                case Bubble.Color.Yellow:   popAnimation = popAnimations[2]; break;
                case Bubble.Color.Green:    popAnimation = popAnimations[3]; break;
                case Bubble.Color.Blue:     popAnimation = popAnimations[4]; break;
                case Bubble.Color.Purple:   popAnimation = popAnimations[5]; break;
                case Bubble.Color.Gray:     popAnimation = popAnimations[6]; break;
                default:                    popAnimation = popAnimations[7]; break;
            }

            Storyboard.SetTargetProperty(popAnimation, new PropertyPath(Image.SourceProperty));
            thisAnim.Children.Add(popAnimation);

            Image popImg = new Image();
            popImg.Width = 56;
            popImg.Height = 56;
            Canvas.SetTop(popImg, Canvas.GetTop(b.Element) - (Bubble.SPRITE_EDGE / 2.0));
            Canvas.SetLeft(popImg, Canvas.GetLeft(b.Element) - (Bubble.SPRITE_EDGE / 2.0));

            canvasPlayArea.Children.Add(popImg);
            canvasPlayArea.Children.Remove(b.Element);

            /*
            CroppedBitmap start = new CroppedBitmap(popSpritesheet, new Int32Rect(x, 0, step, step));
            popImg.Source = start;
            canvasPlayArea.Children.Add(popImg);
            canvasPlayArea.Children.Remove(b.Element);

            popAnimation.KeyFrames.Add(
                new DiscreteObjectKeyFrame(
                    new CroppedBitmap(popSpritesheet, new Int32Rect(x, step, step, step)),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(40)))
                );
            popAnimation.KeyFrames.Add(
                new DiscreteObjectKeyFrame(
                    new CroppedBitmap(popSpritesheet, new Int32Rect(x, step * 2, step, step)),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80)))
                );
            popAnimation.KeyFrames.Add(
                new DiscreteObjectKeyFrame(
                    new CroppedBitmap(popSpritesheet, new Int32Rect(x, step * 3, step, step)),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120)))
                );
            popAnimation.KeyFrames.Add(
                new DiscreteObjectKeyFrame(
                    new CroppedBitmap(popSpritesheet, new Int32Rect(x, step * 4, step, step)),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(160)))
                );
            popAnimation.KeyFrames.Add(
                new DiscreteObjectKeyFrame(
                    new CroppedBitmap(popSpritesheet, new Int32Rect(x, step * 5, step, step)),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)))
                );
            */
            thisAnim.Completed += (s, e) =>
            {
                canvasPlayArea.Children.Remove(popImg);
            };
            thisAnim.Begin(popImg);
        }

        private void AnimateDrop(Bubble b)
        {
            double curY = Canvas.GetTop(b.Element);
            DoubleAnimation dropAnim = new DoubleAnimation(curY, canvasPlayArea.Height, TimeSpan.FromSeconds(((canvasPlayArea.Height - curY) / canvasPlayArea.Height) * FULL_TRAVEL_TIME));
            CubicEase ease = new CubicEase();
            ease.EasingMode = EasingMode.EaseIn;
            dropAnim.EasingFunction = ease;
            dropAnim.Completed += (object sender, EventArgs e) =>
            {
                canvasPlayArea.Children.Remove(b.Element);
            };
            b.Element.BeginAnimation(Canvas.TopProperty, dropAnim);
        }

        private bool CheckForPass(double centerX, double centerY, Bubble target, double angle)
        {
            if (angle == 0)
                return false;
            double m;
            if (angle < 0)
                m = 90 + angle;
            else
                m = angle - 90;
            double r = Bubble.SPRITE_EDGE / 2.0;
            double targetCenterX = Canvas.GetLeft(target.Element) + r;
            double targetCenterY = Canvas.GetTop(target.Element) + r;
            double mT = (targetCenterY - centerY) / (targetCenterX - centerX);
            //double m = - (1 / slope);
            double deltaY = Math.Abs(r * Math.Sin(m * (Math.PI / 180)));
            double deltaX = Math.Sqrt((r * r) - (deltaY * deltaY));
            Point movingEdge = new Point();
            Point targetEdge = new Point();
            double motionSlope = Math.Tan(angle * (Math.PI / 180));

            double checkY;
            if (centerY < targetCenterY)
            {
                movingEdge.Y = centerY + deltaY;
                targetEdge.Y = targetCenterY - deltaY;
                if (angle < 0)
                {
                    movingEdge.X = centerX - deltaX;
                    targetEdge.X = targetCenterX + deltaX;
                }
                else
                {
                    movingEdge.X = centerX + deltaX;
                    targetEdge.X = targetCenterX - deltaX;
                }
                checkY = (-motionSlope * (targetEdge.X - movingEdge.X)) + movingEdge.Y;
                return (checkY < targetEdge.Y);
            }
            else if (angle < 0)
            {
                if (mT < 0 || motionSlope > mT)
                {
                    movingEdge.X = centerX + deltaX;
                    targetEdge.X = targetCenterX - deltaX;
                    movingEdge.Y = centerY - deltaY;
                    targetEdge.Y = targetCenterY + deltaY;
                }
                else
                {
                    /*movingEdge.X = centerX - deltaX;
                    targetEdge.X = targetCenterX + deltaX;
                    movingEdge.Y = centerY + deltaY;
                    targetEdge.Y = targetCenterY - deltaY;*/
                    return false;
                }
            }
            else
            {
                if (mT > 0 || motionSlope > mT)
                {
                    movingEdge.X = centerX - deltaX;
                    targetEdge.X = targetCenterX + deltaX;
                    movingEdge.Y = centerY - deltaY;
                    targetEdge.Y = targetCenterY + deltaY;
                }
                else
                {
                    /*movingEdge.X = centerX + deltaX;
                    targetEdge.X = targetCenterX - deltaX;
                    movingEdge.Y = centerY + deltaY;
                    targetEdge.Y = targetCenterY - deltaY;*/
                    return false;
                }
            }
            checkY = (-motionSlope * (targetEdge.X - movingEdge.X)) + movingEdge.Y;
            return (checkY > targetEdge.Y);

            /*
            if (centerY < Canvas.GetTop(target.Element) + r)
            {
                movingEdge.Y = centerY - deltaY;
                targetEdge.Y = Canvas.GetTop(target.Element) + r + deltaY;
                if (m < 0)
                {
                    movingEdge.X = centerX + deltaX;
                    targetEdge.X = Canvas.GetLeft(target.Element) + r - deltaX;
                }
                else
                {
                    movingEdge.X = centerX - deltaX;
                    targetEdge.X = Canvas.GetLeft(target.Element) + r + deltaX;
                }
                double checkY = (-(deltaX / deltaY)) * (targetEdge.X - movingEdge.X) + movingEdge.Y;

                return (checkY < targetEdge.Y);
            }
            else
            {
                movingEdge.Y = centerY + deltaY;
                targetEdge.Y = Canvas.GetTop(target.Element) + r - deltaY;
                if (m < 0)
                {
                    movingEdge.X = centerX - deltaX;
                    targetEdge.X = Canvas.GetLeft(target.Element) + r + deltaX;
                }
                else
                {
                    movingEdge.X = centerX + deltaX;
                    targetEdge.X = Canvas.GetLeft(target.Element) + r - deltaX;
                }
                double checkY = (-(deltaX / deltaY)) * (targetEdge.X - movingEdge.X) + movingEdge.Y;

                return (checkY > targetEdge.Y);
            }*/
        }

        private bool WallCheck(double x, double w)
        {
            if (x <= wallWidth || (x + w) >= (canvasPlayArea.Width - wallWidth))
            {
                return true;
            }
            return false;
        }

        private bool HitCheck(double x, double y, double w, double h, double lowest, int startRow, double angle)
        {
            // TODO: Fix so that bubbles can pass by on the corner without being pulled
            bubbleAnimationHitData.Target = null;
            double top = y;
            double left = x;
            double right = left + w;
            double bottom = top + h;
            if (top <= lowest)
            {
                for (int row = startRow; row > -1; row--)
                {
                    bool rowDone = false;
                    for (int col = 0; col < bubbleGrid[row].Length && rowDone == false; col++)
                    {
                        Bubble thisBub = bubbleGrid[row][col];
                        if (thisBub != null && thisBub.Destroyed == false)
                        {
                            // thisBub bottom needs to be greater than top
                            if (Canvas.GetTop(thisBub.Element) > bottom || (Canvas.GetTop(thisBub.Element) + thisBub.Element.Height) < top)
                                rowDone = true;
                                //break; // Done checking this row
                            else if (Canvas.GetLeft(thisBub.Element) + thisBub.Element.Width > left && Canvas.GetLeft(thisBub.Element) < right && CheckForPass(left + (w / 2.0), top + (w / 2.0), thisBub, angle) == false)
                            {
                                bubbleAnimationHitData.Target = thisBub;
                                bubbleAnimationHitData.Row = row;
                                bubbleAnimationHitData.Col = col;
                                return true;
                            }
                        }
                    }
                }
                if (top <= ceilHeight)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Grid Functions
        private void ConstructGrid()
        {
            // By this point, the bubbleGrid has been constructed, but is not yet drawn or linked
            double left;
            double top;
            int row;
            int col;
            for (row = 0, top = ceilHeight; row < bubbleGrid.Length; row++, top += Bubble.SPRITE_EDGE)
            {
                if (bubbleGrid[row] != null)
                {
                    for (col = 0, left = wallWidth + ((row % 2 == 0) ? 0 : (Bubble.SPRITE_EDGE / 2)); col < bubbleGrid[row].Length; col++, left += Bubble.SPRITE_EDGE)
                    {
                        Bubble thisBub = bubbleGrid[row][col];
                        if (thisBub != null)
                        {
                            thisBub.Draw();
                            Canvas.SetLeft(thisBub.Element, left);
                            Canvas.SetTop(thisBub.Element, top);
                            canvasPlayArea.Children.Add(thisBub.Element);
                            // North
                            if (row > 0)
                            {
                                if (row % 2 == 0)
                                {
                                    thisBub.NW = (col > 0) ? bubbleGrid[row - 1][col - 1] : null;
                                    thisBub.NE = (col < (bubbleGrid[row].Length - 1)) ? bubbleGrid[row - 1][col] : null;
                                }
                                else
                                {
                                    thisBub.NW = bubbleGrid[row - 1][col];
                                    thisBub.NE = bubbleGrid[row - 1][col + 1];
                                }
                            }
                            thisBub.W = (col > 0) ? bubbleGrid[row][col - 1] : null;
                            if (thisBub.NW != null)
                            {
                                thisBub.NW.SE = thisBub;
                            }
                            if (thisBub.NE != null)
                            {
                                thisBub.NE.SW = thisBub;
                            }
                            if (thisBub.W != null)
                            {
                                thisBub.W.E = thisBub;
                            }
                        }
                    }
                }
            } // end for (row)
            
            // Load Bubbles
            bubbleNext = new Bubble(bubbleCounter.Remaining);
            bubbleNext.Draw();
            canvasPlayArea.Children.Add(bubbleNext.Element);

            curRotation = 0;
            gamePointer.RenderTransform = new RotateTransform(curRotation);
            LoadBubble();
        }
        private void ClearGrid()
        {
            if (bubbleGrid != null)
            {
                for (int row = 0; row < bubbleGrid.Length; row++)
                {
                    if (bubbleGrid[row] != null)
                    {
                        for (int col = 0; col < bubbleGrid[row].Length; col++)
                        {
                            if (bubbleGrid[row][col] != null && bubbleGrid[row][col].Destroyed == false)
                                canvasPlayArea.Children.Remove(bubbleGrid[row][col].Element);
                        }
                    }
                }
            }
            bubbleGrid = new Bubble[STAGE_BUBBLE_HEIGHT][];
            overflowRow = STAGE_BUBBLE_HEIGHT - 1;
            bubbleCounter.Reset();
            if (bubbleReady != null)
                canvasPlayArea.Children.Remove(bubbleReady.Element);
            if (bubbleNext != null)
                canvasPlayArea.Children.Remove(bubbleNext.Element);
        }
        #endregion
        
        #region Ceiling Functions
        private void ResetCeilCounter()
        {
            ceilCounter = CEIL_TIMER_MAX - 8 + bubbleCounter.DistinctColors;
            labelCeilDrop.Visibility = System.Windows.Visibility.Hidden;
            labelCeilingCounter.Content = "";
        }
        private void ResetCeiling()
        {
            ceilHeight = ceilStartHeight;
            Canvas.SetTop(ceiling, ceilHeight - ceiling.Height);
            //ceiling.Height = ceilHeight;
        }
        private void AdvanceCeilingCheck()
        {
            ceilCounter--;
            if (ceilCounter == 0)
            {
                AdvanceCeiling();
                labelCeilDrop.Visibility = System.Windows.Visibility.Hidden;
                labelCeilingCounter.Content = "";
            }
            else if (ceilCounter == 1)
            {
                // Shake faster
                labelCeilDrop.Visibility = System.Windows.Visibility.Visible;
                labelCeilingCounter.Content = "1";
            }
            else if (ceilCounter == 2)
            {
                // Shake some
                labelCeilDrop.Visibility = System.Windows.Visibility.Visible;
                labelCeilingCounter.Content = "2";
            }
            else if (ceilCounter == 3)
            {
                // Shake a little
                labelCeilDrop.Visibility = System.Windows.Visibility.Visible;
                labelCeilingCounter.Content = "3";
            }
            else
            {
                labelCeilDrop.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        private void AdvanceCeiling()
        {
             double deltaY = Bubble.SPRITE_EDGE;

            for (int row = 0; row < bubbleGrid.Length && bubbleGrid[row] != null; row++)
            {
                for (int col = 0; col < bubbleGrid[row].Length; col++)
                {
                    Bubble thisBub = bubbleGrid[row][col];
                    if (thisBub != null && thisBub.Destroyed == false)
                    {
                        Canvas.SetTop(thisBub.Element, Canvas.GetTop(thisBub.Element) + deltaY);
                    }
                }
            }

            ceilHeight += deltaY;
            Canvas.SetTop(ceiling, Canvas.GetTop(ceiling) + deltaY);
            //ceiling.Height = ceilHeight;
            

            overflowRow -= 1;
            if (HasLostGame())
            {
                // TODO: Add Game Logic Here
                labelGameOver.Visibility = System.Windows.Visibility.Visible;
                textBlockPlayAgain.Visibility = System.Windows.Visibility.Visible;
                //MessageBox.Show("Game Over!");
                state = GameState.GameOver;
            }
            ResetCeilCounter();
        }
        #endregion

        #region Game Progress
        private void EndOfFireCheck()
        {
            // This happens after a bubble has been fired and any applicable bubbles have been popped or dropped
            if (bubbleCounter.Total() == 0)
            {
                // Round has been won.
                RoundEnding();
                return;
            }
            // This changes the active bubble if there are no more of its kind on the board (actual game doesn't do this, should I?)
            if (bubbleCounter.Count(bubbleNext.GetColor()) == 0 && bubbleCounter.Total() > 0)
            {
                canvasPlayArea.Children.Remove(bubbleNext.Element);
                bubbleNext = new Bubble(bubbleCounter.Remaining);
                bubbleNext.Draw();
                Canvas.SetLeft(bubbleNext.Element, loadPoint.X);
                Canvas.SetTop(bubbleNext.Element, loadPoint.Y);
                canvasPlayArea.Children.Add(bubbleNext.Element);
            }
            if (HasLostGame())
            {
                // TODO: Add Game Logic Here
                //MessageBox.Show("Game Over!");
                labelGameOver.Visibility = System.Windows.Visibility.Visible;
                textBlockPlayAgain.Visibility = System.Windows.Visibility.Visible;
                state = GameState.GameOver;
                return;
            }
            AdvanceCeilingCheck();
            LoadBubble();
        }
        private void RoundEnding()
        {
            state = GameState.RoundOver;

            // Show Round Clear Message

            LoadLevel(currentLevel + 1);
        }
        private bool HasLostGame()
        {
            if (bubbleGrid[overflowRow] == null)
                return false;

            foreach (Bubble b in bubbleGrid[overflowRow])
            {
                if (b != null && b.Destroyed == false)
                    return true;
            }
            return false;
        }

        private void NewGame()
        {
            Score = 0;
            labelGameOver.Visibility = System.Windows.Visibility.Hidden;
            textBlockPlayAgain.Visibility = System.Windows.Visibility.Hidden;
            LoadLevel(0);
        }

        private void LevelReady()
        {
            state = GameState.Ready;

            // Show Round/Ready Message, switch to active when done

            state = GameState.Active;
        }
        #endregion
    }
}