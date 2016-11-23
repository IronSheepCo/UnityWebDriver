using UnityEngine;
using System.Collections;

using tech.ironsheep.WebDriver.XPath;

namespace tech.ironsheep.WebDriver.Tests
{
	public class XPathParserTests : MonoBehaviour {

		// Use this for initialization
		void Start () {
			ParserTests ();
			ParserEvaluateTest ();
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

			GameObject root = GameObject.Find ("/TestText");

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
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}