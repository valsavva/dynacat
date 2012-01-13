using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Lunohod.Objects
{
    [XmlType("Class")]
    public class XClass : XElement
    {
		private XObject templateObject;
		
		public override void InitHierarchy()
		{
			this.templateObject = this.Subcomponents.Find(o => !(o is XResourceBundle));
			this.Subcomponents.Remove(templateObject);
			
			base.InitHierarchy();
		}
		
		public XObject CreateInstance(XObject placeholder)
		{
			var instance = templateObject.Copy();
			CopyAttributes(placeholder, instance);
			
			// move subcomponents from the placeholder to the new instance
			while(placeholder.Subcomponents.Count > 0)
			{
				var subcomponent = placeholder.Subcomponents[0];
				placeholder.Subcomponents.RemoveAt(0);
				
				instance.Subcomponents.Add(subcomponent);
			}

			var parent = placeholder.Parent;
			
			// replace the placeholder with the new instance
			int instanceIndex = parent.Subcomponents.IndexOf(placeholder);
			parent.Subcomponents.RemoveAt(instanceIndex);
			parent.Subcomponents.Insert(instanceIndex, instance);
			
			ReplaceThisKeyword(instance);
			
			return instance;
		}

		public void CopyAttributes(XObject src, XObject dest)
		{
			dest.Id = src.Id;
			//dest.Class = src.Class;
			
			if (src is XElement)
			{
				((XElement)dest).Bounds = ((XElement)src).Bounds;
			}
		}

		public void ReplaceThisKeyword(XObject instance)
		{
			var descendants = instance.GetAllDescendants();
			
			for(int i = 0; i < descendants.Count; i++)
			{
				var subcomponent = descendants[i];
				
				if (subcomponent.Id != null)
					subcomponent.Id = subcomponent.Id.Replace("this", instance.Id);
				
				// animation
				XAnimationBase animation = subcomponent as XAnimationBase;
				if (animation != null)
				{
					animation.Target = animation.Target.Replace("this", instance.Id);
				}
				
				// triggers
				XTriggerBase trigger = subcomponent as XTriggerBase;
				if (trigger != null)
				{
					if (trigger.EnterAction != null)
						trigger.EnterAction = trigger.EnterAction.Replace("this", instance.Id);
					if (trigger.Action != null)
						trigger.Action = trigger.Action.Replace("this", instance.Id);
					if (trigger.ExitAction != null)
						trigger.ExitAction = trigger.ExitAction.Replace("this", instance.Id);
					
					XDistanceTrigger distanceTrigger = trigger as XDistanceTrigger;
					if (distanceTrigger != null)
					{
						distanceTrigger.ObjectId1 = distanceTrigger.ObjectId1.Replace("this", instance.Id);
						distanceTrigger.ObjectId2 = distanceTrigger.ObjectId2.Replace("this", instance.Id);
					}
				}
				
				// states
				XState state = subcomponent as XState;
				if (state != null)
				{
					if (state.When != null)
						state.When = state.When.Replace("this", instance.Id);
				}
			}
			
			
		}
	}
}

