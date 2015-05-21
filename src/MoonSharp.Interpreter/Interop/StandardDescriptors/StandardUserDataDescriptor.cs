using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Standard descriptor for userdata types.
	/// </summary>
	public class StandardUserDataDescriptor : DispatchingUserDataDescriptor
	{
		/// <summary>
		/// Gets the interop access mode this descriptor uses for members access
		/// </summary>
		public InteropAccessMode AccessMode { get; private set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataDescriptor"/> class.
		/// </summary>
		/// <param name="type">The type this descriptor refers to.</param>
		/// <param name="accessMode">The interop access mode this descriptor uses for members access</param>
		/// <param name="friendlyName">A human readable friendly name of the descriptor.</param>
		public StandardUserDataDescriptor(Type type, InteropAccessMode accessMode, string friendlyName = null)
			: base(type, friendlyName)
		{
			if (accessMode == InteropAccessMode.NoReflectionAllowed)
				throw new ArgumentException("Can't create a StandardUserDataDescriptor under a NoReflectionAllowed access mode");

			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			if (accessMode == InteropAccessMode.Default)
				accessMode = UserData.DefaultAccessMode;

			AccessMode = accessMode;

			FillMemberList();
		}

		/// <summary>
		/// Fills the member list.
		/// </summary>
		private void FillMemberList()
		{
			Type type = this.Type;
			var accessMode = this.AccessMode;

			if (AccessMode == InteropAccessMode.HideMembers)
				return;

			// add declared constructors
			foreach (ConstructorInfo ci in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				AddMember("__new", MethodMemberDescriptor.TryCreateIfVisible(ci, this.AccessMode));
			}

			// valuetypes don't reflect their empty ctor.. actually empty ctors are a perversion, we don't care and implement ours
			if (type.IsValueType)
				AddMember("__new", new ValueTypeDefaultCtorMemberDescriptor(type));


			// add methods to method list and metamethods
			foreach (MethodInfo mi in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				MethodMemberDescriptor md = MethodMemberDescriptor.TryCreateIfVisible(mi, this.AccessMode);

				if (md != null)
				{
					if (!MethodMemberDescriptor.CheckMethodIsCompatible(mi, false))
						continue;

					// transform explicit/implicit conversions to a friendlier name.
					string name = mi.Name;
					if (mi.IsSpecialName && (mi.Name == SPECIALNAME_CAST_EXPLICIT || mi.Name == SPECIALNAME_CAST_IMPLICIT))
					{
						name = mi.ReturnType.GetConversionMethodName();
					}

					AddMember(name, md);

					foreach (string metaname in mi.GetMetaNamesFromAttributes())
					{
						AddMetaMember(metaname, md);
					}
				}
			}

			// get properties
			foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				if (pi.IsSpecialName || pi.GetIndexParameters().Any())
					continue;

				AddMember(pi.Name, PropertyMemberDescriptor.TryCreateIfVisible(pi, this.AccessMode));
			}

			// get fields
			foreach (FieldInfo fi in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				if (fi.IsSpecialName)
					continue;

				AddMember(fi.Name, FieldMemberDescriptor.TryCreateIfVisible(fi, this.AccessMode));
			}

			// get events
			foreach (EventInfo ei in type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
			{
				if (ei.IsSpecialName)
					continue;

				AddMember(ei.Name, EventMemberDescriptor.TryCreateIfVisible(ei, this.AccessMode));
			}

			// get nested types and create statics
			foreach (Type nestedType in type.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
			{
				if (!nestedType.IsGenericTypeDefinition)
				{
					if (nestedType.IsNestedPublic || nestedType.GetCustomAttributes(typeof(MoonSharpUserDataAttribute), true).Length > 0)
					{
						var descr = UserData.RegisterType(nestedType, this.AccessMode);

						if (descr != null)
							AddDynValue(nestedType.Name, UserData.CreateStatic(nestedType));
					}
				}
			}

			if (Type.IsArray)
			{
				int rank = Type.GetArrayRank();

				ParameterDescriptor[] get_pars = new ParameterDescriptor[rank];
				ParameterDescriptor[] set_pars = new ParameterDescriptor[rank + 1];

				for (int i = 0; i < rank; i++)
					get_pars[i] = set_pars[i] = new ParameterDescriptor("idx" + i.ToString(), typeof(int));

				set_pars[rank] = new ParameterDescriptor("value", Type.GetElementType());

				AddMember(SPECIALNAME_INDEXER_SET, new ObjectCallbackMemberDescriptor(SPECIALNAME_INDEXER_SET, ArrayIndexerSet, set_pars));
				AddMember(SPECIALNAME_INDEXER_GET, new ObjectCallbackMemberDescriptor(SPECIALNAME_INDEXER_GET, ArrayIndexerGet, get_pars));
			}
			else if (Type == typeof(Array))
			{
				AddMember(SPECIALNAME_INDEXER_SET, new ObjectCallbackMemberDescriptor(SPECIALNAME_INDEXER_SET, ArrayIndexerSet));
				AddMember(SPECIALNAME_INDEXER_GET, new ObjectCallbackMemberDescriptor(SPECIALNAME_INDEXER_GET, ArrayIndexerGet));
			}
		}


		private int[] BuildArrayIndices(CallbackArguments args, int count)
		{
			int[] indices = new int[count];

			for (int i = 0; i < count; i++)
				indices[i] = args.AsInt(i, "userdata_array_indexer");

			return indices;
		}

		private object ArrayIndexerSet(object arrayObj, ScriptExecutionContext ctx, CallbackArguments args)
		{
			Array array = (Array)arrayObj;
			int[] indices = BuildArrayIndices(args, args.Count - 1);
			DynValue value = args[args.Count - 1];

			Type elemType = array.GetType().GetElementType();

			object objValue = ScriptToClrConversions.DynValueToObjectOfType(value, elemType, null, false);

			array.SetValue(objValue, indices);

			return DynValue.Void;
		}


		private object ArrayIndexerGet(object arrayObj, ScriptExecutionContext ctx, CallbackArguments args)
		{
			Array array = (Array)arrayObj;
			int[] indices = BuildArrayIndices(args, args.Count);

			return array.GetValue(indices);
		}
	}
}
