using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Lunohod
{
	public class MouseProcessor : InputProcessorBase
	{
		public MouseProcessor(GameEngine game)
			: base(game)
		{
		}

		public static Vector2 LastPosition;
		
		public override void Process(GameTime gameTime)
		{
			var mouseState = Mouse.GetState();
			
			LastPosition.X = mouseState.X;
			LastPosition.Y = mouseState.Y;

			if (mouseState.LeftButton == ButtonState.Pressed)
				game.ProcessTouch(gameTime, (int)mouseState.X, (int)mouseState.Y);
            if (mouseState.RightButton == ButtonState.Pressed)
                MoveHero((int)mouseState.X, (int)mouseState.Y);
		}

		private void MoveHero(int x, int y)
        {
            var levelEngine = game.ScreenEngine as LevelEngine;

            if (levelEngine == null || levelEngine.Hero == null)
                return;

            levelEngine.Hero.Bounds.X = x - levelEngine.Hero.Bounds.Width / 2;
            levelEngine.Hero.Bounds.Y = y - levelEngine.Hero.Bounds.Height / 2;
        }
	}
}

