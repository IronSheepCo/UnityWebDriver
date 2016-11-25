using UnityEngine;
using System.Collections;

using tech.ironsheep.WebDriver.Dispatch;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverInit : MonoBehaviour {

		// Use this for initialization
		void Start () {
			//init the webdriver server
			var instance = WebDriverManager.instance;
			FindElementCommands.Init ();
			MainDispatcher.ExecuteBlocking (() => {
				Debug.Log("started dispatcher");
			});
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		void OnDestroy() {
			WebDriverManager.instance.Shutdown ();
		}
	}
}