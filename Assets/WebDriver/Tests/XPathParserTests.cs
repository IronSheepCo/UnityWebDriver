using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Net;
using System.Text;

using tech.ironsheep.WebDriver.XPath;

namespace tech.ironsheep.WebDriver.Tests
{
	public class XPathParserTests : MonoBehaviour {

		private string sessionId;

		// Use this for initialization
		void Start () {
			ParserTests ();
			ParserEvaluateTest ();

			StartCoroutine( FindCommands () );
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
			
			string endPoint = "http://localhost:8080";

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

			Debug.Assert (name.Equals( "\"TestText\"" ));
		
	

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


			req = "{\"using\":\"xpath\",\"value\":\"//button\"}";
			byteReq = ASCIIEncoding.ASCII.GetBytes ( req );

			element = new WWW (string.Format ("{0}/session/{1}/element/{2}/elements", endPoint, sessionId, firstButtonId), byteReq);
			yield return element;

			data = SimpleJSON.JSON.Parse (element.text) ["data"];

			Debug.Assert (data.Count == 3);
			Debug.Assert (data [1] ["name"].ToString ().Equals ("\"Second\""));

			Debug.Log ("time " + (Time.realtimeSinceStartup-startTime));

			Debug.Log ("end find commands");

			StartCoroutine (AttributesCommands ());
		}

		private IEnumerator AttributesCommands()
		{
			Debug.Log ("start attributes commands");

			float startTime = Time.realtimeSinceStartup;

			string endPoint = "http://localhost:8080";

			string req = "{\"using\":\"xpath\",\"value\":\"button\"}";

			byte[] byteReq = ASCIIEncoding.ASCII.GetBytes (req);

			WWW element = new WWW (string.Format ("{0}/session/{1}/element", endPoint, sessionId), byteReq);
			yield return element;

			var data = SimpleJSON.JSON.Parse (element.text) ["data"];

			var first = data [0];

			string name = first ["name"].ToString ();
			string firstButtonId = first [FindElementCommands.WebElementIdentifierKey];

			Debug.Assert (name.Equals( "\"TestText\"" ));


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


			Debug.Log ("time " + (Time.realtimeSinceStartup-startTime));

			Debug.Log ("end attributes commands");
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}