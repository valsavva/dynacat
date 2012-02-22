using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Lunohod.Objects
{
    [XmlType("Enemy")]
    public class XEnemy : XElement, IExploding
    {
		/// <summary>
		/// The damage the enemy will cause when attacking the hero. Default value is 1.
		/// </summary>
		[XmlAttribute]
		public float Damage = 1;
        /// <inheritdoc />
        [XmlAttribute]
        public bool IsExploding { get; set; }
		
		public void Attack()
		{
			var hero = GameEngine.Instance.LevelEngine.Hero;
			hero.InflictDamage(this.Damage);
		}
    }
}

