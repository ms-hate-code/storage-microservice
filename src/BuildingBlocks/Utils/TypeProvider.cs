using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace BuildingBlocks.Utils
{
    public static class TypeProvider
    {
        /// <summary>
        /// get all referenced assemblies from the given assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IReadOnlyList<Assembly> GetReferencedAssemblies(Assembly? assembly)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<Assembly>();
            var result = new List<Assembly>();

            var root = assembly ?? Assembly.GetEntryAssembly();
            queue.Enqueue(root);
            do
            {
                var asm = queue.Dequeue();
                if (asm is null)
                {
                    break;
                }

                result.Add(asm);

                foreach (var reference in asm.GetReferencedAssemblies())
                {
                    if (!visited.Contains(reference.FullName))
                    {
                        queue.Enqueue(Assembly.Load(reference));
                        visited.Add(reference.FullName);
                    }
                }

            } while (queue.Count > 0);

            return result.Distinct().ToList().AsReadOnly();
        }

        /// <summary>
        /// load assemblies list base on ApplicationPartAttribute from the given assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IReadOnlyList<Assembly> GetApplicationPartAssemblies(Assembly assembly)
        {
            var rootNamespace = assembly.GetName().Name!.Split(".").First();
            var result = assembly!.GetCustomAttributes<ApplicationPartAttribute>()
                .Where(x => x.AssemblyName.StartsWith(rootNamespace, StringComparison.InvariantCulture))
                .Select(x => Assembly.LoadFrom(x.AssemblyName))
                .Distinct()
                .ToList()
                .AsReadOnly();

            return result;
        }

        public static Type? GetFirstMatchingTypeFromCurrentDomainAssembly(string typeName)
        {
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
                .FirstOrDefault();

            return result;
        }
    }
}
