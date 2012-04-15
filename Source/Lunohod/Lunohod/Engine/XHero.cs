using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Lunohod.Xge;

namespace Lunohod.Objects
{
    /// <summary>
    /// Represents the main character in the game.
    /// </summary>
    [XmlType("Hero")]
    public class XHero : XElement
    {
		private Vector2 offset;
		private Vector2 towerCenter;
		private Vector2 heroCenter;
		private double distanceToTower;


        /// <summary>
        /// Specifies hero's default speed.
        /// </summary>
        [XmlAttribute]
        public double DefaultSpeed;
        /// <summary>
        /// Specifies hero's current speed.
        /// </summary>
        [XmlAttribute]
        public double Speed;
        /// <summary>
        /// Specifies hero's deceleration.
        /// </summary>
        [XmlAttribute]
        public double Deceleration;
        /// <summary>
		/// Specifies hero's direction.
		/// </summary>
		[XmlIgnore]
		public Vector2 Direction;
		
		/// <summary>
		/// Gets the default health.
		/// </summary>
		/// <value>
		/// The default health.
		/// </value>
		[XmlIgnore]
		public double DefaultHealth { get; private set; }
		/// <summary>
		/// Gets hero's current health level.
		/// </summary>
		[XmlIgnore]
		public double Health { get; private set; }
		/// <summary>
		/// Gets the default bomb count.
		/// </summary>
		/// <value>
		/// The default bomb count.
		/// </value>
		[XmlIgnore]
		public double DefaultBombCount { get; private set; }
		/// <summary>
		/// Gets or sets the current bomb count.
		/// </summary>
		/// <value>
		/// The bomb count.
		/// </value>
		[XmlIgnore]
		public double BombCount { get; set; }
		/// <summary>
		/// Gets a value indicating whether hero is dead.
		/// </summary>
		/// <value>
		/// <c>true</c> if hero is dead; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore]
		public bool IsDead { get { return this.Health <= 0; } }
		/// <summary>
		/// Gets a value indicating whether this <see cref="Lunohod.Objects.XHero"/> is in transaction.
		/// </summary>
		/// <value>
		/// <c>true</c> if in transaction; otherwise, <c>false</c>.
		/// </value>
        [XmlIgnore]
		public bool InTransaction { get; private set; }
		
		
		public double LevelScore;
		public double LevelTime;
		public double LevelRecordScore;
		public double LevelRecordTime;
		public double LevelRecordHealth;
		
        internal double DistanceToTower
        {
            get { return distanceToTower; }
        }
		
		public override void Initialize(InitializeParameters p)
		{
			base.Initialize(p);

			p.LevelEngine.Hero = this;
			
			this.DefaultHealth = p.LevelEngine.LevelInfo.HeroHealth;
			this.Health = this.DefaultHealth;
			
			this.DefaultBombCount = p.LevelEngine.LevelInfo.BombCount;
			this.BombCount = this.DefaultBombCount;

            if (this.DefaultSpeed == 0)
                this.DefaultSpeed = this.Speed;
			
			distanceToTower = double.MaxValue;
		}
		
		public override void Update(UpdateParameters p)
		{
			base.Update(p);
			
			this.LevelTime += p.GameTime.ElapsedGameTime.TotalSeconds;
			
			// calculate the new location of the hero
			if (!this.InTransaction)
			{
                if (this.Direction != Lunohod.Direction.VectorStop)
                    this.Speed = this.Speed * (1.0f - this.Deceleration * p.GameTime.ElapsedGameTime.TotalSeconds);

                offset = this.Direction * (float)(this.Speed * p.GameTime.ElapsedGameTime.TotalSeconds);
	
	            this.Bounds.Offset(offset.X, offset.Y);
			}
			
			// calculate distance to the tower
            p.LevelEngine.Tower.Bounds.Center(ref towerCenter);
			this.Bounds.Center(ref heroCenter);
			distanceToTower = (towerCenter - heroCenter).Length();
		}
        /// <summary>
        /// Puts hero into the "transactional" state, when he does not respond to the player commands.
        /// </summary>
		public void StartTransaction()
		{
			this.InTransaction = true;
		}
        /// <summary>
        /// Ends transactional state.
        /// </summary>
		public void EndTransaction()
		{
			this.InTransaction = false;
		}
		/// <summary>
		/// Sets hero's direction.
		/// </summary>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		public void SetDirection(double x, double y)
		{
			this.Direction = new Vector2((float)x, (float)y);
		}
        /// <summary>
        /// Causes hero to lose the <c>damage</c> amount of health.
        /// </summary>
        /// <param name="damage">Amount of healt hero should lose.</param>
		public void InflictDamage(double damage)
		{
			this.Health -= damage;
		}
		public void AddScore(double score)
		{
			this.LevelScore += score;
		}
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadAttrAsFloat("DefaultSpeed", ref this.DefaultSpeed);
            reader.ReadAttrAsFloat("Speed", ref this.Speed);
            reader.ReadAttrAsFloat("Deceleration", ref this.Deceleration);
            reader.ReadAttrAsVector2("Direction", ref this.Direction);

            base.ReadXml(reader);
        }
		
		public override void GetMethod(string methodName, out Action<List<Expression>> method)
		{
            switch (methodName)
            {
                case "StartTransaction": method = (ps) => StartTransaction(); break;
                case "EndTransaction": method = (ps) => EndTransaction(); break;
                case "SetDirection": method = (ps) => SetDirection(ps[0].GetNumValue(), ps[1].GetNumValue()); break;
				case "AddScore": method = (ps) => AddScore(ps[0].GetNumValue()); break;
                default:
					base.GetMethod(methodName, out method); break;
            }
		}
		
		public override void GetProperty(string propertyName, out Func<bool> getter, out Action<bool> setter)
		{
			switch (propertyName)
			{
                case ("IsDead"): getter = () => IsDead; setter = null; break;
				case ("Enabled") : getter = () => this.Enabled; setter = (v) => this.Enabled = v; break;
				default:
					base.GetProperty(propertyName, out getter, out setter); break;
			}
		}

		public override void GetProperty(string propertyName, out Func<double> getter, out Action<double> setter)
		{
			switch (propertyName)
			{
                case "Health": getter = () => Health; setter = (v) => Health = v; break;
                case "DefaultHealth": getter = () => DefaultHealth; setter = (v) => DefaultHealth = v; break;
                case "Speed": getter = () => Speed; setter = (v) => Speed = v; break;
                case "DefaultSpeed": getter = () => DefaultSpeed; setter = (v) => DefaultSpeed = v; break;
                case "BombCount": getter = () => BombCount; setter = (v) => BombCount = v; break;
                case "DefaultBombCount": getter = () => DefaultBombCount; setter = (v) => DefaultBombCount = v; break;
                case "Deceleration": getter = () => Deceleration; setter = (v) => Deceleration = v; break;
				default :
					base.GetProperty(propertyName, out getter, out setter); break;
			}
		}
	}
}
