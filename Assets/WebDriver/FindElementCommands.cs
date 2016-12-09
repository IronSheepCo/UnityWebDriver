﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Linq;

using tech.ironsheep.WebDriver.Command;
using tech.ironsheep.WebDriver.XPath;

namespace tech.ironsheep.WebDriver
{
	public class FindElementCommands: BaseCommand {

		//<see href="https://w3c.github.io/webdriver/webdriver-spec.html#dfn-web-element-identifier" />
		public static string WebElementIdentifierKey {
			get {
				return  "element-6066-11e4-a52e-4f735466cecf";
			}
		}

		public static XPathParser parser = new XPathParser();

		public static void Init()
		{
			WebDriverManager.instance.RegisterCommand ("element", "POST", FindElement);
			WebDriverManager.instance.RegisterCommand ("element", "POST", FindElementFromElement, "^[^/]*/element$");
			WebDriverManager.instance.RegisterCommand ("element", "POST", FindElementsFromElement, "^[^/]*/elements$");
			WebDriverManager.instance.RegisterCommand ("elements", "POST", FindElements);
		}

		private static void WriteEmptyResult( HttpListenerResponse response )
		{
			var responseBody = @"{
				""data"":[]
			}";

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);
		}

		private static void WriteElementList( HttpListenerResponse response, List<string> results )
		{
			string responseBody = "{ \"data\":[";

			var first = results [0];

			responseBody += first;

			results.Remove (first);

			foreach (var json in results) 
			{
				responseBody += ", \n" + json;
			}

			responseBody += "] }";

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);
		}

		private static Command.FindBody ParseFindElementBody( string body, HttpListenerResponse response )
		{
			var parsedBody = SimpleJSON.JSON.Parse( body );

			FindBody findBody = new FindBody ();

			try{
				findBody.locationStrategy = parsedBody ["using"];
			}
			catch( Exception e ) {
				Debug.Log (e);
				WebDriverManager.instance.InvalidArgument (response);
				return null;
			}

			try{
				findBody.selector = parsedBody ["value"];
			}
			catch(Exception e ){
				Debug.Log (e);
				WebDriverManager.instance.InvalidArgument (response);
				return null;
			}

			//we only support 'xpath' strategy
			if (findBody.locationStrategy != "xpath") 
			{
				//return an empty set
				WebDriverManager.instance.WriteEmptyAlgorithmResponse( response );
				return null;
			}


			return findBody;
		}

		private static bool FindElementFromRoot( string body, List<GameObject> root, HttpListenerResponse response )
		{
			FindBody findRequest = ParseFindElementBody (body, response);

			if ( findRequest == null) 
			{
				return true;
			}

			Component found = null;

			//need to go use all root objects
			//as context
			foreach (var rgo in root) 
			{
				var resp = parser.Evaluate (findRequest.selector, rgo);

				if (resp.Count != 0) 
				{
					found = resp [0];
					break;
				}

			}

			//no results found
			if (found == null) 
			{
				WebDriverManager.instance.WriteElementNotFound (response);

				return true;
			}

			string uuid = System.Guid.NewGuid ().ToString ();

			uuid = WebDriverManager.instance.AddElement (found, uuid);

			string jsonRepr = string.Format ("{{\"name\":\"{0}\", \"{1}\":\"{2}\"}}", found.name, WebElementIdentifierKey, uuid);

			WriteElementList (response, new List<string>{ jsonRepr });

			return true;
		}

		public static bool FindElementsFromRoot( string body, List<GameObject> rootBag, HttpListenerResponse response )
		{
			FindBody findRequest = ParseFindElementBody (body, response);

			if ( findRequest == null) 
			{
				return true;
			}

			List<Component> found = new List<Component> ();

			//need to go use all root objects
			//as context
			foreach (var rgo in rootBag) 
			{
				found.AddRange( parser.Evaluate (findRequest.selector, rgo) );
			}

			found = found.Distinct ().ToList();

			//no results found
			if (found.Count == 0) 
			{
				WriteEmptyResult (response);

				return true;
			}

			List<string> objs = new List<string> ();

			//we have results, lets 
			//add them to context
			foreach (var go in found) 
			{
				string uuid = System.Guid.NewGuid ().ToString ();

				uuid = WebDriverManager.instance.AddElement (go, uuid);

				string jsonRepr = string.Format ("{{\"name\":\"{0}\", \"{1}\":\"{2}\"}}", go.name, WebElementIdentifierKey, uuid);

				objs.Add (jsonRepr);
			}

			WriteElementList (response, objs);

			return true;
		}

		private static List<GameObject> FindStartBag( string body, HttpListenerResponse response )
		{
			FindBody findBody = ParseFindElementBody( body, response );

			if( findBody == null )
			{
				return null;
			}

			var steps = parser.Parse (findBody.selector);
			var step = steps [0];

			Type type = parser.FindType (step.TagName);

			if (type == null) 
			{
				//no type, we need to return an empty result test
				return new List<GameObject>();
			}

			var tmpList = GameObject.FindObjectsOfType ( type );

			List<GameObject> rootBag = new List<GameObject> ();

			//we need only root objects for this
			foreach (var obj in tmpList) 
			{
				Component comp = obj as Component;

				if (comp == null) 
				{
					continue;
				}

				if (step.IsChild == true) 
				{
					if (comp.gameObject.transform.parent == null) 
					{
						rootBag.Add (comp.gameObject);
					}
				}
				else 
				{
					rootBag.Add (comp.gameObject);
				}
			}


			return rootBag;
		}

		public static bool FindElement( string body, string[] args, HttpListenerResponse response )
		{
			var rootBag = FindStartBag (body, response);

			if (rootBag == null) 
			{
				return true;
			}

			return FindElementFromRoot (body, rootBag, response);
		}

		public static bool FindElements( string body, string[] args, HttpListenerResponse response )
		{
			var rootBag = FindStartBag (body, response);

			if (rootBag == null) 
			{
				return true;
			}

			return FindElementsFromRoot (body, rootBag, response);
		}

		public static bool FindElementFromElement( string body, string[] args, HttpListenerResponse response )
		{
			string elementId = args [0].Replace("\"", "");

			GameObject root = WebDriverManager.instance.GetElement (elementId).gameObject;

			if (root == null) 
			{
				WebDriverManager.instance.WriteElementNotFound (response);
			}

			return FindElementFromRoot( body, new List<GameObject>(){ root }, response );
		}

		public static bool FindElementsFromElement( string body, string[] args, HttpListenerResponse response )
		{
			string elementId = args [0].Replace("\"", "");

			GameObject root = WebDriverManager.instance.GetElement (elementId).gameObject;

			if (root == null) 
			{
				WebDriverManager.instance.WriteElementNotFound (response);
			}

			return FindElementsFromRoot (body, new List<GameObject>(){root}, response);
		}
	}
}
