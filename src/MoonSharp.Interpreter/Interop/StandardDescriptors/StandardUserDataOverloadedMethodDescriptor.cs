using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop.StandardDescriptors
{
	/// <summary>
	/// Class providing easier marshalling of overloaded CLR functions
	/// </summary>
	public class StandardUserDataOverloadedMethodDescriptor
	{
		const int CACHE_SIZE = 5;

		private class OverloadCacheItem
		{
			public bool HasObject;
			public StandardUserDataMethodDescriptor Method;
			public List<DataType> ArgsDataType;
			public int HitIndexAtLastHit;
		}

		private List<StandardUserDataMethodDescriptor> m_Overloads = new List<StandardUserDataMethodDescriptor>();
		private bool m_Unsorted = true;
		private OverloadCacheItem[] m_Cache = new OverloadCacheItem[CACHE_SIZE];
		private int m_CacheHits = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataOverloadedMethodDescriptor"/> class.
		/// </summary>
		public StandardUserDataOverloadedMethodDescriptor()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataOverloadedMethodDescriptor"/> class.
		/// </summary>
		/// <param name="descriptor">The descriptor of the first overloaded method.</param>
		public StandardUserDataOverloadedMethodDescriptor(StandardUserDataMethodDescriptor descriptor)
		{
			m_Overloads.Add(descriptor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataOverloadedMethodDescriptor"/> class.
		/// </summary>
		/// <param name="descriptor">The descriptors of the overloaded methods.</param>
		public StandardUserDataOverloadedMethodDescriptor(IEnumerable<StandardUserDataMethodDescriptor> descriptors)
		{
			m_Overloads.AddRange(descriptors);
		}

		/// <summary>
		/// Gets the name of the first described overload
		/// </summary>
		public string Name
		{
			get
			{
				if (m_Overloads.Count > 0)
					return m_Overloads[0].Name;

				return null;
			}
		}

		/// <summary>
		/// Adds an overload.
		/// </summary>
		/// <param name="overload">The overload.</param>
		public void AddOverload(StandardUserDataMethodDescriptor overload)
		{
			m_Overloads.Add(overload);
			m_Unsorted = true;
		}

		/// <summary>
		/// Gets the number of overloaded methods contained in this collection
		/// </summary>
		/// <value>
		/// The overload count.
		/// </value>
		public int OverloadCount
		{
			get { return m_Overloads.Count; }
		}

		/// <summary>
		/// Performs the overloaded call.
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <param name="context">The context.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		/// <exception cref="ScriptRuntimeException">function call doesn't match any overload</exception>
		private DynValue PerformOverloadedCall(Script script, object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			if (m_Overloads.Count == 1)
				return m_Overloads[0].Callback(script, obj, context, args);

			if (m_Unsorted)
			{
				m_Overloads.Sort();
				m_Unsorted = false;
			}

			for (int i = 0; i < m_Cache.Length; i++)
			{
				if (m_Cache[i] != null && CheckMatch(obj != null, args, m_Cache[i]))
				{
					System.Diagnostics.Debug.WriteLine(string.Format("[OVERLOAD] : CACHED! slot {0}, hits: {1}", i, m_CacheHits));
					return m_Cache[i].Method.Callback(script, obj, context, args);
				}
			}


			int maxScore = 0;
			StandardUserDataMethodDescriptor bestOverload = null;

			for (int i = 0; i < m_Overloads.Count; i++)
			{
				if (obj != null || m_Overloads[i].IsStatic)
				{
					int score = CalcScoreForOverload(context, args, m_Overloads[i]);

					if (score > maxScore)
					{
						maxScore = score;
						bestOverload = m_Overloads[i];
					}
				}
			}

			if (bestOverload != null)
			{
				Cache(obj != null, args, bestOverload);
				return bestOverload.Callback(script, obj, context, args);
			}

			throw new ScriptRuntimeException("function call doesn't match any overload");
		}

		private void Cache(bool hasObject, CallbackArguments args, StandardUserDataMethodDescriptor bestOverload)
		{
			int lowestHits = int.MaxValue;
			OverloadCacheItem found = null;
			for (int i = 0; i < m_Cache.Length; i++)
			{
				if (m_Cache[i] == null)
				{
					found = new OverloadCacheItem() { ArgsDataType = new List<DataType>() };
					m_Cache[i] = found;
					break;
				}
				else if (m_Cache[i].HitIndexAtLastHit < lowestHits)
				{
					lowestHits = m_Cache[i].HitIndexAtLastHit;
					found = m_Cache[i];
				}
			}

			if (found == null)
			{
				// overflow..
				m_Cache = new OverloadCacheItem[CACHE_SIZE];
				found = new OverloadCacheItem() { ArgsDataType = new List<DataType>() };
				m_Cache[0] = found;
				m_CacheHits = 0;
			}

			found.Method = bestOverload;
			found.HitIndexAtLastHit = ++m_CacheHits;
			found.ArgsDataType.Clear();
			found.HasObject = hasObject;

			for (int i = 0; i < args.Count; i++)
			{
				found.ArgsDataType.Add(args[i].Type);
			}
		}

		private bool CheckMatch(bool hasObject, CallbackArguments args, OverloadCacheItem overloadCacheItem)
		{
			if (overloadCacheItem.HasObject && !hasObject)
				return false;

			if (args.Count != overloadCacheItem.ArgsDataType.Count)
				return false;

			for (int i = 0; i < args.Count; i++)
			{
				if (args[i].Type != overloadCacheItem.ArgsDataType[i])
					return false;
			}

			overloadCacheItem.HitIndexAtLastHit = ++m_CacheHits;
			return true;
		}


		/// <summary>
		/// Calculates the score for the overload.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="args">The arguments.</param>
		/// <param name="method">The method.</param>
		/// <returns></returns>
		private int CalcScoreForOverload(ScriptExecutionContext context, CallbackArguments args, StandardUserDataMethodDescriptor method)
		{
			int totalScore = ScriptToClrConversions.WEIGHT_EXACT_MATCH;
			int argsBase = args.IsMethodCall ? 1 : 0;
			int argsCnt = argsBase;

			for (int i = 0; i < method.Parameters.Length; i++)
			{
				Type parameterType = method.Parameters[i].ParameterType;

				if ((parameterType == typeof(Script)) || (parameterType == typeof(ScriptExecutionContext)) || (parameterType == typeof(CallbackArguments)))
					continue;

				var arg = args.RawGet(argsCnt, false) ?? DynValue.Void;

				int score = ScriptToClrConversions.DynValueToObjectOfTypeWeight(arg,
					parameterType, method.Parameters[i].DefaultValue != System.DBNull.Value);

				if (parameterType.IsByRef)
					score = Math.Max(0, score - ScriptToClrConversions.WEIGHT_BYREF_BONUSMALUS);

				totalScore = Math.Min(totalScore, score);

				argsCnt += 1;
			}

			if (totalScore > 0)
			{
				if ((args.Count - argsBase) <= method.Parameters.Length)
				{
					totalScore += ScriptToClrConversions.WEIGHT_NO_EXTRA_PARAMS_BONUS;
					totalScore *= 1000;
				}
				else
				{
					totalScore *= 1000;
					totalScore -= ScriptToClrConversions.WEIGHT_EXTRA_PARAMS_MALUS * ((args.Count - argsBase) - method.Parameters.Length);
					totalScore = Math.Max(1, totalScore);
				}
			}

			System.Diagnostics.Debug.WriteLine(string.Format("[OVERLOAD] : Score {0} for method {1}", totalScore, method.SortDiscriminant));
	
			return totalScore;
		}


		/// <summary>
		/// Gets a callback function as a delegate
		/// </summary>
		/// <param name="script">The script for which the callback must be generated.</param>
		/// <param name="obj">The object (null for static).</param>
		/// <returns></returns>
		public Func<ScriptExecutionContext, CallbackArguments, DynValue> GetCallback(Script script, object obj)
		{
			return (context, args) => PerformOverloadedCall(script, obj, context, args);
		}


		internal void Optimize()
		{
			foreach (var d in m_Overloads)
				d.Optimize();
		}



		/// <summary>
		/// Gets the callback function.
		/// </summary>
		/// <param name="script">The script for which the callback must be generated.</param>
		/// <param name="obj">The object (null for static).</param>
		/// <returns></returns>
		public CallbackFunction GetCallbackFunction(Script script, object obj = null)
		{
			return new CallbackFunction(GetCallback(script, obj), this.Name);
		}

		/// <summary>
		/// Gets the callback function as a DynValue.
		/// </summary>
		/// <param name="script">The script for which the callback must be generated.</param>
		/// <param name="obj">The object (null for static).</param>
		/// <returns></returns>
		public DynValue GetCallbackAsDynValue(Script script, object obj = null)
		{
			return DynValue.NewCallback(this.GetCallbackFunction(script, obj));
		}

	}
}
