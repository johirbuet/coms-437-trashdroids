using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ConversionHelper;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUutilities;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.BroadPhaseEntries;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysicsDrawer.Models;
using BEPUphysics.EntityStateManagement;
using BEPUphysics.PositionUpdating;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework.Audio;

namespace Trashdroids
{
    public class Droid : RenderableModel
    {
        #region Private variables

        private Sphere      _collider;
        private PlayerIndex _playerIndex;
        private float       _lastMissileLaunchTime = 0;
        private float       _invincibilityTime     = 0;
        private bool        _isVisible             = false;
        private int         _livesRemaining;
        private bool        _hasRadar              = false;
        private float       _heatSeekingStrength   = 0.005f;
        private SoundEffect _crashSound;
        private SoundEffectInstance _engineSound;

        private static Nullable<Microsoft.Xna.Framework.Matrix> _defaultRootTransform = null;

        #endregion

        #region Accessors

        public bool IsVisible { get { return _isVisible; } }

        public bool HasRadar { get { return _hasRadar; } }
        
        public float HeatSeekingStrength { get { return _heatSeekingStrength; } }

        public int LivesRemaining { get { return _livesRemaining; } set { _livesRemaining = value; } }

        public override Microsoft.Xna.Framework.Matrix World
        {
            get
            {
                return MathConverter.Convert(_collider.WorldTransform);
            }
        }

        public Microsoft.Xna.Framework.Quaternion Orientation
        {
            get
            {
                return MathConverter.Convert(_collider.Orientation);
            }
        }

        #endregion

        public Droid(TrashdroidsGame game, string model, string tag, PlayerIndex playerIndex, BEPUutilities.Vector3 position, SoundEffect crashSound, SoundEffect engineSound)
            : base(game, model)
        {
            
            _playerIndex = playerIndex;
            _crashSound  = crashSound;
            _engineSound = engineSound.CreateInstance();
            _engineSound.IsLooped = true;

            BEPUutilities.Vector3[] vertices;
            int[] indices;

            ModelDataExtractor.GetVerticesAndIndicesFromModel(_model, out vertices, out indices);

            
            if (_defaultRootTransform == null)
            {
                _defaultRootTransform = _model.Root.Transform;
            }

            //Scale model
            _model.Root.Transform = (Microsoft.Xna.Framework.Matrix)_defaultRootTransform * Microsoft.Xna.Framework.Matrix.CreateScale(1 / 100f);

            _collider = new Sphere(position, 0.8f, 10);
            _collider.Tag = tag;
            _collider.CollisionInformation.Tag = tag;
           
            _collider.PositionUpdateMode = PositionUpdateMode.Continuous;

            _collider.CollisionInformation.Events.InitialCollisionDetected += Events_InitialCollisionDetected;

            (game.Services.GetService(typeof(Space)) as Space).Add(_collider);
        }

        void Events_InitialCollisionDetected(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if((other.Tag.ToString().StartsWith("Missile") || _game.GameState != GameState.IN_GAME_MULTIPLAYER))
            {
                if (_invincibilityTime <= 0)
                {
                    _invincibilityTime += 3;
                    _livesRemaining--;
                    _crashSound.Play();
                }
            }
            else if (other.Tag.ToString().StartsWith("Radar"))
            {
                _hasRadar = true;
                _game.RemovePowerup();
            }
            else if (other.Tag.ToString().StartsWith("Seek"))
            {
                _heatSeekingStrength += .01f;
                _game.RemovePowerup();
            }
        }

