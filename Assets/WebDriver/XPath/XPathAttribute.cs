
namespace tech.ironsheep.WebDriver.XPath
{
	public class XPathAttribute
	{
		//The name of the attribute
		public string Name;

		//if set use ValueToMatch
		//to match the attribute named
		//with the value
		//if null then we test for the existence of
		//attribute Name
		public string ValueToMatch;
	}
}