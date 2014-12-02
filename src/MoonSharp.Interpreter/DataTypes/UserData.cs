using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter
{
	public class UserData : RefIdObject
	{
		private UserData()
		{
			// This type can only be instantiated using one of the Create methods
		}

		public DynValue UserValue { get; set; }

		public object Object { get; set; }
		internal UserDataDescriptor Descriptor { get; set; }

#if USE_RW_LOCK
		private static ReaderWriterLockSlim m_Lock = new ReaderWriterLockSlim();
#else
		private static object m_Lock = new object();
#endif
		private static Dictionary<Type, UserDataDescriptor> s_Registry = new Dictionary<Type, UserDataDescriptor>();
		private static InteropAccessMode m_DefaultAccessMode;

		static UserData()
		{
			RegisterType<AnonWrapper>(InteropAccessMode.HideMembers);
			RegisterType<EnumerableWrapper>(InteropAccessMode.HideMembers);
			m_DefaultAccessMode = InteropAccessMode.LazyOptimized;
		}

		public static void RegisterType<T>(InteropAccessMode accessMode = InteropAccessMode.Default, string friendlyName = null)
		{
			RegisterType_Impl(typeof(T), accessMode, friendlyName);
		}

		public static void RegisterType(Type type, InteropAccessMode accessMode = InteropAccessMode.Default, string friendlyName = null)
		{
			RegisterType_Impl(type, accessMode, friendlyName);
		}

		public static void RegisterAssembly(Assembly asm = null)
		{
			asm = asm ?? Assembly.GetCallingAssembly();

			var userDataTypes = from t in asm.GetTypes()
				let attributes = t.GetCustomAttributes(typeof(MoonSharpUserDataAttribute), true)
				where attributes != null && attributes.Length > 0
				select new { Attributes = attributes, DataType = t };

			foreach (var userDataType in userDataTypes)
			{
				UserData.RegisterType(userDataType.DataType, userDataType.Attributes
					.OfType<MoonSharpUserDataAttribute>()
					.First()
					.AccessMode);
			}
		}


		public static DynValue Create(object o)
		{
			var descr = GetDescriptorForObject(o);
			if (descr == null) return null;

			return DynValue.NewUserData(new UserData()
				{
					Descriptor = descr,
					Object = o
				});
		}

		public static DynValue CreateStatic(Type t)
		{
			var descr = GetDescriptorForType(t, false);
			if (descr == null) return null;

			return DynValue.NewUserData(new UserData()
			{
				Descriptor = descr,
				Object = null
			});
		}

		public static DynValue CreateStatic<T>()
		{
			return CreateStatic(typeof(T));
		}

		public static InteropAccessMode DefaultAccessMode
		{
			get { return m_DefaultAccessMode; }
			set
			{
				if (value == InteropAccessMode.Default)
					throw new ArgumentException("DefaultAccessMode");

				m_DefaultAccessMode = value;
			}
		}



		private static void RegisterType_Impl(Type type, InteropAccessMode accessMode, string friendlyName)
		{
			if (accessMode == InteropAccessMode.Default)
			{
				MoonSharpUserDataAttribute attr = type.GetCustomAttributes(true).OfType<MoonSharpUserDataAttribute>()
					.SingleOrDefault();

				if (attr != null)
					accessMode = attr.AccessMode;
			}


			if (accessMode == InteropAccessMode.Default)
				accessMode = m_DefaultAccessMode;

#if USE_RW_LOCK
			m_Lock.EnterWriteLock();
#else
			Monitor.Enter(m_Lock);
#endif

			try
			{
				if (!s_Registry.ContainsKey(type))
				{
					UserDataDescriptor udd = new UserDataDescriptor(type, accessMode, friendlyName);
					s_Registry.Add(udd.Type, udd);

					if (accessMode == InteropAccessMode.BackgroundOptimized)
					{
						ThreadPool.QueueUserWorkItem(o => udd.Optimize());
					}
				}
			}
			finally
			{
#if USE_RW_LOCK
				m_Lock.ExitWriteLock();
#else
				Monitor.Exit(m_Lock);
#endif
			}
		}

		private static UserDataDescriptor GetDescriptorForType<T>(bool deepSearch = true)
		{
			return GetDescriptorForType(typeof(T), deepSearch);
		}

		private static UserDataDescriptor GetDescriptorForType(Type type, bool deepSearch = true)
		{
#if USE_RW_LOCK
			m_Lock.EnterReadLock();
#else
			Monitor.Enter(m_Lock);
#endif

			try
			{
				if (!deepSearch)
					return s_Registry.ContainsKey(type) ? s_Registry[type] : null;

				for (Type t = type; t != typeof(object); t = t.BaseType)
				{
					UserDataDescriptor u;

					if (s_Registry.TryGetValue(t, out u))
						return u;
				}

				foreach (Type t in type.GetInterfaces())
				{
					if (s_Registry.ContainsKey(t))
						return s_Registry[t];
				}

				if (s_Registry.ContainsKey(typeof(object)))
					return s_Registry[type];
			}
			finally
			{
#if USE_RW_LOCK
				m_Lock.ExitReadLock();
#else
				Monitor.Exit(m_Lock);
#endif
			}

			return null;
		}


		private static UserDataDescriptor GetDescriptorForObject(object o)
		{
			return GetDescriptorForType(o.GetType(), true);
		}
	}
}
