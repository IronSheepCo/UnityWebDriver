using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Net;

namespace tech.ironsheep.WebDriver
{
	public class ElementInteractCommands: BaseCommand
	{
		public static void Init()
		{
			WebDriverManager.instance.RegisterCommand ("element", "GET", ClickElement, "^[^/]*/click$");
			WebDriverManager.instance.RegisterCommand ("element", "POST", SendKeys, "^[^/]*/value$");
		}

		public static bool ClickElement( string body, string[] args, HttpListenerResponse response )
		{
			Component comp = GetComponent (args, response);

			if (comp == null) 
			{
				return true;
			}

			string responseBody = "{\"data\":null}";

			if (comp.GetType () == FindElementCommands.parser.FindType ("Button")) 
			{
				Button b = comp as Button;

				//invoke the handler 
				b.onClick.Invoke ();
			} 
			else 
			{
				//using NGUI
				comp.gameObject.SendMessage ("OnClick");
			}

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);

			return true;
		}

		public static bool SendKeys( string body, string[] args, HttpListenerResponse response )
		{
			Component comp = GetComponent (args, response);

			if (comp == null) 
			{
				return true;
			}

			string text = SimpleJSON.JSON.Parse( body )["text"];

			string responseBody = "{\"data\":null}";

			if (comp.GetType () == FindElementCommands.parser.FindType ("Text")) 
			{
				Text t = comp as Text;

				//set the text
				t.text = text;
			} 
			else 
			{
				
			}

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);

			return true;
		}
	}
}