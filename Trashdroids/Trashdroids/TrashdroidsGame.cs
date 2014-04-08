using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using BEPUphysics.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics.EntityStateManagement;
using Particles;
using ConversionHelper;

/**
 * Main Game class
 */
namespace Trashdroids
{
    public enum GameState
    {
        INTRODUCTION,
        MAIN_MENU,
        IN_GAME_SINGLE_PLAYER,
        IN_GAME_MULTIPLAYER,
        POSTGAME,
    }

    enum GameResult
    {
        PLAYER_ONE_WINS,
        PLAYER_TWO_WINS,
        SINGLE_PLAYER_WIN,
        SINGLE_PLAYER_FAIL
    }

    enum MenuItem
    {
        SINGLE_PLAYER,
        MULTIPLAYER,
        REPLAY_INTRO,
        EXIT_GAME
    }

    /// <summary>
    /// This is the core Trashdroids game class
    /// </summary>
    public class TrashdroidsGame : Microsoft.Xna.Framework.Game
    {
        #region Introduction String

        private string _introductionString =
            "It is the distant future, the year 2000. During the robotic uprising of the late\n" +
            "90's, the last remaining humans fled from their robot overlords and launched into\n" +
            "space aboard a small space station. Now, as the colony continues to thrive in a remote\n" +
            "corner of the solar system, they face one great challenge: garbage. If they\n" +
            "jettison their garbage into the void of space, the robots could use their, erm,\n" +
            "magic... garbage scanners... or something... to detect the humans. This challenge\n" +
            "forced the humans to create unique trash receptacles, inside of which trash\n" +
            "must somehow be destroyed. Clearly, the only logical solution is to blow it up\n" +
            "with missiles. The receptacles were filled with tiny, trash-destroying droids and\n" +
            "thus, Trashdroids were born.";
        private Vector2 _introductionSize;
        private float _introductionOffset;
        private int _introductionFallValue;

        #endregion

        #region Game Parameters

        //Asteroid parameters
        public static int   ASTEROIDS_NUM               = 20;
        public static float ASTEROIDS_PCT_TO_DESTROY    = 0.50f;
        public static bool  ASTEROIDS_MOVEABLE          = true;
        public static bool  ASTEROIDS_DESTRUCTABLE      = true;

        //Camera parameters
        public static bool  CAMERA_THIRD_PERSON        = true;
        public static float CAMERA_ZOOM_IN_DISTANCE    = 8;
        public static float CAMERA_ZOOM_OUT_DISTANCE   = 500;
        public static int   WINDOW_SIZE_HEIGHT         = 768;
        public static int   WINDOW_SIZE_WIDTH          = 1024;
        
        //Ship parameters
        public static float SHIP_ANGULAR_ACCEL_COEFF   = 0.6f;
        public static float SHIP_ANGULAR_BRAKING_COEFF = .95f;
        public static float SHIP_ANGULAR_DRAG_COEFF    = 0.97f;
        public static float SHIP_LINEAR_ACCEL_COEFF    = 3f;
        public static float SHIP_LINEAR_BRAKING_COEFF  = .95f;
        public static float SHIP_LINEAR_DRAG_COEFF     = 0.97f;
        public static int   SHIP_STARTING_LIVES = 3;

        //Missile parameters
        public static float MISSILES_PER_SEC           = .5f;
        public static bool  MISSILE_STREAM_MODE        = false;
        public static bool  MISSILE_SPREAD_MODE        = false;
        public static bool  _missileAlternation        = false;

        //Universe parameters
        public static float UNIVERSE_RADIUS            = 40;

        #endregion

        #region Private variables

        //Graphics
        private ScreenManager _screenManager;
        private SpriteBatch           _spriteBatch;
        private SpriteFont            _basicFont;
        private SpriteFont            _titleFont;
        private SpriteFont            _tinyFont;
        private SpriteFont            _introFont;
        private Texture2D             _livesSprite;
        private Texture2D             _logoSprite;

        //Models
        private Droid _droidOne = null;
        private Droid _droidTwo = null;
        private List<Asteroid> _asteroids;
        private List<Missile> _missiles;
        private Universe _universe = null;
        private Powerup _powerup = null;
        private ulong nextMissileIdx = 1;
        public ulong nextAsteroidIdx = 1;

        //BEPU
        private Space _physics;

        //Particles
        private ParticleSystem explosionParticles;
        private ParticleSystem missileTrailParticles;
        private ParticleSystem powerupParticles;

        //Sounds
        SoundEffect _soundLaunch;
        SoundEffect _soundExplodeSmall;
        SoundEffect _soundExplodeLarge;
        SoundEffect _soundCrashMetal;
        SoundEffect _soundEngine;
        SoundEffect _soundIngameAmbient;
        SoundEffectInstance _soundIngameAmbientInstance;
        SoundEffect _soundMenuMusic;
        SoundEffectInstance _soundMenuMusicInstance;

        //State management
        private GameState _state = GameState.INTRODUCTION;
        private MenuItem _menuItem = MenuItem.SINGLE_PLAYER;
        public KeyboardState _kbState_previous;
        public KeyboardState _kbState_current;
        public GamePadState _gp1State_previous;
        public GamePadState _gp1State_current;
        public GamePadState _gp2State_previous;
        public GamePadState _gp2State_current;
        
        //Scoring
        private int targetScore;
        private int currentScore;
        private TimeSpan gameStartTime;
        private String gameTimeString;
        private GameResult gameResult;

        private Random _rand;
        
        #endregion

        public GameState GameState
        {
            get { return _state; }
        }

