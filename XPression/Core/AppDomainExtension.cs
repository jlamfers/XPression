#region  License
/*
Copyright 2017 - Jaap Lamfers - jlamfers@xipton.net

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 * */
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace XPression.Core
{
   internal static class AppDomainExtension
   {

      private static readonly ConcurrentDictionary<AppDomain, AppDomain>
          ProcessedAppDomains = new ConcurrentDictionary<AppDomain, AppDomain>();


      public static Type[] GetVisibleTypes(this Assembly self)
      {
         try
         {
            return self.GetTypes().ToArray();
         }
         catch (ReflectionTypeLoadException e)
         {
            // we still have access to all load attempts
            return e.Types.Where(t => t != null).ToArray();
         }
      }

      private static IList<Type> _exportedTypes;
      public static IEnumerable<Type> GetExportedTypes(this AppDomain self)
      {
         return _exportedTypes ?? (_exportedTypes = self.GetAvailableAssemblies().SelectMany(a => a.GetVisibleTypes()).ToList().AsReadOnly());
      }
      public static IEnumerable<Assembly> GetAvailableAssemblies(this AppDomain self)
      {
         return self.EnsureAvailableAssembliesLoaded().GetAssemblies();
      }
      public static IList<Assembly> GetAssembliesFromDirectory(this AppDomain self, string path)
      {
         if (!Directory.Exists(path))
         {
            var binFolder = !string.IsNullOrEmpty(self.RelativeSearchPath)
                ? Path.Combine(self.BaseDirectory, self.RelativeSearchPath)
                : self.BaseDirectory;
            path = Path.Combine(binFolder, path);
            if (!Directory.Exists(path))
            {
               throw new XPressionException("Path not found: " + path);
            }
         }

         return Directory.GetFiles(path, "*.dll")
            .Union(Directory.GetFiles(path, "*.exe"))
            .Select(s => AppDomain.CurrentDomain.EnsureAssemblyIsLoaded(s))
            .Where(a => a != null)
            .ToList();
      }

      public static AppDomain EnsureAvailableAssembliesLoaded(this AppDomain self)
      {
         ProcessedAppDomains.GetOrAdd(self, ad =>
         {

            var binFolder = !string.IsNullOrEmpty(self.RelativeSearchPath)
               ? Path.Combine(self.BaseDirectory, self.RelativeSearchPath)
               : self.BaseDirectory;

            GetAssembliesFromDirectory(self, binFolder);

            return ad;
         });

         return self;
      }
      public static Assembly EnsureAssemblyIsLoaded(this AppDomain self, string assemblyFileName)
      {
         try
         {
            var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
            var assembly = self.GetAssemblies().FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName())) ??
                           self.Load(assemblyName);
            return assembly;
         }
         catch (BadImageFormatException)
         {
            // thrown by GetAssemblyName
            // ignore this assembly since it is an unmanaged assembly
         }
         return null;
      }

   }
}