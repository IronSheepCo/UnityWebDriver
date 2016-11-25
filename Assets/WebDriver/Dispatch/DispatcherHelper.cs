using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

public class DispatcherHelper : MonoBehaviour {

	private List<Action> actions = new List<Action>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		lock (actions) {
			
			foreach (Action ac in actions) {
				ac ();
			}

			actions = new List<Action> ();
		}
	}

	public void EnqueAction( Action ac )
	{
		lock (actions) {
			actions.Add (ac);
		}
	}
}