        public TrashdroidsGame()
        {
            //Uncomment the following to set up some decent multiplayer parameters
            ASTEROIDS_MOVEABLE = false;
            ASTEROIDS_DESTRUCTABLE = false;
            MISSILE_STREAM_MODE = true;
            UNIVERSE_RADIUS = 75;
            ASTEROIDS_NUM = 150;

            //XNA graphics
            _screenManager = new ScreenManager(this, new GraphicsDeviceManager(this));
            Content.RootDirectory = "Content";

            _screenManager.FullScreenEnable();

            //BEPU
            ParallelLooper looper = new ParallelLooper();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                looper.AddThread();
            }

            _physics = new Space(looper);

            Services.AddService(typeof(Space), _physics);

            //Models
            _asteroids = new List<Asteroid>();
            _missiles = new List<Missile>();

            //Other
            _rand = new Random();

            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            explosionParticles = new ExplosionParticleSystem(this, Content);
            missileTrailParticles = new MissileParticleSystem(this, Content);
            powerupParticles = new PowerupParticleSystem(this, Content);
            Components.Add(explosionParticles);
            Components.Add(missileTrailParticles);
            Components.Add(powerupParticles);
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            //2D graphics
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _basicFont = Content.Load<SpriteFont>("font/BasicFont");
            _titleFont = Content.Load<SpriteFont>("font/TitleFont");
            _tinyFont = Content.Load<SpriteFont>("font/TinyFont");
            _introFont = Content.Load<SpriteFont>("font/IntroFont");
            _livesSprite = Content.Load<Texture2D>("life");
            _logoSprite = Content.Load<Texture2D>("logo");

            
            //Sounds
            _soundLaunch = Content.Load<SoundEffect>("audio/launch");
            _soundExplodeSmall = Content.Load<SoundEffect>("audio/explode_small");
            _soundExplodeLarge = Content.Load<SoundEffect>("audio/explode_large");
            _soundCrashMetal = Content.Load<SoundEffect>("audio/crash_metal");
            _soundEngine = Content.Load<SoundEffect>("audio/engine");
            _soundIngameAmbient = Content.Load<SoundEffect>("audio/ingame_ambient");
            _soundMenuMusic = Content.Load<SoundEffect>("audio/robots");
            _soundMenuMusicInstance = _soundMenuMusic.CreateInstance();
            _soundIngameAmbientInstance = _soundIngameAmbient.CreateInstance();
            _soundMenuMusicInstance.IsLooped = true;
            _soundIngameAmbientInstance.IsLooped = true;

            _screenManager.LoadContent(GraphicsDevice, _spriteBatch);

            explosionParticles.SetCameras(_screenManager.CameraOne, _screenManager.CameraTwo);
            missileTrailParticles.SetCameras(_screenManager.CameraOne, _screenManager.CameraTwo);
            powerupParticles.SetCameras(_screenManager.CameraOne, _screenManager.CameraTwo);

            //Introduction
            _introductionSize = _introFont.MeasureString(_introductionString);
            _introductionOffset = _screenManager.ScreenHeight + 20;


            //Keyboard state init
            _kbState_previous = Keyboard.GetState();
            _gp1State_previous = GamePad.GetState(PlayerIndex.One);
            _gp2State_previous = GamePad.GetState(PlayerIndex.Two);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            //Nah.
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _kbState_current = Keyboard.GetState();
            _gp1State_current = GamePad.GetState(PlayerIndex.One);
            _gp2State_current = GamePad.GetState(PlayerIndex.Two);

            //Fullscreen toggle
            if (KeyWasPressed(Keys.Back))
            {
                _screenManager.FullScreenToggle();
                explosionParticles.UpdateWindowSize(GraphicsDevice.Viewport);
                missileTrailParticles.UpdateWindowSize(GraphicsDevice.Viewport);
                powerupParticles.UpdateWindowSize(GraphicsDevice.Viewport);
            }

            //Cheat
            if (_kbState_current.IsKeyDown(Keys.RightAlt))
            {
                currentScore++;
            }

