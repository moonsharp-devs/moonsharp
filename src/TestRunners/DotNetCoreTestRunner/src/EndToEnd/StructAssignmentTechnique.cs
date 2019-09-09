using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class StructAssignmentTechnique
	{
		public struct Vector3
		{
			public float X;
			public float Y;
			public float Z;
		}


		public class Transform
		{
			public Vector3 position;
		}

		public class Vector3_Accessor 
		{
			Transform transf;

			public Vector3_Accessor(Transform t)
			{
				transf = t;
			}

			public float X
			{
				get { return transf.position.X; }
				set { transf.position.X = value; }
			}

			public float Y
			{
				get { return transf.position.Y; }
				set { transf.position.Y = value; }
			}

			public float Z
			{
				get { return transf.position.Z; }
				set { transf.position.Z = value; }
			}
		}

		//[Test]
		//public void StructField_CanSetWithWorkaround()
		//{
		//	UserData.RegisterType<Vector3>();
		//	UserData.RegisterType<Vector3_Accessor>();

		//	DispatchingUserDataDescriptor descr = (DispatchingUserDataDescriptor)UserData.RegisterType<Transform>();

		//	descr.AddMember("Position", new 


		//	Script S = new Script();

		//	Transform T = new Transform();

		//	T.position.X = 3;

		//	S.Globals["transform"] = T;

		//	S.DoString("transform.position.X = 15;");

		//	Assert.AreEqual(3, T.position.X);
		//	UserData.UnregisterType<Transform>();
		//	UserData.UnregisterType<Vector3>();
		//	UserData.UnregisterType<Vector3_Accessor>();
		//}




		[Test]
		public void StructField_CantSet()
		{
			UserData.RegisterType<Transform>();
			UserData.RegisterType<Vector3>();

			Script S = new Script();

			Transform T = new Transform();

			T.position.X = 3;

			S.Globals["transform"] = T;

			S.DoString("transform.position.X = 15;");

			Assert.AreEqual(3, (int)T.position.X);
			UserData.UnregisterType<Transform>();
			UserData.UnregisterType<Vector3>();
		}









	}
}