        public void Update(GameTime gameTime)
        {
            BEPUutilities.Vector3 linearImpulse = BEPUutilities.Vector3.Zero;
            BEPUutilities.Vector3 angularImpulse = BEPUutilities.Vector3.Zero;

            if (_invincibilityTime > 0)
            {
                _invincibilityTime -= 0.016666666f;
            }
            if (_invincibilityTime % .25 > .125)
            {
                _isVisible = false;
            }
            else
            {
                _isVisible = true;
            }

            #region Keyboard Controls (Player 1 Only)
            if (_playerIndex == PlayerIndex.One)
            {
                if (_game.KeyIsDown(Keys.W))
                {
                    linearImpulse += BEPUutilities.Vector3.Forward * 5;
                    _game.CreateMissileTrailEffect(World.Translation + World.Up * .45f + World.Left * .4f + World.Backward * .3f);
                    _game.CreateMissileTrailEffect(World.Translation + World.Up * .45f + World.Left * .4f + World.Backward * .5f);
                    _game.CreateMissileTrailEffect(World.Translation + World.Up * .45f + World.Right * .4f + World.Backward * .3f);
                    _game.CreateMissileTrailEffect(World.Translation + World.Up * .45f + World.Right * .4f + World.Backward * .5f);
                    if (_engineSound.State != SoundState.Playing)
                    {
                        _engineSound.Play();
                    }
                }
                else
                {
                    if (_engineSound.State == SoundState.Playing)
                    {
                        _engineSound.Stop();
                    }
                }

                if (_game.KeyIsDown(Keys.A))
                {
                    linearImpulse += BEPUutilities.Vector3.Left;
                }
                if (_game.KeyIsDown(Keys.S))
                {
                    linearImpulse += BEPUutilities.Vector3.Backward * 5;
                }
                if (_game.KeyIsDown(Keys.D))
                {
                    linearImpulse += BEPUutilities.Vector3.Right;
                }

                if (_game.KeyIsDown(Keys.Down))
                {
                    angularImpulse += BEPUutilities.Vector3.Right;
                }
                if (_game.KeyIsDown(Keys.Up))
                {
                    angularImpulse += BEPUutilities.Vector3.Left;
                }
                if (_game.KeyIsDown(Keys.Left))
                {
                    angularImpulse += BEPUutilities.Vector3.Backward;
                }
                if (_game.KeyIsDown(Keys.Right))
                {
                    angularImpulse += BEPUutilities.Vector3.Forward;
                }
                if (_game.KeyIsDown(Keys.Q))
                {
                    angularImpulse += BEPUutilities.Vector3.Up;
                }
                if (_game.KeyIsDown(Keys.E))
                {
                    angularImpulse += BEPUutilities.Vector3.Down;
                }
                if (_game.KeyIsDown(Keys.Space))
                {
                    if (gameTime.TotalGameTime.TotalSeconds > _lastMissileLaunchTime + 1.0f / (TrashdroidsGame.MISSILES_PER_SEC * (TrashdroidsGame.MISSILE_STREAM_MODE ? 3 : 1)))
                    {
                        _game.spawnMissile(this);
                        _lastMissileLaunchTime = (float)gameTime.TotalGameTime.TotalSeconds;
                    }
                }
            }
            #endregion

            #region Gamepad Controls
            //Left stick up/down
            linearImpulse += (BEPUutilities.Vector3.Forward) * 5 * (_game.ThumbstickState(true, _playerIndex).Y);

            //Left stick left/right
            linearImpulse += (BEPUutilities.Vector3.Right) * (_game.ThumbstickState(true, _playerIndex).X);

            //Right stick up/down
            angularImpulse += BEPUutilities.Vector3.Left * (_game.ThumbstickState(false, _playerIndex).Y);

            //Right stick left/right
            angularImpulse += BEPUutilities.Vector3.Forward * (_game.ThumbstickState(false, _playerIndex).X);

            if (_game.KeyIsDown(Buttons.LeftShoulder, _playerIndex))
            {
                angularImpulse += BEPUutilities.Vector3.Up;
            }
            if (_game.KeyIsDown(Buttons.RightShoulder, _playerIndex))
            {
                angularImpulse += BEPUutilities.Vector3.Down;
            }
            if (_game.TriggerState(false, _playerIndex) > 0.2f)
            {
                if (gameTime.TotalGameTime.TotalSeconds > _lastMissileLaunchTime + 1.0f / (TrashdroidsGame.MISSILES_PER_SEC * (TrashdroidsGame.MISSILE_STREAM_MODE ? 3 : 1)))
                {
                    _game.spawnMissile(this);
                    _lastMissileLaunchTime = (float)gameTime.TotalGameTime.TotalSeconds;
                }
            }
            #endregion

            //Make movements intrinsic
            angularImpulse = MathConverter.Convert(Microsoft.Xna.Framework.Vector3.Transform(
                MathConverter.Convert(angularImpulse), 
                MathConverter.Convert(_collider.OrientationMatrix)));
            linearImpulse = MathConverter.Convert(Microsoft.Xna.Framework.Vector3.Transform(
                MathConverter.Convert(linearImpulse),
                MathConverter.Convert(_collider.OrientationMatrix)));

            //Apply drag
            _collider.LinearMomentum *= TrashdroidsGame.SHIP_LINEAR_DRAG_COEFF;
            _collider.AngularMomentum *= TrashdroidsGame.SHIP_ANGULAR_DRAG_COEFF;
            if ( (_game.KeyIsDown(Keys.LeftShift) && _playerIndex == PlayerIndex.One) ||
                 (_game.TriggerState(true, _playerIndex) > .2f)
                )
            {
                _collider.AngularMomentum *= TrashdroidsGame.SHIP_ANGULAR_BRAKING_COEFF;
                _collider.LinearMomentum *= TrashdroidsGame.SHIP_LINEAR_BRAKING_COEFF;
            }

            //Apply force based on user input
            _collider.LinearMomentum += linearImpulse * TrashdroidsGame.SHIP_LINEAR_ACCEL_COEFF;
            _collider.AngularMomentum += angularImpulse * TrashdroidsGame.SHIP_ANGULAR_ACCEL_COEFF;
        }

        public void Destroy()
        {
            (_game.Services.GetService(typeof(Space)) as Space).Remove(_collider);
        }
    }
}
