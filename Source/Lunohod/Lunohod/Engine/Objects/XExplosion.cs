using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Lunohod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

namespace Lunohod.Objects
{
	
    [XmlType("Explosion")]
	public class XExplosion : XElement
	{
		private List<XElement> eplodingElements;
		private List<string> rangeNames;
		private List<float> rangeValuesSquared;
		
		[XmlAttribute]
		public string Ranges;
		
		public override void Initialize(InitializeParameters p)
		{
			base.Initialize(p);
			
			var pairs = this.Ranges.Split(',');
			
			eplodingElements = new List<XElement>(pairs.Length);
			rangeNames = new List<string>(pairs.Length);
			rangeValuesSquared = new List<float>(pairs.Length);
	
			pairs.ForEach(s => {
				var parts = s.Split('=');
				rangeNames.Add(parts[0]);
				var rangeValue = float.Parse(parts[1], CultureInfo.InvariantCulture);
				rangeValuesSquared.Add(rangeValue * rangeValue);
			});
		}
		
		public void Explode()
		{
			this.eplodingElements.Clear();
			
			this.GetScreenBounds();
			
			this.PropState.ScreenBounds.Value.Center(ref this.tmpVector1);
			
			XLevel level = this.GetRoot() as XLevel;
			
			for(int i = 0; i < level.Exploding.Count; i++)
			{
				XElement e = level.Exploding[i];
				
				if (!e.Enabled)
					continue;
				
				e.GetScreenBounds();
				
				e.PropState.ScreenBounds.Value.Center(ref this.tmpVector2);
				
				var distanceSquared = this.tmpVector1.SquaredDistanceTo(this.tmpVector2);
				
				for(int j = 0; j < rangeNames.Count; j++)
				{
					if (distanceSquared <= rangeValuesSquared[j])
						e.GetSignalContainer("events").Signal(rangeNames[j]);
				}
			}
		}
	}
}
