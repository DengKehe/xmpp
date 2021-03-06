// CompressionRegistry.cs
//
//Ubiety XMPP Library Copyright (C) 2006 - 2012 Dieter Lunn
//
//This library is free software; you can redistribute it and/or modify it under
//the terms of the GNU Lesser General Public License as published by the Free
//Software Foundation; either version 3 of the License, or (at your option)
//any later version.
//
//This library is distributed in the hope that it will be useful, but WITHOUT
//ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
//
//You should have received a copy of the GNU Lesser General Public License along
//with this library; if not, write to the Free Software Foundation, Inc., 59
//Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.Collections.Generic;
using System.Reflection;
using ubiety.common;
using ubiety.common.attributes;
using ubiety.common.extensions;
using ubiety.common.logging;

namespace ubiety.registries
{
	///<summary>
	///</summary>
	public static class CompressionRegistry
	{
        private static Dictionary<string, Type> RegisteredItems = new Dictionary<string, Type>();

		/// <summary>
		/// Add a compression stream to the library.  Zlib is the default.
		/// </summary>
		/// <param name="a">
		/// The assembly containing the stream definition.
		/// </param>
		public static void AddCompression(Assembly a)
		{
			Logger.DebugFormat(typeof(CompressionRegistry), "Adding assembly {0}", a.FullName);
			
			var tags = a.GetAttributes<CompressionAttribute>();
			foreach (var tag in tags)
			{
                Logger.DebugFormat(typeof(CompressionRegistry), "Adding {0}", tag.Algorithm);
				RegisteredItems.Add(tag.Algorithm, tag.ClassType);
			}			
		}

		/// <summary>
		/// Creates the stream class for the compression algorithm specified.
		/// </summary>
		/// <param name="algorithm">
		/// The algorithm we want to create the stream for.
		/// </param>
		/// <returns>
		/// The wrapped stream ready for compression.
		/// </returns>
		public static ICompression GetCompression(string algorithm)
		{
			Logger.InfoFormat(typeof(CompressionRegistry), "Finding algorithm {0}.", algorithm);
			ICompression stream = null;
			try
			{
				Type t;
				if (RegisteredItems.TryGetValue(algorithm, out t))
				{				
					stream = (ICompression)Activator.CreateInstance(t);
				}
				else
				{
					Errors.SendError(typeof(CompressionRegistry), ErrorType.UnregisteredItem, "Unable to find requested compression algorithm");
					return null;
				}
			}
			catch (Exception e)
			{
				Errors.SendError(typeof(CompressionRegistry), ErrorType.UnregisteredItem, "Unable to find requested compression algorithm");
				Logger.Error(typeof(CompressionRegistry), e);
			}
			return stream;
		}

		/// <summary>
		/// Does the library support the algorithm the server is requesting.
		/// </summary>
		/// <param name="algorithm">
		/// The algorithm we are looking for.
		/// </param>
		/// <returns>
		/// True if we have a stream class available.  False if not.
		/// </returns>
		public static bool SupportsAlgorithm(string algorithm)
		{
			return RegisteredItems.ContainsKey(algorithm);
		}

		/// <value>
		/// Do we have any algorithms to use?
		/// </value>
		public static bool AlgorithmsAvailable
		{
			get
			{
				return RegisteredItems.Count >= 1;
			}
		}
	}
}
