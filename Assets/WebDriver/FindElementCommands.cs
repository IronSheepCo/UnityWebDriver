using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;

using tech.ironsheep.WebDriver.Command;

namespace tech.ironsheep.WebDriver
{
	public class FindElementCommands {

		public static void Init()
		{
			WebDriverManager.instance.RegisterCommand ("element", "POST", FindElement);
		}

		private static void WriteElementNotFound( HttpListenerResponse response )
		{
			var responseBody = @"{
				""error"":""no such element"",
				""message"":""An element could not be located on the page using the given search parameters."",
				""stacktrace"":""""
			}";

			WebDriverManager.instance.WriteResponse (response, responseBody, 400);
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

		public static bool FindElement( string body, string[] args, HttpListenerResponse response )
		{
			if (ParseFindElementBody (body, response) == null) 
			{
				return true;
			}

			//return an empty set
			WebDriverManager.instance.WriteEmptyAlgorithmResponse( response );

			return true;
		}
	}
}
