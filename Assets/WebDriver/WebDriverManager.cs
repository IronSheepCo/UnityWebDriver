using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.IO;
using System;
using System.Text.RegularExpressions;

using tech.ironsheep.WebDriver.Dispatch;

namespace tech.ironsheep.WebDriver
{
	public class WebDriverManager
	{
		private static WebDriverManager _instance;

		private static object instanceLock = new System.Object();

		private HttpListener listener;

		private string sessionId = null;

		private Dictionary<string, Dictionary<string, Dictionary<Regex, Func<string, string[], HttpListenerResponse, bool> > > > commands = new Dictionary< string, Dictionary<string, Dictionary<Regex, Func<string, string[], HttpListenerResponse, bool> > > >();

		//current browsing context
		//hash from uuid to GameObject
		private Dictionary<string, GameObject> browsingContext = new Dictionary<string, GameObject>();
		private Dictionary<GameObject, string> reversedBrowsingContext = new Dictionary<GameObject, string>();

		//root game objects
		public List<GameObject> RootGameObjects = new List<GameObject> ();

		private WebDriverManager()
		{
			listener = new HttpListener ();

			listener.Prefixes.Add ("http://*:8080/");

			RegisterSceneManagement ();

			//getting the root game objects
			RootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects ().ToList ();

			StartWebDriver ();
		}

		private void RegisterSceneManagement()
		{
			SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
			SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		void SceneManager_sceneLoaded (Scene arg0, LoadSceneMode arg1)
		{
			//get the root objects
			RootGameObjects = arg0.GetRootGameObjects ().ToList ();

			Debug.Log ("getting root objects");
		}

		void SceneManager_sceneUnloaded (Scene arg0)
		{
			//new browsing context
			browsingContext = new Dictionary<string, GameObject> ();

			Debug.Log ("removing browsing context");
		}

		public void WriteResponse( HttpListenerResponse response, string body, int code )
		{
			response.StatusCode = code;

			var writer = new StreamWriter (response.OutputStream);
			writer.Write (body);
			writer.Close ();
		}

		public void WriteEmptyAlgorithmResponse( HttpListenerResponse response )
		{
			WriteResponse( response, "{\"data\":null}", 200 );
		}

		public void WriteElementNotFound( HttpListenerResponse response )
		{
			var responseBody = @"{
				""error"":""no such element"",
				""message"":""An element could not be located on the page using the given search parameters."",
				""stacktrace"":""""
			}";

			WebDriverManager.instance.WriteResponse (response, responseBody, 400);
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
										var registeredCommands = commands[ realCommand ][ request.HttpMethod ];

										var restOfCommandSeg = args.Skip(1);

										string restOfCommand = "";

										if( restOfCommandSeg.Count() > 0 )
										{
											restOfCommand = restOfCommandSeg.Aggregate( (i,j) => i+"/"+j );
										}

										//the command to be executed
										Func<string, string[], HttpListenerResponse, bool> registeredCommand = null;

										//searching for a match
										foreach( var entry in registeredCommands )
										{
											if( entry.Key.IsMatch( restOfCommand ) )
											{
												registeredCommand = entry.Value;
												break;
											}
										}

										if( registeredCommand == null )
										{
											RespondUnkownMethod( response );

											return;
										}

										MainDispatcher.ExecuteBlocking( ()=>{
											//need to see which ones of the reg exp match 
											registeredCommand( body, args.Skip(1).ToArray(), response );
										});
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

		public void RegisterCommand( string command, string httpMethod, Func<string, string[], HttpListenerResponse, bool> callback, string matchArgs = "^$" )
		{
			if (commands.ContainsKey (command) == false) 
			{
				commands [command] = new Dictionary<string, Dictionary<Regex, Func<string, string[], HttpListenerResponse, bool> > > ();
			}

			if (commands [command].ContainsKey (httpMethod) == false) 
			{
				commands [command] [httpMethod] = new Dictionary<Regex, Func<string, string[], HttpListenerResponse, bool> > ();
			}

			Regex reg = new Regex (matchArgs);

			commands [command][httpMethod][reg] = callback;
		}

		public string AddElement( GameObject obj, string uuid )
		{
			string existingUUID = GetUUID (obj);

			if (existingUUID != null) 
			{
				return existingUUID;
			}
				
			browsingContext [uuid] = obj;
			reversedBrowsingContext [obj] = uuid;

			return uuid;
		}

		public GameObject GetElement( string uuid )
		{
			GameObject ret = null;

			browsingContext.TryGetValue (uuid, out ret);

			return ret;
		}

		public string GetUUID( GameObject go )
		{
			string ret = null;

			reversedBrowsingContext.TryGetValue ( go, out ret );

			return ret;
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