//
// Application.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using Mono.Unix;

//using Banshee.Library;
//using Banshee.Sources;

namespace Banshee.ServiceStack
{
    public static class Application
    {
        public static void Run ()
        {
            ServiceManager.Instance.Run ();
            //ServiceManager.SourceManager.AddSource(new LibrarySource());
        }
        
        public static string Title {
            get { return "Banshee"; }
        }
        
        public static string InternalName {
            get { return "banshee"; }
        }
        
        private static string version;
        public static string Version {
            get { 
                if (version != null) {
                    return version;
                }
                
                try {
                    AssemblyName name = Assembly.GetEntryAssembly ().GetName ();
                    version = String.Format ("{0}.{1}.{2}", name.Version.Major, 
                        name.Version.Minor, name.Version.Build);
                } catch {
                    version = Catalog.GetString ("Unknown");
                }
                
                return version;
            }
        }
    }
}
