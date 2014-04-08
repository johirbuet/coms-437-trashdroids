using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trashdroids
{

    class ScreenManager
    {
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        private Viewport _defaultViewport;
        private Viewport _leftViewport;
        private Viewport _rightViewport;
        private Camera _cameraOne;
        private Camera _cameraTwo;

        //2D graphics
        private SpriteFont _basicFont;
        private SpriteFont _titleFont;
        private SpriteFont _tinyFont;
        private SpriteFont _introFont;
        private Texture2D _livesSprite;
        private Texture2D _reticleSprite;

        private TrashdroidsGame _game;

        private bool _splitscreen = false;

        public bool SplitScreen
        {
            get { return _splitscreen; }
            set { _splitscreen = value; }
        }


        public int ScreenWidth
        {
            get { return _graphics.PreferredBackBufferWidth; }
            set { _graphics.PreferredBackBufferWidth = value; }
        }

        public int ScreenHeight
        {
            get { return _graphics.PreferredBackBufferHeight; }
            set { _graphics.PreferredBackBufferHeight = value; }
        }

        public Camera CameraOne
        {
            get { return _cameraOne; }
            set { _cameraOne = value; }
        }

        public Camera CameraTwo
        {
            get { return _cameraTwo; }
            set { _cameraTwo = value; }
        }

        public bool IsFullScreen
        {
            get { return _graphics.IsFullScreen; }
            set { _graphics.IsFullScreen = value; }
        }

        public ScreenManager(TrashdroidsGame game, GraphicsDeviceManager graphics)
        {
            _graphics = graphics;
            _game = game;
        }

        //Must be called after graphics are initialized (call in Game.LoadContent())
        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;

            _basicFont = _game.Content.Load<SpriteFont>("font/BasicFont");
            _titleFont = _game.Content.Load<SpriteFont>("font/TitleFont");
            _tinyFont = _game.Content.Load<SpriteFont>("font/TinyFont");
            _introFont = _game.Content.Load<SpriteFont>("font/IntroFont");
            _livesSprite = _game.Content.Load<Texture2D>("life");
            _reticleSprite = _game.Content.Load<Texture2D>("reticle");

            RefreshViewports();

            _cameraOne = new Camera(graphicsDevice);
            _cameraTwo = new Camera(graphicsDevice);
        }

        public void CameraOneLookAt(ITargetable target)
        {
            _cameraOne.Target = target;
        }

        public void CameraTwoLookAt(ITargetable target)
        {
            _cameraTwo.Target = target;
        }

        public void FullScreenEnable()
        {
            IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.ApplyChanges();
        }

        public void FullScreenDisable()
        {
            IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = TrashdroidsGame.WINDOW_SIZE_WIDTH;
            _graphics.PreferredBackBufferHeight = TrashdroidsGame.WINDOW_SIZE_HEIGHT;
            _graphics.ApplyChanges();
        }

        public void FullScreenToggle()
        {
            if (IsFullScreen)
            {
                FullScreenDisable();
                RefreshViewports();
            }
            else
            {
                FullScreenEnable();
                RefreshViewports();
            }
        }

        private void RefreshViewports()
        {
            _defaultViewport = _graphicsDevice.Viewport;
            _leftViewport = _defaultViewport;
            _rightViewport = _defaultViewport;
            _leftViewport.Width = _leftViewport.Width / 2;
            _rightViewport.Width = _rightViewport.Width / 2;
            _rightViewport.X = _leftViewport.Width;
        }

        public void Draw3D(RenderableModel toDraw, GameTime gameTime, bool forceSingleScreen = false)
        {
            if (toDraw == null)
            {
                return;
            }

            _graphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            _graphicsDevice.BlendState = BlendState.Opaque;

            Viewport previousViewport = _graphicsDevice.Viewport;

            if (_splitscreen && forceSingleScreen == false)
            {
                _graphicsDevice.Viewport = _leftViewport;
                _cameraOne.Update(gameTime);
                toDraw.Draw(gameTime, _cameraOne);

                _graphicsDevice.Viewport = _rightViewport;
                _cameraTwo.Update(gameTime);
                toDraw.Draw(gameTime, _cameraTwo);
            }
            else
            {
                _graphicsDevice.Viewport = _defaultViewport;
                _cameraOne.Update(gameTime);
                toDraw.Draw(gameTime, _cameraOne);
            }

            _graphicsDevice.Viewport = previousViewport;
        }

        public void Draw3DOverTop(RenderableModel toDraw, GameTime gameTime, bool leftViewport)
        {
            _graphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = false };
            _graphicsDevice.BlendState = BlendState.Opaque;

            Viewport previousViewport = _graphicsDevice.Viewport;

            if (leftViewport)
            {
                _graphicsDevice.Viewport = _leftViewport;
                _cameraOne.Update(gameTime);
            }
            else
            {
                _graphicsDevice.Viewport = _rightViewport;
                _cameraTwo.Update(gameTime);
            }

            toDraw.Draw(gameTime, _cameraOne);
            


            _graphicsDevice.Viewport = previousViewport;
        }

        public void DrawHUDSinglePlayer(GameTime gameTime, TimeSpan gameStartTime, int livesRemaining, float completionPercentage)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_basicFont, "Destroy half of the asteroids!",
                    new Vector2(20, 20), Color.CornflowerBlue);
            _spriteBatch.DrawString(_basicFont, "Time Elapsed: " +
                    (gameTime.TotalGameTime - gameStartTime).Minutes.ToString("D2") + ":" +
                    (gameTime.TotalGameTime - gameStartTime).Seconds.ToString("D2") + ":" +
                    ((gameTime.TotalGameTime - gameStartTime).Milliseconds / 10).ToString("D2"),
                    new Vector2(20, 50), Color.CornflowerBlue);
            _spriteBatch.DrawString(_basicFont, "Completion Progress: " + completionPercentage.ToString("N2") + "%",
                    new Vector2(20, 80), Color.CornflowerBlue);

            _spriteBatch.DrawString(_basicFont, "Lives Remaining:",
                    new Vector2(500, 20), Color.CornflowerBlue);
            for (int i = 0; i < livesRemaining; i++)
            {
                _spriteBatch.Draw(_livesSprite, new Vector2(500 + (i * 50), 55), null, Color.White, 0f, Vector2.Zero, .05f, SpriteEffects.None, 1);
            }

            float reticleScale = 0.5f;
            _spriteBatch.Draw(
                _reticleSprite,
                new Vector2(_graphicsDevice.Viewport.Width / 2 - _reticleSprite.Width * reticleScale / 2, _graphicsDevice.Viewport.Height / 2 - _reticleSprite.Height * reticleScale / 2),
                null, Color.White, 0f, Vector2.Zero, reticleScale, SpriteEffects.None, 1);

            _spriteBatch.End();
        }

        public void DrawHUDMultiplayer(GameTime gameTime, TimeSpan gameStartTime, int livesRemaining_p1, int livesRemaining_p2)
        {
            Viewport previousViewport = _graphicsDevice.Viewport;
            
            _graphicsDevice.Viewport = _leftViewport;
            DrawHUDMultiplayerHelper(gameTime, gameStartTime, livesRemaining_p1);

            _graphicsDevice.Viewport = _rightViewport;
            DrawHUDMultiplayerHelper(gameTime, gameStartTime, livesRemaining_p2);

            //Restore viewport
            _graphicsDevice.Viewport = previousViewport;
        }
        private void DrawHUDMultiplayerHelper(GameTime gameTime, TimeSpan gameStartTime, int livesRemaining)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_basicFont, "Destroy your opponent!",
                    new Vector2(20, 20), Color.CornflowerBlue);
            _spriteBatch.DrawString(_basicFont, "Time Elapsed: " +
                    (gameTime.TotalGameTime - gameStartTime).Minutes.ToString("D2") + ":" +
                    (gameTime.TotalGameTime - gameStartTime).Seconds.ToString("D2") + ":" +
                    ((gameTime.TotalGameTime - gameStartTime).Milliseconds / 10).ToString("D2"),
                    new Vector2(20, 50), Color.CornflowerBlue);

            _spriteBatch.DrawString(_basicFont, "Lives:",
                    new Vector2(350, 20), Color.CornflowerBlue);
            for (int i = 0; i < livesRemaining; i++)
            {
                _spriteBatch.Draw(_livesSprite, new Vector2(350 + (i * 50), 55), null, Color.White, 0f, Vector2.Zero, .05f, SpriteEffects.None, 1);
            }

            float reticleScale = 0.5f;
            _spriteBatch.Draw(
                _reticleSprite, 
                new Vector2(_graphicsDevice.Viewport.Width / 2 - _reticleSprite.Width * reticleScale / 2, _graphicsDevice.Viewport.Height / 2 - _reticleSprite.Height * reticleScale / 2),
                null, Color.White, 0f, Vector2.Zero, reticleScale, SpriteEffects.None, 1);

            _spriteBatch.End();
        }
    }
}
