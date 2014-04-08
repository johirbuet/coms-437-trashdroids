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
using BEPUphysics.CollisionRuleManagement;

namespace Trashdroids
{
    public class Powerup : RenderableModel
    {
        private Sphere _collider;
        private float velocityMagnitude;

        private static Nullable<Microsoft.Xna.Framework.Matrix> _defaultRootTransform = null;
        
        public override Microsoft.Xna.Framework.Matrix World
        {
            get
            {
                return MathConverter.Convert(_collider.WorldTransform);
            }
        }

        public string Tag { get { return _collider.Tag.ToString(); } }
        public Sphere Collider { get { return _collider; } }

        public Powerup(TrashdroidsGame game, string model, string tag, BEPUutilities.Vector3 position)
            : base(game, model)
        {
            if (_defaultRootTransform == null)
            {
                _defaultRootTransform = _model.Root.Transform;
            }

            _model.Root.Transform = (Microsoft.Xna.Framework.Matrix)_defaultRootTransform;

            BEPUutilities.Vector3[] vertices;
            int[] indices;

            ModelDataExtractor.GetVerticesAndIndicesFromModel(_model, out vertices, out indices);

            _collider = new Sphere(position, 1, 1);
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
            _collider.LinearVelocity = new BEPUutilities.Vector3(
                (float)rand.NextDouble() * 6 * (rand.Next(2) == 0 ? -1 : 1),
                (float)rand.NextDouble() * 6 * (rand.Next(2) == 0 ? -1 : 1),
                (float)rand.NextDouble() * 6 * (rand.Next(2) == 0 ? -1 : 1));

            velocityMagnitude = _collider.LinearVelocity.Length();


            //Can't stop won't stop
            _collider.AngularDamping = 0;
            _collider.LinearDamping = 0;

            (game.Services.GetService(typeof(Space)) as Space).Add(_collider);
        }

        //Removes this object from simulation
        public void Destroy()
        {
            (_game.Services.GetService(typeof(Space)) as Space).Remove(_collider);
        }

        public void Update()
        {
            //Ensure velocity is retained
            _collider.LinearVelocity = BEPUutilities.Vector3.Normalize(_collider.LinearVelocity);
            _collider.LinearVelocity *= velocityMagnitude;

            _game.CreatePowerupEffect(World.Translation);
        }
    }
}
