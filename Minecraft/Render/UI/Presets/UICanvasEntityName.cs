using OpenTK;
using System;

namespace Minecraft
{
    class UICanvasEntityName : UICanvas
    {
        private readonly Game game;
        private readonly ClientPlayer myPlayer;
        private readonly Entity otherEntity;

        public UICanvasEntityName(Game game, Entity otherEntity, string text) :
            base(otherEntity.Position + new Vector3(0, 1, 0), Vector3.Zero, 800, 450, RenderSpace.World)
        {
            this.otherEntity = otherEntity;
            this.myPlayer = game.ClientPlayer;
            this.game = game;

            otherEntity.OnDespawnedHandler += OnEntityDespawned;

            UIText playerName = new UIText(this, game.MasterRenderer.GetFont(FontType.Arial), new Vector2(0, 0), Vector2.One, text);
            AddComponentToRender(playerName);
        }

        public override void Update()
        {
            Position = otherEntity.Position + new Vector3(0, 1, 0);
            Vector3 dir = myPlayer.Position - otherEntity.Position;
            float theta = (float)Math.Atan2(dir.X * Vector3.UnitZ.Z - dir.Z * Vector3.UnitZ.X, dir.X * Vector3.UnitZ.X + dir.Z * Vector3.UnitZ.Z);
            Rotation = new Vector3(0, Maths.RadianToDegree(theta), 0);
        }

        private void OnEntityDespawned()
        {
            game.MasterRenderer.RemoveCanvas(this);
        }
    }
}
