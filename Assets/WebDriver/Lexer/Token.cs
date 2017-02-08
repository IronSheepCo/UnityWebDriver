using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace tech.ironsheep.WebDriver.Lexer
{
	public class Token {
		private string _content = "";

		public string Content
		{
			get{
				return _content;
			}

			set{
				_content = value;
			}
		}

		private string _desc = "";

		public string Desc{
			get{
				return _desc;
			}
		}

		public int LengthOfMatch
		{
			get
			{
				return Content.Length;
			}
		}

		public Token( string content, string desc )
		{
			this._content = content;
			this._desc = desc;
		}

		public Token():this("", "NONE")
		{
		}
	}
}