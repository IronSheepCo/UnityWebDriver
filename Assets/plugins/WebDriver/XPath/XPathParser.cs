using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using tech.ironsheep.WebDriver.Lexer;

namespace tech.ironsheep.WebDriver.XPath
{
	public class XPathParser{

		//list of all the steps in the XPath
		//we go through the list one node at the time
		//and build a Node List
		//when reaching the last node, we return the remaining list
		private List<XPathNode> steps = new List<XPathNode>();

		//current node set Evaluate is working on
		private List<GameObject> currentSet = new List<GameObject> ();

		private List<Assembly> assemblies = new List<Assembly> ();

		//Dictionary from class name to type
		private Dictionary<string, Type> classNameToType = new Dictionary<string, Type> ();

		private Type lastTypeUsed;

		private Lexer.Lexer _lexer = new Lexer.Lexer ();

		public XPathParser()
		{
			//get all assemblies
			var assemblies = AppDomain.CurrentDomain.GetAssemblies ();

			//only use UnityEngine
			//and C# assemblies
			foreach (var ass in assemblies) 
			{
				string name = ass.GetName().Name;

				if (name == "UnityEngine" ||
					name == "UnityEngine.UI" ||
					name == "Assembly-CSharp-firstpass") 
				{
					this.assemblies.Add (ass);
				}
			}
		}

		public List<XPathNode> Parse( string xPath )
		{
			List<XPathNode> ret = new List<XPathNode> ();

			//adding // if the path doesn't 
			if (xPath.StartsWith ("/") == false && xPath.StartsWith ("//") == false) 
			{
				xPath = "//" + xPath;
			}

			List<Token> tokens = _lexer.Tokenize (xPath);

			int currentIndex = 0;

			while (currentIndex < tokens.Count) 
			{
				//searching for ancestry
				XPathNode newNode = new XPathNode();
				newNode.IsChild = true;

				if (tokens [currentIndex].Desc == "ANCESTOR") 
				{
					newNode.IsChild = false;
				}

				currentIndex++;

				while (tokens [currentIndex].Desc == "WHITE_SPACE") 
				{
					currentIndex++;
				}

				newNode.TagName = tokens [currentIndex].Content;

				currentIndex++;

				while (currentIndex < tokens.Count && tokens [currentIndex].Desc == "WHITE_SPACE") 
				{
					currentIndex++;
				}

				//check for attributes
				if (currentIndex < tokens.Count && tokens [currentIndex].Desc == "OPEN_PREDICATE") 
				{
					//maybe the input is broken and we don't have 
					//a closing bracket
					while( currentIndex < tokens.Count && tokens [currentIndex].Desc != "END_PREDICATE" )
					{
						currentIndex++;

						while (currentIndex < tokens.Count && tokens [currentIndex].Desc == "WHITE_SPACE") 
						{
							currentIndex++;
						}

						//number
						if (tokens [currentIndex].Desc == "NUMBER") 
						{
							var numberPredicate = new XPathNumberPredicate ();
							numberPredicate.Number = int.Parse( tokens[ currentIndex ].Content );

							newNode.predicates.Add (numberPredicate);

							currentIndex++;

							while (currentIndex < tokens.Count && tokens [currentIndex].Desc == "WHITE_SPACE") 
							{
								currentIndex++;
							}

							//we have some junk in here
							//we gonna parse until here and bail
							if (tokens [currentIndex].Desc != "END_PREDICATE") 
							{
								ret.Add (newNode);
								break;
							}
						}

						//attribute
						if (tokens [currentIndex].Desc == "ATTRIBUTE") 
						{
							XPathAttribute attribute = new XPathAttribute ();
							attribute.Name = tokens [currentIndex].Content;
							newNode.predicates.Add (attribute);

							currentIndex++;

							while (currentIndex < tokens.Count && tokens [currentIndex].Desc == "WHITE_SPACE") 
							{
								currentIndex++;
							}

							//maybe we have a test for existence of attribute
							if (tokens [currentIndex].Desc != "END_PREDICATE") {

								//how to compare
								if (tokens [currentIndex].Desc == "EQUAL") {

								}

								currentIndex++;

								while (currentIndex < tokens.Count && tokens [currentIndex].Desc == "WHITE_SPACE") {
									currentIndex++;
								}

								attribute.ValueToMatch = tokens [currentIndex].Content.Replace("\\n","\n");

								currentIndex++;

								while (currentIndex < tokens.Count && tokens [currentIndex].Desc == "WHITE_SPACE") {
									currentIndex++;
								}

								if (currentIndex < tokens.Count && tokens [currentIndex].Desc == "AND") {
									currentIndex++;
								}
							}
						}
					}

					currentIndex ++;
				}

				ret.Add (newNode);
			}

			return ret;
		}

