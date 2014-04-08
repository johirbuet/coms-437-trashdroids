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
using BEPUphysics.Materials;
using BEPUphysics.DataStructures;
using BEPUphysics.PositionUpdating;

namespace Trashdroids
{

    public class Universe: RenderableModel
    {
        private MobileMesh _collider;

        private static Nullable<Microsoft.Xna.Framework.Matrix> _defaultRootTransform = null;

        public override Microsoft.Xna.Framework.Matrix World
        {
            get
            {
                return MathConverter.Convert(_collider.WorldTransform);
            }
        }

        /**
         * Creates a new Universe of the specified radius, centered at
         * position (0, 0, 0).
         */
        public Universe(TrashdroidsGame game, string model, string tag)
            : base(game, model)
        {
            BEPUutilities.Vector3[] vertices;
            int[] indices;

            if (_defaultRootTransform == null)
            {
                _defaultRootTransform = _model.Root.Transform;
            }

            _model.Root.Transform = (Microsoft.Xna.Framework.Matrix)_defaultRootTransform * Microsoft.Xna.Framework.Matrix.CreateScale(TrashdroidsGame.UNIVERSE_RADIUS);
            ModelDataExtractor.GetVerticesAndIndicesFromModel(_model, out vertices, out indices);

            _collider = new MobileMesh(vertices, indices, AffineTransform.Identity, MobileMeshSolidity.DoubleSided);
            _collider.PositionUpdateMode = PositionUpdateMode.Continuous;
            _collider.Tag = tag;
            _collider.CollisionInformation.Tag = tag;

            //Giving the collider some mass results in some interesting gameplay...
            //_collider.Mass = 10;

            (game.Services.GetService(typeof(Space)) as Space).Add(_collider);
        }

        public void Destroy()
        {
            (_game.Services.GetService(typeof(Space)) as Space).Remove(_collider);
        }
    }
}
