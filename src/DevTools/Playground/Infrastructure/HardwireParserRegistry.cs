using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Playground
{
	public static class HardwireGeneratorRegistry
	{
		static Dictionary<string, IHardwireGenerator> m_Generators = new Dictionary<string, IHardwireGenerator>();

		public static void Register(IHardwireGenerator g)
		{
			m_Generators.Add(g.ManagedType, g);
		}

		public static IHardwireGenerator GetGenerator(string type)
		{
			if (m_Generators.ContainsKey(type))
				return m_Generators[type];
			else
				return new NullGenerator(type);
		}

		public static void AutoRegister(Assembly asm = null)
		{
			if (asm == null)
				asm = Assembly.GetExecutingAssembly();

			foreach (Type type in asm.DefinedTypes
				.Where(t => !(t.IsAbstract || t.IsGenericTypeDefinition || t.IsGenericType))
				.Where(t => (typeof(IHardwireGenerator)).IsAssignableFrom(t)))

			{
				IHardwireGenerator g = (IHardwireGenerator)Activator.CreateInstance(type);
				Register(g);
			}
		}

	}
}
