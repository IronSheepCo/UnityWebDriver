using UnityEngine;
using System.Collections;

using tech.ironsheep.WebDriver.Dispatch;
using System.Net;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverInit : MonoBehaviour {

        const float TIME_BETWEEN_BROADCASTS = 1f;
        float timeLeftForBroadcast = TIME_BETWEEN_BROADCASTS;
        private bool _sessionStarted = false;
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

            WebDriverManager.instance.SessionStarted += HandleSessionStarted;
		}

        private void HandleSessionStarted(bool state)
        {
            this._sessionStarted = state;
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            if (this._sessionStarted == false)
            {
                timeLeftForBroadcast -= Time.deltaTime;
                if (timeLeftForBroadcast < 0)
                {
                    timeLeftForBroadcast = TIME_BETWEEN_BROADCASTS;
                    ClientSearch.ClientSearch.BroadcastAppReady();
                }
            }
        }
               

        //void OnGUI()
        //{
        //    if (GUI.Button(new Rect(100f, 100f, 100f, 50f), "App Ready!"))
        //    {
        //        Debug.Log(ClientSearch.ClientSearch.BroadcastAppReady().ToString());
        //    }
        //}

		void OnDestroy() {
			WebDriverManager.instance.Shutdown ();
            WebDriverManager.instance.SessionStarted -= HandleSessionStarted;
		}
	}
}