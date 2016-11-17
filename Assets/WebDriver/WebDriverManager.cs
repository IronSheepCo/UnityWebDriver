using UnityEngine;
using System.Collections;
using System.Net;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverManager
	{
		private static WebDriverManager _instance;

		private static object instanceLock = new Object();

		private HttpListener listener;


		private WebDriverManager()
		{
			listener = new HttpListener ();

			listener.Prefixes.Add ("http://*:8080/");

			listener.Start ();
		}

		public static WebDriverManager instance {
			get{
				if (_instance == null) 
				{
					lock (instanceLock) 
					{
						if (_instance == null) 
						{
							_instance = new WebDriverManager ();
						}
					}
				}

				return _instance;
			}
		}
	}
}