            switch (_state)
            {
                #region State: Pre-Game
                case GameState.INTRODUCTION:
                    _introductionOffset -= 0.5f;

                    if (_introductionOffset <= -_introductionSize.Y)
                    {
                        _introductionFallValue += 20;
                    }
                    if (_introductionFallValue >= _screenManager.ScreenHeight)
                    {
                        _state = GameState.MAIN_MENU;

                    }

                    if (_soundMenuMusicInstance.State != SoundState.Playing)
                    {
                        _soundMenuMusicInstance.Play();
                    }

                    if (_soundIngameAmbientInstance.State == SoundState.Playing)
                    {
                        _soundIngameAmbientInstance.Stop();
                    }

                    if (KeyWasPressed(Keys.Space) ||
                        KeyWasPressed(Keys.Enter) ||
                        KeyWasPressed(Buttons.A, PlayerIndex.One) ||
                        KeyWasPressed(Buttons.A, PlayerIndex.Two) ||
                        KeyWasPressed(Buttons.Start, PlayerIndex.One) ||
                        KeyWasPressed(Buttons.Start, PlayerIndex.Two))
                    {
                        _state = GameState.MAIN_MENU;
                    }
                    // Return to menu
                    if (KeyWasPressed(Buttons.Back, PlayerIndex.One) ||
                        KeyWasPressed(Buttons.Back, PlayerIndex.Two) ||
                        KeyWasPressed(Keys.Escape))
                    {
                        this.Exit();
                    }

                    break;
                #endregion

                #region State: Main Menu
                case GameState.MAIN_MENU:
                    
                    if (_soundMenuMusicInstance.State == SoundState.Stopped)
                    {
                        _soundMenuMusicInstance.Play();
                    }
                    if (_soundIngameAmbientInstance.State == SoundState.Playing)
                    {
                        _soundIngameAmbientInstance.Stop();
                    }

                    //Select current menu option
                    if (KeyWasPressed(Keys.Space) ||
                        KeyWasPressed(Keys.Enter) ||
                        KeyWasPressed(Buttons.A, PlayerIndex.One) ||
                        KeyWasPressed(Buttons.A, PlayerIndex.Two) ||
                        KeyWasPressed(Buttons.Start, PlayerIndex.One) ||
                        KeyWasPressed(Buttons.Start, PlayerIndex.Two))
                    {
                        if (_menuItem == MenuItem.SINGLE_PLAYER)
                        {
                            ResetSimulationSinglePlayer(gameTime);
                        }
                        else if (_menuItem == MenuItem.MULTIPLAYER)
                        {
                            ResetSimulationMultiplayer(gameTime);
                        }
                        else if (_menuItem == MenuItem.REPLAY_INTRO)
                        {
                            _introductionOffset = _screenManager.ScreenHeight + 20; // _graphics.PreferredBackBufferHeight + 20;
                            _introductionFallValue = 0; 
                            _state = GameState.INTRODUCTION;
                        }
                        else if (_menuItem == MenuItem.EXIT_GAME)
                        {
                            this.Exit();
                        }
                    }
                    if (KeyWasPressed(Keys.Up) || KeyWasPressed(Buttons.DPadUp, PlayerIndex.One) || KeyWasPressed(Buttons.DPadUp, PlayerIndex.Two))
                    {
                        _menuItem = (_menuItem == MenuItem.SINGLE_PLAYER) ? MenuItem.SINGLE_PLAYER : _menuItem - 1;
                    }
                    if (KeyWasPressed(Keys.Down) || KeyWasPressed(Buttons.DPadDown, PlayerIndex.One) || KeyWasPressed(Buttons.DPadDown, PlayerIndex.Two))
                    {
                        _menuItem = (_menuItem == MenuItem.EXIT_GAME) ? MenuItem.EXIT_GAME : _menuItem + 1;
                    }
                    if (_kbState_current.IsKeyDown(Keys.Q))
                    {
                        UNIVERSE_RADIUS += 5;
                        if (UNIVERSE_RADIUS > 500)
                            UNIVERSE_RADIUS = 500;
                    }
                    if (_kbState_current.IsKeyDown(Keys.A))
                    {
                        UNIVERSE_RADIUS -= 5;
                        if (UNIVERSE_RADIUS < 40)
                            UNIVERSE_RADIUS = 40;
                    }
                    if (_kbState_current.IsKeyDown(Keys.E))
                    {
                        ASTEROIDS_NUM += 1;
                        if (ASTEROIDS_NUM > 500)
                            ASTEROIDS_NUM = 500;
                    }
                    if (_kbState_current.IsKeyDown(Keys.D))
                    {
                        ASTEROIDS_NUM -= 1;
                        if (ASTEROIDS_NUM < 2)
                            ASTEROIDS_NUM = 2;
                    }
                    if (KeyWasPressed(Keys.T))
                    {
                        MISSILE_STREAM_MODE = !MISSILE_STREAM_MODE;
                    }
                    if (KeyWasPressed(Keys.G))
                    {
                        MISSILE_SPREAD_MODE = !MISSILE_SPREAD_MODE;
                        if (MISSILE_SPREAD_MODE)
                        {
                            //Enable stream mode, since this is needed
                            MISSILE_STREAM_MODE = true;
                        }
                    }
                    if (KeyWasPressed(Keys.U))
                    {
                        ASTEROIDS_DESTRUCTABLE = !ASTEROIDS_DESTRUCTABLE;
                    }
                    if (KeyWasPressed(Keys.J))
                    {
                        ASTEROIDS_MOVEABLE = !ASTEROIDS_MOVEABLE;
                    }
                    // Return to menu
                    if (KeyWasPressed(Buttons.Back, PlayerIndex.One) ||
                        KeyWasPressed(Buttons.Back, PlayerIndex.Two) ||
                        KeyWasPressed(Keys.Escape))
                    {
                        this.Exit();
                    }
                    break;

                #endregion

                #region State: Single Player
                case GameState.IN_GAME_SINGLE_PLAYER:

                    // Return to menu
                    if (KeyWasPressed(Buttons.Back, PlayerIndex.One) ||
                        KeyWasPressed(Buttons.Back, PlayerIndex.Two) ||
                        KeyWasPressed(Keys.Escape))
                    {
                        _state = GameState.MAIN_MENU;
                    }

                    //Scoring
                    if (currentScore >= targetScore)
                    {
                        _state = GameState.POSTGAME;
                        gameTimeString = 
                             (gameTime.TotalGameTime - gameStartTime).Minutes.ToString("D2") + ":" +
                             (gameTime.TotalGameTime - gameStartTime).Seconds.ToString("D2") + ":" + 
                            ((gameTime.TotalGameTime - gameStartTime).Milliseconds/10).ToString("D2");
                        gameResult = GameResult.SINGLE_PLAYER_WIN;
                    }
                    if (_droidOne.LivesRemaining <= 0)
                    {
                        _state = GameState.POSTGAME;
                        gameTimeString =
                             (gameTime.TotalGameTime - gameStartTime).Minutes.ToString("D2") + ":" +
                             (gameTime.TotalGameTime - gameStartTime).Seconds.ToString("D2") + ":" +
                            ((gameTime.TotalGameTime - gameStartTime).Milliseconds / 10).ToString("D2");
                        gameResult = GameResult.SINGLE_PLAYER_FAIL;
                    }

                    _physics.Update();

                    //Update models
                    _droidOne.Update(gameTime);
                    foreach (Missile missile in _missiles)
                    {
                        missile.Update(gameTime);
                    }
                    if (_powerup != null)
                    {
                        _powerup.Update();
                    }

                    break;
                #endregion

                #region State: Multiplayer
                case GameState.IN_GAME_MULTIPLAYER:

                    // Return to menu
                    if (KeyWasPressed(Buttons.Back, PlayerIndex.One) ||
                        KeyWasPressed(Buttons.Back, PlayerIndex.Two) ||
                        KeyWasPressed(Keys.Escape))
                    {
                        _state = GameState.MAIN_MENU;
                    }

                    //Scoring
                    if (_droidOne.LivesRemaining <= 0 && _droidOne.LivesRemaining < _droidTwo.LivesRemaining)
                    {
                        _state = GameState.POSTGAME;
                        gameTimeString =
                             (gameTime.TotalGameTime - gameStartTime).Minutes.ToString("D2") + ":" +
                             (gameTime.TotalGameTime - gameStartTime).Seconds.ToString("D2") + ":" +
                            ((gameTime.TotalGameTime - gameStartTime).Milliseconds / 10).ToString("D2");
                        gameResult = GameResult.PLAYER_TWO_WINS;
                    }
                        else if (_droidTwo.LivesRemaining <= 0 && _droidOne.LivesRemaining > _droidTwo.LivesRemaining)
                    {
                        _state = GameState.POSTGAME;
                        gameTimeString =
                             (gameTime.TotalGameTime - gameStartTime).Minutes.ToString("D2") + ":" +
                             (gameTime.TotalGameTime - gameStartTime).Seconds.ToString("D2") + ":" +
                            ((gameTime.TotalGameTime - gameStartTime).Milliseconds / 10).ToString("D2");
                        gameResult = GameResult.PLAYER_ONE_WINS;
                    }

                    _physics.Update();

                    //Update models
                    _droidOne.Update(gameTime);
                    _droidTwo.Update(gameTime);
                    foreach (Missile missile in _missiles)
                    {
                        missile.Update(gameTime);
                    }
                    if (_powerup != null)
                    {
                        _powerup.Update();
                    }

                    //Create new powerup?
                    if (_rand.NextDouble() <= .001 && _powerup == null)
                    {
                        GeneratePowerup();
                    }

                    break;
                #endregion

                #region State: Postgame
                case GameState.POSTGAME:
                    // Return to menu
                    if(KeyWasPressed(Keys.Enter))
                    {
                        _state = GameState.MAIN_MENU;
                    }

                    break;
                #endregion

                default:
                    break;
            }

