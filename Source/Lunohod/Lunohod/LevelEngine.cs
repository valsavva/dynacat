using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lunohod.Objects;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml.Serialization;

namespace Lunohod
{
	public class LevelEngine : ScreenEngine
	{
		public XHero hero;
		public List<XElement> obstacles;
		private XLevel level;

		private InitializeParameters initializeParameters;
		private UpdateParameters updateParameters;
		private DrawParameters drawParameters;
		
		private SpriteBatch spriteBatch;
		
		public LevelEngine(GameEngine gameEngine, string name)
			: base(gameEngine, name)
		{
		}

		public override XObject RootComponent { get { return level; } }
		
		
		public override void ProcessEvent(GameEvent e)
		{
			e.IsHandled = true;
			
			switch (e.EventType)
			{
				case GameEventType.Up : this.hero.Direction = Direction.VectorUp; break;
				case GameEventType.Down : this.hero.Direction = Direction.VectorDown; break;
				case GameEventType.Left : this.hero.Direction = Direction.VectorLeft; break;
				case GameEventType.Right : this.hero.Direction = Direction.VectorRight; break;
				default: e.IsHandled = false; break;
			}
			
			base.ProcessEvent(e);
		}
		
		
		public override void Initialize()
		{
			base.Initialize();
			
			this.obstacles = new List<XElement>();

			spriteBatch = new SpriteBatch(this.game.GraphicsDevice);

			initializeParameters = new InitializeParameters() { Game = game, ScreenEngine = this };
			updateParameters = new UpdateParameters() { Game = game, ScreenEngine = this };
			drawParameters = new DrawParameters() { Game = game, ScreenEngine = this, SpriteBatch = spriteBatch };
			
			LoadLevelObjects();
		}
		
		private void LoadLevelObjects()
		{
			string levelXmlFile = Path.ChangeExtension(
				Path.Combine(GameEngine.MetadataRootDirectory, this.name), ".xml"
			);
			
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(XLevel));
				
				using (FileStream stream = new FileStream(levelXmlFile, FileMode.Open, FileAccess.Read))
				{
					this.level = (XLevel)serializer.Deserialize(stream);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.ToString());
				
				throw;
			}
			
			level.InitHierarchy();
			level.Initialize(initializeParameters);
		}
		
		public override void Update(GameTime gameTime)
		{
			updateParameters.GameTime = gameTime;
			
			this.game.GameObject.Update(updateParameters);
			
			this.level.Update(updateParameters);
			
			ProcessCollisions();
		}
		
		private List<Tuple<XElement, Rectangle, int>> colliders = new List<Tuple<XElement, Rectangle, int>>();
		
		public void ProcessCollisions()
		{
			// if not moving - we don't process collisions
            if (this.hero.Direction == Direction.VectorStop)
                return;
			
			// find objects we collided with
			FindCollisions();
			
			if (colliders.Count() == 0)
				return;
			
			// sort them by the amounth of intersection
			colliders.Sort((t1, t2) => t1.Item3.CompareTo(t2.Item3));
			
			for(int i = 0; i < colliders.Count; i++)
			{
				// process collision with the object
				if (colliders[i].Item1.ProcessCollision(this, colliders[i].Item2))
					// quit if collision was processed or go to the next object
					break;
			}
		}

		void FindCollisions()
		{
			colliders.Clear();
			
			XElement obstacle;
			Rectangle heroBounds = this.hero.Bounds;
			Rectangle obstacleBounds;
			Rectangle intersect;
			
			for(int i = 0; i < this.obstacles.Count; i++)
			{
				obstacle = this.obstacles[i];
				obstacleBounds = obstacle.GetScreenBounds();
				
				Rectangle.Intersect(ref heroBounds, ref obstacleBounds, out intersect);
				
				if (!intersect.IsEmpty)
				{
					colliders.Add(Tuple.Create(obstacle, intersect, intersect.Area()));
				}
			}
		}
		
		public override void Draw(GameTime gameTime)
		{
			drawParameters.GameTime = gameTime;
			
			try 
			{
				//scale(40/viewport.Height) * scale(1, -1) * translate(viewport.Width/2, viewport.Height/2)
					
				//Matrix transform = Matrix.Identity;
				
				// *
//					Matrix.CreateScale(40f / 320) *
//					//Matrix.CreateScale(1, -1, 1) *
//					Matrix.CreateTranslation(480f / 2, 320f / 2, 0);
						
//				Matrix transform = Matrix.Identity;
				
				if (this.game.Window.ClientBounds.Height > 900)
				{
					Matrix transform = Matrix.Identity *
						Matrix.CreateScale(2.0f);
					this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
						DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transform );
				}
				else
				{
					this.spriteBatch.Begin();
				}
				
				this.level.Draw(drawParameters);
				this.game.GameObject.Draw(drawParameters);
				
				this.spriteBatch.End();
				
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		public override void Unload()
		{
			this.level.Dispose();
			this.spriteBatch.Dispose();

			this.level = null;
			this.obstacles = null;
			this.spriteBatch = null;
		}
	}
}	
/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lunohod.Objects;
using System.Threading;

namespace Lunohod
{
    public class GameEngine
    {
        private Canvas container;
        private XLevel level;

