using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Phantom.Core;
using MusketLine.Core;

namespace MusketLine.Mobs
{
    public class General : Soldier
    {
        public static float Range = 100;
        public static float Effect = 0.2f;
        public static float EffectFrequency = 4;
        public static float GeneralSpeed = Soldier.Speed * 2f;
        private float timer;
        private int controller;
        private List<Soldier> holding;

        public General(Vector2 position, int team, int controller)
            : base(position, team)
        {
            SetState(SoldierState.Command);
            AddComponent(new Phantom.Physics.Components.BounceAgainstWorldBoundaries(Soldier.Size, 0));
            timer = 1;
            this.controller = controller;
            this.holding = new List<Soldier>();
        }

        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            base.Render(info);
            Color c = GetColor();
            info.Canvas.LineWidth = 2;
            info.Canvas.StrokeColor = c;
            if (Team == 0)
                info.Canvas.StrokeCircle(this.Position, Soldier.Size + 3);
            else
                info.Canvas.StrokeRect(this.Position, new Vector2(Soldier.Size + 3, Soldier.Size + 3), Orientation);
        }

        public override void Update(float elapsed)
        {
            GamePadState state = GamePad.GetState((PlayerIndex)controller);
            Mover.Velocity = state.ThumbSticks.Right * GeneralSpeed;
            if (state.ThumbSticks.Right.LengthSquared()==0)
                Mover.Velocity = state.ThumbSticks.Left * GeneralSpeed;
            Mover.Velocity.Y *= -1;
            timer -= elapsed * EffectFrequency;

            if (Settings.HoldCommand)
            {
                if (state.Buttons.A == ButtonState.Pressed && timer<0)
                {
                    DoHold();
                }
                else if (state.Buttons.A == ButtonState.Released)
                {
                    foreach (Soldier s in holding)
                    {
                        s.EndHolding();
                    }
                    holding.Clear();
                }
            }

            if (timer < 0)
            {
                timer += 1;
                if (Settings.GeneralsIncreaseOrder)
                    DoEffect();
            }
        }

        private void DoEffect()
        {
            foreach (Entity e in GetAncestor<EntityLayer>().GetEntitiesInRect(this.Position - new Vector2(Range, Range), this.Position + new Vector2(Range, Range), true))
            {
                Soldier s = e as Soldier;
                if (s != null && s.Team == this.Team && (s.Position - this.Position).LengthSquared() < Range * Range)
                {
                    s.Order += Effect;
                    s.Order = Math.Min(s.Order, 1);
                }
            }
        }
        private void DoHold()
        {
            foreach (Entity e in GetAncestor<EntityLayer>().GetEntitiesInRect(this.Position - new Vector2(Range, Range), this.Position + new Vector2(Range, Range), true))
            {
                Soldier s = e as Soldier;
                if (s is General)
                    s = null;
                if (s != null && s.Team == this.Team && (s.Position - this.Position).LengthSquared() < Range * Range)
                {
                    if (!holding.Contains(s))
                        holding.Add(s);
                }
            }
            foreach (Soldier s in holding)
            {
                s.Holding = true;
            }
        }
    }
}
