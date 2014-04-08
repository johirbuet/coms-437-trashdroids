using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//TODO: Credit author (Taken from greg's)
namespace Trashdroids
{
    public class RenderableModel : ITargetable
    {
        protected Model _model;
        protected Matrix _world;

        protected TrashdroidsGame _game;

        public virtual Matrix World
        {
            get { return _world; }
        }
        
        public RenderableModel(TrashdroidsGame game, string model)
        {
            _model = game.Content.Load<Model>(model);
            _game = game;
        }

        public void enableLighting()
        {
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect b in mesh.Effects)
                {
                    b.AmbientLightColor = new Vector3(.5f, .5f, .5f);
                    b.LightingEnabled = true;
                    b.PreferPerPixelLighting = true;
                }
            }
        }

        public virtual void Draw(GameTime gameTime, Camera camera)
        {
            Matrix[] boneTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect b in mesh.Effects)
                {
                    b.World = boneTransforms[mesh.ParentBone.Index] * World;
                    b.View = camera.View;
                    b.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }
    }
}
