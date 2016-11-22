using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace tech.ironsheep.WebDriver.XPath
{
	public class XPathPredicate
	{
		virtual public List<GameObject> Evaluate( List<GameObject> set, string scriptName )
		{
			return set;
		}
	}
}