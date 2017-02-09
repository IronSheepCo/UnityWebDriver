using UnityEngine;
using System;
using System.Collections.Generic;

namespace tech.ironsheep.WebDriver.XPath
{
	public class XPathNumberPredicate: XPathPredicate
	{
		public int Number;

		override public List<GameObject> Evaluate( List<GameObject> nodeList, Type componentType )
		{
			if (nodeList.Count <= Number) 
			{
				return new List<GameObject>();
			}

			List<GameObject> ret = new List<GameObject> ();
			ret.Add (nodeList [Number]);

			return ret;
		}
	}
}