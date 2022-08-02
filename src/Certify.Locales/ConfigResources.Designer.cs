﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Certify.Locales {
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
    public class ConfigResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ConfigResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Certify.Locales.ConfigResources", typeof(ConfigResources).Assembly);
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
        ///   Looks up a localized string similar to 63154103-66ba-4636-8c6b-7081084daa0e.
        /// </summary>
        public static string AIInstrumentationKey {
            get {
                return ResourceManager.GetString("AIInstrumentationKey", resourceCulture);
            }
        }
        
        
        /// <summary>
        ///   Looks up a localized string similar to Certify Certificate Manager.
        /// </summary>
        public static string AppName {
            get {
                return ResourceManager.GetString("AppName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://certifytheweb.com/downloads/version.json.
        /// </summary>
        public static string AppUpdateCheckURI {
            get {
                return ResourceManager.GetString("AppUpdateCheckURI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://certifytheweb.com.
        /// </summary>
        public static string AppWebsiteURL {
            get {
                return ResourceManager.GetString("AppWebsiteURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning: This software is a pre-release version for testing and feedback purposes. You should expect bugs. Do not rely on this software for important production sites..
        /// </summary>
        public static string BetaWarning {
            get {
                return ResourceManager.GetString("BetaWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Software is provided &quot;as is&quot; without warranty of any kind, either express or implied, including without limitation any implied warranties of condition, uninterrupted use, merchantability, fitness for a particular purpose, or non-infringement.
        ///
        ///Use of the LetsEncrypt.org service for free SSL/TLS certificates is subject to the LetsEncrypt.org terms of service. This software is not affiliated with or endorsed by LetsEncrypt.org
        ///
        ///This software uses the following Open Source software (or significant port [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Credits {
            get {
                return ResourceManager.GetString("Credits", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Get Started: Browse the Vault on the left to see current certificate information..
        /// </summary>
        public static string GettingStartedExistingVault {
            get {
                return ResourceManager.GetString("GettingStartedExistingVault", resourceCulture);
            }
        }
        

        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///
        ///&lt;configuration&gt;
        ///  &lt;system.webServer&gt;
        ///    &lt;validation validateIntegratedModeConfiguration=&quot;false&quot; /&gt;
        ///    &lt;staticContent&gt;
        ///        &lt;mimeMap fileExtension=&quot;.&quot; mimeType=&quot;text/json&quot; /&gt;
        ///    &lt;/staticContent&gt;
        ///  &lt;handlers&gt;
        ///      &lt;clear /&gt;
        ///    &lt;add name=&quot;StaticFile&quot; path=&quot;*&quot; verb=&quot;*&quot; type=&quot;&quot; modules=&quot;StaticFileModule,DefaultDocumentModule,DirectoryListingModule&quot; scriptProcessor=&quot;&quot; resourceType=&quot;Either&quot; requireAccess=&quot;Read&quot; allowPathInfo=&quot;false&quot; preCondition=&quot;&quot; response [rest of string was truncated]&quot;;.
        /// </summary>
        public static string IISWebConfig {
            get {
                return ResourceManager.GetString("IISWebConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///
        ///&lt;configuration&gt;
        ///  &lt;system.webServer&gt;
        ///    &lt;validation validateIntegratedModeConfiguration=&quot;false&quot; /&gt;
        ///    &lt;staticContent&gt;
        ///      &lt;mimeMap fileExtension=&quot;.&quot; mimeType=&quot;text/json&quot; /&gt;
        ///    &lt;/staticContent&gt;
        ///  &lt;/system.webServer&gt;
        ///  &lt;system.web&gt;
        ///    &lt;authorization&gt;
        ///      &lt;allow users=&quot;*&quot; /&gt;
        ///    &lt;/authorization&gt;
        ///  &lt;/system.web&gt;
        ///&lt;/configuration&gt;.
        /// </summary>
        public static string IISWebConfigAlt {
            get {
                return ResourceManager.GetString("IISWebConfigAlt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///
        ///&lt;configuration&gt;
        ///  &lt;system.webServer&gt;
        ///    &lt;validation validateIntegratedModeConfiguration=&quot;false&quot; /&gt;
        ///    &lt;staticContent&gt;
        ///        &lt;mimeMap fileExtension=&quot;.*&quot; mimeType=&quot;text/json&quot; /&gt;
        ///    &lt;/staticContent&gt;
        ///  &lt;handlers&gt;
        ///      &lt;clear /&gt;
        ///    &lt;add name=&quot;StaticFile&quot; path=&quot;*&quot; verb=&quot;*&quot; type=&quot;&quot; modules=&quot;StaticFileModule,DefaultDocumentModule,DirectoryListingModule&quot; scriptProcessor=&quot;&quot; resourceType=&quot;Either&quot; requireAccess=&quot;Read&quot; allowPathInfo=&quot;false&quot; preCondition=&quot;&quot; respons [rest of string was truncated]&quot;;.
        /// </summary>
        public static string IISWebConfigAlt2 {
            get {
                return ResourceManager.GetString("IISWebConfigAlt2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://localhost:9696.
        /// </summary>
        public static string LocalServiceBaseURI {
            get {
                return ResourceManager.GetString("LocalServiceBaseURI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://localhost:9695.
        /// </summary>
        public static string LocalServiceBaseURIDebug {
            get {
                return ResourceManager.GetString("LocalServiceBaseURIDebug", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Certify Certificate Manager.
        /// </summary>
        public static string LongAppName {
            get {
                return ResourceManager.GetString("LongAppName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are using the latest version..
        /// </summary>
        public static string UpdateCheckLatestVersion {
            get {
                return ResourceManager.GetString("UpdateCheckLatestVersion", resourceCulture);
            }
        }
    }
}
