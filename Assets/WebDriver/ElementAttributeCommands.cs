using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Reflection;

using tech.ironsheep.WebDriver.Command;
using tech.ironsheep.WebDriver.XPath;

namespace tech.ironsheep.WebDriver
{
	public class ElementAttributeCommands  
	{
		public static void Init()
		{
			WebDriverManager.instance.RegisterCommand ("element", "GET", ElementAttribute, "^[^/]*/attribute/.*$");
			WebDriverManager.instance.RegisterCommand ("element", "GET", ElementAttribute, "^[^/]*/property/.*$");
			WebDriverManager.instance.RegisterCommand ("element", "GET", ElementName, "^[^/]*/name$");
		}

		private static void WriteEmptyAttributeValue( HttpListenerResponse response)
		{
			string responseBody = @"{ ""data"":null }";

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);
		}

		private static void WriteAttributeValue(string value, HttpListenerResponse response )
		{
			string responseBody = @"{""data"":"""+value+@"""}";

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);
		}

		public static bool ElementAttribute( string body, string[] args, HttpListenerResponse response )
		{
			string uuid = args [0].Replace ("\"", "");

			string attributeName = args [2];

			Component comp = WebDriverManager.instance.GetElement (uuid);

			//element is not found
			if (comp == null) 
			{
				WebDriverManager.instance.WriteElementNotFound (response);
				return true;
			}

			Type type = comp.GetType ();

			PropertyInfo propertyInfo = type.GetProperty (attributeName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase );

			if (propertyInfo != null) 
			{
				string value = propertyInfo.GetValue (comp, null).ToString ();
				WriteAttributeValue (value, response);
			}
			else 
			{
				WriteEmptyAttributeValue (response);
			}

			return true;
		}

		public static bool ElementName( string body, string[] args, HttpListenerResponse response )
		{
			string uuid = args [0].Replace ("\"", "");

			Component comp = WebDriverManager.instance.GetElement (uuid);

			//element is not found
			if (comp == null) 
			{
				WebDriverManager.instance.WriteElementNotFound (response);
				return true;
			}

			Type type = comp.GetType ();

			string responseBody = "{\"data\":\""+type.Name+"\"}";

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);

			return true;
		}
	}
}
