using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.GameUI;
using Phantom.Graphics;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom;
using MusketLine.Mobs;

namespace MusketLine.Core
{
    public class SettingsMenu : GameState
    {
        private UILayer uiLayer;
        private ToggleButton btnGenerals;
        private ToggleButton btnGenralAffectsOrder;
        private ToggleButton btnAffectsRate;
        private ToggleButton btnDissipates;
        private ToggleButton btnHitsIncrease;
        private ToggleButton btnVisualizeOrder;
        private ToggleButton btnHold;
        private Slider sldShotAtEffect;
        private Slider sldKillEffect;
        private Slider sldKillEffectRadius;
        private Slider sldHitEffect;
        private Slider sldHitEffectRadius;
        private ToggleButton btnShot;
        private Slider sldFlee;

        public SettingsMenu()
        {
            this.AddComponent(uiLayer = new UILayer(new Renderer(1, Renderer.ViewportPolicy.Fit, Renderer.RenderOptions.Canvas), 1));
            Vector2 position = new Vector2(PhantomGame.Game.Width*0.25f, 50);
            uiLayer.AddComponent(btnGenerals = new ToggleButton("Generals", "Generals:", position, new OABB(new Vector2(150, 20)), Settings.Generals ? 1 : 0, "Off", "On"));
            position.Y += 50;
            uiLayer.AddComponent(btnGenralAffectsOrder = new ToggleButton("GeneralAffectsOrder", "Generals Increase Order:", position, new OABB(new Vector2(150, 20)), Settings.GeneralsIncreaseOrder ? 1 : 0, "Off", "On"));
            position.Y += 50;
            uiLayer.AddComponent(btnHold = new ToggleButton("HoldCommand", "Hold Command:", position, new OABB(new Vector2(150, 20)), Settings.HoldCommand ? 1 : 0, "Off", "On"));
            position.Y += 50;
            uiLayer.AddComponent(btnAffectsRate = new ToggleButton("AffectsRate", "Order Affects Rate:", position, new OABB(new Vector2(150, 20)), Settings.OrderAffectsRate ? 1 : 0, "Off", "On"));
            position.Y += 50;
            uiLayer.AddComponent(btnDissipates = new ToggleButton("Dissipates", "Order Dissipates:", position, new OABB(new Vector2(150, 20)), Settings.OrderDissipates ? 1 : 0, "Off", "On"));
            position.Y += 50;
            uiLayer.AddComponent(btnShot = new ToggleButton("ShotAtEffect", "Shot At Effect:", position, new OABB(new Vector2(150, 20)), Settings.ShotAtEffect ? 1 : 0, "Off", "On"));
            position.Y += 50;
            uiLayer.AddComponent(btnKilled = new ToggleButton("KillEffect", "Friend Killed Effect:", position, new OABB(new Vector2(150, 20)), Settings.FriendKilledEffect ? 1 : 0, "Off", "On"));
            position.Y += 50;
            uiLayer.AddComponent(btnHitsIncrease = new ToggleButton("HitEffect", "Hit Enemy Effect:", position, new OABB(new Vector2(150, 20)), Settings.HitEnemyEffect ? 1 : 0, "Off", "On"));
            position.Y += 50;
            uiLayer.AddComponent(btnVisualizeOrder = new ToggleButton("VisualizeOrder", "Visualize Order:", position, new OABB(new Vector2(150, 20)), Settings.VisualizeOrder ? 1 : 0, "Off", "On"));
            position.Y += 50;


            position = new Vector2(PhantomGame.Game.Width * 0.75f, 50);
            uiLayer.AddComponent(sldShotAtEffect = new Slider("ShotAtEffect", "Shot At Effect", position, new OABB(new Vector2(150, 30)), 0, 100, Settings.ShotAtEffectValue * -100));
            position.Y += 50;
            uiLayer.AddComponent(sldKillEffect = new Slider("KillEffect", "Friend Killed Effect", position, new OABB(new Vector2(150, 30)), 0, 100, Settings.FriendKilledEffectValue * -100));
            position.Y += 50;
            uiLayer.AddComponent(sldKillEffectRadius = new Slider("KillEffectRadius", "Friend Killed Effect Radius", position, new OABB(new Vector2(150, 30)), 0, 200, Settings.FriendKilledEffectRadius));
            position.Y += 50;
            uiLayer.AddComponent(sldHitEffect = new Slider("HitEffect", "Hit Enemy Effect", position, new OABB(new Vector2(150, 30)), 0, 100, Settings.HitEnemyEffectValue * 100));
            position.Y += 50;
            uiLayer.AddComponent(sldHitEffectRadius = new Slider("HitEffectRadius", "Hit Enemy Effect Radius", position, new OABB(new Vector2(150, 30)), 0, 200, Settings.HitEnemyEffectRadius));
            position.Y += 50;
            uiLayer.AddComponent(sldFlee = new Slider("FleeThreshold", "Flee Threshold", position, new OABB(new Vector2(150, 30)), -100, 100, Settings.FleeThreshold * 100));
            position.Y += 50;

            position.Y += 50;
            uiLayer.AddComponent(new Button("Back", "Done", position, new OABB(new Vector2(150, 20))));
            uiLayer.AddComponent(new UIMouseHandler());

            //uiLayer.AddComponent(new UIGamePadHandler(PlayerIndex.One));
            //uiLayer.ConnectControls(UILayer.Ordering.TopToBottom);

        }

        public override void HandleMessage(Message message)
        {
            base.HandleMessage(message);
            if (message.Type == Messages.UIElementClicked && message.Data is UIElement)
            {
                if (((UIElement)message.Data).Name == "Back")
                {
                    //save settings
                    Settings.Generals = btnGenerals.Option == 1;
                    Settings.GeneralsIncreaseOrder = btnGenralAffectsOrder.Option == 1;
                    Settings.OrderAffectsRate = btnAffectsRate.Option == 1;
                    Settings.OrderDissipates = btnDissipates.Option == 1;
                    Settings.ShotAtEffect = btnShot.Option == 1;
                    Settings.HitEnemyEffect = btnHitsIncrease.Option == 1;
                    Settings.FriendKilledEffect = btnKilled.Option == 1;
                    Settings.VisualizeOrder = btnVisualizeOrder.Option == 1;
                    Settings.HoldCommand = btnHold.Option == 1;

                    Settings.ShotAtEffectValue = sldShotAtEffect.GetValue() * -0.01f;
                    Settings.FriendKilledEffectValue = sldKillEffect.GetValue() * -0.01f;
                    Settings.FriendKilledEffectRadius = sldKillEffectRadius.GetValue();
                    Settings.HitEnemyEffectValue = sldHitEffect.GetValue() * 0.01f;
                    Settings.HitEnemyEffectRadius = sldHitEffectRadius.GetValue();
                    Settings.FleeThreshold = sldFlee.GetValue() * 0.01f;


                    PhantomGame.Game.PopState();
                }
            }
        }

        public ToggleButton btnKilled { get; set; }
    }
}
