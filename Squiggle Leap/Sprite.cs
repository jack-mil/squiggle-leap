using System;
using System.Windows;
using System.Windows.Controls;

namespace SquiggleLeap
{
    public class Sprite : Image
    {
        //The collider of the sprite
        public Rect Collider = new Rect();

        //The position on the canvas of the sprite
        public Point Position
        {
            get
            {
                return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            }
            set
            {
                Canvas.SetLeft(this, value.X);
                Canvas.SetTop(this, value.Y);
            }
        }

        //bool value to enable/disable the sprite
        public bool Enabled
        {
            get
            {
                if (Visibility == Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (value)
                {
                    Visibility = Visibility.Visible;
                }
                else
                {
                    Visibility = Visibility.Collapsed;
                }
            }
        }

        public Sprite()
        {
            // Add a protected method to the Game Tick Timer
            MainWindow.GameTick.Tick += Update;
        }

        //On the same timer as the main Tick Update
        protected virtual void Update(object sender, EventArgs e)
        {
            //ensure collider is at the same position as the sprite
            MoveCollider();
        }

        public void MoveCollider()
        {
            Collider.X = Position.X;
            Collider.Y = Position.Y;

        }
    }
}
