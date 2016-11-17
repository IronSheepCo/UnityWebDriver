using UnityEngine;
using System.Collections;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverInit : MonoBehaviour {

		// Use this for initialization
		void Start () {
			//init the webdriver server
			var instance = WebDriverManager.instance;
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}