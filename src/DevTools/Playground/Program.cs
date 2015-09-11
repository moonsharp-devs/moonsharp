using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Playground
{
	public class ScriptSharedVars : IUserDataType
	{
		Dictionary<string, DynValue> m_Values = new Dictionary<string, DynValue>();
		object m_Lock = new object();

		public object this[string property]
		{
			get { return m_Values[property].ToObject(); }
			set { m_Values[property] = DynValue.FromObject(null, value); }
		}

		public DynValue Index(Script script, DynValue index, bool isDirectIndexing)
		{
			if (index.Type != DataType.String)
				throw new ScriptRuntimeException("string property was expected");

			lock (m_Lock)
			{
				if (m_Values.ContainsKey(index.String))
					return m_Values[index.String].Clone();
				else
					return DynValue.Nil;
			}
		}

		public bool SetIndex(Script script, DynValue index, DynValue value, bool isDirectIndexing)
		{
			if (index.Type != DataType.String)
				throw new ScriptRuntimeException("string property was expected");

			lock (m_Lock)
			{
				switch (value.Type)
				{
					case DataType.Void:
					case DataType.Nil:
						m_Values.Remove(index.String);
						return true;
					case DataType.UserData:
						// HERE YOU CAN CHOOSE A DIFFERENT POLICY.. AND TRY TO SHARE IF NEEDED. DANGEROUS, THOUGH.
						throw new ScriptRuntimeException("Cannot share a value of type {0}", value.Type.ToErrorTypeString());
					case DataType.ClrFunction:
						// HERE YOU CAN CHOOSE A DIFFERENT POLICY.. AND TRY TO SHARE IF NEEDED. DANGEROUS, THOUGH.
						throw new ScriptRuntimeException("Cannot share a value of type {0}", value.Type.ToErrorTypeString());
					case DataType.Boolean:
					case DataType.Number:
					case DataType.String:
						m_Values[index.String] = value.Clone();
						return true;
					case DataType.Function:
					case DataType.Table:
					case DataType.Tuple:
					case DataType.Thread:
					case DataType.TailCallRequest:
					case DataType.YieldRequest:
					default:
						throw new ScriptRuntimeException("Cannot share a value of type {0}", value.Type.ToErrorTypeString());
				}
			}
		}

		public DynValue MetaIndex(Script script, string metaname)
		{
			return null;
		}
	}

	
	class Program
	{
		static void Main(string[] args)
		{
			UserData.RegisterType<ScriptSharedVars>();

			ScriptSharedVars sharedVars = new ScriptSharedVars();

			sharedVars["mystring"] = "let's go:";

			ManualResetEvent ev = new ManualResetEvent(false);

			StartScriptThread(sharedVars, "bum ", ev);
			StartScriptThread(sharedVars, "chack ", ev);

			ev.Set();

			Thread.Sleep(2000); // too bored to do proper synchronization at this time of the evening...

			Console.WriteLine("{0}", sharedVars["mystring"]);

			Console.ReadKey();
		}

		private static void StartScriptThread(ScriptSharedVars sharedVars, string somestr, ManualResetEvent ev)
		{
			Thread T = new Thread((ThreadStart)delegate
			{
				string script = @"
				for i = 1, 1000 do
					shared.mystring = shared.mystring .. somestring;
				end
			";

				Script S = new Script();

				S.Globals["shared"] = sharedVars;
				S.Globals["somestring"] = somestr;

				ev.WaitOne();

				S.DoString(script);
			});

			T.IsBackground = true;
			T.Name = "Lua script for " + somestr;
			T.Start();
		}

	}
}
