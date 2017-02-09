using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace tech.ironsheep.WebDriver.Dispatch
{
	public class MainDispatcher{
		//use to force blocking behaviour
		private static Dictionary<string, bool> busyWaitingFlags = new Dictionary<string, bool> ();

		private static DispatcherHelper realDispatcher;

		private static Thread uiThread;

		static MainDispatcher()
		{
			var go = new GameObject ("ui dispatcher");
			go.AddComponent<DispatcherHelper> ();

			GameObject.DontDestroyOnLoad (go);

			realDispatcher = go.GetComponent<DispatcherHelper>();

			uiThread = System.Threading.Thread.CurrentThread;
		}

		public static void ExecuteCoroutine( IEnumerator coroutine )
		{
			realDispatcher.EnqueCo (coroutine);
		}

		public static void ExecuteBlocking( Action ac )
		{
			//check if we're on the main thread
			if (System.Threading.Thread.CurrentThread == uiThread) 
			{
				ac ();
				return;
			}

			string uid = System.Guid.NewGuid ().ToString();

			busyWaitingFlags [uid] = false;

			realDispatcher.EnqueAction (() => {
				try
				{
					ac();
				}
				catch(Exception e)
				{
					Debug.LogError(e);
				}
				finally
				{
					busyWaitingFlags[uid] = true;
				}
			});

			//busy waiting
			while (busyWaitingFlags [uid] == false) 
			{
			}

			//done
		}

	}
}