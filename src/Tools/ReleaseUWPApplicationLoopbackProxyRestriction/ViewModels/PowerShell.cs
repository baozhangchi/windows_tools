#region FileHeader

// // Project:  ReleaseUWPApplicationLoopbackProxyRestriction
// // File:  PowerShell.cs
// // CreateTime:  2022-12-30 16:29
// // LastUpdateTime:  2023-01-05 9:23

#endregion

#region Nmaespaces

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;

#endregion

namespace ReleaseUWPApplicationLoopbackProxyRestriction.ViewModels
{
    internal static class PowerShell
    {
        #region Methods

        public static string RunScript(string script)
        {
            // create Powershell runspace

            var results = ExecuteScript(script);

            // convert the script result into a single string

            var stringBuilder = new StringBuilder();
            foreach (var obj in results) stringBuilder.AppendLine(obj.ToString());

            return stringBuilder.ToString();
        }

        public static T RunScript<T>(string script)
            where T : class, new()
        {
            var results = ExecuteScript(script);
            if (results == null || results.Count == 0) return default;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var tmp = Activator.CreateInstance<T>();
            var item = results.First();
            foreach (var property in properties)
            {
                var psProperty = item.Properties.FirstOrDefault(x => x.Name == property.Name);
                if (psProperty != null) property.SetValue(tmp, psProperty.Value);
            }

            return tmp;
        }

        public static List<T> RunScriptList<T>(string script)
            where T : class, new()
        {
            var results = ExecuteScript(script);
            if (results == null || results.Count == 0) return default;
            var list = new List<T>();
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var item in results)
            {
                var tmp = Activator.CreateInstance<T>();
                foreach (var property in properties)
                {
                    var psProperty = item.Properties.FirstOrDefault(x => x.Name == property.Name);
                    if (psProperty != null) property.SetValue(tmp, psProperty.Value);
                }

                list.Add(tmp);
            }

            return list;
        }

        private static Collection<PSObject> ExecuteScript(string script)
        {
            var runspace = RunspaceFactory.CreateRunspace();

            // open it

            runspace.Open();

            // create a pipeline and feed it the script text

            var pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(script);

            // add an extra command to transform the script
            // output objects into nicely formatted strings

            // remove this line to get the actual objects
            // that the script returns. For example, the script

            // "Get-Process" returns a collection
            // of System.Diagnostics.Process instances.

            //pipeline.Commands.Add("Out-String");

            // execute the script

            var results = pipeline.Invoke();

            // close the runspace

            runspace.Close();
            return results;
        }

        #endregion
    }
}