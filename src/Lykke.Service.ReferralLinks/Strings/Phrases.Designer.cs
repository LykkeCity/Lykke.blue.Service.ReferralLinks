﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Lykke.Blue.Service.ReferralLinks.Strings {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Phrases {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Phrases() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Lykke.Blue.Service.ReferralLinks.Strings.Phrases", typeof(Phrases).Assembly);
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
        ///   Looks up a localized string similar to Amount cannot be less then zero..
        /// </summary>
        public static string InvalidAmount {
            get {
                return ResourceManager.GetString("InvalidAmount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided asset is not valid..
        /// </summary>
        public static string InvalidAsset {
            get {
                return ResourceManager.GetString("InvalidAsset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided claiming client id is not valid..
        /// </summary>
        public static string InvalidClaimingClientId {
            get {
                return ResourceManager.GetString("InvalidClaimingClientId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided referral link id is not valid..
        /// </summary>
        public static string InvalidReferralLinkId {
            get {
                return ResourceManager.GetString("InvalidReferralLinkId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request cannot be empty..
        /// </summary>
        public static string InvalidRequest {
            get {
                return ResourceManager.GetString("InvalidRequest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided sender client id is not valid..
        /// </summary>
        public static string InvalidSenderClientId {
            get {
                return ResourceManager.GetString("InvalidSenderClientId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided state is not valid..
        /// </summary>
        public static string InvalidState {
            get {
                return ResourceManager.GetString("InvalidState", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You do not have enough TREEs in wallet..
        /// </summary>
        public static string InvalidTreesAmount {
            get {
                return ResourceManager.GetString("InvalidTreesAmount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Maximum number of referral links reached..
        /// </summary>
        public static string ReferralLinksLimitReached {
            get {
                return ResourceManager.GetString("ReferralLinksLimitReached", resourceCulture);
            }
        }
    }
}
