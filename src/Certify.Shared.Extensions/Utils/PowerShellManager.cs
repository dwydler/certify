﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Certify.Models;
using Certify.Models.Config;
using SimpleImpersonation;

namespace Certify.Management
{
    /// <summary>
    /// PowerShell script execution manager. 
    /// Manage the execution of PowerShell scripts, either in-process or by launching a new process.
    /// </summary>
    public class PowerShellManager
    {
        /// <summary>
        /// Run a PowerShell script, either in-process or by launching a new process.
        /// </summary>
        /// <param name="powershellExecutionPolicy">Unrestricted etc, see https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_execution_policies?view=powershell-7.3</param>
        /// <param name="result">Result object to pass to the script</param>
        /// <param name="scriptFile">Path to the script file</param>
        /// <param name="parameters">Parameters to pass to the script</param>
        /// <param name="scriptContent">Content of the script</param>
        /// <param name="credentials">Credentials to use for running the script</param>
        /// <param name="logonType">Logon type to use for running the script</param>
        /// <param name="ignoredCommandExceptions">Commands to ignore exceptions for</param>
        /// <param name="timeoutMinutes">Timeout in minutes</param>
        /// <param name="launchNewProcess">Launch a new process</param>
        /// <returns>ActionResult</returns>
        public static async Task<ActionResult> RunScript(
            string powershellExecutionPolicy,
            CertificateRequestResult result = null,
            string scriptFile = null,
            Dictionary<string, object> parameters = null,
            string scriptContent = null,
            Dictionary<string, string> credentials = null,
            string logonType = null,
            string[] ignoredCommandExceptions = null,
            int timeoutMinutes = 5,
            bool launchNewProcess = false
            )
        {
            // argument check for script file existence and .ps1 extension
            FileInfo scriptInfo = null;
            if (scriptContent == null)
            {
                scriptInfo = new FileInfo(scriptFile);
                if (!scriptInfo.Exists)
                {
                    throw new ArgumentException($"File '{scriptFile}' does not exist.");
                }

                if (scriptInfo.Extension.ToLower() != ".ps1")
                {
                    throw new ArgumentException($"File '{scriptFile}' is not a powershell script.");
                }
            }

            if (launchNewProcess)
            {
                // spawn new process as the given user
                return await ExecutePowershellAsProcess(result, powershellExecutionPolicy, scriptFile, parameters, credentials, logonType, scriptContent, null, ignoredCommandExceptions: ignoredCommandExceptions, timeoutMinutes: timeoutMinutes);
            }
            else
            {
                // run powershell script in-process, optionally with impersonation
                try
                {
                    // create a new runspace to isolate the scripts
                    using (var runspace = RunspaceFactory.CreateRunspace())
                    {
                        runspace.Open();

                        // set working directory to the script file's directory
                        if (scriptInfo != null)
                        {
                            runspace.SessionStateProxy.Path.SetLocation(scriptInfo.DirectoryName);
                        }

                        using (var shell = PowerShell.Create())
                        {
                            shell.Runspace = runspace;

                            // running PowerShell under credentials currently only supported for windows
                            var credentialsProvidedButNotSupported = false;

                            if (credentials?.Any() == true && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                // TODO: warn credentials not supported on this platform
                                credentialsProvidedButNotSupported = true;
                            }

                            if (credentials?.Any() == true && credentialsProvidedButNotSupported == false && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                // run as windows user
                                UserCredentials windowsCredentials = null;

                                try
                                {
                                    windowsCredentials = GetWindowsCredentials(credentials);
                                }
                                catch
                                {
                                    var err = "Command with Windows Credentials requires username and password.";

                                    return new ActionResult(err, false);
                                }

                                var _defaultLogonType = GetLogonType(logonType);

                                ActionResult powerShellResult = null;
                                using (var userHandle = windowsCredentials.LogonUser(_defaultLogonType))
                                {
                                    WindowsIdentity.RunImpersonated(userHandle, () =>
                                    {
                                        powerShellResult = InvokePowershell(result, powershellExecutionPolicy, scriptFile, parameters, scriptContent, shell, ignoredCommandExceptions: ignoredCommandExceptions, timeoutMinutes: timeoutMinutes);

                                    });
                                }

                                return powerShellResult;
                            }
                            else
                            {
                                // run as current user
                                return InvokePowershell(result, powershellExecutionPolicy, scriptFile, parameters, scriptContent, shell, ignoredCommandExceptions: ignoredCommandExceptions, timeoutMinutes: timeoutMinutes);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return await Task.FromResult(new ActionResult($"Error - {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", false));
                }
            }
        }

        private static LogonType GetLogonType(string logonType)
        {
            return logonType?.ToLower() switch
            {
                "network" => LogonType.Network,
                "batch" => LogonType.Batch,
                "service" => LogonType.Service,
                "interactive" => LogonType.Interactive,
                "newcredentials" => LogonType.NewCredentials,
                _ => LogonType.NewCredentials,
            };
        }

        /// <summary>
        /// Get the path to the pwoershell exe, optionally using a preferred path first
        /// </summary>
        /// <param name="powershellPathPreference"></param>
        /// <returns></returns>
        private static string GetPowershellExePath(string powershellPathPreference)
        {
            var searchPaths = new List<string>() {
                "%WINDIR%\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
                "%PROGRAMFILES%\\PowerShell\\7\\pwsh.exe",
                "/usr/bin/pwsh"
            };

            if (!string.IsNullOrWhiteSpace(powershellPathPreference))
            {
                searchPaths.Insert(0, powershellPathPreference);
            }

            // if powershell exe path supplied, use that (with expansion) and check exe exists
            // otherwise detect powershell exe location
            foreach (var exePath in searchPaths)
            {
                var filePath = Environment.ExpandEnvironmentVariables(exePath);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }

            return null;
        }

        private static async Task<ActionResult> ExecutePowershellAsProcess(CertificateRequestResult result, string executionPolicy, string scriptFile, Dictionary<string, object> parameters, Dictionary<string, string> credentials, string logonType, string scriptContent, PowerShell shell, bool autoConvertBoolean = true, string[] ignoredCommandExceptions = null, int timeoutMinutes = 5, string powershellPathPreference = null)
        {
            var _log = new StringBuilder();

            var commandExe = GetPowershellExePath(powershellPathPreference);

            if (commandExe == null)
            {
                return new ActionResult("Failed to locate powershell executable. Cannot launch as new process.", false);
            }

            if (!string.IsNullOrEmpty(scriptContent))
            {
                // script content would need to be run from a file, for that we need to run encrypted script content otherwise credentials would appear in temp file
                return new ActionResult("Script content is not yet supported when used with launch as new process.", false);
            }

            var resultObj = parameters?.Where(p => p.Key == "result" && p.Value != null).FirstOrDefault().Value;
            var resultJson = resultObj != null ? Newtonsoft.Json.JsonConvert.SerializeObject(resultObj) : null;

            var resultsJsonTempPath = string.Empty;
            var resultsJsonExported = false;

            var appBasePath = AppContext.BaseDirectory;

            var wrapperScriptPath = Path.Combine(new string[] { appBasePath, "Scripts", "Internal", "Script-Wrapper.ps1" });
            var wrapperScriptSourceText = File.ReadAllText(wrapperScriptPath);

            var isUsingCredentials = (credentials != null && credentials.ContainsKey("username") && credentials.ContainsKey("password"));

            if (isUsingCredentials && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)))
            {
                // The impersonating user must be able to read the script wrapper so that the process starting under their credentials can call it. They will also need to be able to read the users supplied target script (not addressed here).
                // If the Results object is also being used we write that to a temp file and set the ACL to allow read by the impersonating user.

                try
                {
                    var username = GetWindowsCredentialsUsername(credentials);

                    var wrapperTempPath = Path.GetTempPath();
                    var wrapperTempFilePath = Path.GetTempFileName();
                    wrapperScriptPath = Path.ChangeExtension(wrapperTempFilePath, ".ps1");
                    File.WriteAllText(wrapperScriptPath, wrapperScriptSourceText);
                    ApplyFileACL(wrapperScriptPath, username);

                    resultsJsonTempPath = Path.GetTempFileName();
                    File.WriteAllText(resultsJsonTempPath, resultJson);
                    ApplyFileACL(resultsJsonTempPath, username);

                    resultsJsonExported = true;
                }
                catch
                {
                    var err = "A command with Windows Credentials requires a correct username and password. Check credentials.";

                    return new ActionResult(err, false);
                }
            }

            var arguments = $" -File \"{wrapperScriptPath}\"";

            if (parameters?.Any(p => p.Key.ToLower() == "executionpolicy") == true)
            {
                executionPolicy = parameters.FirstOrDefault(p => p.Key.ToLower() == "executionpolicy").Value?.ToString();
            }

            if (!string.IsNullOrEmpty(executionPolicy))
            {
                arguments = $"-ExecutionPolicy {executionPolicy} {arguments}";
            }

            arguments += $" -scriptFile \"{scriptFile}\"";

            if (parameters?.Any() == true)
            {
                foreach (var p in parameters)
                {
                    if (p.Key == "result" && p.Value != null)
                    {
                        if (!resultsJsonExported)
                        {   // if results file not already exported for the impersonated user export now

                            // "result" is reserved parameter name for the ManagedCertificate object
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(p.Value);

                            resultsJsonTempPath = Path.GetTempFileName();
                            File.WriteAllText(resultsJsonTempPath, json);
                            resultsJsonExported = true;
                        }

                        arguments += $" -resultJsonFile \"{resultsJsonTempPath}\"";
                    }
                    else
                    {
                        arguments = arguments += $" -{p.Key}{(p.Value != null ? " " + p.Value : "")}";
                    }
                }
            }

            var scriptProcessInfo = new ProcessStartInfo()
            {
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = commandExe,
                Arguments = arguments,
                Verb = "RunAs",
                WorkingDirectory = Path.GetDirectoryName(scriptFile)
            };

            // launch process with user credentials set
            if (credentials != null && credentials.ContainsKey("username") && credentials.ContainsKey("password"))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {

                    var username = credentials["username"];
                    var pwd = credentials["password"];

                    credentials.TryGetValue("domain", out var domain);

                    if (domain == null && !username.Contains(".\\") && !username.Contains("@"))
                    {
                        domain = ".";
                    }

                    // Note: process running as local system cannot start a process as different user due to lack of security token context
                    scriptProcessInfo.UserName = username;
                    scriptProcessInfo.Domain = domain;

                    var sPwd = new SecureString();
                    foreach (var c in pwd)
                    {
                        sPwd.AppendChar(c);
                    }

                    sPwd.MakeReadOnly();

                    scriptProcessInfo.Password = sPwd;

                    _log.AppendLine($"Launching Process {commandExe} as User: {domain}\\{username}");
                }
                else
                {
                    _log.AppendLine($"Running PowerShell As New Process: Running as specific user credentials are not supported on this platform.");
                }
            }

            try
            {
                var process = new Process { StartInfo = scriptProcessInfo };

                var logMessages = new StringBuilder();

                // capture output streams and add to log
                process.OutputDataReceived += (obj, a) =>
                {
                    if (a.Data != null)
                    {
                        logMessages.AppendLine(a.Data);
                    }
                };

                process.ErrorDataReceived += (obj, a) =>
                {
                    if (!string.IsNullOrWhiteSpace(a.Data))
                    {
                        logMessages.AppendLine($"Error: {a.Data}");
                    }
                };

                try
                {
                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit((timeoutMinutes * 60) * 1000);
                }
                catch (Exception exp)
                {
                    _log.AppendLine("Error Running Script: " + exp.ToString());
                }

                // append output to main log
                _log.Append(logMessages.ToString());

                if (!process.HasExited)
                {
                    //process still running, kill task
                    process.CloseMainWindow();

                    _log.AppendLine("Warning: Script ran but took too long to exit and was closed.");
                    return new ActionResult { IsSuccess = false, Message = _log.ToString() };
                }
                else if (process.ExitCode != 0)
                {
                    _log.AppendLine("Warning: Script exited with the following ExitCode: " + process.ExitCode);
                    return new ActionResult { IsSuccess = false, Message = _log.ToString() };
                }

                return new ActionResult { IsSuccess = true, Message = _log.ToString() };

            }
            catch (Exception exp)
            {
                _log.AppendLine("Error: " + exp.ToString());
                return new ActionResult
                {
                    IsSuccess = false,
                    Message = _log.ToString()
                };
            }
            finally
            {

                // cleanup temp json
                if (resultsJsonTempPath != null)
                {
                    try
                    {
                        File.Delete(resultsJsonTempPath);
                    }
                    catch
                    {
                        _log.AppendLine("Running Powershell As New Process: Could not delete temp results file.");
                    }
                }
            }
        }

