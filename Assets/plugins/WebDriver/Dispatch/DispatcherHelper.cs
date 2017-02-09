using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

public class DispatcherHelper : MonoBehaviour {

	private List<Action> actions = new List<Action>();
	private List<IEnumerator> coroutines = new List<IEnumerator>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		lock (actions) {
			
			foreach (Action ac in actions) {
				try{
					ac ();
				}catch(Exception e) {
					Debug.LogError (e);
				}
			}

			actions = new List<Action> ();
		}

		lock (coroutines) {
			foreach (IEnumerator en in coroutines) {
				StartCoroutine (en);
			}

			coroutines = new List<IEnumerator> ();
		}
	}

	public void EnqueCo( IEnumerator en )
	{
		lock (coroutines) {
			coroutines.Add (en);
		}
	}

	public void EnqueAction( Action ac )
	{
		lock (actions) {
			actions.Add (ac);
		}
	}
}
