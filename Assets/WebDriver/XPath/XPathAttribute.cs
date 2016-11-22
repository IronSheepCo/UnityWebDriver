using UnityEngine;
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

		override public List<GameObject> Evaluate( List<GameObject> set, string scriptName )
		{
			return set;
		}
	}
}