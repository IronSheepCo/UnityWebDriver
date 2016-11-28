using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;

using tech.ironsheep.WebDriver.Command;
using tech.ironsheep.WebDriver.XPath;

namespace tech.ironsheep.WebDriver
{
	public class ElementAttributeCommands  
	{
		public static void Init()
		{
			WebDriverManager.instance.RegisterCommand ("element", "GET", ElementAttribute, "^[^/]/attribute/.*$");
			WebDriverManager.instance.RegisterCommand ("element", "GET", ElementAttribute, "^[^/]/property/.*$");
		}

		public static bool ElementAttribute( string body, string[] args, HttpListenerResponse response )
		{
			string uuid = args [0].Replace ("\"", "");

			Component go = WebDriverManager.instance.GetElement (uuid);

			//element is not found
			if (go == null) 
			{
				WebDriverManager.instance.WriteElementNotFound (response);
				return true;
			}

			return true;
		}
	}
}
