using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Lunohod.Objects
{
	abstract public class XObject : IDisposable
	{
		protected bool isDisposed = false;
		private Dictionary<string, SignalContainer> signalContainers = null;
		private XObjectCollection subcomponents = null;
		private Dictionary<string, XObject> componentDict;
		
		[XmlAttribute]
		public string Id { get; set;}
		
		[XmlAttribute]
		public string Class { get; set; }

		[XmlAttribute]
		public string ClassParams { get; set; }

		[XmlIgnore]
		public XObject Parent { get; set; }

        [XmlAttribute]
        public bool Enabled = true;
		
		public virtual XObject Copy()
		{
			var result = (XObject)this.MemberwiseClone();
			result.Subcomponents = new XObjectCollection(this.Subcomponents.Count);
			for(int i = 0; i < this.Subcomponents.Count; i++)
			{
				result.Subcomponents.Add(this.Subcomponents[i].Copy());
			}
			result.InitHierarchy();
			
			return result;
		}
		
		// Game
        [XmlElement(ElementName = "Resources", Type = typeof(XResourceBundle))]
        [XmlElement(ElementName = "Dashboard", Type = typeof(XDashboard))]
        [XmlElement(ElementName = "TapArea", Type = typeof(XTapArea))]
        [XmlElement(ElementName = "Include", Type = typeof(XInclude))]

        // Media
        [XmlElement(ElementName = "Music", Type = typeof(XMusic))]
        [XmlElement(ElementName = "Sound", Type = typeof(XSound))]
		
        // Dashboard
        [XmlElement(ElementName = "Viewport", Type = typeof(XViewport))]

        // Level
        [XmlElement(ElementName = "Layer", Type = typeof(XLayer))]
		
		// Triggers
        [XmlElement(ElementName = "IntersectionTrigger", Type = typeof(XIntersectionTrigger))]
        [XmlElement(ElementName = "DistanceTrigger", Type = typeof(XDistanceTrigger))]
		[XmlElement(ElementName = "BoolTrigger", Type = typeof(XBoolTrigger))]
		[XmlElement(ElementName = "NumTrigger", Type = typeof(XNumTrigger))]
		
		// Classes
        [XmlElement(ElementName = "Class", Type = typeof(XClass))]

		// Characters
        [XmlElement(ElementName = "Tower", Type = typeof(XTower))]
        [XmlElement(ElementName = "Hero", Type = typeof(XHero))]
        [XmlElement(ElementName = "Enemy", Type = typeof(XEnemy))]
        [XmlElement(ElementName = "Food", Type = typeof(XFood))]

		// Sets
        [XmlElement(ElementName = "SequenceSet", Type = typeof(XSequenceSet))]
        [XmlElement(ElementName = "RandomSet", Type = typeof(XRandomSet))]
        [XmlElement(ElementName = "ParallelSet", Type = typeof(XParallelSet))]

        // Actions
        [XmlElement(ElementName = "Do", Type = typeof(XDo))]
        [XmlElement(ElementName = "Delay", Type = typeof(XDelay))]

        // Animation
        [XmlElement(ElementName = "NumAnimation", Type = typeof(XNumAnimation))]
        [XmlElement(ElementName = "KeyFrame", Type = typeof(XKeyFrame))]

		// Basic elements
		[XmlElement(ElementName = "Group", Type = typeof(XGroup))]
        [XmlElement(ElementName = "Image", Type = typeof(XImage))]
        [XmlElement(ElementName = "Text", Type = typeof(XText))]
        [XmlElement(ElementName = "Block", Type = typeof(XBlock))]
        [XmlElement(ElementName = "Sprite", Type = typeof(XSprite))]
        public XObjectCollection Subcomponents
		{
			get { return subcomponents; }
			set {
				if (subcomponents == value)
					return;
				
				if (subcomponents != null)
					subcomponents.Parent = null;
				
				subcomponents = value;
				
				if (subcomponents != null)
					subcomponents.Parent = this;
			}
		}
		
		public virtual void InitHierarchy()
		{
			if (this.Subcomponents == null)
				return;
			
			// replace includes with their children
			var index = this.Subcomponents.FindIndex(c => c is XInclude);
			
			while (index != -1)
			{
				var include = this.Subcomponents[index];
				this.Subcomponents.RemoveAt(index);
				this.Subcomponents.InsertRange(index, include.Subcomponents);
				
				index = this.Subcomponents.FindIndex(c => c is XInclude);
			}
			
			// classes
			if (!string.IsNullOrEmpty(this.Class))
			{
				InitiazeFromClass();
				return;
			}
			else
			{
				if (this.Subcomponents != null)
					for(int i = 0; i < this.Subcomponents.Count; i++)
					{
						var subcomponent = this.Subcomponents[i];
						subcomponent.InitHierarchy();
					}
			}
			
			// force to rebiuld subcomponent hash
			this.componentDict = null;
		}
		
		public virtual void Initialize(InitializeParameters p)
		{
			if (this.Subcomponents != null)
				for(int i = 0; i < this.Subcomponents.Count; i++)
				{
					var subcomponent = this.Subcomponents[i];
					subcomponent.Initialize(p);
				}
		}

		private void InitiazeFromClass()
		{
			XClass cls = (XClass)this.GetRoot().FindDescendant(this.Class);
			
			if (cls == null)
				throw new InvalidOperationException("Could not find class: " + this.Class);
			
			var instance = cls.CreateInstance(this);
			instance.InitHierarchy();
		}
		
		public virtual void Update(UpdateParameters p)
		{
			if (this.Subcomponents != null)
				for(int i = 0; i < this.Subcomponents.Count; i++)
				{
					var subcomponent = this.Subcomponents[i];
                    if (subcomponent.Enabled)
    					subcomponent.Update(p);
				}

			if (signalContainers != null)
			{
				foreach(var item in signalContainers)
					item.Value.Clear();
			}
		}
		
		public virtual void Draw(DrawParameters p)
		{
			if (this.Subcomponents != null)
				for(int i = 0; i < this.Subcomponents.Count; i++)
				{
					var subcomponent = this.Subcomponents[i];
                    if (subcomponent.Enabled)
                        subcomponent.Draw(p);
				}
		}
		
        public void Enable()
        {
            this.Enabled = true;
        }

        public void Disable()
        {
            this.Enabled = false;
        }

		public T GetComponent<T>() where T : XObject
		{
			if (this.Subcomponents == null)
				return null;
			
			return (T)this.Subcomponents.FirstOrDefault(c => c is T);
		}
		
		public IEnumerable<T> GetComponents<T>() where T : XObject
		{
			if (this.Subcomponents == null)
				return null;
			
			return this.Subcomponents.Where(c => c is T).Cast<T>();
		}
		
		public XObject FindDescendant(string id)
		{
			if (this.componentDict == null)
			{
				this.componentDict = new Dictionary<string, XObject>();
				this.TraveseTree(o => {
					if (o.Id != null)
					{
						if (!this.componentDict.ContainsKey(o.Id))
							this.componentDict.Add(o.Id, o);
		#if DEBUG
						else
							Console.WriteLine("*** Component with id '{0}' already exists! ***", this.Id);
		#endif
					}
				});
			}
			
			XObject result = null;
			
			this.componentDict.TryGetValue(id, out result);
			
			return result;
		}
		
		public void TraveseTree(Action<XObject> action)
		{
			action(this);

			if (this.Subcomponents != null)
				for(int i = 0; i < this.Subcomponents.Count; i++)
					this.Subcomponents[i].TraveseTree(action);
		}
		
		public XObject FindAncestor(Predicate<XObject> p)
		{
			XObject result = this.Parent;
			
			while((result != null) && !p(result))
			{
				result = result.Parent;
			}
			
			return result;
		}
		
		public XObject GetRoot()
		{
			XObject result = this;
	
			while (result.Parent != null)
				result = result.Parent;

			return result;
		}
		
		public void GetTargetFromDescriptor(string descriptor, out XObject target, out string targetMember)
		{
			if (descriptor.Contains(".") || descriptor.Contains(":"))
			{
				var parts = descriptor.Split('.', ':');

				target = this.GetRoot().FindDescendant(parts[0]);
				
				if (target == null)
					throw new InvalidOperationException(string.Format("Could not find object with id [{0}]", parts[0]));
				targetMember = parts[1];
			}
			else
			{
				target = this.Parent;
				targetMember = descriptor;
			}
		}
		
		public SignalContainer GetSignalContainer(string containerName)
		{
			if (signalContainers == null)
				signalContainers = new Dictionary<string, SignalContainer>();
			
			SignalContainer container = null;
			
			if (!signalContainers.TryGetValue(containerName, out container))
			{
				container = new SignalContainer();
				signalContainers.Add(containerName, container);
			}
			
			return container;
		}

        public List<IRunnable> CollectRunnables(List<IRunnable> result = null)
        {
            if (result == null)
                result = new List<IRunnable>();

            if (this.Subcomponents != null)
                for (int i = 0; i < this.Subcomponents.Count; i++)
                {
                    var subcomponent = this.Subcomponents[i];
                    IRunnable runnable = subcomponent as IRunnable;
                    if (runnable != null)
                        result.Add(runnable);

                    // we don't collect other sets' runnables
                    if (!(subcomponent is XSetBase))
                        subcomponent.CollectRunnables(result);
                }

            return result;
        }
        //public IEnumerable<T> TraverseChildren<T>() where T : class
        //{
        //    for (int i = 0; i < this.Subcomponents.Count; i++)
        //    {
        //        if (this.Subcomponents[i] is T)
        //            yield return (this.Subcomponents[i] as T);
        //    }
        //}

		public virtual void ReplaceParameter(string par, string val)
		{
			if (this.Id != null)
				this.Id = this.Id.Replace(par, val);
		}
		
		public void ReplaceParameters(List<string> pars, List<string> vals)
		{
			if (this.subcomponents != null)
				for(int i = 0; i < this.subcomponents.Count; i++)
				{
					for(int j = 0; j < pars.Count; j++)
						subcomponents[i].ReplaceParameter(pars[j], vals[j]);
					subcomponents[i].ReplaceParameters(pars, vals);
				}
		}
		
		#region IDisposable implementation
		public virtual void Dispose()
		{
			isDisposed = true;
			
			if (this.Subcomponents != null)
				for(int i = 0; i < this.Subcomponents.Count; i++)
				{
					var subcomponent = this.Subcomponents[i];
					subcomponent.Dispose();
				}
		}
		#endregion
	}
}

