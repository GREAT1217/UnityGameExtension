using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameExtension.Editor
{
    internal static class TypeUtility
    {
        private static readonly string[] EditorAssemblyNames = {"Assembly-CSharp-Editor", "GameExtension.Editor", "Game.Editor"};
        private static readonly string[] RuntimeAssemblyNames = {"Assembly-CSharp", "GameExtension", "Game"};

        public static Type GetEditorType(string typeName)
        {
            return GetType(typeName, EditorAssemblyNames);
        }

        public static Type GetRuntimeType(string typeName)
        {
            return GetType(typeName, RuntimeAssemblyNames);
        }

        public static string[] GetEditorTypeNames(Type typeBase)
        {
            return GetTypeNames(typeBase, EditorAssemblyNames);
        }

        public static string[] GetRuntimeTypeNames(Type typeBase)
        {
            return GetTypeNames(typeBase, RuntimeAssemblyNames);
        }

        private static Type GetType(string typeName, string[] assemblyNames)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            Type type1 = Type.GetType(typeName);
            if (type1 != null)
            {
                return type1;
            }

            foreach (string assemblyName in assemblyNames)
            {
                Type type2 = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
                if (type2 != null)
                {
                    return type2;
                }
            }

            return null;
        }

        private static string[] GetTypeNames(Type typeBase, string[] assemblyNames)
        {
            List<string> typeNames = new List<string>();
            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch
                {
                    continue;
                }

                if (assembly == null)
                {
                    continue;
                }

                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        typeNames.Add(type.FullName);
                    }
                }
            }

            typeNames.Sort();
            return typeNames.ToArray();
        }
    }
}
