using UnityEngine;
using System.Collections;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverManager
	{
		private static WebDriverManager _instance;

		private WebDriverManager()
		{
			
		}

		public static WebDriverManager instance {
			get{
				if (_instance == null) 
				{
					lock (_instance) 
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