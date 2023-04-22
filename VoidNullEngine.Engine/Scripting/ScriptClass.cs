using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Utils;
using NLua;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace VoidNullEngine.Engine.Scripting
{
    public sealed class ScriptClass<T> where T : class, IScriptObject, new()
    {
        private const string INSTANCE_NAME = "class";
        private const string BINDING_TABLE = "bindings";

        private const string INVOKE_FUNCTION = BINDING_TABLE + ".invoke";
        private const string SETTER_FUNCTION = BINDING_TABLE + ".set";
        private const string GETTER_FUNCTION = BINDING_TABLE + ".get";

        #region Code Generation
        #region Lua Bindings
        private static void InitObject(T instance) => instance._init();

        private static object CallMethod(T instance, string name, object[] args)
        {
            // Debug.WriteLine($"Searching for {name}({string.Join(",", from arg in args select arg.GetType())})");
            if (instance.GetType().GetMethod(name, args.GetTypes()) is MethodInfo method)
            {
                // Debug.WriteLine($"Calling {method}");
                try { return method.Invoke(instance, args); }
                catch { throw; }
            }
            throw new MissingMemberException();
        }

        private static object GetProperty(T instance, string name)
        {
            try
            {
                if (instance.GetType().GetField(name) is FieldInfo field) return field.GetValue(instance);
                if (instance.GetType().GetProperty(name) is PropertyInfo property) return property.GetValue(instance);
            }
            catch
            {
                throw;
            }
            throw new MissingMemberException();
        }

        private static void SetProperty(T instance, string name, object value)
        {
            try
            {
                if (instance.GetType().GetField(name) is FieldInfo field)
                {
                    field.SetValue(instance, value);
                    return;
                }
                
                if (instance.GetType().GetProperty(name) is PropertyInfo property)
                {
                    property.SetValue(instance, value);
                    return;
                }
            }
            catch
            {
                throw;
            }
            
            throw new MissingMemberException();
        }
        #endregion

        public static ScriptClass<T> Compile(string script, bool rawScript = false)
        {
            if (!IsValid) throw new Exception($"{typeof(T)} is not a valid target for scripting");
            if (!rawScript) script = File.ReadAllText(script);

            lock (_registry)
            {
                if (_registry.TryGetValue(script, out var @class)) return @class;

                Lua Generator()
                {
                    const string INIT_FUNCTION = BINDING_TABLE + ".init";

                    Lua lua = new Lua();
                    lua.State.Encoding = Encoding.UTF8;
                    lua.LoadCLRPackage();
                    lua.DoString(GetImports());

                    lua.NewTable(BINDING_TABLE);
                    lua.RegisterFunction(INIT_FUNCTION, typeof(ScriptClass<T>).GetMethod(nameof(InitObject), BindingFlags.NonPublic | BindingFlags.Static));

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("import = function () end");
                    sb.AppendLine($"{INSTANCE_NAME} = {{}}");
                    sb.AppendLine(script);
                    sb.AppendLine($"luanet.make_object({INSTANCE_NAME}, '{typeof(T).FullName}')");
                    sb.AppendLine($"{INIT_FUNCTION}({INSTANCE_NAME})");
                    sb.AppendLine($"{INIT_FUNCTION} = nil");

                    lua.DoString(sb.ToString());

                    lua.RegisterFunction(INVOKE_FUNCTION, typeof(ScriptClass<T>).GetMethod(nameof(CallMethod), BindingFlags.NonPublic | BindingFlags.Static));
                    lua.RegisterFunction(GETTER_FUNCTION, typeof(ScriptClass<T>).GetMethod(nameof(GetProperty), BindingFlags.NonPublic | BindingFlags.Static));
                    lua.RegisterFunction(SETTER_FUNCTION, typeof(ScriptClass<T>).GetMethod(nameof(SetProperty), BindingFlags.NonPublic | BindingFlags.Static));

                    return lua;
                }

                @class = new ScriptClass<T>(Generator);
                _registry.Add(script, @class);

                return @class;
            }
        }

        private static string GetImports()
        {
            const string BASE_IMPORT = "import('VoidNullEngine.Engine.Scripting', 'VoidNullEngine.Engine')\n";
            StringBuilder sb = new StringBuilder(BASE_IMPORT);
            HashSet<string> imports = new HashSet<string> { BASE_IMPORT };
            Type type = typeof(T);
            foreach (var import in type.GetCustomAttributes<ScriptClassImportAttribute>())
            {
                string i;
                if (import.Assembly is not null) i = $"import('{import.Assembly}','{import.Namespace}')";
                else i = $"import('{import.Namespace}')";
                if (imports.Add(i)) sb.AppendLine(i);
            }
            return sb.AppendLine().ToString();
        }

        private static bool AssertClass()
        {
            Type type = typeof(T);

            var initMethod = type.GetMethod(nameof(IScriptObject._init), Type.EmptyTypes);
            if (!initMethod.IsVirtual)
            {
                Debug.WriteLine($"{initMethod} in {type} must be virtual");
                return false;
            }

            return true;
        }
        #endregion

        #region ScriptObject Methods
        internal static bool TryGetMember(Lua lua, string name, out object result)
        {
            try
            {
                result = lua.GetFunction(GETTER_FUNCTION).Call(lua[INSTANCE_NAME], name)[0];
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        internal static bool TrySetMember(Lua lua, string name, object value)
        {
            try
            {
                lua.GetFunction(SETTER_FUNCTION).Call(lua[INSTANCE_NAME], name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static bool TryInvokeMember(Lua lua, string name, object[] args, out object result)
        {
            try
            {
                result = lua.GetFunction(INVOKE_FUNCTION).Call(lua[INSTANCE_NAME], name, args)[0];
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        internal static void Dispose(Lua lua)
        {
            lua.DoString($"luanet.free_object({INSTANCE_NAME}); {INSTANCE_NAME} = nil");
            lua.Dispose();
        }
        #endregion

        #region Class Data
        public static readonly bool IsValid;
        private static readonly Dictionary<string, ScriptClass<T>> _registry;

        static ScriptClass()
        {
            if (IsValid = AssertClass()) _registry = new Dictionary<string, ScriptClass<T>>();
        }

        private readonly Func<Lua> _generator;

        private ScriptClass(Func<Lua> generator) => _generator = generator;

        public ScriptObject<T> NewInstance() =>
            new ScriptObject<T>(this, _generator());
        #endregion
    }
}
