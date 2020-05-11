using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minecraft
{
    class UICanvasIngame : UICanvas
    {
        public bool IsTyping { get; private set; }

        private readonly Game game;
        private readonly UIText chatbox;
        private readonly UIText inputField;

        private List<string> messageHistory = new List<string>();
        private DateTime lastTimeChatVisible = DateTime.Now;

        public UICanvasIngame(Game game) 
            : base(Vector3.Zero, Vector3.Zero, game.Window.Width, game.Window.Height, RenderSpace.Screen)
        {
            this.game = game;

            int midX = game.Window.Width / 2;
            int midY = game.Window.Height / 2;

            Texture cursorTexture = new Texture("../../Resources/cursor.png", 512, 512);
            UIImage cursor = new UIImage(this, new Vector2(midX - 10, midY - 10), new Vector2(20, 20), cursorTexture);
            AddComponentToRender(cursor);

            chatbox = new UIText(this, FontRegistry.GetFont(FontType.Arial), new Vector2(10, midY), new Vector2(0.35F,0.35F), "");
            chatbox.Color = Vector3.Zero;
            AddComponentToRender(chatbox);

            inputField = new UIText(this, FontRegistry.GetFont(FontType.Arial), new Vector2(10, midY - 50), new Vector2(0.35F, 0.35F), "");
            inputField.Color = Vector3.Zero;
            AddComponentToRender(inputField);
        }

        public void AddUserMessage(string sender, string message)
        {
            string newText = sender + ": " + message;
            messageHistory.Add(newText);
            if(messageHistory.Count > 10)
            {
                messageHistory.RemoveAt(0);
            }

            StringBuilder sb = new StringBuilder();
            foreach(string chatLine in messageHistory)
            {
                sb.AppendLine(chatLine);
            }
            chatbox.Text = sb.ToString();

            lastTimeChatVisible = DateTime.Now;
            chatbox.Transparency = 1.0F;
        }

        public override void Update()
        {
            if(IsTyping)
            {
                chatbox.Transparency = 1.0F;
                lastTimeChatVisible = DateTime.Now;
            } else
            {
                float elapsedSecs = (float)(DateTime.Now - lastTimeChatVisible).TotalSeconds;
                if(elapsedSecs > 10)
                {
                    elapsedSecs -= 10;
                    chatbox.Transparency = Math.Min(1, 1 - elapsedSecs);
                }
            }
           
            bool wFocused = game.Window.Focused;
            if(wFocused && Game.Input.OnKeyPress(Key.Enter))
            {
                IsTyping = !IsTyping;

                if(!IsTyping)
                {
                    game.Client.WritePacket(new ChatPacket(game.ClientPlayer.Name, inputField.Text));
                    inputField.Text = string.Empty;
                }
            }

            if(!IsTyping)
                return;

            if(Game.Input.OnKeyPress(Key.BackSpace))
                inputField.Text = inputField.Text.Substring(0, Math.Max(0, inputField.Text.Length - 1));

            List<string> inputChars = Game.Input.GetWriteKeys();
            if(inputChars.Count == 0)
                return;

            StringBuilder stringBuilder = new StringBuilder();
            foreach(string chr in inputChars)
            {
                stringBuilder.Append(chr);
            }
            inputField.Text += stringBuilder.ToString();
        }
    }
}
