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
using Particles;

namespace Trashdroids
{
    public class Missile : RenderableModel
    {
        private Capsule _collider;
        private Droid _owner;
        private Droid _target;

        private float _velocity = 20f;

        private static Nullable<Microsoft.Xna.Framework.Matrix> _defaultRootTransform = null;
        
        public override Microsoft.Xna.Framework.Matrix World
        {
            get
            {
                return MathConverter.Convert(_collider.WorldTransform);
            }
        }

        public Missile(TrashdroidsGame game, string model, string tag, Droid owner, Droid target, bool isLeft)
            : base(game, model)
        {
            _owner = owner;
            _target = target;

            BEPUutilities.Vector3[] vertices;
            int[] indices;


            if (_defaultRootTransform == null)
            {
                _defaultRootTransform = _model.Root.Transform;
            }
            _model.Root.Transform = (Microsoft.Xna.Framework.Matrix)_defaultRootTransform * Microsoft.Xna.Framework.Matrix.CreateScale(1 / 100f);

            

            ModelDataExtractor.GetVerticesAndIndicesFromModel(_model, out vertices, out indices);

            MotionState initialMotionState     = new MotionState();
            initialMotionState.LinearVelocity  = MathConverter.Convert(owner.World.Forward*_velocity);
            initialMotionState.Position        = MathConverter.Convert(owner.World.Translation + owner.World.Forward*2);
            initialMotionState.Position += MathConverter.Convert(owner.World.Up/2.35f);
            initialMotionState.Position += MathConverter.Convert((isLeft?owner.World.Left:owner.World.Right)/3f);

            initialMotionState.AngularVelocity = BEPUutilities.Vector3.Zero;
            initialMotionState.Orientation = MathConverter.Convert(Microsoft.Xna.Framework.Quaternion.Concatenate(
                    owner.Orientation,
                    Microsoft.Xna.Framework.Quaternion.CreateFromAxisAngle(owner.World.Left, (float)Math.PI / 2)
            ));

            _collider = new Capsule(initialMotionState, .75f, .05f, 1f);
            _collider.Tag = tag;
            _collider.CollisionInformation.Tag = tag;
            _collider.PositionUpdateMode = PositionUpdateMode.Continuous;

            _collider.AngularDamping = 0;
            _collider.LinearDamping = .8f;

            _collider.LinearMomentum = _collider.WorldTransform.Forward * 200;



            _collider.CollisionInformation.Events.InitialCollisionDetected += Events_InitialCollisionDetected;

            (game.Services.GetService(typeof(Space)) as Space).Add(_collider);
        }

        void Events_InitialCollisionDetected(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if (!(other.Tag.ToString().StartsWith("Missile")))
            {
                (_game.Services.GetService(typeof(Space)) as Space).Remove(_collider);
                _game.DetonateMissile(this);
            }
            if (other.Tag.ToString().StartsWith("Asteroid"))
            {
                if (TrashdroidsGame.ASTEROIDS_DESTRUCTABLE)
                {
                    _game.AsteroidExplode(other.Tag.ToString());
                }
            }    
        }

        //Removes this model from simulation
        public void Destroy()
        {
            (_game.Services.GetService(typeof(Space)) as Space).Remove(_collider);
        }

        public void Update(GameTime gameTime)
        {
            if (_game.GameState == GameState.IN_GAME_MULTIPLAYER)
            {
                //Homing!
                BEPUutilities.Vector3 targetPos;

                if (_target != null)
                {
                    targetPos = MathConverter.Convert(
                        Microsoft.Xna.Framework.Vector3.Transform(_target.World.Translation - World.Translation, Microsoft.Xna.Framework.Matrix.Invert(MathConverter.Convert(_collider.OrientationMatrix))));
                }
                else
                {
                    targetPos = MathConverter.Convert(
                        Microsoft.Xna.Framework.Vector3.Transform(-World.Translation, Microsoft.Xna.Framework.Matrix.Invert(MathConverter.Convert(_collider.WorldTransform))));
                }

                _collider.AngularMomentum = BEPUutilities.Vector3.Zero;

                if (targetPos.X < 0)
                {
                    _collider.AngularMomentum -= MathConverter.Convert(this.World.Forward * _owner.HeatSeekingStrength);
                }
                else if (targetPos.X > 0)
                {
                    _collider.AngularMomentum += MathConverter.Convert(this.World.Forward * _owner.HeatSeekingStrength);
                }

                if (targetPos.Z < 0)
                {
                    _collider.AngularMomentum -= MathConverter.Convert(this.World.Right * _owner.HeatSeekingStrength);
                }
                else if (targetPos.Z > 0)
                {
                    _collider.AngularMomentum += MathConverter.Convert(this.World.Right * _owner.HeatSeekingStrength);
                }                
            }

            _game.CreateMissileTrailEffect(World.Translation);
            _collider.LinearVelocity = MathConverter.Convert(this.World.Up * Math.Max(_velocity, _collider.LinearVelocity.Length()));
        }
    }
}
