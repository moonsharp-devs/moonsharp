using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Class exposing C# objects as Lua userdata.
	/// For efficiency, a global registry of types is maintained, instead of a per-script one.
	/// </summary>
	public class UserData : RefIdObject
	{
		private UserData()
		{
			// This type can only be instantiated using one of the Create methods
		}

		/// <summary>
		/// Gets or sets the "uservalue". See debug.getuservalue and debug.setuservalue.
		/// http://www.lua.org/manual/5.2/manual.html#pdf-debug.setuservalue
		/// </summary>
		public DynValue UserValue { get; set; }

		/// <summary>
		/// Gets the object associated to this userdata (null for statics)
		/// </summary>
		public object Object { get; private set; }

		/// <summary>
		/// Gets the type descriptor of this userdata
		/// </summary>
		public IUserDataDescriptor Descriptor { get; private set; }

		private static object s_Lock = new object();
		private static Dictionary<Type, IUserDataDescriptor> s_Registry = new Dictionary<Type, IUserDataDescriptor>();
		private static InteropAccessMode s_DefaultAccessMode;
		private static MultiDictionary<string, StandardUserDataMethodDescriptor> s_ExtensionMethodRegistry = new MultiDictionary<string, StandardUserDataMethodDescriptor>();
		private static int s_ExtensionMethodChangeVersion = 0;

		static UserData()
		{
			RegisterType<AnonWrapper>(InteropAccessMode.HideMembers);
			RegisterType<EnumerableWrapper>(InteropAccessMode.HideMembers);
			s_DefaultAccessMode = InteropAccessMode.LazyOptimized;
		}

		/// <summary>
		/// Registers a type for userdata interop
		/// </summary>
		/// <typeparam name="T">The type to be registered</typeparam>
		/// <param name="accessMode">The access mode (optional).</param>
		/// <param name="friendlyName">Friendly name for the type (optional)</param>
		public static void RegisterType<T>(InteropAccessMode accessMode = InteropAccessMode.Default, string friendlyName = null)
		{
			RegisterType_Impl(typeof(T), accessMode, friendlyName, null);
		}

		/// <summary>
		/// Registers a type for userdata interop
		/// </summary>
		/// <param name="type">The type to be registered</param>
		/// <param name="accessMode">The access mode (optional).</param>
		/// <param name="friendlyName">Friendly name for the type (optional)</param>
		public static void RegisterType(Type type, InteropAccessMode accessMode = InteropAccessMode.Default, string friendlyName = null)
		{
			RegisterType_Impl(type, accessMode, friendlyName, null);
		}

		/// <summary>
		/// Registers a type with a custom userdata descriptor
		/// </summary>
		/// <typeparam name="T">The type to be registered</typeparam>
		/// <param name="customDescriptor">The custom descriptor.</param>
		public static void RegisterType<T>(IUserDataDescriptor customDescriptor)
		{
			RegisterType_Impl(typeof(T), InteropAccessMode.Default, null, customDescriptor);
		}

		/// <summary>
		/// Registers a type with a custom userdata descriptor
		/// </summary>
		/// <param name="type">The type to be registered</param>
		/// <param name="customDescriptor">The custom descriptor.</param>
		public static void RegisterType(Type type, IUserDataDescriptor customDescriptor)
		{
			RegisterType_Impl(type, InteropAccessMode.Default, null, customDescriptor);
		}

		/// <summary>
		/// Registers all types marked with a MoonSharpUserDataAttribute that ar contained in an assembly.
		/// </summary>
		/// <param name="asm">The assembly.</param>
		/// <param name="includeExtensionTypes">if set to <c>true</c> extension types are registered to the appropriate registry.</param>
		public static void RegisterAssembly(Assembly asm = null, bool includeExtensionTypes = false)
		{
			asm = asm ?? Assembly.GetCallingAssembly();

			if (includeExtensionTypes)
			{
				var extensionTypes = from t in asm.GetTypes()
									 let attributes = t.GetCustomAttributes(typeof(ExtensionAttribute), true)
									 where attributes != null && attributes.Length > 0
									 select new { Attributes = attributes, DataType = t };

				foreach (var extType in extensionTypes)
				{
					UserData.RegisterExtensionType(extType.DataType);
				}
			}


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

		/// <summary>
		/// Unregisters a type
		/// </summary>
		/// <typeparam name="T">The type to be unregistered</typeparam>
		public static void UnregisterType<T>()
		{
			UnregisterType(typeof(T));
		}

		/// <summary>
		/// Unregisters a type
		/// </summary>
		/// <param name="t">The The type to be unregistered</param>
		public static void UnregisterType(Type t)
		{
			lock (s_Lock)
			{
				if (s_Registry.ContainsKey(t))
					s_Registry.Remove(t);
			}
		}

		/// <summary>
		/// Creates a userdata DynValue from the specified object
		/// </summary>
		/// <param name="o">The object</param>
		/// <returns></returns>
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

		/// <summary>
		/// Creates a static userdata DynValue from the specified Type
		/// </summary>
		/// <param name="t">The type</param>
		/// <returns></returns>
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

		/// <summary>
		/// Creates a static userdata DynValue from the specified Type
		/// </summary>
		/// <typeparam name="T">The Type</typeparam>
		/// <returns></returns>
		public static DynValue CreateStatic<T>()
		{
			return CreateStatic(typeof(T));
		}

		/// <summary>
		/// Gets or sets the registration policy to be used in the whole application
		/// </summary>
		public static InteropRegistrationPolicy RegistrationPolicy { get; set; }

		/// <summary>
		/// Gets or sets the default access mode to be used in the whole application
		/// </summary>
		/// <value>
		/// The default access mode.
		/// </value>
		/// <exception cref="System.ArgumentException">InteropAccessMode is InteropAccessMode.Default</exception>
		public static InteropAccessMode DefaultAccessMode
		{
			get { return s_DefaultAccessMode; }
			set
			{
				if (value == InteropAccessMode.Default)
					throw new ArgumentException("InteropAccessMode is InteropAccessMode.Default");

				s_DefaultAccessMode = value;
			}
		}

		/// <summary>
		/// Registers an extension Type (that is a type containing extension methods)
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="mode">The InteropAccessMode.</param>
		public static void RegisterExtensionType(Type type, InteropAccessMode mode = InteropAccessMode.Default)
		{
			lock (s_Lock)
			{
				foreach (MethodInfo mi in type.GetMethods().Where(_mi => _mi.IsStatic))
				{
					if (!StandardUserDataMethodDescriptor.CheckMethodIsCompatible(mi, false))
						continue;

					if (mi.GetCustomAttributes(typeof(ExtensionAttribute), false).Length == 0)
						continue;

					var desc = new StandardUserDataMethodDescriptor(mi, mode);

					s_ExtensionMethodRegistry.Add(mi.Name, desc);

					++s_ExtensionMethodChangeVersion;
				}
			}
		}


		/// <summary>
		/// Gets all the extension methods which can match a given name
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static IEnumerable<StandardUserDataMethodDescriptor> GetExtensionMethodsByName(string name)
		{
			lock (s_Lock)
				return s_ExtensionMethodRegistry.Find(name);
		}

		/// <summary>
		/// Gets a number which gets incremented everytime the extension methods registry changes.
		/// Use this to invalidate caches based on extension methods
		/// </summary>
		/// <returns></returns>
		public static int GetExtensionMethodsChangeVersion()
		{
			return s_ExtensionMethodChangeVersion;
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

			lock (s_Lock)
			{
				if (!s_Registry.ContainsKey(type))
				{
					if (descriptor == null)
					{
						if (type.GetInterfaces().Any(ii => ii == typeof(IUserDataType)))
						{
							AutoDescribingUserDataDescriptor audd = new AutoDescribingUserDataDescriptor(type, friendlyName);
							s_Registry.Add(type, audd);
							return audd;
						}
						else
						{
							StandardUserDataDescriptor udd = new StandardUserDataDescriptor(type, accessMode, friendlyName);
							s_Registry.Add(type, udd);

							if (accessMode == InteropAccessMode.BackgroundOptimized)
							{
								ThreadPool.QueueUserWorkItem(o => udd.Optimize());
							}

							return udd;
						}
					}
					else
					{
						s_Registry.Add(type, descriptor);
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
			lock (s_Lock)
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
