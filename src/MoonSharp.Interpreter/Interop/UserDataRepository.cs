using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public class UserDataRepository
	{
		private Dictionary<Type, UserDataDescriptor> m_ByType = new Dictionary<Type, UserDataDescriptor>();
		private Dictionary<string, UserDataDescriptor> m_ByName = new Dictionary<string, UserDataDescriptor>();

		public Script OwnerScript { get; private set; }

		internal UserDataRepository(Script script)
		{
			OwnerScript = script;
			this.RegisterType<EnumerableIterator>(UserDataOptimizationMode.HideMembers);
		}

		public UserDataDescriptor RegisterType<T>(UserDataOptimizationMode optimizationMode = UserDataOptimizationMode.None)
		{
			return RegisterType(typeof(T), optimizationMode);
		}


		public UserDataDescriptor RegisterType(Type type, UserDataOptimizationMode optimizationMode = UserDataOptimizationMode.None)
		{
			if (m_ByType.ContainsKey(type))
				return m_ByType[type];

			UserDataDescriptor udd = new UserDataDescriptor(this, type, optimizationMode);
			m_ByType.Add(udd.Type, udd);
			m_ByName.Add(udd.Name, udd);
			return udd;
		}

		public UserDataDescriptor GetDescriptorForType<T>(bool deepSearch = true)
		{
			return GetDescriptorForType(typeof(T), deepSearch);
		}

		public UserDataDescriptor GetDescriptorForType(Type type, bool deepSearch = true)
		{
			if (!deepSearch)
				return m_ByType.ContainsKey(type) ? m_ByType[type] : null;

			for (Type t = type; t != typeof(object); t = t.BaseType)
			{
				if (m_ByType.ContainsKey(t))
					return m_ByType[t];
			}

			foreach (Type t in type.GetInterfaces())
			{
				if (m_ByType.ContainsKey(t))
					return m_ByType[t];
			}

			if (m_ByType.ContainsKey(typeof(object)))
				return m_ByType[type];

			return null;
		}

		public UserDataDescriptor GetDescriptorForObject(object o)
		{
			return GetDescriptorForType(o.GetType(), true);
		}

		public DynValue CreateUserData(object o)
		{
			var descr = GetDescriptorForObject(o);
			if (descr == null) return null;
				
			return DynValue.NewUserData(new UserData()
				{
					Descriptor = descr,
					Object = o
				});
		}

		public DynValue CreateStaticUserData(Type t)
		{
			var descr = GetDescriptorForType(t, false);
			if (descr == null) return null;

			return DynValue.NewUserData(new UserData()
			{
				Descriptor = descr,
				Object = null
			});
		}

		public DynValue CreateStaticUserData<T>()
		{
			return CreateStaticUserData(typeof(T));
		}



	}
}
