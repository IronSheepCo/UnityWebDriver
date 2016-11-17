using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.IO;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverManager
	{
		private static WebDriverManager _instance;

		private static object instanceLock = new Object();

		private HttpListener listener;

		private string sessionId = null;

		private WebDriverManager()
		{
			listener = new HttpListener ();

			listener.Prefixes.Add ("http://*:8080/");

			StartWebDriver ();
		}

		private void StartWebDriver()
		{
			listener.Start ();

			ThreadPool.QueueUserWorkItem ((_) => {
				while( true )
				{
					HttpListenerContext ctx = listener.GetContext();

					ThreadPool.QueueUserWorkItem( (idontcare)=>{
						string command = ctx.Request.Url.Segments[1].Replace("/","");

						var request = ctx.Request;

						var response = ctx.Response;

						var body = new StreamReader( request.InputStream ).ReadToEnd();

						var args = request.Url.Segments.Skip(2);

						switch( command ){
						case "status":
							break;
						case "session":
							switch( args.Count() ){
							case 0:
								switch( request.HttpMethod ){
								case "GET":
									break;
								case "DELETE":
									break;
								default:
									
									break;
								}
								break;
							case 1:
								break;
							default:
								break;
							}
							break;
						default:
								break;
						}
					});
				}
			});
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

		public void Shutdown(){
			listener.Stop ();
		}
	}
}