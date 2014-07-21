using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom;
using Phantom.Misc;
using MusketLine.Core;

namespace MusketLine.Mobs
{
    public enum SoldierState { Wait, Advance, Load, Aim, Fire, Flee, Command}
    public class Soldier : Entity
    {
        public static float LoadingTime = 4;
        public static float Size = 6;
        public static float Speed = 30;

        public static Color[] TeamColors = { Color.Gray, Color.Black };
        public static Color[] UnorderColors = { Color.Blue, Color.Green };
        public static Color[] BulletColor = { Color.Yellow, Color.OrangeRed};

        public int Team;
        protected SoldierState state;
        private float stateTimer;
        private bool loaded;
        private Raytracer raytracer;
        private Soldier targetSoldier;
        private Vector2 impactPostion;
        private float firing;
        public float Order = 0;
        internal bool Holding = false;

        public Soldier(Vector2 position, int team)
            : base(position)
        {
            this.Team = Math.Min(Math.Max(0, team), TeamColors.Length - 1);
            AddComponent(new Circle(Size));
            AddComponent(new Mover(new Vector2(0, 0), 1f, 0.1f, 0f));

            this.Orientation = -MathHelper.PiOver2;
            if (this.Team == 1)
                this.Orientation = MathHelper.PiOver2;

            SetState(SoldierState.Wait);
            this.loaded = false;
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            raytracer = parent.GetComponentByType<Raytracer>();
        }

        public override void Render(Phantom.Graphics.RenderInfo info)
        {
            base.Render(info);

            Color c = GetColor();


            if (Team == 0)
            {
                info.Canvas.FillColor = GetFillColor();
                info.Canvas.FillCircle(this.Position, Size);
                info.Canvas.StrokeColor = c;
                info.Canvas.LineWidth = 2;
                info.Canvas.StrokeCircle(this.Position, Size);
            }
            else
            {
                info.Canvas.FillColor = GetFillColor();
                info.Canvas.FillRect(this.Position, new Vector2(Size, Size), Orientation);
                info.Canvas.StrokeColor = c;
                info.Canvas.LineWidth = 2;
                info.Canvas.StrokeRect(this.Position, new Vector2(Size, Size), Orientation);
            }


            if (state == SoldierState.Aim || firing>0)
            {
                Vector2 p = this.Position + new Vector2(-this.Direction.Y, this.Direction.X) * Size * 0.8f;
                info.Canvas.StrokeLine(p - this.Direction * Size * 0.5f, p + this.Direction * Size * 2f);

                if (firing > 0 && impactPostion.X >0 && !float.IsNaN(impactPostion.X))
                {
                    info.Canvas.StrokeColor = TeamColors[Team];
                    info.Canvas.LineWidth = 2;
                    info.Canvas.StrokeLine(p + this.Direction * Size * 2f, impactPostion);
                    info.Canvas.FillColor = Color.OrangeRed;
                    info.Canvas.FillCircle(p + this.Direction * Size * 2f, 4);

                }
                firing -= info.Elapsed * 2;
            }
            else if (state != SoldierState.Load && state != SoldierState.Command)
            {
                Vector2 p = new Vector2(-this.Direction.Y, this.Direction.X) * Size;
                info.Canvas.StrokeColor = c;
                info.Canvas.LineWidth = 2;
                info.Canvas.StrokeLine(this.Position + p - this.Direction * Size*0.2f, this.Position - p*0.5f+ this.Direction * Size * 1.5f);
            }


        }

        protected Color GetColor()
        {
            Color c = TeamColors[Team];
            //if (Settings.VisualizeOrder)
            //    c = Color.Lerp(c, UnorderColors[Team], Order);
            return c;
        }

