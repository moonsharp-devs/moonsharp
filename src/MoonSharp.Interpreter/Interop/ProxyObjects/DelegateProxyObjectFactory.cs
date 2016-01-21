using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Implementation of IScriptProxyObjectFactory taking two delegates for simple instancing of proxies.
	/// </summary>
	/// <typeparam name="TProxy">The type of the proxy.</typeparam>
	/// <typeparam name="TTarget">The type of the target.</typeparam>
	public class DelegateProxyHandler<TProxy, TTarget> : IProxyHandler<TProxy, TTarget>
		where TProxy : class
		where TTarget : class
	{
		Func<TTarget, TProxy> wrapDelegate;
		Func<TProxy, TTarget> unwrapDelegate;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateProxyHandler{TProxy, TTarget}"/> class.
		/// </summary>
		/// <param name="wrapDelegate">The proxy.</param>
		/// <param name="unwrapDelegate">The deproxy.</param>
		public DelegateProxyHandler(Func<TTarget, TProxy> wrapDelegate, Func<TProxy, TTarget> unwrapDelegate)
		{
			this.wrapDelegate = wrapDelegate;
			this.unwrapDelegate = unwrapDelegate;
		}

		/// <summary>
		/// Takes an instance of a proxy object and returns the real object behind it
		/// </summary>
		public TTarget ProxyUnwrap(TProxy proxyobj)
		{
			return unwrapDelegate(proxyobj);
		}

		/// <summary>
		/// Takes an instance of a target object and returns a proxy object wrapping it
		/// </summary>
		public TProxy ProxyWrap(TTarget target)
		{
			return wrapDelegate(target);
		}
	}

}
