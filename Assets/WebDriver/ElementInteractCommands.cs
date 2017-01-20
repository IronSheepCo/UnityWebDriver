using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

using tech.ironsheep.WebDriver.Dispatch;

namespace tech.ironsheep.WebDriver
{
	public class ElementInteractCommands: BaseCommand
	{
		public static void Init()
		{
			WebDriverManager.instance.RegisterCommand ("element", "GET", ClickElement, "^[^/]*/click$");
			WebDriverManager.instance.RegisterCommand ("element", "POST", SendKeys, "^[^/]*/value$");
			WebDriverManager.instance.RegisterCommand ("element", "GET", HighlightElement, "^[^/]*/highlight$");
			WebDriverManager.instance.RegisterCommand ("element", "GET", IsVisible, "^[^/]*/visible$");
		}

		private static Material _highlightMat;
		private static String highlightId = "webdriver_highlight";

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
				comp.gameObject.SendMessage ("set_text", text);
			}

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);

			return true;
		}

		private static IEnumerator HighlightStuff()
		{
			float a = 0;
			float change = 0.02f;

			for( int i = 0; i < 200; i++ )
			{
				if (a > 1f || a < 0f) 
				{
					change = -change;
				}

				_highlightMat.SetColor ("_Color", new Color (1.0f, 0.0f, 0.0f, a));

				a += change;

				yield return null;
			}

			//remove highlighter
			GameObject.DestroyObject( GameObject.Find( highlightId ) );
		}

		public static bool HighlightElement( string body, string[] args, HttpListenerResponse response )
		{
			Component comp = GetComponent (args, response);

			if (comp == null) 
			{
				return true;
			}

			string responseBody = "{\"data\":null}";

			//highlight the element now
			float xMin = float.MaxValue;
			float xMax = float.MinValue;
			float yMin = float.MaxValue;
			float yMax = float.MinValue;
			float zMin = float.MaxValue;
			float zMax = float.MinValue;

			Camera cam = Camera.main;

			GameObject go = comp.gameObject;

			//list with all the bounds
			List<Bounds> bounds = new List<Bounds> ();
			List<Transform> transforms = new List<Transform> ();

			//bounds from colliders
			Collider[] colliders = go.GetComponentsInChildren<Collider> ();

			foreach( var c in colliders )
			{
				bounds.Add (c.bounds);
				transforms.Add (c.gameObject.transform);
			}

			//bounds from renderers
			Renderer[] renderers = go.GetComponentsInChildren<Renderer> ();

			foreach (var r in renderers) 
			{
				bounds.Add (r.bounds);
				transforms.Add (r.gameObject.transform);
			}

			//for each bounds get the min and max
			//expand the bounding box
			var transformEnumerator = transforms.GetEnumerator();

			foreach (Bounds bound in bounds) 
			{
				transformEnumerator.MoveNext ();

				Vector3 min = transformEnumerator.Current.TransformPoint(bound.min);
				Vector3 max = transformEnumerator.Current.TransformPoint(bound.max);

				if (min.x < xMin)
					xMin = min.x;

				if (min.y < yMin)
					yMin = min.y;

				if (max.x > xMax)
					xMax = max.x;

				if (max.y > yMax)
					yMax = max.y;

				if (min.z < zMin)
					zMin = min.z;

				if (max.z > zMax)
					zMax = max.z;
			}

			RectTransform[] rectTransforms = go.GetComponentsInChildren<RectTransform> ();

			Vector3[] corners = new Vector3[4];

			foreach (var t in rectTransforms) 
			{
				t.GetWorldCorners (corners);

				if (corners [0] [0] < xMin)
					xMin = corners [0] [0];

				if (corners [0] [1] < yMin)
					yMin = corners [0] [1];

				if (corners [2] [0] > xMax)
					xMax = corners [3] [0];

				if (corners [2] [1] > yMax)
					yMax = corners [2] [1];

				foreach (var c in corners) 
				{
					if (c.z < zMin)
						zMin = c.z;

					if (c.z > zMax)
						zMax = c.z;
				}
			}

			Debug.Log (string.Format ("xMin:{0} xMax:{1} yMin:{2} yMax:{3}", xMin, xMax, yMin, yMax));

			GameObject selection = GameObject.CreatePrimitive (PrimitiveType.Quad);
			selection.name = highlightId;

			Vector3 topLeftCorner = new Vector3 (xMin, yMin, zMin);
			Vector3 bottomRightCorner = new Vector3 (xMax, yMax, zMin);

			Vector3 scale = selection.transform.localScale;
			scale.x = bottomRightCorner.x - topLeftCorner.x;
			scale.y = bottomRightCorner.y - topLeftCorner.y;

			Vector3 position = selection.transform.position;
			position.z = zMin;

			selection.transform.localScale = scale;
			selection.transform.position = position;

			_highlightMat = GameObject.Instantiate( Resources.Load<Material> ("WDHighlight") ); 
			selection.GetComponent<MeshRenderer> ().material = _highlightMat;

			MainDispatcher.ExecuteCoroutine (HighlightStuff());

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);

			return true;
		}

		public static bool IsVisible( string body, string[] args, HttpListenerResponse response )
		{
			Component comp = GetComponent (args, response);

			if (comp == null) 
			{
				return true;
			}

			string responseBody = "";

			//highlight the element now
			float xMin = float.MaxValue;
			float xMax = float.MinValue;
			float yMin = float.MaxValue;
			float yMax = float.MinValue;
			float zMin = float.MaxValue;
			float zMax = float.MinValue;

			GameObject go = comp.gameObject;

			//list with all the bounds
			List<Bounds> bounds = new List<Bounds> ();
			List<Transform> transforms = new List<Transform> ();

			//bounds from colliders
			Collider[] colliders = go.GetComponentsInChildren<Collider> ();

			foreach( var c in colliders )
			{
				bounds.Add (c.bounds);
				transforms.Add (c.gameObject.transform);
			}

			//bounds from renderers
			Renderer[] renderers = go.GetComponentsInChildren<Renderer> ();

			foreach (var r in renderers) 
			{
				bounds.Add (r.bounds);
				transforms.Add (r.gameObject.transform);
			}

			bool visible = false;

			//for each camera in the scene
			foreach (Camera cam in Camera.allCameras) {

				if (cam.isActiveAndEnabled == false) 
				{
					continue;
				}

				//check if the current layer is rendered by it
				if ((cam.cullingMask & (1 << go.layer)) == 0) 
				{
					continue;
				}

				xMin = yMin = zMin = float.MaxValue;
				xMax = yMax = zMax = float.MinValue;

				//for each bounds get the min and max
				//expand the bounding box
				var transformEnumerator = transforms.GetEnumerator ();

				foreach (Bounds bound in bounds) {
					transformEnumerator.MoveNext ();

					Vector3 min = cam.WorldToViewportPoint (bound.min);
					Vector3 max = cam.WorldToViewportPoint (bound.max);

					if (min.x < xMin)
						xMin = min.x;

					if (min.y < yMin)
						yMin = min.y;

					if (max.x > xMax)
						xMax = max.x;

					if (max.y > yMax)
						yMax = max.y;

					if (min.z < zMin)
						zMin = min.z;

					if (max.z > zMax)
						zMax = max.z;
				}
				RectTransform[] rectTransforms = go.GetComponentsInChildren<RectTransform> ();

				Vector3[] corners = new Vector3[4];

				foreach (var t in rectTransforms) {

					t.GetWorldCorners (corners);

					for (int i = 0; i < corners.Length; i++) {
						corners [i] = cam.WorldToViewportPoint (corners [i]);
					}

					if (corners [0] [0] < xMin)
						xMin = corners [0] [0];

					if (corners [0] [1] < yMin)
						yMin = corners [0] [1];

					if (corners [2] [0] > xMax)
						xMax = corners [3] [0];

					if (corners [2] [1] > yMax)
						yMax = corners [2] [1];

					foreach (var c in corners) {
						if (c.z < zMin)
							zMin = c.z;

						if (c.z > zMax)
							zMax = c.z;
					}
				}

				//if the element is partial in the screen
				//then it is visible

				if (xMin <= 1 && xMax >= 0 &&
					yMin <= 1 && yMax >= 0 &&
					zMin <= cam.farClipPlane && zMax >= cam.nearClipPlane ) 
				{
					visible = true;
					break;
				}

			}

			if( visible )
			{
				responseBody = "{\"data\":true}";
			} 
			else 
			{
				responseBody = "{\"data\":false}";
			}

			WebDriverManager.instance.WriteResponse (response, responseBody, 200);

			return true;
		}
	}
}