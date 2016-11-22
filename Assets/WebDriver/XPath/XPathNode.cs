using System.Collections.Generic;

namespace tech.ironsheep.WebDriver.XPath
{
	public class XPathNode
	{
		public string TagName;

		//if true, TagName needs to be a child
		//of the current node
		//if false, TagName needs to be a descendant
		//of the current node
		public bool IsChild;

		//attributes that need to match for the
		//current segment of the path
		public List<XPathAttribute> attributes = new List<XPathAttribute>();
	}
}