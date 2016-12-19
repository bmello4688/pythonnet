using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Security;

namespace Python.Runtime
{
    /// <summary>
    /// This class is responsible for efficiently maintaining the bits
    /// of information we need to support aliases with 'nice names'.
    /// </summary>
    internal class GenericUtil
    {
        static Dictionary<string, Dictionary<string, List<string>>> mapping;

        private GenericUtil()
        {
        }

        static GenericUtil()
        {
            mapping = new
                Dictionary<string, Dictionary<string, List<string>>>();
        }

        //====================================================================
        // Register a generic type that appears in a given namespace.
        //====================================================================

        internal static void Register(Type t)
        {
            if (null == t.Namespace || null == t.Name)
                return;

            Dictionary<string, List<string>> nsmap = null;
            mapping.TryGetValue(t.Namespace, out nsmap);
            if (nsmap == null)
            {
                nsmap = new Dictionary<string, List<string>>();
                mapping[t.Namespace] = nsmap;
            }
            string basename = t.Name;
            int tick = basename.IndexOf("`");
            if (tick > -1)
            {
                basename = basename.Substring(0, tick);
            }
            List<string> gnames = null;
            nsmap.TryGetValue(basename, out gnames);
            if (gnames == null)
            {
                gnames = new List<string>();
                nsmap[basename] = gnames;
            }
            gnames.Add(t.Name);
        }

        //====================================================================
        // xxx
        //====================================================================

        public static List<string> GetGenericBaseNames(string ns)
        {
            Dictionary<string, List<string>> nsmap = null;
            mapping.TryGetValue(ns, out nsmap);
            if (nsmap == null)
            {
                return null;
            }
            List<string> names = new List<string>();
            foreach (string key in nsmap.Keys)
            {
                names.Add(key);
            }
            return names;
        }

        //====================================================================
        // xxx
        //====================================================================

        public static Type GenericForType(Type t, int paramCount)
        {
            return GenericByName(t.Namespace, t.Name, paramCount);
        }

        public static Type GenericByName(string ns, string name, int paramCount)
        {
            foreach (Type t in GenericsByName(ns, name))
            {
                if (t.GetGenericArguments().Length == paramCount)
                    return t;
            }
            return null;
        }

        public static List<Type> GenericsForType(Type t)
        {
            return GenericsByName(t.Namespace, t.Name);
        }

        public static List<Type> GenericsByName(string ns, string basename)
        {
            Dictionary<string, List<string>> nsmap = null;
            mapping.TryGetValue(ns, out nsmap);
            if (nsmap == null)
            {
                return null;
            }

            int tick = basename.IndexOf("`");
            if (tick > -1)
            {
                basename = basename.Substring(0, tick);
            }

            List<string> names = null;
            nsmap.TryGetValue(basename, out names);
            if (names == null)
            {
                return null;
            }

            List<Type> result = new List<Type>();
            foreach (string name in names)
            {
                string qname = ns + "." + name;
                Type o = AssemblyManager.LookupType(qname);
                if (o != null)
                {
                    result.Add(o);
                }
            }

            return result;
        }

        //====================================================================
        // xxx
        //====================================================================

        public static string GenericNameForBaseName(string ns, string name)
        {
            Dictionary<string, List<string>> nsmap = null;
            mapping.TryGetValue(ns, out nsmap);
            if (nsmap == null)
            {
                return null;
            }
            List<string> gnames = null;
            nsmap.TryGetValue(name, out gnames);
            if (gnames == null)
            {
                return null;
            }
            if (gnames.Count > 0)
            {
                return gnames[0];
            }
            return null;
        }
    }
}