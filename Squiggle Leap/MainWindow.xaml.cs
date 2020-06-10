using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SquiggleLeap
{
    public partial class MainWindow : Window
    {
        /* Squiggle Leap
         * Made by Jackson Miller
         * Dalat Computer Science
         * December 7, 2017
         */

        #region Variables and Stuff

        // Public value storing the dimensions of the game window
        public static Thickness Bounds;

        // The timer that runs the game. (Public, see Sprite class)
        public static DispatcherTimer GameTick = new DispatcherTimer();

        // List of all the platforms in the game
        List<Platform> platforms = new List<Platform>();

        // Random Number Generator
        Random rng = new Random();

        // Parameters for the platform placement algorithm
        int minHeight = 30;
        int maxHeight = 170;
        int maxDistance = 210;

        // Refrence to the last platform placed. Used in the placement algorithm
        Platform lastPlatform;

        // Values to calculate the player's score
        int score;
        double previousPosition;

        // Player's position from the top of the window. Updated in ScrollView() function
        double yOffset;

        // Gameover flag 
        bool GameOver = false;

        // Game over SFX
        SoundPlayer fallSound = new SoundPlayer(Properties.Resources.Fall);

        #endregion

        #region Setup

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            // Starting Visibility
            Menu.Visibility = Visibility.Visible;
            GameOverScreen.Visibility = Visibility.Collapsed;
            ScoreBlock.Visibility = Visibility.Collapsed;

            // Set the High Score text block to the current highscore stored in project propeties
            HighScoreText.Text = Properties.Settings.Default.high_score.ToString();

            // Set up the Game Tick Timer
            GameTick.Interval = new TimeSpan(0, 0, 0, 0, 20);
            GameTick.Tick += UpdateMain;
        }

        // When the content is rendered for the first time.
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Set the bounds to the width and hight of the game canvas
            Bounds = new Thickness(0, 0, GameCanvas.ActualWidth - Player.Width, GameCanvas.ActualHeight);

            // Fill the list with all of the platforms in the scene
            foreach (var platform in GameCanvas.Children)
            {
                if (platform != Player)
                {
                    platforms.Add(platform as Platform);
                }
            }
        }

        // This function is called when the game is started
        // The score is reset and the player and starting platform are placed at the bottom of the screen
        private void SetupGame()
        {
            // Reset the score and retrieve the high score from Properties file
            score = 0;
            ScoreText.Text = score.ToString();

            // Show the score text block
            ScoreBlock.Visibility = Visibility.Visible;

            // Disable all the platforms
            foreach (Platform platform in platforms)
            {
                platform.Enabled = false;
            }

            // Set the first platform to the bottom center of the level and enable it
            startPlat.Position = new Point((Bounds.Right / 2), Bounds.Bottom - startPlat.Height);
            startPlat.Enabled = true;

            // The starting platform used in placing platforms
            lastPlatform = startPlat;

            // Set the starting postion of the player right above first platform
            // Scorekeeping starts from this position
            Player.Position = new Point(startPlat.Position.X, startPlat.Position.Y - Player.Height);
            previousPosition = Player.Position.Y;

            // Scroll to the bottom of the canvas
            Scroller.ScrollToBottom();
        }


        #endregion

        #region GameCode

        // This is the main tick function. Called by the timer every 20 milliseconds
        void UpdateMain(object sender, EventArgs e)
        {
            // Detect when the player hits a platform, and jump them upwards
            DetectCollisions();

            // Keep player in view at all times
            ScrollView();

            // Place any disabled platforms randomly at the top of the level
            SpawnPlatforms();

            // Monitor game state and end the game if the player has fallen
            CheckForGameOver();
        }

        void DetectCollisions()
        {
            // Check the list of platforms for a Player collision
            // Also check if a any platform is offscreen
            foreach (Platform thisPlatform in platforms)
            {
                // Only jump if the player has a downwards velocity
                if (Player.Collider.IntersectsWith(thisPlatform.Collider) && Player.isFalling && thisPlatform.Enabled)
                {
                    // Make the player jump
                    Player.Jump();

                    // Only calculate score when the player jumps
                    KeepScore();
                }

                // Get the position relative to screen size 
                double y = thisPlatform.Position.Y - Scroller.VerticalOffset;

                // If a platform is off screen, disable it and disable movement
                if (y >= Scroller.ViewportHeight)
                {
                    thisPlatform.Enabled = false;

                    thisPlatform.Mover = false;
                }
            }
        }

        void SpawnPlatforms()
        {
            // Loop through the platforms and find one that is disabled.
            foreach (Platform thisPlatform in platforms)
            {
                if (!thisPlatform.Enabled)
                {
                    // Randomize x and y position 
                    double placementX = rng.Next((int)Bounds.Right);
                    double placementY = rng.Next(minHeight, maxHeight);

                    // Set the position to the new point, building from the y position of the last platform placed
                    thisPlatform.Position = new Point(placementX, lastPlatform.Position.Y - placementY);

                    // Calculate the distance between the last platform placed, and this one
                    // This is done using pythagorean theorem. I use the existing Vector.Length for simplicity 
                    double x = thisPlatform.Position.X - lastPlatform.Position.X;
                    double y = thisPlatform.Position.Y - lastPlatform.Position.Y;
                    Vector d = new Vector(x, y);

                    // If the platforms are too far away to jump, skip to next disabled item in the list. Because thisPlatform is never enabled,
                    //      a new position will be calculated
                    if (d.Length >= maxDistance)
                    {
                        continue;
                    }

                    // 1 in 15 chance that a spawned platform will be moving type. Platform movement is handled in the platform class
                    if (rng.Next(15) == 0)
                    {
                        thisPlatform.Mover = true;
                    }

                    // Enable the new platform
                    thisPlatform.Enabled = true;

                    // thisPlatform is the last one placed
                    lastPlatform = thisPlatform;
                }
            }
        }

        void ScrollView()
        {
            // This function handles keeping the player on screen by scrolling the Game Canvas upwards. 
            // The game starts at the bottom of the level, then the vertical offset is reduced.

            // Update the current vetical position of the player relitive to the top of the window (distance from top of screen)
            yOffset = Player.Position.Y - Scroller.VerticalOffset;

            // If the distance is less then around half the hight of the window, scroll slightly upwards
            if (yOffset <= (Scroller.ViewportHeight / 2) - 80)
            {
                Scroller.ScrollToVerticalOffset(Scroller.VerticalOffset - 12);
            }

        }

        void CheckForGameOver()
        {
            // Check if the player has fallen out of the level and the game is not already over
            if (yOffset > Scroller.ViewportHeight && !GameOver)
            {
                // The game is over
                GameOver = true;

                // Play death sound effect
                fallSound.Play();

                // If the player has a new high score, save it to file and display textbox
                if (score > Properties.Settings.Default.high_score)
                {
                    Properties.Settings.Default.high_score = score;
                    newHighscore.Visibility = Visibility.Visible;
                }
                else
                {
                    newHighscore.Visibility = Visibility.Collapsed;
                }

                // Show the death screen and stop the game timer
                GameOverScreen.Visibility = Visibility.Visible;
                endHighScoreText.Text = Properties.Settings.Default.high_score.ToString();

                GameTick.Stop();
            }
        }

        void KeepScore()
        {
            // Score is based on the net distance traveled
            // This function is called everytime the player jumps on a platform. 

            // Checks for an increase in Y position (top is 0) 
            if (Player.Position.Y < previousPosition)
            {
                // If the player has gone upwards then add distance traveled to the score and update the text box
                // The score can only be a whole number
                score += (int)(previousPosition - Player.Position.Y) / 10;
                ScoreText.Text = score.ToString();

                // This postion is the new previousPosition
                previousPosition = Player.Position.Y;
            }

        }

        #endregion

        #region Event Handlers

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // On the main menu, start the game when SPACE is pressed
            if (e.Key == Key.Space && !GameTick.IsEnabled && !GameOver)
            {
                Menu.Visibility = Visibility.Collapsed;

                SetupGame();
                GameTick.Start();
            }

            // However, if SPACE is pressed when the game is over, restart the game
            if (e.Key == Key.Space && GameOver)
            {
                // Close the Gameover UI
                GameOverScreen.Visibility = Visibility.Collapsed;

                // The game is no longer over
                GameOver = false;

                // Setup the new game and start the timer
                SetupGame();
                GameTick.Start();
            }
        }

        // Handler for Quit button
        private void Quit_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Save the current high score when the window closes 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        // Override for scrollview mousewheel event. Prevents player from scrolling with mouse
        private void Scroller_PreviewMouseWheel_Override(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

    }
}
