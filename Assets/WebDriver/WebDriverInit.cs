using UnityEngine;
using System.Collections;

using tech.ironsheep.WebDriver.Dispatch;
using System.Net;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverInit : MonoBehaviour {

		// Use this for initialization
		void Start () {
			//init the webdriver server
			var instance = WebDriverManager.instance;
			FindElementCommands.Init ();
			ElementAttributeCommands.Init ();
			ElementInteractCommands.Init ();
			MainDispatcher.ExecuteBlocking (() => {
				Debug.Log("started dispatcher");
			});

            
		}

        void OnGUI()
        {
            if (GUI.Button(new Rect(100f, 100f, 100f, 50f), "App Ready!"))
            {
                Debug.Log(ClientSearch.ClientSearch.BroadcastAppReady().ToString());
            }
        }
		
		// Update is called once per frame
		void Update () {
		
		}

		void OnDestroy() {
			WebDriverManager.instance.Shutdown ();
		}
	}
}