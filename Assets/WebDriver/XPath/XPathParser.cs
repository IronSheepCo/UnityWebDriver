using UnityEngine;
using System;
using System.Collections.Generic;

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

		//next set of parsing nodes
		private List<GameObject> nextSet = new List<GameObject> ();

		public List<XPathNode> Parse( string xPath )
		{
			List<XPathNode> ret = new List<XPathNode> ();
			
			//adding // if the path doesn't 
			if (xPath.StartsWith ("/") == false && xPath.StartsWith ("//") == false) 
			{
				xPath = "//" + xPath;
			}

			int currentIndex = 0;
			int length = xPath.Length;

			//use path to get the nodes and the 
			//attributes
			while (currentIndex < length) 
			{
				XPathNode newNode = new XPathNode ();

				//we have a node that's not a child
				if (xPath [currentIndex] == '/' && xPath [currentIndex + 1] == '/') 
				{
					newNode.IsChild = false;

					currentIndex += 2;
				}

				if (xPath [currentIndex] == '/' && xPath [currentIndex + 1] != '/') 
				{
					newNode.IsChild = true;

					currentIndex++;
				}

				//get the node
				int endIndex = xPath.IndexOf( "/", currentIndex );

				//end of string
				if (endIndex == -1) 
				{
					endIndex = xPath.Length;
				}
						
				//node info here
				string nodeInfo = xPath.Substring (currentIndex, endIndex-currentIndex);

				currentIndex = endIndex;

				//look for predicates
				if (nodeInfo.Contains ("[")) 
				{
					//get the attribute infor from it
					int predicateStartIndex = nodeInfo.IndexOf("[")+1;
					int predicateEndIndex = nodeInfo.IndexOf ("]");

					//malformed predicate
					if (predicateEndIndex == -1) 
					{
						predicateEndIndex = nodeInfo.Length;
					}

					string predicates = nodeInfo.Substring (predicateStartIndex, predicateEndIndex - predicateStartIndex);

					//need to split the predicate by AND
					//we only support AND at the moment
					var splitPredicates = predicates.Split( new string[]{ "and" }, StringSplitOptions.RemoveEmptyEntries );

					foreach (var predicate in splitPredicates) 
					{
						var trimedPredicate = predicate.Trim ();

						//the predicate is an attributes
						if (trimedPredicate.StartsWith ("@")) 
						{
							var attribute = XPathAttribute.fromString (trimedPredicate);

							newNode.predicates.Add (attribute);
						}

						int predicateNumber;
						// the predicate is a number in the array
						if ( int.TryParse(trimedPredicate, out predicateNumber) ) 
						{
							var numberPredicate = new XPathNumberPredicate ();
							numberPredicate.Number = predicateNumber;

							newNode.predicates.Add (numberPredicate);
						}
					}
						
					//we get the node tag name
					nodeInfo = nodeInfo.Substring (0, predicateStartIndex-1);
				}

				newNode.TagName = nodeInfo;

				ret.Add (newNode);
			}

			return ret;
		}

		//Evaluates the xpath expression in the context of root
		public List<GameObject> Evaluate( string xPath, GameObject root )
		{
			return null;
		}
	}
}