using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

using tech.ironsheep.WebDriver.XPath;
using tech.ironsheep.WebDriver.Lexer;

namespace tech.ironsheep.WebDriver.Tests
{
	public class XPathParserTests : MonoBehaviour {

		private string sessionId;

		// Use this for initialization
		void Start () {
			LexerTests ();
			ParserTests ();
			ParserEvaluateTest ();

			StartCoroutine( FindCommands () );
		}

		private void LexerTests()
		{
			Lexer.Lexer lex = new Lexer.Lexer ();

			List<Token> tokens = lex.Tokenize ("//uibutton");

			Debug.Assert (tokens.Count == 2);
			Debug.Assert (tokens [0].Content == "//");
			Debug.Assert (tokens [0].Desc == "ANCESTOR");
			Debug.Assert (tokens [1].Content == "uibutton");
			Debug.Assert (tokens [1].Desc == "IDENTIFIER");


			tokens = lex.Tokenize ("/uibutton");

			Debug.Assert (tokens.Count == 2);
			Debug.Assert (tokens [0].Content == "/");
			Debug.Assert (tokens [0].Desc == "CHILD");
			Debug.Assert (tokens [1].Content == "uibutton");
			Debug.Assert (tokens [1].Desc == "IDENTIFIER");

			tokens = lex.Tokenize ("//uilabel[3]");

			Debug.Assert (tokens.Count == 5);
			Debug.Assert (tokens [0].Content == "//");
			Debug.Assert (tokens [1].Content == "uilabel");
			Debug.Assert (tokens [2].Content == "[");
			Debug.Assert (tokens [3].Content == "3");
			Debug.Assert (tokens [4].Content == "]");
			Debug.Assert (tokens [2].Desc == "OPEN_PREDICATE");
			Debug.Assert (tokens [3].Desc == "NUMBER");


			tokens = lex.Tokenize ("/ uibutton");

			Debug.Assert (tokens.Count == 3);
			Debug.Assert (tokens [0].Content == "/");
			Debug.Assert (tokens [0].Desc == "CHILD");
			Debug.Assert (tokens [2].Content == "uibutton");
			Debug.Assert (tokens [1].Content == " ");
			Debug.Assert (tokens [1].Desc == "WHITE_SPACE");
			Debug.Assert (tokens [2].Desc == "IDENTIFIER");

			tokens = lex.Tokenize ("//uilabel/uibutton//altceva");

			Debug.Assert (tokens.Count == 6 );
			Debug.Assert (tokens [3].Content == "uibutton");
			Debug.Assert (tokens [3].Desc == "IDENTIFIER");
			Debug.Assert (tokens [2].Desc == "CHILD");

			tokens = lex.Tokenize ("//label[@text=\"ceva\"]");

			Debug.Assert (tokens.Count == 7);
			Debug.Assert (tokens [3].Content == "@text");
			Debug.Assert (tokens [3].Desc == "ATTRIBUTE");
			Debug.Assert (tokens [4].Content == "=");
			Debug.Assert (tokens [4].Desc == "EQUAL");
			Debug.Assert (tokens [5].Content == "ceva");
			Debug.Assert (tokens [5].Desc == "STRING");

			tokens = lex.Tokenize ("//label[@text=\"ceva\" and @tre=\"23232\"]");

			Debug.Assert (tokens.Count == 13);
			Debug.Assert (tokens [3].Content == "@text");
			Debug.Assert (tokens [3].Desc == "ATTRIBUTE");
			Debug.Assert (tokens [4].Content == "=");
			Debug.Assert (tokens [4].Desc == "EQUAL");
			Debug.Assert (tokens [5].Content == "ceva");
			Debug.Assert (tokens [5].Desc == "STRING");
			Debug.Assert (tokens [7].Content == "and");
			Debug.Assert (tokens [7].Desc == "AND");
			Debug.Assert (tokens [11].Content == "23232");
			Debug.Assert (tokens [11].Desc == "STRING");

			tokens = lex.Tokenize ("//label[@text=\"ceva\" and @tre=\"23232\" and @rez=\"ultim and\"]");

			Debug.Assert (tokens.Count == 19);
			Debug.Assert (tokens [3].Content == "@text");
			Debug.Assert (tokens [3].Desc == "ATTRIBUTE");
			Debug.Assert (tokens [4].Content == "=");
			Debug.Assert (tokens [4].Desc == "EQUAL");
			Debug.Assert (tokens [5].Content == "ceva");
			Debug.Assert (tokens [5].Desc == "STRING");
			Debug.Assert (tokens [7].Content == "and");
			Debug.Assert (tokens [7].Desc == "AND");
			Debug.Assert (tokens [11].Content == "23232");
			Debug.Assert (tokens [11].Desc == "STRING");
			Debug.Assert (tokens [13].Content == "and");
			Debug.Assert (tokens [13].Desc == "AND");
			Debug.Assert (tokens [15].Content == "@rez");
			Debug.Assert (tokens [15].Desc == "ATTRIBUTE");
			Debug.Assert (tokens [17].Content == "ultim and");
			Debug.Assert (tokens [17].Desc == "STRING");

		}

