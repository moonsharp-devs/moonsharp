using System.Reflection;

namespace MoonSharp.Interpreter.Interop 
{
    public class MethodMemberDescriptorCoroutine : MethodMemberDescriptor 
	{
        public MethodMemberDescriptorCoroutine(MethodBase methodBase, InteropAccessMode accessMode = InteropAccessMode.Default) : base(methodBase, accessMode) 
		{
        }

        public override DynValue GetValue(Script script, object obj) 
		{
            var enumerateYielder = script.DoString(@"return function (callable) 
    return function (...)
        for y in callable(...) do
            if coroutine.is_return_value(y) then
                return coroutine.get_return_value(y)
            else 
                coroutine.yield(y)
            end
        end
    end
end", null, MethodInfo + "_yielder");
            return script.Call(enumerateYielder, base.GetValue(script, obj));
        }
        
        /// Tries to create a new MethodMemberDescriptorCoroutine, returning 
        /// <c>null</c> in case the method is not
        /// visible to script code.
        /// </summary>
        /// <param name="methodBase">The MethodBase.</param>
        /// <param name="accessMode">The <see cref="InteropAccessMode" /></param>
        /// <param name="forceVisibility">if set to <c>true</c> forces visibility.</param>
        /// <returns>
        /// A new MethodMemberDescriptor or null.
        /// </returns>
        public static MethodMemberDescriptorCoroutine TryCreateCoroutineIfVisible(MethodBase methodBase, InteropAccessMode accessMode, bool forceVisibility = false)
        {
            if (!CheckMethodIsCompatible(methodBase, false))
                return null;

            if (forceVisibility || (methodBase.GetVisibilityFromAttributes() ?? methodBase.IsPublic))
                return new MethodMemberDescriptorCoroutine(methodBase, accessMode);

            return null;
        }
    }
}