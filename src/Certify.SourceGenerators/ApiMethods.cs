﻿using System;
using System.Collections.Generic;
using System.Linq;
using SourceGenerator;

namespace Certify.SourceGenerators
{
    public class ApiMethods
    {
        public static string HttpGet = "HttpGet";
        public static string HttpPost = "HttpPost";
        public static string HttpDelete = "HttpDelete";

        public static string GetFormattedTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments()
                                    .Select(x => x.FullName)
                                    .Aggregate((x1, x2) => $"{x1}, {x2}");
                return $"{type.FullName.Substring(0, type.FullName.IndexOf("`"))}"
                     + $"<{genericArguments}>";
            }

            return type.FullName;
        }
        public static List<GeneratedAPI> GetApiDefinitions()
        {
            // declaring an API definition here is then used by the source generators to:
            // - create the public API endpoint
            // - map the call from the public API to the background service API in the service API Client (interface and implementation)
            // - to then generate the public API clients, run nswag when the public API is running.

            return new List<GeneratedAPI> {

                    new GeneratedAPI {

                        OperationName = "GetSecurityPrincipleAssignedRoles",
                        OperationMethod = HttpGet,
                        Comment = "Get list of Assigned Roles for a given security principle",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "securityprinciple/{id}/assignedroles",
                        ServiceAPIRoute = "access/securityprinciple/{id}/assignedroles",
                        ReturnType = "ICollection<AssignedRole>",
                        Params =new Dictionary<string, string>{{"id","string"}}
                    },
                    new GeneratedAPI {

                        OperationName = "GetSecurityPrincipleRoleStatus",
                        OperationMethod = HttpGet,
                        Comment = "Get list of Assigned Roles etc for a given security principle",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "securityprinciple/{id}/rolestatus",
                        ServiceAPIRoute = "access/securityprinciple/{id}/rolestatus",
                        ReturnType = "RoleStatus",
                        Params =new Dictionary<string, string>{{"id","string"}}
                    },
                    new GeneratedAPI {

                        OperationName = "GetAccessRoles",
                        OperationMethod = HttpGet,
                        Comment = "Get list of available security Roles",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "roles",
                        ServiceAPIRoute = "access/roles",
                        ReturnType = "ICollection<Role>"
                    },
                    new GeneratedAPI {

                        OperationName = "GetSecurityPrinciples",
                        OperationMethod = HttpGet,
                        Comment = "Get list of available security principles",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "securityprinciples",
                        ServiceAPIRoute = "access/securityprinciples",
                        ReturnType = "ICollection<SecurityPrinciple>"
                    },
                    new GeneratedAPI {
                        OperationName = "ValidateSecurityPrinciplePassword",
                        OperationMethod = HttpPost,
                        Comment = "Check password valid for security principle",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "validate",
                        ServiceAPIRoute = "access/validate",
                        ReturnType = "Certify.Models.Hub.SecurityPrincipleCheckResponse",
                        Params = new Dictionary<string, string>{{"passwordCheck", "Certify.Models.Hub.SecurityPrinciplePasswordCheck" } }
                    },
                    new GeneratedAPI {

                        OperationName = "UpdateSecurityPrinciplePassword",
                        OperationMethod = HttpPost,
                        Comment = "Update password for security principle",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "updatepassword",
                        ServiceAPIRoute = "access/updatepassword",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{{"passwordUpdate", "Certify.Models.Hub.SecurityPrinciplePasswordUpdate" } }
                    },
                    new GeneratedAPI {

                        OperationName = "AddSecurityPrinciple",
                        OperationMethod = HttpPost,
                        Comment = "Add new security principle",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "securityprinciple",
                        ServiceAPIRoute = "access/securityprinciple",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{{"principle", "Certify.Models.Hub.SecurityPrinciple" } }
                    },
                    new GeneratedAPI {

                        OperationName = "UpdateSecurityPrinciple",
                        OperationMethod = HttpPost,
                        Comment = "Update existing security principle",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "securityprinciple/update",
                        ServiceAPIRoute = "access/securityprinciple/update",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{
                            { "principle", "Certify.Models.Hub.SecurityPrinciple" }
                        }
                    },
                      new GeneratedAPI {

                        OperationName = "UpdateSecurityPrincipleAssignedRoles",
                        OperationMethod = HttpPost,
                        Comment = "Update assigned roles for a security principle",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "securityprinciple/roles/update",
                        ServiceAPIRoute = "access/securityprinciple/roles/update",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{
                            { "update", "Certify.Models.Hub.SecurityPrincipleAssignedRoleUpdate" }
                        }
                    },
                    new GeneratedAPI {

                        OperationName = "RemoveSecurityPrinciple",
                        OperationMethod = HttpDelete,
                        Comment = "Remove security principle",
                        PublicAPIController = "Access",
                        PublicAPIRoute = "securityprinciple",
                        ServiceAPIRoute = "access/securityprinciple/{id}",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{{"id","string"}}
                    },
                    new GeneratedAPI {

                        OperationName = "GetManagedChallenges",
                        OperationMethod = HttpGet,
                        Comment = "Get list of available managed challenges (DNS challenge delegation etc)",
                        PublicAPIController = "ManagedChallenge",
                        PublicAPIRoute = "list",
                        ServiceAPIRoute = "managedchallenge",
                        ReturnType = "ICollection<ManagedChallenge>"
                    },

                    new GeneratedAPI {

                        OperationName = "UpdateManagedChallenge",
                        OperationMethod = HttpPost,
                        Comment = "Add/update a managed challenge (DNS challenge delegation etc)",
                        PublicAPIController = "ManagedChallenge",
                        PublicAPIRoute = "update",
                        ServiceAPIRoute = "managedchallenge",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{
                            { "update", "Certify.Models.Hub.ManagedChallenge" }
                        }
                    },

                    new GeneratedAPI {

                        OperationName = "RemoveManagedChallenge",
                        OperationMethod = HttpDelete,
                        Comment = "Delete a managed challenge (DNS challenge delegation etc)",
                        PublicAPIController = "ManagedChallenge",
                        PublicAPIRoute = "remove",
                        ServiceAPIRoute = "managedchallenge",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{
                            { "id", "string" }
                        }
                    },
                    new GeneratedAPI {

                        OperationName = "PerformManagedChallenge",
                        OperationMethod = HttpPost,
                        Comment = "Perform a managed challenge (DNS challenge delegation etc)",
                        PublicAPIController=null, // skip public controller implementation
                        ServiceAPIRoute = "managedchallenge/request",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{
                            { "request", "Certify.Models.Hub.ManagedChallengeRequest" }
                        }
                    },
                    new GeneratedAPI {

                        OperationName = "CleanupManagedChallenge",
                        OperationMethod = HttpPost,
                        Comment = "Perform cleanup for a previously managed challenge (DNS challenge delegation etc)",
                        PublicAPIController=null, // skip public controller implementation
                        ServiceAPIRoute = "managedchallenge/cleanup",
                        ReturnType = "Models.Config.ActionResult",
                        Params = new Dictionary<string, string>{
                            { "request", "Certify.Models.Hub.ManagedChallengeRequest" }
                        }
                    },
                    /* per instance API, via management hub */
                    new GeneratedAPI {
                        OperationName = "GetAcmeAccounts",
                        OperationMethod = HttpGet,
                        Comment = "Get All Acme Accounts",
                        UseManagementAPI = true,
                        PublicAPIController = "CertificateAuthority",
                        PublicAPIRoute = "{instanceId}/accounts/",
                        ReturnType = "ICollection<Models.AccountDetails>",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" } }
                    },
                    new GeneratedAPI {
                        OperationName = "AddAcmeAccount",
                        OperationMethod = HttpPost,
                        Comment = "Add New Acme Account",
                        UseManagementAPI = true,
                        PublicAPIController = "CertificateAuthority",
                        PublicAPIRoute = "{instanceId}/account/",
                        ReturnType = "Models.Config.ActionResult",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" },{ "registration", "Certify.Models.ContactRegistration" } }
                    },
                    new GeneratedAPI {
                        OperationName = "GetCertificateAuthorities",
                        OperationMethod = HttpGet,
                        Comment = "Get list of defined Certificate Authorities",
                        UseManagementAPI = true,
                        PublicAPIController = "CertificateAuthority",
                        PublicAPIRoute = "{instanceId}/authority",
                        ReturnType = "ICollection<Models.CertificateAuthority>",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" } }
                    },
                    new GeneratedAPI {
                        OperationName = "UpdateCertificateAuthority",
                        OperationMethod = HttpPost,
                        Comment = "Add/Update Certificate Authority",
                        UseManagementAPI = true,
                        PublicAPIController = "CertificateAuthority",
                        PublicAPIRoute = "{instanceId}/authority",
                        ReturnType = "Models.Config.ActionResult",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" }, { "ca", "Certify.Models.CertificateAuthority" } }
                    },
                    new GeneratedAPI {
                        OperationName = "RemoveCertificateAuthority",
                        OperationMethod = HttpDelete,
                        Comment = "Remove Certificate Authority",
                        UseManagementAPI = true,
                        PublicAPIController = "CertificateAuthority",
                        PublicAPIRoute = "{instanceId}/authority/{id}",
                        ReturnType = "Models.Config.ActionResult",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" },{ "id", "string" } }
                    },
                    new GeneratedAPI {
                        OperationName = "RemoveAcmeAccount",
                        OperationMethod = HttpDelete,
                        Comment = "Remove ACME Account",
                        UseManagementAPI = true,
                        PublicAPIController = "CertificateAuthority",
                        PublicAPIRoute = "{instanceId}/accounts/{storageKey}/{deactivate}",
                        ReturnType = "Models.Config.ActionResult",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" }, { "storageKey", "string" }, { "deactivate", "bool" } }
                    },
                     new GeneratedAPI {
                        OperationName = "GetStoredCredentials",
                        OperationMethod = HttpGet,
                        Comment = "Get List of Stored Credentials",
                        UseManagementAPI = true,
                        PublicAPIController = "StoredCredential",
                        PublicAPIRoute = "{instanceId}",
                        ReturnType = "ICollection<Models.Config.StoredCredential>",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" } }
                    },
                    new GeneratedAPI {
                        OperationName = "UpdateStoredCredential",
                        OperationMethod = HttpPost,
                        Comment = "Add/Update Stored Credential",
                        PublicAPIController = "StoredCredential",
                        PublicAPIRoute = "{instanceId}",
                        ReturnType = "Models.Config.ActionResult",
                        UseManagementAPI = true,
                        Params =new Dictionary<string, string>{ { "instanceId", "string" }, { "item", "Models.Config.StoredCredential" } }
                    },
                    new GeneratedAPI {
                        OperationName = "RemoveStoredCredential",
                        OperationMethod = HttpDelete,
                        Comment = "Remove Stored Credential",
                        UseManagementAPI = true,
                        PublicAPIController = "StoredCredential",
                        PublicAPIRoute = "{instanceId}/{storageKey}",
                        ReturnType = "Models.Config.ActionResult",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" },{ "storageKey", "string" } }
                    },
                    new GeneratedAPI {
                        OperationName = "GetDeploymentProviders",
                        OperationMethod = HttpGet,
                        Comment = "Get Deployment Task Providers",
                        UseManagementAPI = true,
                        PublicAPIController = "DeploymentTask",
                        PublicAPIRoute = "{instanceId}",
                        ReturnType = "ICollection<Certify.Models.Config.DeploymentProviderDefinition>",
                        Params =new Dictionary<string, string>{
                            { "instanceId", "string" }
                        }
                    },
                    new GeneratedAPI {
                        OperationName = "GetChallengeProviders",
                        OperationMethod = HttpGet,
                        Comment = "Get Dns Challenge Providers",
                        UseManagementAPI = true,
                        PublicAPIController = "ChallengeProvider",
                        PublicAPIRoute = "{instanceId}",
                        ReturnType = "ICollection<Certify.Models.Config.ChallengeProviderDefinition>",
                        Params =new Dictionary<string, string>{
                            { "instanceId", "string" }
                        }
                    },
                      new GeneratedAPI {
                        OperationName = "GetDnsZones",
                        OperationMethod = HttpGet,
                        Comment = "Get List of Zones with the current DNS provider and credential",
                        UseManagementAPI = true,
                        PublicAPIController = "ChallengeProvider",
                        PublicAPIRoute = "{instanceId}/dnszones/{providerTypeId}/{credentialId}",
                        ReturnType = "ICollection<Certify.Models.Providers.DnsZone>",
                        Params =new Dictionary<string, string>{
                            { "instanceId", "string" } ,
                            { "providerTypeId", "string" },
                            { "credentialId", "string" }
                        }
                    },
                    new GeneratedAPI {
                        OperationName = "ExecuteDeploymentTask",
                        OperationMethod = HttpGet,
                        Comment = "Execute Deployment Task",
                        UseManagementAPI = true,
                        PublicAPIController = "DeploymentTask",
                        PublicAPIRoute = "{instanceId}/execute/{managedCertificateId}/{taskId}",
                        ReturnType = "ICollection<ActionStep>",
                        Params =new Dictionary<string, string>{
                            { "instanceId", "string" },
                            { "managedCertificateId", "string" },
                            { "taskId", "string" }
                        }
                    },
                    new GeneratedAPI {
                        OperationName = "RemoveManagedCertificate",
                        OperationMethod = HttpDelete,
                        Comment = "Remove Managed Certificate",
                        UseManagementAPI = true,
                        PublicAPIController = "Certificate",
                        PublicAPIRoute = "{instanceId}/settings/{managedCertId}",
                        ReturnType = "bool",
                        Params =new Dictionary<string, string>{ { "instanceId", "string" },{ "managedCertId", "string" } }
                    },
                    // TODO
                    new GeneratedAPI {
                        OperationName = "PerformExport",
                        OperationMethod = HttpPost,
                        Comment = "Perform an export of all settings",
                        PublicAPIController = "System",
                        PublicAPIRoute = "system/migration/export",
                        ServiceAPIRoute = "system/migration/export",
                        ReturnType = "Models.Config.Migration.ImportExportPackage",
                        Params =new Dictionary<string, string>{{ "exportRequest", "Certify.Models.Config.Migration.ExportRequest" } }
                    },
                     new GeneratedAPI {
                        OperationName = "PerformImport",
                        OperationMethod = HttpPost,
                        Comment = "Perform an import of all settings",
                        PublicAPIController = "System",
                        PublicAPIRoute = "system/migration/import",
                        ServiceAPIRoute = "system/migration/import",
                        ReturnType = "ICollection<ActionStep>",
                        Params =new Dictionary<string, string>{{ "importRequest", "Certify.Models.Config.Migration.ImportRequest" } }
                    },
                };
        }
    }
}
