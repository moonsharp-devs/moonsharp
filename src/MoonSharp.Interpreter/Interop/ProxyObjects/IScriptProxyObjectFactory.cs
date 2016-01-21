using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Interface for proxy objects 
	/// </summary>
	/// <typeparam name="TProxy">The type of the proxy.</typeparam>
	/// <typeparam name="TTarget">The type of the target.</typeparam>
	public interface IProxyHandler<TProxy, TTarget>
		where TProxy : class
		where TTarget : class
	{
		/// <summary>
		/// Takes an instance of a target object and returns a proxy object wrapping it
		/// </summary>
		TProxy ProxyWrap(TTarget target);
		/// <summary>
		/// Takes an instance of a proxy object and returns the real object behind it
		/// </summary>
		TTarget ProxyUnwrap(TProxy proxy);
	}
}
