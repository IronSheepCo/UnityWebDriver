using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;

namespace tech.ironsheep.WebDriver
{
	public class FindElementCommands {

		public static void Init()
		{
			WebDriverManager.instance.RegisterCommand ("element", "POST", FindElement);
		}

		public static bool FindElement( string body, string[] args, HttpListenerResponse response )
		{
			var findBody = SimpleJSON.JSON.Parse (body);

			return true;
		}
	}
}