        protected Color GetFillColor()
        {
            Color c = TeamColors[Team];
            if (Settings.VisualizeOrder)
                c = Color.Lerp(Color.White, c, Order);
            return c;
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            
            stateTimer -= elapsed * (1 + (Settings.OrderAffectsRate ? Order : 0));

            if (stateTimer <= 0)
                SwitchState();

            if (Settings.OrderDissipates)
                Order *= Settings.OrderDissipation;


            DoState(elapsed);

            if (this.Position.Y < -Size || this.Position.Y > PhantomGame.Game.Height + Size)
            {
                Destroyed = true;
                if (this.state == SoldierState.Advance)
                    GetAncestor<PlayState>().GetComponentByType<Hud>().HandleMessage(Settings.MessageScore, this.Team);
            }
        }

        public override void AfterCollisionWith(Entity other, Phantom.Physics.CollisionData collision)
        {
            base.AfterCollisionWith(other, collision);
            if (Vector2.Dot(collision.Normal, this.Direction)>0)
                SetState(SoldierState.Wait);
        }

        private void DoState(float elapsed)
        {
            if (state == SoldierState.Advance || state == SoldierState.Flee)
            {
                this.Mover.Velocity = this.Mover.Velocity * 0.95f+ 0.05f * this.Direction * Speed;
            }
            else
            {
                this.Mover.Velocity *= 0.8f;
            }

            if (state == SoldierState.Aim)
            {
                if (this.targetSoldier != null)
                {
                    Vector2 delta = this.targetSoldier.Position - this.Position;
                    this.Orientation = (float)Math.Atan2(delta.Y, delta.X);
                }
            }
        }

        private void SwitchState()
        {
            //do the effects for ending a state
            switch (state)
            {
                case SoldierState.Fire:
                    SetState(SoldierState.Wait);
                    break;
                case SoldierState.Load:
                    loaded = true;
                    break;
                case SoldierState.Aim:
                    AcquireTarget();
                    if (targetSoldier != null)
                    {
                        if (Holding)
                            SetState(SoldierState.Aim);
                        else
                            SetState(SoldierState.Fire);
                        return;
                    }
                    break;
            }

            //determine new state
            if (Settings.Flee && Order < Settings.FleeThreshold)
                SetState(SoldierState.Flee);
            else if (!loaded)
                SetState(SoldierState.Load);
            else
            {
                AcquireTarget();

                if (targetSoldier != null)
                    SetState(SoldierState.Aim);
                else
                    SetState(SoldierState.Advance);
            }
        }

        private void AcquireTarget()
        {
            //check for enemy
            if (raytracer != null)
            {
                for (float a = 0; a < 0.4f; a += 0.05f)
                {
                    float hitDistance = 0;
                    Entity target = null;
                    if (raytracer.Cast(this.Position + this.Direction * Size * 1.5f, this.Direction.RotateBy(a) * 200, out target, out hitDistance, out impactPostion))
                    {
                        this.targetSoldier = target as Soldier;
                        if (hitDistance > 200 || this.targetSoldier.Team == this.Team || impactPostion.X <= 0 || float.IsNaN(impactPostion.X))
                            this.targetSoldier = null;
                    }
                    else
                    {
                        this.targetSoldier = null;
                    }

                    if (this.targetSoldier != null)
                        return;

                    if (raytracer.Cast(this.Position + this.Direction * Size * 1.5f, this.Direction.RotateBy(-a) * 200, out target, out hitDistance, out impactPostion))
                    {
                        this.targetSoldier = target as Soldier;
                        if (hitDistance > 200 || this.targetSoldier.Team == this.Team)
                            this.targetSoldier = null;
                    }
                    else
                    {
                        this.targetSoldier = null;
                    }

                    if (this.targetSoldier != null)
                        return;
                }
            }
        }

