using System;
using System.Media;
using System.Windows;
using System.Windows.Input;

namespace SquiggleLeap
{
    public class Player : Sprite
    {
        // Velocity vector and position point
        Vector Velocity = new Vector(0, 0);
        Point newPosition;

        // Flag for when player is jumping
        public bool isFalling = false;

        // Physics constants
        const double SPEED = 2;
        const double SMOOTHING = .8;
        const double JUMP = -60;
        const double GRAVITY = 3;

        // Jump sound effect
        SoundPlayer jumpSound = new SoundPlayer(Properties.Resources.Jump);

        public Player()
        {
            // Width is less than height to account for character's nose. Purely visual effect
            Collider.Width = 35;
            Collider.Height = 60;
        }

        // Inherited tick update function. Every update, move the player.
        protected override void Update(object sender, EventArgs e)
        {
            // Call the base method
            base.Update(sender, e);

            // Move player from user input
            MovePlayer();
        }

        private void MovePlayer()
        {
            // If the user presses the Left key or A key, move left
            if (Keyboard.IsKeyDown(Key.Left) || Keyboard.IsKeyDown(Key.A))
            {
                Velocity.X -= SPEED;
            }

            // If the user presses the Rigt key or D key, move right
            if (Keyboard.IsKeyDown(Key.Right) || Keyboard.IsKeyDown(Key.D))
            {
                Velocity.X += SPEED;
            }

            // Decrease the Y position by gravity
            Velocity.Y += GRAVITY;

            // Smooth out the movement
            Velocity *= SMOOTHING;

            // Calculate the new position of the player
            newPosition = Position + Velocity;

            // If the player is falling, update the flag
            if (Position.Y - newPosition.Y < 0)
                isFalling = true;

            // If the player leaves the game area from the left or right, place them on the opposite side
            if (newPosition.X < -30)
            {
                newPosition.X = MainWindow.Bounds.Right;
            }
            else if (newPosition.X > MainWindow.Bounds.Right + Width)
            {
                newPosition.X = -30;
            }

            // Update the position
            Position = newPosition;
        }

        // Public Jump method to be called when colliding with a platform from above
        public void Jump()
        {
            // The player is no longer falling
            isFalling = false;

            // Increase the upwards velocity by jump
            Velocity.Y = JUMP;

            // Play the sound effect
            jumpSound.Play();
        }

    }
}
