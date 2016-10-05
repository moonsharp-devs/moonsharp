/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace MoonSharp.VsCodeDebugger.SDK
{
	public class Utilities
	{
		private static readonly Regex VARIABLE = new Regex(@"\{(\w+)\}");


		/*
		 * Resolve hostname, dotted-quad notation for IPv4, or colon-hexadecimal notation for IPv6 to IPAddress.
		 * Returns null on failure.
		 */

		public static string ExpandVariables(string format, object variables, bool underscoredOnly = true)
		{
			if (variables == null)
			{
				variables = new { };
			}
			Type type = variables.GetType();
			return VARIABLE.Replace(format, match => {
				string name = match.Groups[1].Value;
				if (!underscoredOnly || name.StartsWith("_"))
				{

					PropertyInfo property = type.GetProperty(name);
					if (property != null)
					{
						object value = property.GetValue(variables, null);
						return value.ToString();
					}
					return '{' + name + ": not found}";
				}
				return match.Groups[0].Value;
			});
		}

		/**
		 * converts the given absPath into a path that is relative to the given dirPath.
		 */
		public static string MakeRelativePath(string dirPath, string absPath)
		{
			if (!dirPath.EndsWith("/"))
			{
				dirPath += "/";
			}
			if (absPath.StartsWith(dirPath))
			{
				return absPath.Replace(dirPath, "");
			}
			return absPath;
			/*
			Uri uri1 = new Uri(path);
			Uri uri2 = new Uri(dir_path);
			return uri2.MakeRelativeUri(uri1).ToString();
			*/
		}
	}
}