		private void ParserTests()
		{
			Debug.Log ("starting parser tests");

			var parser = new XPathParser ();

			float startTime = Time.realtimeSinceStartup;

			var results = parser.Parse ("books");

			Debug.Assert (results.Count == 1);
			Debug.Assert (results [0].TagName == "books");
			Debug.Assert (results [0].IsChild == false);
			Debug.Assert (results [0].predicates.Count == 0);

			results = parser.Parse ("//books");

			Debug.Assert (results.Count == 1);
			Debug.Assert (results [0].TagName == "books");
			Debug.Assert (results [0].IsChild == false);
			Debug.Assert (results [0].predicates.Count == 0);

			results = parser.Parse ("/books/book");

			Debug.Assert (results.Count == 2);
			Debug.Assert (results [0].TagName == "books");
			Debug.Assert (results [0].IsChild == true);
			Debug.Assert (results [0].predicates.Count == 0);
			Debug.Assert (results [1].TagName == "book");
			Debug.Assert (results [1].IsChild == true);
			Debug.Assert (results [1].predicates.Count == 0);

			results = parser.Parse ("books//letter");

			Debug.Assert (results.Count == 2);
			Debug.Assert (results [0].TagName == "books");
			Debug.Assert (results [0].IsChild == false);
			Debug.Assert (results [0].predicates.Count == 0);
			Debug.Assert (results [1].TagName == "letter");
			Debug.Assert (results [1].IsChild == false);
			Debug.Assert (results [1].predicates.Count == 0);


			results = parser.Parse ("somescript/uilabel[3]");

			Debug.Assert (results.Count == 2);
			Debug.Assert (results [0].TagName == "somescript");
			Debug.Assert (results [0].IsChild == false);
			Debug.Assert (results [0].predicates.Count == 0);
			Debug.Assert (results [1].TagName == "uilabel");
			Debug.Assert (results [1].IsChild == true);
			Debug.Assert (results [1].predicates.Count == 1);
			Debug.Assert (results [1].predicates[0] is XPathNumberPredicate);
			Debug.Assert ((results [1].predicates[0] as XPathNumberPredicate).Number == 3);

			results = parser.Parse ("otherscript//uilabel[@text=\"something\"]");

			Debug.Assert (results.Count == 2);
			Debug.Assert (results [0].TagName == "otherscript");
			Debug.Assert (results [0].IsChild == false);
			Debug.Assert (results [0].predicates.Count == 0);
			Debug.Assert (results [1].TagName == "uilabel");
			Debug.Assert (results [1].IsChild == false);
			Debug.Assert (results [1].predicates.Count == 1);
			Debug.Assert (results [1].predicates[0] is XPathAttribute);
			Debug.Assert ((results [1].predicates[0] as XPathAttribute).Name == "text");
			Debug.Assert ((results [1].predicates[0] as XPathAttribute).ValueToMatch == "something");

			results = parser.Parse ("/otherscript/uilabel[@text=\"something\" and @lang=\"en\"]");

			Debug.Assert (results.Count == 2);
			Debug.Assert (results [0].TagName == "otherscript");
			Debug.Assert (results [0].IsChild == true);
			Debug.Assert (results [0].predicates.Count == 0);
			Debug.Assert (results [1].TagName == "uilabel");
			Debug.Assert (results [1].IsChild == true);
			Debug.Assert (results [1].predicates.Count == 2);
			Debug.Assert (results [1].predicates[0] is XPathAttribute);
			Debug.Assert ((results [1].predicates[0] as XPathAttribute).Name == "text");
			Debug.Assert ((results [1].predicates[0] as XPathAttribute).ValueToMatch == "something");
			Debug.Assert (results [1].predicates[1] is XPathAttribute);
			Debug.Assert ((results [1].predicates[1] as XPathAttribute).Name == "lang");
			Debug.Assert ((results [1].predicates[1] as XPathAttribute).ValueToMatch == "en");

			Debug.Log ("time " + (Time.realtimeSinceStartup-startTime));

			Debug.Log ("end parser test");
		}

