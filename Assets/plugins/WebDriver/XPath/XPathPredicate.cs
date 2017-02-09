using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace tech.ironsheep.WebDriver.XPath
{
	public class XPathPredicate
	{
		virtual public List<GameObject> Evaluate( List<GameObject> set, Type componentType )
		{
			return set;
		}
	}
}