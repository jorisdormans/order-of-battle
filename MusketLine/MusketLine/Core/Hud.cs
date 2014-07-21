using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Phantom;
using Microsoft.Xna.Framework;
using MusketLine.Mobs;

namespace MusketLine.Core
{
    public class Hud : RenderLayer
    {
        private static SpriteFont font = PhantomGame.Game.Content.Load<SpriteFont>("hud");

        private int[] scores = {0, 0};
        public Hud()
            : base(new Renderer(1, Renderer.ViewportPolicy.Fit, Renderer.RenderOptions.Canvas))
        {

        }

        public override void Render(RenderInfo info)
        {
            base.Render(info);
            if (info != null)
            {
                DrawScore(info, scores[0].ToString(), new Vector2(PhantomGame.Game.Width - 50, PhantomGame.Game.Height - 50), Soldier.TeamColors[0]);
                DrawScore(info, scores[1].ToString(), new Vector2(50, 50), Soldier.TeamColors[1]);
            }
        }

        private void DrawScore(RenderInfo info, string score, Vector2 position, Color color)
        {
            Vector2 size = font.MeasureString(score);
            info.Batch.DrawString(font, score, position, color, 0, size * 0.5f, 1, SpriteEffects.None, 0);
        }

        public override void HandleMessage(Message message)
        {
            base.HandleMessage(message);
            if (message == Settings.MessageScore && message.Data is int)
                scores[(int)message.Data]++;
        }
    }
}
