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

namespace Trashdroids
{
    public enum AsteroidSize {
        SMALL,
        MEDIUM,
        LARGE,
    };

    public class Asteroid : RenderableModel
    {
        private Sphere _collider;

        private AsteroidSize _size;

        private const float _extraScale = 1f;

        private static Nullable<Microsoft.Xna.Framework.Matrix> _defaultRootTransform = null;
        
        public override Microsoft.Xna.Framework.Matrix World
        {
            get
            {
                return MathConverter.Convert(_collider.WorldTransform);
            }
        }

        public string Tag { get { return _collider.Tag.ToString(); } }
        public AsteroidSize Size { get { return _size; } }
        public Sphere Collider { get { return _collider; } }

        public Asteroid(TrashdroidsGame game, string model, string tag, BEPUutilities.Vector3 position, AsteroidSize size)
            : base(game, model)
        {
            if (_defaultRootTransform == null)
            {
                _defaultRootTransform = _model.Root.Transform;
            }

            _model.Root.Transform = (Microsoft.Xna.Framework.Matrix)_defaultRootTransform  * Microsoft.Xna.Framework.Matrix.CreateScale(_extraScale);

            float colliderSize;
            _size = size;

            if (size == AsteroidSize.SMALL)
            {
                colliderSize = 1;
            }
            else if (size == AsteroidSize.MEDIUM)
            {
                colliderSize = 2;
            }
            else if (size == AsteroidSize.LARGE)
            {
                colliderSize = 4;
            }
            else
            {
                colliderSize = 0;
            }

            colliderSize *= _extraScale;

            BEPUutilities.Vector3[] vertices;
            int[] indices;

            ModelDataExtractor.GetVerticesAndIndicesFromModel(_model, out vertices, out indices);
            if (TrashdroidsGame.ASTEROIDS_MOVEABLE)
            {
                _collider = new Sphere(position, colliderSize, colliderSize);
            }
            else
            {
                _collider = new Sphere(position, colliderSize);
            }
            _collider.Tag = tag;
            _collider.CollisionInformation.Tag = tag;
            _collider.PositionUpdateMode = PositionUpdateMode.Continuous;

            Random rand = new Random();
            
            //Set randomized orientation
            _collider.OrientationMatrix = _collider.OrientationMatrix *
                BEPUutilities.Matrix3x3.CreateFromAxisAngle(_collider.WorldTransform.Up, (float)(rand.NextDouble() * 360)) *
                BEPUutilities.Matrix3x3.CreateFromAxisAngle(_collider.WorldTransform.Right, (float)(rand.NextDouble() * 360)) *
                BEPUutilities.Matrix3x3.CreateFromAxisAngle(_collider.WorldTransform.Forward, (float)(rand.NextDouble() * 360));

            //Set a randomized spin
            _collider.AngularVelocity = new BEPUutilities.Vector3(
                (float)rand.NextDouble() * 1 * (rand.Next(2) == 0 ? -1 : 1),
                (float)rand.NextDouble() * 1 * (rand.Next(2) == 0 ? -1 : 1),
                (float)rand.NextDouble() * 1 * (rand.Next(2) == 0 ? -1 : 1));

            //Set a randomized velocity
            if (TrashdroidsGame.ASTEROIDS_MOVEABLE)
            {
                _collider.LinearVelocity = new BEPUutilities.Vector3(
                    (float)rand.NextDouble() * 18 * (rand.Next(2) == 0 ? -1 : 1),
                    (float)rand.NextDouble() * 18 * (rand.Next(2) == 0 ? -1 : 1),
                    (float)rand.NextDouble() * 18 * (rand.Next(2) == 0 ? -1 : 1));
            }


            //Can't stop won't stop
            _collider.AngularDamping = 0;
            _collider.LinearDamping = 0;

            enableLighting();
            
            (game.Services.GetService(typeof(Space)) as Space).Add(_collider);
        }

        //Removes this object from simulation
        public void Destroy()
        {
            (_game.Services.GetService(typeof(Space)) as Space).Remove(_collider);
        }


        //Blows up this asteroid (and spawns smaller ones, if needed)
        public void Explode()
        {
            Random rand = new Random();
            Asteroid child1 = null;
            Asteroid child2 = null;

            int asteroidModelIdx;

            BEPUutilities.Vector3 thisLoc = MathConverter.Convert(World.Translation);
            BEPUutilities.Vector3 thisLinearVel = Collider.LinearVelocity;
            BEPUutilities.Vector3 thisAngularVel = Collider.AngularVelocity;

            (_game.Services.GetService(typeof(Space)) as Space).Remove(_collider);

            if (_size == AsteroidSize.LARGE)
            {
                //Choose random asteroid style for child 1
                asteroidModelIdx = (rand.Next(2) + 1);
                child1 = new Asteroid(
                    _game,
                    "model/asteroid_medium_" + asteroidModelIdx,
                    "Asteroid" + _game.nextAsteroidIdx,
                    thisLoc,
                    AsteroidSize.MEDIUM);
                _game.nextAsteroidIdx++;

                child1.Collider.LinearVelocity = thisLinearVel;

                _game.AddAsteroid(child1);

                //Choose random asteroid style for child 2
                asteroidModelIdx = (rand.Next(2) + 1);
                child2 = new Asteroid(
                    _game,
                    "model/asteroid_medium_" + asteroidModelIdx,
                    "Asteroid" + _game.nextAsteroidIdx,
                    thisLoc,
                    AsteroidSize.MEDIUM);
                _game.nextAsteroidIdx++;

                child2.Collider.LinearVelocity = thisLinearVel;

                _game.AddAsteroid(child2);
            }

            else if (_size == AsteroidSize.MEDIUM)
            {
                //Choose random asteroid style for child 1
                asteroidModelIdx = (rand.Next(2) + 1);
                child1 = new Asteroid(
                    _game,
                    "model/asteroid_small_" + asteroidModelIdx,
                    "Asteroid" + _game.nextAsteroidIdx,
                    thisLoc,
                    AsteroidSize.SMALL);
                _game.nextAsteroidIdx++;

                child1.Collider.AngularVelocity = thisAngularVel;
                child1.Collider.LinearVelocity = thisLinearVel;

                _game.AddAsteroid(child1);

                //Choose random asteroid style for child 2
                asteroidModelIdx = (rand.Next(2) + 1);
                child2 = new Asteroid(
                    _game,
                    "model/asteroid_small_" + asteroidModelIdx,
                    "Asteroid" + _game.nextAsteroidIdx,
                    thisLoc,
                    AsteroidSize.SMALL);
                _game.nextAsteroidIdx++;

                child2.Collider.AngularVelocity = thisAngularVel;
                child2.Collider.LinearVelocity = thisLinearVel;

                _game.AddAsteroid(child2);
            }

            _game.CreateMissileTrailEffect(MathConverter.Convert(thisLoc));
        }
    }
}
