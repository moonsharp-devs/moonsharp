using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	public abstract class SandboxedPlatformAccessor : PlatformAccessorBase
	{
		public SandboxedPlatformAccessor()
			: base()
		{ }

		public SandboxedPlatformAccessor(params string[] modulePaths)
			: base(modulePaths)
		{ }


		public override System.IO.Stream OpenFileForIO(Script script, string filename, Encoding encoding, string mode)
		{
			throw new InvalidOperationException("SandboxedPlatformAccessor does not support 'io' and 'os' operations. Provide your own implementation of platform to work around this limitation, if needed.");
		}

		public override string GetEnvironmentVariable(string envvarname)
		{
			throw new InvalidOperationException("SandboxedPlatformAccessor does not support 'io' and 'os' operations. Provide your own implementation of platform to work around this limitation, if needed.");
		}

		public override CoreModules FilterSupportedCoreModules(CoreModules module)
		{
			return module & (~(CoreModules.IO | CoreModules.OS_System));
		}
	}
}
