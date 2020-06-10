using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SquiggleLeap
{
    public class Platform : Sprite
    {
        // Refrences to the different image variations
        Uri bluePlat = new Uri("Assets/BluePlatform.png", UriKind.Relative);
        Uri greenPlat = new Uri("Assets/Platform.png", UriKind.Relative);

        // Bool determining if this is a moving platform
        public bool Mover = false;

        // The speed at which a platform will move and Point used in movement
        int speed = 3;
        Point newPosition;

        // Constructor sets the correct height and width of the collider.
        public Platform()
        {
            Collider.Height = 16;
            Collider.Width = 60;
        }

        // The inherited virtual method, called every tick by the MainTimer 
        protected override void Update(object sender, EventArgs e)
        {
            // Call the base method
            base.Update(sender, e);

            // If this is a moving platform, then translate the X position by speed every tick
            if (Mover)
            {
                // Change this platform's color
                Source = new BitmapImage(bluePlat);

                // Store the current position
                newPosition = Position;

                // Translate the postion by speed each update
                newPosition.X += speed;

                // If the platform hits one of the edges, reverse the direction
                if (newPosition.X > MainWindow.Bounds.Right || newPosition.X < 0)
                {
                    speed *= -1;
                }

                // Update the position
                Position = newPosition;
            }
            else
            {
                Source = new BitmapImage(greenPlat);
            }
        }
    }
}