        private XLayer actionLayer;
        private XTower tower;
        public XHero hero;
        public List<XElement> obstacles;

        public Queue<ActionInfo> actions = new Queue<ActionInfo>();

        private int frequency = 60;
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        private static Dictionary<Key, XHeroMoveType> KeyMapping = new Dictionary<Key, XHeroMoveType> 
        {
            { Key.Left, XHeroMoveType.Left },
            { Key.Right, XHeroMoveType.Right },
            { Key.Up, XHeroMoveType.Up },
            { Key.Down, XHeroMoveType.Down },
        };

        public GameEngine(Canvas container, XLevel level)
        {
            this.container = container;
            this.level = level;
        }

        public void container_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point location = e.GetPosition(this.container);
            location.X -= hero.Window.Width / 2;
            location.Y -= hero.Window.Height / 2;
            hero.SetLocation(location);
        }

        public void container_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                hero.Move = XHeroMoveType.None;
            }
            else
            {
                XHeroMoveType move = XHeroMoveType.None;

                if (KeyMapping.TryGetValue(e.Key, out move))
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        hero.Move = move;
                    else
                        EnqueueAction(move);
                }

            }
        }

        private void EnqueueAction(XHeroMoveType move)
        {
            var moveInfo = new ActionInfo { Move = move, StartTime = DateTime.Now };
            moveInfo.Circle.SetBounds(new RectangleF(this.tower.Bounds.ToRect().Center(), new Point(1, 1)));
            this.actionLayer.Window.Children.Add(moveInfo.Circle);
            actions.Enqueue(moveInfo);
        }

        private void DequeueAction(ActionInfo action)
        {
            actionLayer.Window.Children.Remove(action.Circle);
            actions.Dequeue();
        }

        public void StartLevel()
        {
            CreateLevelWindow();

            ThreadPool.QueueUserWorkItem(Loop);            
        }

        private void Loop(object o)
        {
            while (true)
            {
                dispatcher.Invoke((Action)LoopOnMainThread, null);

                Thread.Sleep((int)(1.0 / frequency * 1000));
            }
        }

        private void LoopOnMainThread()
        {
            try
            {
                AnalyzeActions();
                ExecuteMove();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: \n" + ex.ToString());
            }
        }

        private void AnalyzeActions()
        {
            if (actions.Count == 0)
                return;

            RectangleF towerRect = tower.Bounds.ToRect();
            RectangleF heroRect = hero.Bounds.ToRect();
            double heroToTowerDistance = towerRect.Location.DistanceTo(heroRect.Location);

            while (actions.Count > 0)
            {
                var action = actions.Peek();
                var actionRadius = tower.ActionRadius(action);
                action.SetRadius(tower, actionRadius);
                if (actionRadius > heroToTowerDistance)
                {
                    hero.Move = action.Move;
                    DequeueAction(action);
                }
                else
                    break;
            }
        }


        // removes actions that the hero escapes by using teleport
        public void DequeuePastActions()
        {
            RectangleF towerRect = tower.Bounds.ToRect();
            RectangleF heroRect = hero.Bounds.ToRect();
            double heroToTowerDistance = towerRect.Location.DistanceTo(heroRect.Location);

            while (actions.Count > 0)
            {
                var action = actions.Peek();
                var actionRadius = tower.ActionRadius(action);
                if (actionRadius > heroToTowerDistance)
                {
                    DequeueAction(action);
                }
                else
                    break;
            }
        }

        private void ExecuteMove()
        {
            if (hero.Move == XHeroMoveType.None)
                return;

            double d = hero.Speed / frequency;
            RectangleF newBounds = hero.Bounds.ToRect();
            Point newLocation = newBounds.Location;

            if (hero.Move == XHeroMoveType.Left)
                newLocation.X -= d;
            else if (hero.Move == XHeroMoveType.Right)
                newLocation.X += d;
            else if (hero.Move == XHeroMoveType.Up)
                newLocation.Y -= d;
            else if (hero.Move == XHeroMoveType.Down)
                newLocation.Y += d;

            newBounds.Location = newLocation;

            var obstacle = (
                from o in obstacles
                let bounds = o.Bounds.ToRect()
                let intersect = RectangleF.Intersect(bounds, newBounds)
                where !intersect.IsEmpty
                orderby intersect.Area() descending
                select o
            ).FirstOrDefault();

            if (obstacle != null)
                obstacle.ProcessCollision(this, newBounds);
            else
                hero.SetBounds(newBounds);
        }

        private void CreateLevelWindow()
        {
            level.Window = new Canvas() {
                Width = level.Width,
                Height = level.Height
            };

            container.Children.Add(level.Window);

            int zIndex = 0;
            foreach (var layer in level.Layers)
            {
                CreateLayerWindow(zIndex++, layer);
            }
        }

        private void CreateLayerWindow(int zIndex, XLayer layer)
        {
            layer.CreateWindow(level.Window);

            Canvas.SetZIndex(layer.Window, zIndex);

            if (layer.Name == "action")
            {
                actionLayer = layer;
                tower = (XTower)layer.Elements.First(e => e is XTower);
                hero = (XHero)layer.Elements.First(e => e is XHero);
                obstacles = layer.Elements.Where(e => e is XBlock).ToList();
            }
        }
    }
}
*/