		private List<Component> EvaluateStep( XPathNode step, List<GameObject> nodeSet, bool treatChildAsSelf = false )
		{
			List<Component> ret = new List<Component> ();

			HashSet<Component> added = new HashSet<Component> ();

			foreach (var node in nodeSet) 
			{
				//get the type for the current tag
				Type nodeType = FindType( step.TagName );

				lastTypeUsed = nodeType;

				//couldn't find the type for this node
				//return null
				if (nodeType == null) 
				{
					return new List<Component> ();
				}

				List<Component> results = new List<Component>();

				if (step.IsChild) 
				{
					if (treatChildAsSelf) 
					{
						var component = node.gameObject.GetComponent (nodeType);

						if (component != null) 
						{
							results.Add (component);
						}
					} 
					else 
					{
						//look for tag name in children	
						for (int i = 0; i < node.transform.childCount; i++) {
							var child = node.transform.GetChild (i).gameObject;

							var component = child.GetComponent (nodeType);

							if (component != null) {
								results.Add (component);
							}
						}
					}
				} 
				else 
				{
					//look for tag name in all descendents
					var res = node.GetComponentsInChildren( nodeType );

					if( res != null )
					{
						results = res.ToList();

						//remove self from the list
						if (!treatChildAsSelf && node.GetComponent (nodeType)) 
						{
							results.Remove (node.GetComponent (nodeType));
						}
					}
				}

				foreach (var comp in results) 
				{
					//this may happen because Unity
					//doesn't have a root object(s)
					//so we need to rely on GetObjectsOfType
					//which return not only the root, but all objects
					//in a random order
					if (added.Contains (comp)) 
					{
						continue;
					}

					ret.Add (comp);	
					added.Add (comp);
				}

				//need to use the predicates
				foreach (var predicate in step.predicates) 
				{
					var retGo = ret.Select (comp => comp.gameObject).ToList();
					ret = predicate.Evaluate ( retGo, nodeType).Select( go => go.GetComponent( nodeType ) ).ToList();
				}
			}

			return ret;
		}

		//Evaluates the xpath expression in the context of root
		public List<Component> Evaluate( string xPath, GameObject root )
		{
			steps = Parse (xPath);

			currentSet = new List<GameObject> ();
			currentSet.Add (root);

			//need to do things a little
			//bit different for the first 
			//step
			var firstStep = steps [0];

			currentSet = EvaluateStep (firstStep, currentSet, true).Select( comp => comp.gameObject ).ToList();

			//removing the first step
			steps.Remove (firstStep);

			//evaluate each steps in the current context
			foreach (var step in steps) 
			{
				currentSet = EvaluateStep( step, currentSet ).Select( comp => comp.gameObject ).ToList();
			}

			return currentSet.Select( go => go.GetComponent(lastTypeUsed) ).ToList();
		}

		public Type FindType( string className )
		{
			string lower = className.ToLower ();

			if (classNameToType.ContainsKey (lower)) 
			{
				return classNameToType [lower];
			}

			//search all assemblies
			foreach (var assembly in assemblies) 
			{
				foreach (var t in assembly.GetTypes ()) 
				{
					if (t.Name.ToLower() == lower) 
					{
						//add it to cache
						classNameToType [lower] = t;

						return t;
					}
				}
			}

			return null;
		}
	}
}