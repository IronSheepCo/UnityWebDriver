using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace tech.ironsheep.WebDriver.XPath
{
	public class XPathAttribute: XPathPredicate
	{
		//The name of the attribute
		public string Name;

		//if set use ValueToMatch
		//to match the attribute named
		//with the value
		//if null then we test for the existence of
		//attribute Name
		public string ValueToMatch;

		override public List<GameObject> Evaluate( List<GameObject> set, Type componentType )
		{
			if (ValueToMatch == null) 
			{
				return EvaluateNonNull (set, componentType);
			}

			//special attribute
			//we look for this on the game object itself
			//and not the component
			if (Name == "name") 
			{
				return EvaluateName (set);
			}

			return null;
		}

		private List<GameObject> EvaluateName( List<GameObject> set )
		{
			return set.Where (go => go.name == ValueToMatch).ToList();
		}

		private List<GameObject> EvaluateNonNull( List<GameObject> set, Type componentType )
		{
			List<GameObject> filtered = new List<GameObject> ();

			foreach (var go in set) 
			{
				Component comp = go.GetComponent (componentType);

				if (comp == null) 
				{
					continue;
				}

				//looking into properties
				if (componentType.GetProperty (Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase ) != null) 
				{
					filtered.Add (go);
					continue;
				}

				//looking int fields
				if (componentType.GetField (Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase) != null) 
				{
					filtered.Add (go);
					continue;
				}
			}

			return filtered;
		}

		public static XPathAttribute fromString( string data )
		{
			//removing the @
			string kernel = data.Remove (0, 1);

			XPathAttribute ret = new XPathAttribute ();

			var split = kernel.Split (new char[]{'='}, System.StringSplitOptions.RemoveEmptyEntries);

			ret.Name = split [0];

			//the attribute has a value
			if (split.Length > 1) 
			{
				ret.ValueToMatch = split [1].Substring(1, split[1].Length-2);
			}

			return ret;
		}
	}
}