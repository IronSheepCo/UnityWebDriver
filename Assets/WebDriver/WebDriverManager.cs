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

		private void WriteResponse( HttpListenerResponse response, string body, int code )
		{
			response.StatusCode = code;

			var writer = new StreamWriter (response.OutputStream);
			writer.Write (body);
			writer.Close ();
		}

		private void RespondUnkownMethod( HttpListenerResponse response )
		{
			var responseBody = @"{
				""error"":""unknown method"",
				""message"":""method not defined for command"",
				""stacktrace"":""""
			}";

			WriteResponse (response, responseBody, 405);
		}

		private void NewSession( string body, HttpListenerResponse response )
		{
			//session already started
			if (sessionId != null) 
			{
				var responseBody = @"{
					""error"":""session not created"",
					""message"":""A session is already started"",
					""stacktrace"":""""
				}";

				WriteResponse (response, responseBody, 500);

				return;
			}

			//need to start a session
			sessionId = System.Guid.NewGuid().ToString();

			var reBody = string.Format( "{\"sessionId\":\"{0}\",\"capabilities\":\"\"}", sessionId);

			WriteResponse (response, reBody, 200);
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
							switch( request.HttpMethod ){
							case "GET":
								break;
							default:
								RespondUnkownMethod( response );
								break;
							}
							break;
						case "session":
							switch( args.Count() ){
							case 0:
								switch( request.HttpMethod ){
								case "POST":
									NewSession( body, response );
									break;
								default:
									RespondUnkownMethod( response );
									break;
								}
								break;
							case 1:
								if( request.HttpMethod == "DELETE" )
								{
								}
								else
								{
									RespondUnkownMethod( response );
								}
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