		private void ParserEvaluateTest()
		{
			Debug.Log ("start evaluate test");

			var parser = new XPathParser ();

			float startTime = Time.realtimeSinceStartup;

			GameObject root = GameObject.Find ("TestText");

			var results = parser.Evaluate ("/button", root);

			Debug.Assert (results.Count == 1);
			Debug.Assert (results[0].name == "TestText");

			results = parser.Evaluate ("//button//text", root);

			Debug.Assert (results.Count == 1);
			Debug.Assert (results[0].name == "Text");

			results = parser.Evaluate ("/text", root);

			Debug.Assert (results.Count == 0);

			results = parser.Evaluate ("//text[@text]", root);

			Debug.Assert (results.Count == 1);
			Debug.Assert (results[0].name == "Text");

			results = parser.Evaluate ("/button[@text]", root);

			Debug.Assert (results.Count == 0);

			results = parser.Evaluate ("/button[@interactable]", root);

			Debug.Assert (results.Count == 1);
			Debug.Assert (results[0].name == "TestText");

			results = parser.Evaluate ("button", root);

			Debug.Assert (results.Count == 3);

			results = parser.Evaluate ("//button[@name=\"Second\"]", root);

			Debug.Assert (results.Count == 1);
			Debug.Assert (results[0].name == "Second");

			results = parser.Evaluate ("//button[@interactable=\"False\"]", root);

			Debug.Assert (results.Count == 1);
			Debug.Assert (results[0].name == "Third");

			results = parser.Evaluate ("//text[@text=\"some text overhere\"]", root);

			Debug.Assert (results.Count == 1);
			Debug.Assert (results[0].name == "Text");

			results = parser.Evaluate ("/button//button", root);

			Debug.Assert (results.Count == 2);

			Debug.Log ("time " + (Time.realtimeSinceStartup-startTime));

			Debug.Log ("end evaluate test");
		}

