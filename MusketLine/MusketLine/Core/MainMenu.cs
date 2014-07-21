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

namespace MusketLine.Core
{
    public class MainMenu : GameState
    {
        private UILayer uiLayer;

        public MainMenu()
        {
            this.AddComponent(uiLayer = new UILayer(new Renderer(1, Renderer.ViewportPolicy.Fit, Renderer.RenderOptions.Canvas), 1));
            Vector2 position = new Vector2(PhantomGame.Game.Width*0.5f, 150);
            uiLayer.AddComponent(new Button("Play", "Play", position, new OABB(new Vector2(80, 20))));
            position.Y += 50;

            uiLayer.AddComponent(new Button("Settings", "Settings", position, new OABB(new Vector2(80, 20))));
            uiLayer.AddComponent(new UIMouseHandler());
            //uiLayer.AddComponent(new UIGamePadHandler(PlayerIndex.One));
            //uiLayer.ConnectControls(UILayer.Ordering.TopToBottom);
        }

        public override void HandleMessage(Message message)
        {
            base.HandleMessage(message);
            if (message.Type == Messages.UIElementClicked && message.Data is UIElement)
            {
                UIElement element = (message.Data as UIElement);
                if (element.Name == "Settings")
                {
                    PhantomGame.Game.PushState(new SettingsMenu());
                }
                if (element.Name == "Play")
                {
                    PhantomGame.Game.PushState(new PlayState());
                }
            }
        }
    }
}
