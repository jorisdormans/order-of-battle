using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Physics;
using Phantom.Graphics.Components;
using Microsoft.Xna.Framework;
using Phantom;
using MusketLine.Mobs;
using Microsoft.Xna.Framework.Input;

namespace MusketLine.Core
{
    public class PlayState : GameState
    {
        private EntityLayer entities;

        public PlayState()
        {
            AddComponent(entities = new EntityLayer(new Renderer(1, Renderer.ViewportPolicy.Fit, Renderer.RenderOptions.Canvas), new TiledIntegrator(1, 32)));
            entities.AddComponent(new Raytracer());
            entities.AddComponent(new Background(Color.White));
            AddComponent(new Hud());

            CreateLines();
        }

        private void CreateLines()
        {
            for (float x = 20; x < PhantomGame.Game.Width - 30; x += Soldier.Size * 4)
            {
                entities.AddComponent(new Soldier(new Vector2(x, PhantomGame.Game.Height - 80), 0));
                entities.AddComponent(new Soldier(new Vector2(x + Soldier.Size * 2, PhantomGame.Game.Height - 100), 0));

                entities.AddComponent(new Soldier(new Vector2(x + Soldier.Size * 2, 100), 1));
                entities.AddComponent(new Soldier(new Vector2(x, 80), 1));
            }
            if (Settings.Generals)
            {
                List<int> controllers = new List<int>();
                if (GamePad.GetState(PlayerIndex.One).IsConnected)
                    controllers.Add(0);
                if (GamePad.GetState(PlayerIndex.Two).IsConnected)
                    controllers.Add(1);
                if (GamePad.GetState(PlayerIndex.Three).IsConnected)
                    controllers.Add(2);
                if (GamePad.GetState(PlayerIndex.Four).IsConnected)
                    controllers.Add(3);


                if (controllers.Count == 1)
                {
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.5f, PhantomGame.Game.Height - 60), 0, controllers[0]));
                }
                if (controllers.Count == 2)
                {
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.5f, PhantomGame.Game.Height - 60), 0, controllers[0]));
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.5f, 60), 1, controllers[1]));
                }
                if (controllers.Count == 3)
                {
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.25f, PhantomGame.Game.Height - 60), 0, controllers[0]));
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.5f, 60), 1, controllers[1]));
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.75f, PhantomGame.Game.Height - 60), 0, controllers[2]));
                }
                if (controllers.Count == 4)
                {
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.25f, PhantomGame.Game.Height - 60), 0, controllers[0]));
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.25f, 60), 1, controllers[1]));
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.75f, PhantomGame.Game.Height - 60), 0, controllers[2]));
                    entities.AddComponent(new General(new Vector2(PhantomGame.Game.Width * 0.75f, 60), 1, controllers[3]));
                }
            }
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                PhantomGame.Game.PopState();
        }
            
    }
}
