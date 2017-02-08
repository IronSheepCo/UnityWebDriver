using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace tech.ironsheep.WebDriver.Lexer
{
	public class Lexer {
		private List<TokenDef> _tokenDefs = new List<TokenDef>();

		public Lexer()
		{
			AddTokenDef (new TokenDef ("^//", "ANCESTOR") );
			AddTokenDef (new TokenDef ("^/", "CHILD") );
			AddTokenDef (new TokenDef ("^\\s+", "WHITE_SPACE") );
			AddTokenDef (new TokenDef ("^and|AND", "AND") );
			AddTokenDef (new TokenDef (@"^\[", "OPEN_PREDICATE") );
			AddTokenDef (new TokenDef ("^]", "END_PREDICATE") );
			AddTokenDef (new TokenDef ("^@[a-zA-Z0-9_-]+", "ATTRIBUTE") );
			AddTokenDef (new TokenDef ("^=", "EQUAL") );
			AddTokenDef (new TokenDef ("^~", "SIMILAR") );
			AddTokenDef (new TokenDef ("^[0-9]+", "NUMBER") );
			AddTokenDef (new TokenDef ("^[a-zA-Z0-9_-]+", "IDENTIFIER") );
			AddTokenDef (new TokenDef ("^\"([^\"\\\\]*(?:\\\\.[^\"\\\\]*)*)\"", "STRING") );
		}

		public void AddTokenDef( TokenDef tok )
		{
			_tokenDefs.Add (tok);
		}

		public List<Token> Tokenize( string input )
		{
			string remainingInput = input;
			string potentialRemainingInput = "";

			List<Token> ret = new List<Token> ();
			
			int largestMatch = 0;
			Token currentLargestToken = null;

			while (remainingInput.Length > 0) 
			{
				currentLargestToken = null;
				largestMatch = 0;

				foreach (TokenDef tokDef in _tokenDefs) 
				{
					Token tok = tokDef.Match (remainingInput);

					if (tok.LengthOfMatch > largestMatch) 
					{
						currentLargestToken = tok;
						largestMatch = tok.LengthOfMatch;
						potentialRemainingInput = tokDef.RestOfString;
					}
				}

				//we don't have a match anymore
				//so we have shit at the end of the 
				//input
				if( currentLargestToken == null )
				{
					break;
				}

				remainingInput = potentialRemainingInput;

				ret.Add (currentLargestToken);
			}

			return ret;
		}
	}
}