        protected void SetState(SoldierState soldierState)
        {
            if (this.Destroyed)
                return;

            this.state = soldierState;
            //activate state
            switch (state)
            {
                default:
                    stateTimer = (PhantomGame.Randy.NextFloat() + PhantomGame.Randy.NextFloat()) * 0.5f;
                    break;
                case SoldierState.Fire:
                    stateTimer = (PhantomGame.Randy.NextFloat() + PhantomGame.Randy.NextFloat()) * 0.5f;
                    ShootAtTarget();
                    SetState(SoldierState.Wait);
                    break;
                case SoldierState.Wait:
                    stateTimer = (PhantomGame.Randy.NextFloat() + PhantomGame.Randy.NextFloat()) * 0.5f;
                    break;
                case SoldierState.Aim:
                    stateTimer = 1 + (PhantomGame.Randy.NextFloat() + PhantomGame.Randy.NextFloat()) * 0.5f;
                    break;
                case SoldierState.Advance:
                    if (Team == 0)
                        Orientation = -MathHelper.PiOver2;
                    else
                        Orientation = MathHelper.PiOver2;

                    Orientation += (PhantomGame.Randy.NextFloat() - PhantomGame.Randy.NextFloat()) * 0.2f;
                    stateTimer = (PhantomGame.Randy.NextFloat() + PhantomGame.Randy.NextFloat()) * 0.5f;
                    break;
                case SoldierState.Flee:
                    if (Team == 0)
                        Orientation = MathHelper.PiOver2;
                    else
                        Orientation = -MathHelper.PiOver2;

                    Orientation += (PhantomGame.Randy.NextFloat() - PhantomGame.Randy.NextFloat()) * 0.2f;
                    stateTimer = (PhantomGame.Randy.NextFloat() + PhantomGame.Randy.NextFloat()) * 0.5f;
                    break;
                case SoldierState.Load:
                    loaded = true;
                    stateTimer = LoadingTime + (PhantomGame.Randy.NextFloat() + PhantomGame.Randy.NextFloat()) * 0.5f;
                    break;
            }
        }

        private void ShootAtTarget()
        {
            firing = 1f;
            loaded = false;
            if (targetSoldier != null)
            {
                targetSoldier.ShotAt();
                if (PhantomGame.Randy.NextFloat() < 0.1f)
                {
                    targetSoldier.Killed();
                    if (Settings.HitEnemyEffect)
                        KilledEnemy();
                }
            }
        }

        private void Killed()
        {
            this.Destroyed = true;
            //todo affect order with friends
            if (Settings.FriendKilledEffect)
            {
                foreach (Entity e in GetAncestor<EntityLayer>().GetEntitiesInRect(this.Position - new Vector2(Settings.FriendKilledEffectRadius, Settings.FriendKilledEffectRadius), this.Position + new Vector2(Settings.FriendKilledEffectRadius, Settings.FriendKilledEffectRadius), true))
                {
                    Soldier s = e as Soldier;
                    if (s != null && s.Team == this.Team && (s.Position - this.Position).LengthSquared() < Settings.FriendKilledEffectRadius * Settings.FriendKilledEffectRadius)
                    {
                        s.Order += Settings.FriendKilledEffectValue;
                    }
                }
            }
        }

        private void KilledEnemy()
        {
            //todo affect order with friends
            if (Settings.HitEnemyEffect)
            {
                foreach (Entity e in GetAncestor<EntityLayer>().GetEntitiesInRect(this.Position - new Vector2(Settings.HitEnemyEffectRadius, Settings.HitEnemyEffectRadius), this.Position + new Vector2(Settings.HitEnemyEffectRadius, Settings.HitEnemyEffectRadius), true))
                {
                    Soldier s = e as Soldier;
                    if (s != null && s.Team == this.Team && (s.Position - this.Position).LengthSquared() < Settings.HitEnemyEffectRadius * Settings.HitEnemyEffectRadius)
                    {
                        s.Order += Settings.HitEnemyEffectValue;
                    }
                }
            }
        }

        private void ShotAt()
        {
            if (Settings.ShotAtEffect)
                Order += Settings.ShotAtEffectValue;
        }


        internal void EndHolding()
        {
            Holding = false;
            if (state == SoldierState.Aim)
                this.stateTimer = Math.Min(this.stateTimer, 0.1f + 0.2f * (PhantomGame.Randy.NextFloat() + PhantomGame.Randy.NextFloat()));
        }
    }
}
