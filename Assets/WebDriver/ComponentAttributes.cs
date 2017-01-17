using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace tech.ironsheep.WebDriver
{
	public class ComponentAttributes 
	{
		public static Dictionary<string,string> Attributes( Type t, object obj )
		{
			Dictionary<string, string> ret = new Dictionary<string, string> ();

			//getting properties
			PropertyInfo[] properties = t.GetProperties (BindingFlags.Instance | BindingFlags.Public );

			foreach (PropertyInfo info in properties) 
			{
				if (info.CanRead) 
				{
					try
					{
						ret[ info.Name ] =  info.GetValue (obj, null).ToString();
					}
					catch(Exception e) 
					{
						
					}
				}
			}

			//getting fields
			FieldInfo[] fields = t.GetFields (BindingFlags.Instance | BindingFlags.Public);

			foreach (FieldInfo fi in fields) 
			{
				try
				{
					ret [fi.Name] = fi.GetValue (obj).ToString ();
				}
				catch(Exception e)
				{
					
				}
			}

			return ret;
		}
	}
}