		private IEnumerator FindCommands()
		{
			Debug.Log ("start find commands");

			float startTime = Time.realtimeSinceStartup;
			
			string endPoint = "http://localhost:4569";

			string req = "{}";

			byte[] byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			WWW session = new WWW (endPoint + "/session", byteReq );
			yield return session;

			sessionId = SimpleJSON.JSON.Parse (session.text) ["sessionId"];

			Debug.Assert ( sessionId != null );

			req = "{\"using\":\"xpath\",\"value\":\"button\"}";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			WWW element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			var data = SimpleJSON.JSON.Parse (element.text) ["data"];

			var first = data [0];

			string name = first ["name"].ToString ();
			string firstButtonId = first [FindElementCommands.WebElementIdentifierKey];

			Debug.Assert ( firstButtonId != null);
		
	

			req = "{\"using\":\"xpath\",\"value\":\"button[1]\"}";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			var second = data [0];

			name = second ["name"].ToString ();

			var secondId = second [FindElementCommands.WebElementIdentifierKey];

			Debug.Assert (name.Equals( "\"Second\"" ));



			req = "{\"using\":\"xpath\",\"value\":\"//button\"}";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			element = new WWW (string.Format ("{0}/session/{1}/elements", endPoint, sessionId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.Count == 3);
			Debug.Assert ( firstButtonId.Equals( data[0][FindElementCommands.WebElementIdentifierKey] ) );


			req = "{\"using\":\"xpath\",\"value\":\"//button\"}";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/element", endPoint, sessionId, secondId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.Count == 1);
			Debug.Assert (data [0] ["name"].ToString ().Equals ("\"Second\""));


			req = "{\"using\":\"xpath\",\"value\":\"//button[@name=\\\"TestText\\\"]\"}";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.Count == 1);
			firstButtonId = data[0][FindElementCommands.WebElementIdentifierKey];



			req = "{\"using\":\"xpath\",\"value\":\"//button\"}";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/elements", endPoint, sessionId, firstButtonId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.Count == 3);
			Debug.Assert (data [1] ["name"].ToString ().Equals ("\"Second\""));


			//negative testing here
			req = "{\"using\":\"xpath\",\"value\":\"//muie_dna\"}";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			element = new WWW (string.Format ("{0}/session/{1}/element/", endPoint, sessionId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.Count == 0);

			Debug.Log ("time " + (Time.realtimeSinceStartup-startTime));

			Debug.Log ("end find commands");

			StartCoroutine (AttributesCommands ());
		}

		private IEnumerator AttributesCommands()
		{
			Debug.Log ("start attributes commands");

			float startTime = Time.realtimeSinceStartup;

			string endPoint = "http://localhost:4569";

			string req = "{\"using\":\"xpath\",\"value\":\"button\"}";

			byte[] byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			WWW element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			var data = SimpleJSON.JSON.Parse (element.text) ["data"];

			var first = data [0];

			string name = first ["name"].ToString ();
			string firstButtonId = first [FindElementCommands.WebElementIdentifierKey];

			Debug.Assert (name.Equals( "\"Second\"" ));


			req = "{\"using\":\"xpath\",\"value\":\"text[@text=\\\"some text overhere\\\"]\"}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			first = data [0];

			string textId = first [FindElementCommands.WebElementIdentifierKey];



			req = "";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/attribute/atributeCareNuExista", endPoint, sessionId, firstButtonId) );
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"].AsObject;

			Debug.Assert (data == null);


			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/attribute/interactable", endPoint, sessionId, firstButtonId) );
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.ToString() == "\"True\"" );



			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/attribute/text", endPoint, sessionId, textId) );
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.ToString() == "\"some text overhere\"" );


			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/attribute/color", endPoint, sessionId, textId) );
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.ToString() != null );


			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/name", endPoint, sessionId, textId) );
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.ToString().ToLower()=="\"text\"" );



			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/enabled", endPoint, sessionId, textId) );
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.ToString().ToLower()=="\"true\"" );


			GameObject.Find ("Text").GetComponent<Text>().enabled = false;

			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/enabled", endPoint, sessionId, textId) );
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.ToString().ToLower()=="\"false\"" );

			GameObject.Find ("Text").GetComponent<Text>().enabled = true;



			req = "{\"using\":\"xpath\",\"value\":\"button[@name=\\\"Second\\\"\"}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			string secondButtonId = data[0][FindElementCommands.WebElementIdentifierKey];


	

			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/click", endPoint, sessionId, secondButtonId) );
			yield return element;



			req = "{\"text\":\"text from test\"}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/value", endPoint, sessionId, textId ), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			first = data [0];

			req = "{\"using\":\"xpath\",\"value\":\"button[@name=\\\"Third\\\"]\"}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			first = data [0];

			name = first ["name"].ToString ();
			string thirdButtonId = first [FindElementCommands.WebElementIdentifierKey];

			Debug.Assert (name.Equals( "\"Third\"" ));

			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/highlight", endPoint, sessionId, thirdButtonId) );
			yield return element;

			Debug.Log ("time " + (Time.realtimeSinceStartup-startTime));

			Debug.Log ("end attributes commands");

            EndSession();

			StartCoroutine (TestTimeouts ());
		}

		private IEnumerator TestTimeouts()
		{
			Debug.Log ("start timeouts tests");

			float startTime = Time.realtimeSinceStartup;

			string endPoint = "http://localhost:4569";

			string req = "{\"capabilitites\":{}}";

			byte[] byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			WWW session = new WWW (endPoint + "/session", byteReq );
			yield return session;

			sessionId = SimpleJSON.JSON.Parse (session.text) ["sessionId"];

			Debug.Assert (WebDriverManager.instance.ImplicitTimeout == 0);
			Debug.Assert (WebDriverManager.instance.ScriptTimeout == 30000);
			Debug.Assert (WebDriverManager.instance.PageLoadTimeout == 300000);

			EndSession ();

			req = "{\"capabilities\":{\"implicit\":1000, \"page load\":2500, \"script\":456} }";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			session = new WWW (endPoint + "/session", byteReq );
			yield return session;

			sessionId = SimpleJSON.JSON.Parse (session.text) ["sessionId"];

			Debug.Assert (WebDriverManager.instance.ImplicitTimeout == 1000);
			Debug.Assert (WebDriverManager.instance.ScriptTimeout == 456);
			Debug.Assert (WebDriverManager.instance.PageLoadTimeout == 2500);

			WWW request = new WWW (endPoint + "/session/" + sessionId + "/timeouts");
			yield return request;

			var response = SimpleJSON.JSON.Parse (request.text);

			Debug.Assert ( Int64.Parse(response["data"]["implicit"]) == 1000);
			Debug.Assert ( Int64.Parse(response["data"]["script"]) == 456);
			Debug.Assert ( Int64.Parse(response["data"]["page load"]) == 2500);

			EndSession ();

			req = "{\"capabilities\":{\"implicit\":1000} }";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			session = new WWW (endPoint + "/session", byteReq );
			yield return session;

			sessionId = SimpleJSON.JSON.Parse (session.text) ["sessionId"];

			Debug.Assert (WebDriverManager.instance.ImplicitTimeout == 1000);
			Debug.Assert (WebDriverManager.instance.ScriptTimeout == 30000);
			Debug.Assert (WebDriverManager.instance.PageLoadTimeout == 300000);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts");
			yield return request;

			response = SimpleJSON.JSON.Parse (request.text);

			Debug.Assert ( Int64.Parse(response["data"]["implicit"]) == 1000);
			Debug.Assert ( Int64.Parse(response["data"]["script"]) == 30000);
			Debug.Assert ( Int64.Parse(response["data"]["page load"]) == 300000);

			//pushing timeouts with invalid arguments
			req = "asd";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts", byteReq );
			yield return request;

			string error = SimpleJSON.JSON.Parse( request.text) ["error"];

			Debug.Assert ( error == "invalid argument" );

			req = "{}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts", byteReq );
			yield return request;

			error = SimpleJSON.JSON.Parse( request.text) ["error"];

			Debug.Assert ( error == "invalid argument" );


			req = "{\"parameters:{}\"}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts", byteReq );
			yield return request;

			error = SimpleJSON.JSON.Parse( request.text) ["error"];

			Debug.Assert ( error == "invalid argument" );



			req = "{\"parameters\":{\"aasd\":10000}}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts", byteReq );
			yield return request;

			error = SimpleJSON.JSON.Parse( request.text) ["error"];

			Debug.Assert ( error == "invalid argument" );



			req = "{\"parameters\":{\"implicit\":-232}}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts", byteReq );
			yield return request;

			error = SimpleJSON.JSON.Parse( request.text) ["error"];

			Debug.Assert ( error == "invalid argument" );


			req = "{\"parameters\":{\"implicit\":34567}}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts", byteReq );
			yield return request;

			error = SimpleJSON.JSON.Parse( request.text) ["data"];

			Debug.Assert (error == "null");
			Debug.Assert (WebDriverManager.instance.ImplicitTimeout == 34567);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts");
			yield return request;

			response = SimpleJSON.JSON.Parse (request.text);

			Debug.Assert ( Int64.Parse(response["data"]["implicit"]) == 34567);
			Debug.Assert ( Int64.Parse(response["data"]["script"]) == 30000);
			Debug.Assert ( Int64.Parse(response["data"]["page load"]) == 300000);

			req = "{\"parameters\":{\"implicit\":34567, \"page load\":435}}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts", byteReq );
			yield return request;

			error = SimpleJSON.JSON.Parse( request.text) ["data"];

			Debug.Assert (error == "null");
			Debug.Assert (WebDriverManager.instance.ImplicitTimeout == 34567);

			request = new WWW (endPoint + "/session/" + sessionId + "/timeouts");
			yield return request;

			response = SimpleJSON.JSON.Parse (request.text);

			Debug.Assert ( Int64.Parse(response["data"]["implicit"]) == 34567);
			Debug.Assert ( Int64.Parse(response["data"]["script"]) == 30000);
			Debug.Assert ( Int64.Parse(response["data"]["page load"]) == 435);

			EndSession ();

			Debug.Log ("time " + (Time.realtimeSinceStartup-startTime));

			Debug.Log ("end timeout tests");

			StartCoroutine (FindElementsWithTimeouts ());
		}

		private IEnumerator CreateButton( string name, float when )
		{
			yield return new WaitForSeconds (when);

			GameObject go = new GameObject (name);
			go.AddComponent<Button> ();
		}

		private IEnumerator FindElementsWithTimeouts()
		{
			Debug.Log ("start find elements timeouts tests");

			float startTime = Time.realtimeSinceStartup;

			string endPoint = "http://localhost:4569";

			string req = "{\"capabilities\":{\"implicit\":2500, \"page load\":1500, \"script\":1500} }";

			byte[] byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			WWW session = new WWW (endPoint + "/session", byteReq );
			yield return session;

			sessionId = SimpleJSON.JSON.Parse (session.text) ["sessionId"];

			//set creation of element in 1.5 seconds
			StartCoroutine( CreateButton("TimeoutElement", 1.5f) );

			req = "{\"using\":\"xpath\",\"value\":\"button[@name=\\\"TimeoutElement\\\"]\"}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			WWW element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			var data = SimpleJSON.JSON.Parse (element.text) ["data"];

			var first = data [0];

			name = first ["name"].ToString ();
			string thirdButtonId = first [FindElementCommands.WebElementIdentifierKey];

			Debug.Assert (name.Equals( "\"TimeoutElement\"" ));


			//set creation of element in 3 seconds
			StartCoroutine( CreateButton("54", 3f) );

			req = "{\"using\":\"xpath\",\"value\":\"button[@name=\\\"54\\\"]\"}";

			byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["error"];

			Debug.Assert (data.Value == "no such element");

			EndSession ();

			Debug.Log ("time " + (Time.realtimeSinceStartup-startTime));

			Debug.Log ("end find elements timeout tests");
		}

        private void EndSession()
        {
            WebDriverManager.instance.DeleteSession(this.sessionId, null);
        }
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}