            _kbState_previous = _kbState_current;
            _gp1State_previous = _gp1State_current;
            _gp2State_previous = _gp2State_current;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (_state)
            {
                #region State: Pre-Game
                case GameState.INTRODUCTION:
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(_logoSprite, new Vector2(_screenManager.ScreenWidth / 2 - _logoSprite.Width / 2, _introductionOffset - _screenManager.ScreenHeight/2 - _logoSprite.Height/2 + _introductionFallValue), null,
                            Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                    _spriteBatch.DrawString(_introFont, _introductionString,
                            new Vector2(_screenManager.ScreenWidth / 2 - _introductionSize.X / 2, _introductionOffset), Color.Yellow);

                    _spriteBatch.End();
                    break;
                #endregion

                #region State: Main Menu
                case GameState.MAIN_MENU:
                    int yTitleOffset = 200;
                    int colStartLeft = 50;
                    int colDivider1 = _screenManager.ScreenWidth - 300;
                    int colDivider2 = _screenManager.ScreenWidth - 150;

                    _spriteBatch.Begin();

                    //Header
                    _spriteBatch.Draw(_logoSprite, new Vector2(_screenManager.ScreenWidth / 2 - _logoSprite.Width / 2, 50), null,
                            Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                    //Selectable options
                    _spriteBatch.DrawString(
                            _titleFont,
                            "Main Menu",
                            new Vector2(colStartLeft, 50 + yTitleOffset),
                            Color.Blue);
                    _spriteBatch.DrawString(
                            _basicFont,
                            "Single Player",
                            new Vector2(colStartLeft + 50 + (_menuItem == MenuItem.SINGLE_PLAYER ? 10 : 0), 120 + yTitleOffset),
                            (_menuItem == MenuItem.SINGLE_PLAYER ? Color.Yellow : Color.Gray));
                    _spriteBatch.DrawString(
                            _basicFont,
                            "Multiplayer",
                            new Vector2(colStartLeft + 50 + (_menuItem == MenuItem.MULTIPLAYER ? 10 : 0), 170 + yTitleOffset),
                            (_menuItem == MenuItem.MULTIPLAYER ? Color.Yellow : Color.Gray));
                    _spriteBatch.DrawString(
                            _basicFont,
                            "Return to Introduction",
                            new Vector2(colStartLeft + 50 + (_menuItem == MenuItem.REPLAY_INTRO ? 10 : 0), 220 + yTitleOffset),
                            (_menuItem == MenuItem.REPLAY_INTRO ? Color.Yellow : Color.Gray));
                    _spriteBatch.DrawString(
                            _basicFont,
                            "Exit Game",
                            new Vector2(colStartLeft + 50 + (_menuItem == MenuItem.EXIT_GAME ? 10 : 0), 270 + yTitleOffset),
                            (_menuItem == MenuItem.EXIT_GAME ? Color.Yellow : Color.Gray));

                    //Game Parameters
                    _spriteBatch.DrawString(_titleFont, "Game Parameters",
                            new Vector2(colDivider1 - _titleFont.MeasureString("Game Parameters").X, 50 + yTitleOffset), Color.White);

                    _spriteBatch.DrawString(_basicFont, "Parameter Name",
                            new Vector2(colDivider1 - _basicFont.MeasureString("Parameter Name").X, 85 + yTitleOffset), Color.Gray);
                    _spriteBatch.DrawString(_basicFont, "+/-",
                            new Vector2(colDivider1 + 50, 85 + yTitleOffset), Color.Gray);
                    _spriteBatch.DrawString(_basicFont, "Value",
                            new Vector2(colDivider2, 85 + yTitleOffset), Color.Gray);

                    _spriteBatch.DrawString(_basicFont, "Universe Size",
                            new Vector2(colDivider1 - _basicFont.MeasureString("Universe Size").X, 150 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "Q/A",
                            new Vector2(colDivider1 + 50, 150 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "" + UNIVERSE_RADIUS,
                            new Vector2(colDivider2, 150 + yTitleOffset), Color.CornflowerBlue);

                    _spriteBatch.DrawString(_basicFont, "Asteroid Count",
                            new Vector2(colDivider1 - _basicFont.MeasureString("Asteroid Count").X, 190 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "E/D",
                            new Vector2(colDivider1 + 50, 190 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "" + ASTEROIDS_NUM,
                            new Vector2(colDivider2, 190 + yTitleOffset), Color.CornflowerBlue);

                    _spriteBatch.DrawString(_basicFont, "Uber-Torpedo Mode",
                            new Vector2(colDivider1 - _basicFont.MeasureString("Uber-Missile Mode").X, 230 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "T",
                            new Vector2(colDivider1 + 50, 230 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, (MISSILE_STREAM_MODE ? "Enabled" : "Disabled"),
                            new Vector2(colDivider2, 230 + yTitleOffset), Color.CornflowerBlue);

                    _spriteBatch.DrawString(_basicFont, "BEEEEEEEEES Mode*",
                            new Vector2(colDivider1 - _basicFont.MeasureString("BEEEEEEEEES Mode*").X, 270 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_tinyFont, "*Requires Uber-Missile Mode",
                            new Vector2(colDivider1 - _tinyFont.MeasureString("*Requires Uber-Missile Mode").X, 297 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "G",
                            new Vector2(colDivider1 + 50, 270 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, (MISSILE_SPREAD_MODE ? "Enabled" : "Disabled"),
                            new Vector2(colDivider2, 270 + yTitleOffset), Color.CornflowerBlue);

                    _spriteBatch.DrawString(_basicFont, "Destructible Asteroids",
                            new Vector2(colDivider1 - _basicFont.MeasureString("Destructible Asteroids").X, 310 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "U",
                            new Vector2(colDivider1 + 50, 310 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, (ASTEROIDS_DESTRUCTABLE ? "Enabled" : "Disabled"),
                            new Vector2(colDivider2, 310 + yTitleOffset), Color.CornflowerBlue);

                    _spriteBatch.DrawString(_basicFont, "Moveable Asteroids",
                            new Vector2(colDivider1 - _basicFont.MeasureString("Moveable Asteroids").X, 350 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "J",
                            new Vector2(colDivider1 + 50, 350 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, (ASTEROIDS_MOVEABLE ? "Enabled" : "Disabled"),
                            new Vector2(colDivider2, 350 + yTitleOffset), Color.CornflowerBlue);

                    _spriteBatch.DrawString(_basicFont, "Fullscreen Mode",
                            new Vector2(colDivider1 - _basicFont.MeasureString("Fullscreen Mode").X, 390 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, "Bkspc",
                            new Vector2(colDivider1 + 50, 390 + yTitleOffset), Color.CornflowerBlue);
                    _spriteBatch.DrawString(_basicFont, (_screenManager.IsFullScreen ? "Enabled" : "Disabled"),
                            new Vector2(colDivider2, 390 + yTitleOffset), Color.CornflowerBlue);

                    _spriteBatch.End();
                    break;
                #endregion

                #region State: Single Player
                case GameState.IN_GAME_SINGLE_PLAYER:
                    DrawScreenSinglePlayer(gameTime);
                    break;
                #endregion

                #region State: Multiplayer
                case GameState.IN_GAME_MULTIPLAYER:
                    DrawScreenMultiplayer(gameTime);
                    break;
                #endregion

                #region State: Postgame
                case GameState.POSTGAME:
                    string messageString;

                    switch (gameResult)
                    {
                        case GameResult.PLAYER_ONE_WINS:
                            messageString = "Player One is Victorious!";
                            break;
                        case GameResult.PLAYER_TWO_WINS:
                            messageString = "Player Two is Victorious!";
                            break;
                        case GameResult.SINGLE_PLAYER_FAIL:
                            messageString = "You have Failed.";
                            break;
                        case GameResult.SINGLE_PLAYER_WIN:
                            messageString = "You are Victorious!";
                            break;
                        default:
                            messageString = "";
                            break;
                    }

                    _spriteBatch.Begin();

                    //Selectable options
                    _spriteBatch.DrawString(_titleFont, "Game Over",
                        new Vector2(_screenManager.ScreenWidth / 2 - _titleFont.MeasureString("Game Over").X / 2, 100), Color.Red);
                    _spriteBatch.DrawString(_titleFont, messageString,
                        new Vector2(_screenManager.ScreenWidth / 2 - _titleFont.MeasureString(messageString).X / 2, 180), Color.Red);
                    _spriteBatch.DrawString(_titleFont, "Total Game Time: " + gameTimeString,
                        new Vector2(_screenManager.ScreenWidth / 2 - _titleFont.MeasureString("Total Game Time: " + gameTimeString).X / 2, 230), Color.Red);
                    _spriteBatch.DrawString(_titleFont, "Press Enter to Return to the Main Menu",
                        new Vector2(_screenManager.ScreenWidth / 2 - _titleFont.MeasureString("Press Enter to Return to the Main Menu").X / 2, 500), Color.Red);


                    _spriteBatch.End();
                    break;
                #endregion
                default:
                    break;
            }


            base.Draw(gameTime);
        }

        /// <summary>
        /// Creates and launches a missile from the specified owner's ship
        /// </summary>
        /// <param name="owner">The ship to which the missile belongs</param>
        public void spawnMissile(Droid owner)
        {
            Droid target = null;

            if (owner == _droidOne)
            {
                target = _droidTwo;
            }
            else if (owner == _droidTwo)
            {
                target = _droidOne;
            }

            int numToSpawn = (MISSILE_SPREAD_MODE ? 10 : 1);
            
            for (int i = 0; i < numToSpawn; i++)
            {
                _missiles.Add(new Missile(this, "model/missile", "Missile" + nextMissileIdx, owner, target, _missileAlternation));
                nextMissileIdx++;
            }
            _missileAlternation = !_missileAlternation;

            _soundLaunch.Play(.5f, 0f, 0f);
        }

        /// <summary>
        /// Performs steps needed to detonate a missile (including removing it from the game)
        /// </summary>
        /// <param name="missile">The missile to remove</param>
        public void DetonateMissile(Missile missile)
        {
            CreateExplosionEffect(missile.World.Translation);

            _soundExplodeSmall.Play(.8f, 0f, 0f);

            _missiles.Remove(missile);
        }

        /// <summary>
        /// Play a 3D sound at the given location
        /// 
        /// NOTE: This is currently unused :( It works beautifully, but when
        /// new game objects were added while audio was playing, the audio cut
        /// out. Specifically, something in the Asteroid constructor (which is
        /// pretty simple...) caused the audio to get VERY choppy. Maybe a bug in
        /// XNA / BEPU, but more likely a lack of understanding on my part.
        /// </summary>
        /// <param name="sound">The sound effect to be played</param>
        /// <param name="source">The location at which to play the sound</param>
        void PlaySoundAt(SoundEffect sound, Vector3 source)
        {
            SoundEffectInstance inst = sound.CreateInstance();

            AudioEmitter emitter = new AudioEmitter();
            AudioListener listener = new AudioListener();
            
            Vector3 localizedSource = new Vector3(0,0,0);
            localizedSource += Microsoft.Xna.Framework.Vector3.Transform((source - _screenManager.CameraOne.View.Translation), _screenManager.CameraOne.View);

            localizedSource.Z = -localizedSource.Z;

            //Scale to fit this universe
            localizedSource = localizedSource / 100;

            emitter.Position = localizedSource;
            listener.Position = Vector3.Zero;

            inst.Apply3D(listener, emitter);
            inst.Play();
        }

        /// <summary>
        /// Create a standard particle explosion at the given location
        /// using the particle system.
        /// </summary>
        /// <param name="location">The source location of the explosion</param>
        public void CreateExplosionEffect(Vector3 location)
        {
            for (int i = 0; i < 300; i++)
            {
                explosionParticles.AddParticle(location, Microsoft.Xna.Framework.Vector3.Zero);
            }
        }

        /// <summary>
        /// Create powerup particles at the given location
        /// using the particle system.
        /// </summary>
        /// <param name="location">The source location of the effect</param>
        public void CreatePowerupEffect(Vector3 location)
        {
            powerupParticles.AddParticle(location, Microsoft.Xna.Framework.Vector3.Zero);
        }

        /// <summary>
        /// Create missile trailparticles at the given location
        /// using the particle system.
        /// </summary>
        /// <param name="location">The source location of the effect</param>
        public void CreateMissileTrailEffect(Vector3 location)
        {
            missileTrailParticles.AddParticle(location, Microsoft.Xna.Framework.Vector3.Zero);
        }

        /// <summary>
        /// Performs steps needed to destroy an asteroid.
        /// </summary>
        /// <param name="asteroidTag">Tag of the asteroid to destroy</param>
        public void AsteroidExplode(string asteroidTag)
        {
            foreach (Asteroid asteroid in _asteroids)
            {
                if (asteroid.Tag.Equals(asteroidTag))
                {
                    Vector3 loc = new Vector3(asteroid.World.Translation.X, asteroid.World.Translation.Y, asteroid.World.Translation.Z);
                    _soundExplodeLarge.Play(0.3f, 1f, 0f);
                    _asteroids.Remove(asteroid);
                    asteroid.Explode();
                    currentScore++;
                    break;
                }
            }
        }

        /// <summary>
        /// Adds the given asteroid to the game
        /// </summary>
        /// <param name="toAdd">The asteroid to add</param>
        public void AddAsteroid(Asteroid toAdd)
        {
            _asteroids.Add(toAdd);
        }

        /// <summary>
        /// A globally accessible way of checking if a key or button was pressed within
        /// the last frame.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True, if the key was pressed</returns>
        public bool KeyWasPressed(Keys key)
        {
            if (!_kbState_previous.IsKeyDown(key) && _kbState_current.IsKeyDown(key))
                return true;
            else
                return false;
        }

        /// <summary>
        /// A globally accessible way of checking if a key or button was pressed within
        /// the last frame.
        /// </summary>
        /// <param name="button">The button to check</param>
        /// <param name="player">The player to which the button belongs</param>
        /// <returns>True, if the button was pressed</returns>
        public bool KeyWasPressed(Buttons button, PlayerIndex player)
        {
            if (player == PlayerIndex.One)
            {
                if (!_gp1State_previous.IsButtonDown(button) && _gp1State_current.IsButtonDown(button))
                    return true;
                else
                    return false;
            }
            else if (player == PlayerIndex.Two)
            {
                if (!_gp2State_previous.IsButtonDown(button) && _gp2State_current.IsButtonDown(button))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// A globally accessible way of checking if a key or button is currently
        /// held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True, if the key is held down</returns>
        public bool KeyIsDown(Keys key)
        {
            if (_kbState_current.IsKeyDown(key))
                return true;
            else
                return false;
        }

        /// <summary>
        /// A globally accessible way of checking if a key or button is currently
        /// held down.
        /// </summary>
        /// <param name="button">The button to check</param>
        /// <param name="player">The player to which the button belongs</param>
        /// <returns>True, if the button is down.</returns>
        public bool KeyIsDown(Buttons button, PlayerIndex player)
        {
            if (player == PlayerIndex.One)
            {
                if (_gp1State_current.IsButtonDown(button))
                    return true;
                else
                    return false;
            }
            else if (player == PlayerIndex.Two)
            {
                if (_gp2State_current.IsButtonDown(button))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Returns the state of the specified thumbstick.
        /// </summary>
        /// <param name="left">If true, checks the left thumbstick; else, it checks the right thumbstick</param>
        /// <param name="player">The player to which the thumbstick belongs</param>
        /// <returns>The state of the thumbstick</returns>
        public Vector2 ThumbstickState(bool left, PlayerIndex player)
        {
            GamePadState gamePadToUse;

            if (player == PlayerIndex.One)
            {
                gamePadToUse = _gp1State_current;
            }
            else if (player == PlayerIndex.Two)
            {
                gamePadToUse = _gp2State_current;
            }
            else
            {
                return Vector2.Zero;
            }

            if (left)
            {
                return gamePadToUse.ThumbSticks.Left;
            }
            else
            {
                return gamePadToUse.ThumbSticks.Right;
            }

        }

        /// <summary>
        /// Gets the state of the specified gamepad trigger.
        /// </summary>
        /// <param name="left">Which trigger (left or right) to get</param>
        /// <param name="player">The player to which the trigger belongs</param>
        /// <returns>The position of the specified trigger</returns>
        public float TriggerState(bool left, PlayerIndex player)
        {
            GamePadState gamePadToUse;

            if (player == PlayerIndex.One)
            {
                gamePadToUse = _gp1State_current;
            }
            else if (player == PlayerIndex.Two)
            {
                gamePadToUse = _gp2State_current;
            }
            else
            {
                return 0f;
            }

            if (left)
            {
                return gamePadToUse.Triggers.Left;
            }
            else
            {
                return gamePadToUse.Triggers.Right;
            }

        }

        /// <summary>
        /// Empties the simulation and configures it for single player mode
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        private void ResetSimulationSinglePlayer(GameTime gameTime)
        {
            _state = GameState.IN_GAME_SINGLE_PLAYER;
            if(_soundMenuMusicInstance.State == SoundState.Playing)
            {
                _soundMenuMusicInstance.Stop();
            }
            if (_soundIngameAmbientInstance.State != SoundState.Playing)
            {
                _soundIngameAmbientInstance.Play();
            }
            

            gameStartTime = gameTime.TotalGameTime;
            explosionParticles.SetSplitscreen(false);
            missileTrailParticles.SetSplitscreen(false);
            powerupParticles.SetSplitscreen(false);

            //Destroy / nullify all models
            if (_universe != null)
            {
                _universe.Destroy();
                _universe = null;
            }
            if (_droidOne != null)
            {
                _droidOne.Destroy();
                _droidOne = null;
            }
            if (_droidTwo != null)
            {
                _droidTwo.Destroy();
                _droidTwo = null;
            }
            if (_powerup != null)
            {
                _powerup.Destroy();
                _powerup = null;
            }
            while(_asteroids.Count > 0)
            {
                _asteroids[0].Destroy();
                _asteroids.RemoveAt(0);
            }
            while (_missiles.Count > 0)
            {
                _missiles[0].Destroy();
                _missiles.RemoveAt(0);
            }

            //Begin model creation
            _droidOne = new Droid(this, "model/droid", "Droid1", PlayerIndex.One, BEPUutilities.Vector3.Zero, _soundCrashMetal, _soundEngine);
            _universe = new Universe(this, "model/universe", "Universe");
            
            _screenManager.CameraOneLookAt(_droidOne);
            _screenManager.SplitScreen = false;

            //Create asteroids with randomized position / velocity
            BEPUutilities.Vector3 asteroidPos;
            float posMagnitude;
            int asteroidModelIdx;
            BEPUutilities.Vector3 zeroVec = BEPUutilities.Vector3.Zero; //Create a zero vector instance for Vector3.Distance()
            for (int i = 0; i < ASTEROIDS_NUM; i++)
            {
                //Find a position inside the universe
                do
                {
                    asteroidPos = new BEPUutilities.Vector3
                    (
                        (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                        (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                        (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f
                    );

                    BEPUutilities.Vector3.Distance(ref asteroidPos, ref zeroVec, out posMagnitude);
                } while (posMagnitude <= 25);

                //Choose random asteroid style
                asteroidModelIdx = (_rand.Next(2) + 1);

                _asteroids.Add(new Asteroid(this, "model/asteroid_large_" + asteroidModelIdx, "Asteroid" + nextAsteroidIdx, asteroidPos, AsteroidSize.LARGE));
                nextAsteroidIdx++;
            }

            //Scoring init
            targetScore = (int)(7 * ASTEROIDS_NUM * ASTEROIDS_PCT_TO_DESTROY);
            currentScore = 0;
            _droidOne.LivesRemaining = SHIP_STARTING_LIVES;
        }

        /// <summary>
        /// Empties the simulation and configures it for multiplayer mode
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        private void ResetSimulationMultiplayer(GameTime gameTime)
        {
            _state = GameState.IN_GAME_MULTIPLAYER;
            if (_soundMenuMusicInstance.State == SoundState.Playing)
            {
                _soundMenuMusicInstance.Stop();
            }
            if (_soundIngameAmbientInstance.State != SoundState.Playing)
            {
                _soundIngameAmbientInstance.Play();
            }
            gameStartTime = gameTime.TotalGameTime;
            explosionParticles.SetSplitscreen(true);
            missileTrailParticles.SetSplitscreen(true);
            powerupParticles.SetSplitscreen(true);

            //Destroy all models
            if (_universe != null)
            {
                _universe.Destroy();
                _universe = null;
            }
            if (_droidOne != null)
            {
                _droidOne.Destroy();
                _droidOne = null;
            }
            if (_droidTwo != null)
            {
                _droidTwo.Destroy();
                _droidTwo = null;
            }
            if (_powerup != null)
            {
                _powerup.Destroy();
                _powerup = null;
            }
            while (_asteroids.Count > 0)
            {
                _asteroids[0].Destroy();
                _asteroids.RemoveAt(0);
            }
            while (_missiles.Count > 0)
            {
                _missiles[0].Destroy();
                _missiles.RemoveAt(0);
            }

            BEPUutilities.Vector3 position;
            float posMagnitude;
            BEPUutilities.Vector3 compareVec = BEPUutilities.Vector3.Zero; //Create a zero vector instance for Vector3.Distance()

            position = new BEPUutilities.Vector3
            (
                    (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                    (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                    (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f
            );

            BEPUutilities.Vector3.Distance(ref position, ref compareVec, out posMagnitude);

            _droidOne = new Droid(this, "model/droid", "Droid1", PlayerIndex.One, position, _soundCrashMetal, _soundEngine);

            position = new BEPUutilities.Vector3
            (
                    (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                    (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                    (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f
            );

            BEPUutilities.Vector3.Distance(ref position, ref compareVec, out posMagnitude);

            _droidTwo = new Droid(this, "model/droid", "Droid2", PlayerIndex.Two, position, _soundCrashMetal, _soundEngine);

            _universe = new Universe(this, "model/universe", "Universe");

            _screenManager.CameraOneLookAt(_droidOne);
            _screenManager.CameraTwoLookAt(_droidTwo);
            _screenManager.SplitScreen = true;


            //Create asteroids with randomized position / velocity
            int asteroidModelIdx;
            for (int i = 0; i < ASTEROIDS_NUM; i++)
            {
                //Find a position inside the universe
                while(true)
                {
                    position = new BEPUutilities.Vector3
                    (
                        (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                        (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                        (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f
                    );

                    compareVec = BEPUutilities.Vector3.Zero;

                    compareVec = MathConverter.Convert(_droidOne.World.Translation);
                    BEPUutilities.Vector3.Distance(ref position, ref compareVec, out posMagnitude);
                    if (posMagnitude <= 25)
                    {
                        //Too close to player 1: try again
                        continue;
                    }
                    compareVec = MathConverter.Convert(_droidTwo.World.Translation);
                    BEPUutilities.Vector3.Distance(ref position, ref compareVec, out posMagnitude);
                    if (posMagnitude <= 25)
                    {
                        //Too close to player 2: try again
                        continue;
                    }

                    //Valid position: exit the loop!
                    break;
                }

                //Choose random asteroid style
                asteroidModelIdx = (_rand.Next(2) + 1);

                _asteroids.Add(new Asteroid(this, "model/asteroid_large_" + asteroidModelIdx, "Asteroid" + nextAsteroidIdx, position, AsteroidSize.LARGE));
                nextAsteroidIdx++;
            }

            //Scoring init
            _droidOne.LivesRemaining = SHIP_STARTING_LIVES;
            _droidTwo.LivesRemaining = SHIP_STARTING_LIVES;
        }

        private void DrawScreenSinglePlayer(GameTime gameTime)
        {
            if (_droidOne.IsVisible)
            {
                _screenManager.Draw3D(_droidOne, gameTime);
            }

            _screenManager.Draw3D(_universe, gameTime);
            
            _screenManager.Draw3D(_powerup, gameTime);

            foreach (Missile missile in _missiles)
            {
                _screenManager.Draw3D(missile, gameTime);
            }
            foreach (Asteroid asteroid in _asteroids)
            {
                _screenManager.Draw3D(asteroid, gameTime);
            }

            //2D Drawing
            _screenManager.DrawHUDSinglePlayer(gameTime, gameStartTime, _droidOne.LivesRemaining, 100 * currentScore / (float)targetScore);
        }

        private void DrawScreenMultiplayer(GameTime gameTime)
        {
            for (int i = 0; i < 16; i++)
            {
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            }

            if (_droidOne.IsVisible)
            {
                _screenManager.Draw3D(_droidOne, gameTime);
            }
            if (_droidTwo.IsVisible)
            {
                _screenManager.Draw3D(_droidTwo, gameTime);
            }

            _screenManager.Draw3D(_powerup, gameTime);

            _screenManager.Draw3D(_universe, gameTime);

            foreach (Missile missile in _missiles)
            {
                _screenManager.Draw3D(missile, gameTime);
            }
            foreach (Asteroid asteroid in _asteroids)
            {
                _screenManager.Draw3D(asteroid, gameTime);
            }

            if (_droidOne.HasRadar)
            {
                _screenManager.Draw3DOverTop(_droidTwo, gameTime, true);
            }

            if (_droidTwo.HasRadar)
            {
                _screenManager.Draw3DOverTop(_droidOne, gameTime, false);
            }

            //2D drawing
            _screenManager.DrawHUDMultiplayer(gameTime, gameStartTime, _droidOne.LivesRemaining, _droidTwo.LivesRemaining);
        }

        private void GeneratePowerup()
        {
            //Choose random location
            BEPUutilities.Vector3 location = new BEPUutilities.Vector3
            (
                (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f,
                (float)(_rand.NextDouble() * UNIVERSE_RADIUS * .9f * 2) - UNIVERSE_RADIUS * .9f
            );

            //Choose random powerup
            int powerupNum = _rand.Next(2);

            if (powerupNum == 0)
            {
                _powerup = new Powerup(this, "model/radar", "Radar", BEPUutilities.Vector3.Zero);
            }
            else if (powerupNum == 1)
            {
                _powerup = new Powerup(this, "model/homing", "Seek", BEPUutilities.Vector3.Zero);
            }
        }

        public void RemovePowerup()
        {
            _powerup.Destroy();
            _powerup = null;
        }
    }
}
