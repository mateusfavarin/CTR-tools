﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CTRFramework.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CTRFramework.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap _default {
            get {
                object obj = ResourceManager.GetObject("_default", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 06.05.2024 12:39:50,50 
        ///.
        /// </summary>
        public static string BuildDate {
            get {
                return ResourceManager.GetString("BuildDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to (C) 1999, Mutato Muzika: Mark Mothersbaugh, Josh Mancell.
        ///
        ///Converted to MIDI using CTR-Tools by DCxDemo*..
        /// </summary>
        public static string midi_copyright {
            get {
                return ResourceManager.GetString("midi_copyright", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Press any key....
        /// </summary>
        public static string PressAnyKey {
            get {
                return ResourceManager.GetString("PressAnyKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 2016-2024, DCxDemo*..
        /// </summary>
        public static string signature {
            get {
                return ResourceManager.GetString("signature", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to r15prev.
        /// </summary>
        public static string Version {
            get {
                return ResourceManager.GetString("Version", resourceCulture);
            }
        }
    }
}
