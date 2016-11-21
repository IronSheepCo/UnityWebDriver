using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.IO;
using System;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverManager
	{
		private static WebDriverManager _instance;

		private static object instanceLock = new System.Object();

		private HttpListener listener;

		private string sessionId = null;

		private Dictionary< string, Dictionary<string, Func<string, string[], HttpListenerResponse, bool> > > commands = new Dictionary< string, Dictionary<string, Func<string, string[], HttpListenerResponse, bool> > >();

		private WebDriverManager()
		{
			listener = new HttpListener ();

			listener.Prefixes.Add ("http://*:8080/");

			StartWebDriver ();
		}

		public void WriteResponse( HttpListenerResponse response, string body, int code )
		{
			response.StatusCode = code;

			var writer = new StreamWriter (response.OutputStream);
			writer.Write (body);
			writer.Close ();
		}

		public void RespondUnkownMethod( HttpListenerResponse response )
		{
			var responseBody = @"{
				""error"":""unknown method"",
				""message"":""method not defined for command"",
				""stacktrace"":""""
			}";

			WriteResponse (response, responseBody, 405);
		}

		private void BadSessionId( string body, HttpListenerResponse response )
		{
			string responseBody = @"{
					""error"":""No session with that id""
				}";

			WriteResponse (response, responseBody, 400);
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

			Debug.Log( string.Format("started session with id {0}", sessionId ) );
				
			var reBody = string.Format( "{{ \"sessionId\":\"{0}\",\"capabilities\":{{}} }}", sessionId);

			WriteResponse (response, reBody, 200);
		}

		private void DeleteSession( string sessId, HttpListenerResponse response )
		{
			string responseBody;
			
			if (sessionId == null || sessionId != sessId) 
			{
				BadSessionId ("", response);

				return;
			}

			responseBody = @"{
				""data"":null
			}";

			sessionId = null;

			WriteResponse (response, responseBody, 200);
		}

		private void RespondToStatus( string body, HttpListenerResponse response )
		{
			string responseBody;

			if (sessionId != null) 
			{
				responseBody = @"{
					""ready"":false,
					""message"":""A session is already opened""
				}";

				WriteResponse (response, responseBody, 400);

				return;
			}

			responseBody = @"{
				""ready"":true,
				""message"":""Ready for session""
			}";

			WriteResponse (response, responseBody, 200);
		}

		private void CommandNotImplemented( string command, HttpListenerResponse response )
		{
			var responseBody = string.Format ("{{ \"error\":\"Command {0} not implemented \" }}", command);

			WriteResponse (response, responseBody, 400);
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

						var args = request.Url.Segments.Skip(2).Select( x => x.Replace("/","") ).ToArray<string>();

						switch( command ){
						case "status":
							switch( request.HttpMethod ){
							case "GET":
								RespondToStatus( body, response );
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
									var sessionId = args[0];

									DeleteSession( sessionId, response );
								}
								else
								{
									RespondUnkownMethod( response );
								}
								break;
							default:
								//other commands over here

								//getting the session id here
								//if not a match return error
								var reqSessId = args[0];

								if( reqSessId.Equals(sessionId) == false )
								{
									BadSessionId(body, response);

									return;
								}

								//search if someone registered a handle for 
								//the command
								args = args.Skip(1).ToArray();

								var realCommand = args[0];

								if( commands.ContainsKey( realCommand ) )
								{
									if( commands[ realCommand ].ContainsKey( request.HttpMethod ) )
									{
										var registeredCommand = commands[ realCommand ][ request.HttpMethod ];

										registeredCommand( body, args.Skip(1).ToArray(), response );
									}
									else
									{
										RespondUnkownMethod( response );
									}
								}
								else
								{
									CommandNotImplemented( realCommand, response );
								}

								break;
							}
							break;
						default:
							CommandNotImplemented( command, response );
							break;
						}
					});
				}
			});
		}

		public void InvalidArgument(HttpListenerResponse response)
		{
			var responseBody =  @"{
					""error"":""invalid argument"",
					""message"":""Invalid argument"",
					""stacktrace"":""""
				}";

			WriteResponse (response, responseBody, 400);
		}

		public void RegisterCommand( string command, string httpMethod, Func<string, string[], HttpListenerResponse, bool> callback )
		{
			if (commands.ContainsKey (command) == false) 
			{
				commands [command] = new Dictionary<string, Func<string, string[], HttpListenerResponse, bool> > ();
			}

			commands [command][httpMethod] = callback;
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