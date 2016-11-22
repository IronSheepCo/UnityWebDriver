using UnityEngine;
using System.Collections.Generic;

namespace tech.ironsheep.WebDriver.XPath
{
	public class XPathNumberPredicate: XPathPredicate
	{
		public int Number;

		override public List<GameObject> Evaluate( List<GameObject> nodeList, string scriptName )
		{
			if (nodeList.Count <= Number) 
			{
				return null;
			}

			List<GameObject> ret = new List<GameObject> ();
			ret.Add (nodeList [Number]);

			return ret;
		}
	}
}