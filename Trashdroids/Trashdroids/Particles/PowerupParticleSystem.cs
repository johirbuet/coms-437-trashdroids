#region File Description
//-----------------------------------------------------------------------------
// ExplosionParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//
// Modified by Matt Mayer
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Particles
{
    /// <summary>
    /// Custom particle system for creating the powerup "glow" particles.
    /// </summary>
    class PowerupParticleSystem : ParticleSystem
    {
        public PowerupParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            float scale = .1f;

            settings.TextureName = "particles/glow";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(.5);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 10 * scale * 5;
            settings.MaxHorizontalVelocity = 10 * scale * 5;

            settings.MinVerticalVelocity = -10 * scale * 5;
            settings.MaxVerticalVelocity = 10 * scale * 5;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 7 * scale;
            settings.MaxStartSize = 7 * scale;

            settings.MinEndSize = 7 * scale;
            settings.MaxEndSize = 14 * scale;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
