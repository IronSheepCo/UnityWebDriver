using UnityEngine;
using System;
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
			return set;
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