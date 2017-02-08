using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace tech.ironsheep.WebDriver.Lexer
{
	public class TokenDef {
		
		private Regex _regex;
		private string _desc;

		private string _restOfString = "";

		public string RestOfString
		{
			get{
				return _restOfString;
			}
		}

		public string Desc
		{
			get{
				return _desc;
			}
		}

		public TokenDef( Regex regex, string desc )
		{
			this._regex = regex;
			this._desc = desc;
		}

		public TokenDef( string regex, string desc ):this( new Regex( regex ), desc )
		{
		}

		public Token Match( string input )
		{
			Match m = _regex.Match (input);

			Token tok = new Token ("", Desc);

			if (m.Success) 
			{
				tok.Content = m.Groups[ m.Groups.Count -1 ].Value;
				_restOfString = input.Substring (m.Length);
			}

			return tok;
		}
	}
}