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
		internal IUserDataDescriptor Descriptor { get; set; }

		private static object s_Lock = new object();
		private static Dictionary<Type, IUserDataDescriptor> s_Registry = new Dictionary<Type, IUserDataDescriptor>();
		private static InteropAccessMode s_DefaultAccessMode;

		static UserData()
		{
			RegisterType<AnonWrapper>(InteropAccessMode.HideMembers);
			RegisterType<EnumerableWrapper>(InteropAccessMode.HideMembers);
			s_DefaultAccessMode = InteropAccessMode.LazyOptimized;
		}

		public static void RegisterType<T>(InteropAccessMode accessMode = InteropAccessMode.Default, string friendlyName = null)
		{
			RegisterType_Impl(typeof(T), accessMode, friendlyName, null);
		}

		public static void RegisterType(Type type, InteropAccessMode accessMode = InteropAccessMode.Default, string friendlyName = null)
		{
			RegisterType_Impl(type, accessMode, friendlyName, null);
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

		public static void UnregisterType<T>()
		{
			UnregisterType(typeof(T));
		}

		public static void UnregisterType(Type t)
		{
			lock (s_Lock)
				if (s_Registry.ContainsKey(t))
					s_Registry.Remove(t);
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

		public static InteropRegistrationPolicy RegistrationPolicy { get; set; }

		public static InteropAccessMode DefaultAccessMode
		{
			get { return s_DefaultAccessMode; }
			set
			{
				if (value == InteropAccessMode.Default)
					throw new ArgumentException("DefaultAccessMode");

				s_DefaultAccessMode = value;
			}
		}



		private static IUserDataDescriptor RegisterType_Impl(Type type, InteropAccessMode accessMode, string friendlyName, IUserDataDescriptor descriptor)
		{
			if (accessMode == InteropAccessMode.Default)
			{
				MoonSharpUserDataAttribute attr = type.GetCustomAttributes(true).OfType<MoonSharpUserDataAttribute>()
					.SingleOrDefault();

				if (attr != null)
					accessMode = attr.AccessMode;
			}


			if (accessMode == InteropAccessMode.Default)
				accessMode = s_DefaultAccessMode;

			lock(s_Lock)
			{
				if (!s_Registry.ContainsKey(type))
				{
					if (descriptor == null)
					{
						if (type.GetInterfaces().Any(ii => ii == typeof(IUserDataType)))
						{
							AutoDescribingUserDataDescriptor audd = new AutoDescribingUserDataDescriptor(type, friendlyName);
							s_Registry.Add(audd.Type, audd);
							return audd;
						}
						else
						{
							StandardUserDataDescriptor udd = new StandardUserDataDescriptor(type, accessMode, friendlyName);
							s_Registry.Add(udd.Type, udd);

							if (accessMode == InteropAccessMode.BackgroundOptimized)
							{
								ThreadPool.QueueUserWorkItem(o => udd.Optimize());
							}

							return udd;
						}
					}
					else
					{
						s_Registry.Add(descriptor.Type, descriptor);
						return descriptor;
					}
				}
				else return s_Registry[type];
			}
		}

		private static IUserDataDescriptor GetDescriptorForType<T>(bool searchInterfaces)
		{
			return GetDescriptorForType(typeof(T), searchInterfaces);
		}

		private static IUserDataDescriptor GetDescriptorForType(Type type, bool searchInterfaces)
		{
			lock(s_Lock)
			{
				IUserDataDescriptor typeDescriptor = null;

				// if the type has been explicitly registered, return its descriptor as it's complete
				if (s_Registry.ContainsKey(type))
					return s_Registry[type];

				if (RegistrationPolicy == InteropRegistrationPolicy.Automatic)
				{
					return RegisterType_Impl(type, DefaultAccessMode, type.FullName, null);
				}

				// search for the base object descriptors
				for (Type t = type; t != null; t = t.BaseType)
				{
					IUserDataDescriptor u;

					if (s_Registry.TryGetValue(t, out u))
					{
						typeDescriptor = u;
						break;
					}
				}

				// we should not search interfaces (for example, it's just for statics..), no need to look further
				if (!searchInterfaces)
					return typeDescriptor;

				List<IUserDataDescriptor> descriptors = new List<IUserDataDescriptor>();

				if (typeDescriptor != null)
					descriptors.Add(typeDescriptor);


				if (searchInterfaces)
				{
					foreach (Type t in type.GetInterfaces())
					{
						IUserDataDescriptor u;

						if (s_Registry.TryGetValue(t, out u))
							descriptors.Add(u);
					}
				}

				if (descriptors.Count == 1)
					return descriptors[0];
				else if (descriptors.Count == 0)
					return null;
				else
					return new CompositeUserDataDescriptor(descriptors, type);
			}
		}


		private static IUserDataDescriptor GetDescriptorForObject(object o)
		{
			return GetDescriptorForType(o.GetType(), true);
		}
	}
}