        private static bool ApplyFileACL(string filePath, string fullUsername)
        {
            var fileInfo = new FileInfo(filePath);
            var accessControl = fileInfo.GetAccessControl();

            accessControl.AddAccessRule(new FileSystemAccessRule(fullUsername, FileSystemRights.ReadAndExecute, AccessControlType.Allow));

            try
            {
                fileInfo.SetAccessControl(accessControl);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static ActionResult InvokePowershell(CertificateRequestResult result, string executionPolicy, string scriptFile, Dictionary<string, object> parameters, string scriptContent, PowerShell shell, bool autoConvertBoolean = true, string[] ignoredCommandExceptions = null, int timeoutMinutes = 5)
        {
            // ensure execution policy will allow the script to run, default to system default, default policy is set in service config object 

            // option to set execution policy as a parameter at task level
            if (parameters?.Any(p => p.Key.ToLower() == "executionpolicy") == true)
            {
                executionPolicy = parameters.FirstOrDefault(p => p.Key.ToLower() == "executionpolicy").Value?.ToString();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // on windows we may need to set execution policy depending on user preferences
                if (!string.IsNullOrEmpty(executionPolicy))
                {
                    shell.AddCommand("Set-ExecutionPolicy")
                            .AddParameter("ExecutionPolicy", executionPolicy)
                            .AddParameter("Scope", "Process")
                            .AddParameter("Force")
                            .Invoke();
                }
            }

            // add script command to invoke
            if (scriptFile != null)
            {
                shell.AddCommand(scriptFile);
            }
            else
            {
                shell.AddScript(scriptContent);
            }

            // pass the result to the script if present
            if (result != null)
            {
                shell.AddParameter("result", result);
            }

            // pass parameters to script if present
            if (parameters != null)
            {
                foreach (var a in parameters)
                {
                    var val = a.Value;
                    if (autoConvertBoolean)
                    {

                        if (val != null && val?.ToString().ToLower() == "true")
                        {
                            val = true;
                        }
                        else if (val != null && val?.ToString().ToLower() == "false")
                        {
                            val = false;
                        }
                    }

                    shell.AddParameter(a.Key, val);
                }
            }

            var errors = new List<string>();

            // accumulate output
            var output = new StringBuilder();

            // capture errors

            if (ignoredCommandExceptions == null)
            {
                ignoredCommandExceptions = new string[] { };
            }

            shell.Streams.Error.DataAdded += (sender, args) =>
                {
                    var error = shell.Streams.Error[args.Index];
                    var src = error.InvocationInfo.MyCommand?.ToString() ?? error.InvocationInfo.InvocationName;
                    var msg = $"{src}: {error}\n{error.InvocationInfo.PositionMessage}";
                    if (!ignoredCommandExceptions.Contains(error.InvocationInfo.MyCommand?.Name))
                    {
                        errors.Add(msg);
                    }
                };

            // capture write-* methods (except write-host)

            // TODO: one of these streams may be causing ssh hang when ssh spawned as part of script..

            shell.Streams.Warning.DataAdded += (sender, args) => output.AppendLine(shell.Streams.Warning[args.Index].Message);
            shell.Streams.Debug.DataAdded += (sender, args) => output.AppendLine(shell.Streams.Debug[args.Index].Message);
            shell.Streams.Verbose.DataAdded += (sender, args) => output.AppendLine(shell.Streams.Verbose[args.Index].Message);

            var outputData = new PSDataCollection<PSObject>();

            outputData.DataAdded += (sender, args) =>
                    {
                        // capture all main output
                        var data = outputData[args.Index]?.BaseObject;
                        if (data != null)
                        {
                            output.AppendLine(data.ToString());
                        }
                    };

            try
            {
                var async = shell.BeginInvoke<PSObject, PSObject>(null, outputData);

                var maxWait = 60 * timeoutMinutes; // N min timeout
                var currentWait = 0;
                var pollSeconds = 5;

                var timeoutOccurred = false;

                while (!timeoutOccurred && !async.AsyncWaitHandle.WaitOne(pollSeconds * 1000, false))
                {
                    // poll while async task is still running
                    currentWait += pollSeconds;

                    if (currentWait <= maxWait)
                    {
                        output.AppendLine($"Waiting for powershell to complete..{currentWait}s");
                    }
                    else
                    {
                        output.AppendLine($"Timeout waiting for powershell to complete ({currentWait}s)");
                        errors.Add($"Script did not complete in the required time. ({maxWait}s)");
                        timeoutOccurred = true;
                    }
                }

                try
                {
                    if (async.IsCompleted)
                    {
                        shell.EndInvoke(async);
                        output.Append($"Powershell Task Completed.");
                    }
                }
                catch (System.Management.Automation.RuntimeException ex)
                {
                    errors.Add($"{ex.ErrorRecord} {ex.ErrorRecord.ScriptStackTrace}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Script invoke failed: {ex}");
                }

                if (errors.Any())
                {
                    foreach (var e in errors)
                    {
                        output.AppendLine("Error: " + e);
                    }
                }

                return new ActionResult(output.ToString().TrimEnd('\n'), !errors.Any());
            }
            catch (ParseException ex)
            {
                // this should only happen in case of script syntax errors, otherwise
                // errors would be output via the invoke's error stream
                output.AppendLine($"{ex.Message}");

                return new ActionResult(output.ToString().TrimEnd('\n'), false);
            }
        }

        public static UserCredentials GetWindowsCredentials(Dictionary<string, string> credentials)
        {
            UserCredentials windowsCredentials;

            var username = credentials["username"];
            var pwd = credentials["password"];

            credentials.TryGetValue("domain", out var domain);

            if (domain == null && !username.Contains(".\\") && !username.Contains("@"))
            {
                domain = ".";
            }

            if (domain != null)
            {
                windowsCredentials = new UserCredentials(domain, username, pwd);
            }
            else
            {
                windowsCredentials = new UserCredentials(username, pwd);
            }

            return windowsCredentials;
        }

        public static string GetWindowsCredentialsUsername(Dictionary<string, string> credentials, bool includeAutoLocalDomain = false)
        {
            var username = credentials["username"];

            credentials.TryGetValue("domain", out var domain);

            if (includeAutoLocalDomain)
            {
                if (domain == null && !username.Contains(".\\") && !username.Contains("@"))
                {
                    domain = ".";
                }
            }

            if (domain != null)
            {
                return $"{domain}\\{username}";
            }
            else
            {
                return username;
            }
        }
    }
}
