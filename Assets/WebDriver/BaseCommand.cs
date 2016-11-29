using UnityEngine;
using System.Collections;
using System.Net;

namespace tech.ironsheep.WebDriver
{
	public class BaseCommand {
		protected static Component GetComponent( string[] args, HttpListenerResponse response )
		{
			string uuid = args [0].Replace ("\"", "");

			Component comp = WebDriverManager.instance.GetElement (uuid);

			//element is not found
			if (comp == null) 
			{
				WebDriverManager.instance.WriteElementNotFound (response);
				return null;
			}

			return comp;
		}
	}
}