using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Phantom;
using Phantom.Core;
using MusketLine.Core;
using Phantom.GameUI;

namespace MusketLine
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MusketGame : PhantomGame
    {

        public MusketGame()
            : base (1024, 768, "Order of Battle")
        {
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            XnaGame.IsMouseVisible = true;
            UILayer.Font = Content.Load<SpriteFont>("ui");
            PushState(new MainMenu());
        }

        protected override void LoadContent(Content content)
        {
        }
    }
}
