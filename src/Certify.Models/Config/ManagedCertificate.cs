﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Certify.Config;
using Newtonsoft.Json;

namespace Certify.Models
{
    public enum ManagedCertificateType
    {
        SSL_ACME = 1,
        SSL_Manual = 2,
        SSL_ExternallyManaged = 3
    }

    public enum RequiredActionType
    {
        NewCertificate,
        ReplaceCertificate,
        KeepCertificate,
        Ignore
    }

    public enum ManagedCertificateHealth
    {
        Unknown,
        OK,
        AwaitingUser,
        Warning,
        Error
    }

    public class RenewalDueInfo
    {
        public DateTime? DateNextRenewalAttempt { get; set; }
        public TimeSpan? CertLifetime { get; set; }
        public bool IsRenewalDue { get; set; }
        public bool IsRenewalOnHold { get; set; }
        public string Reason { get; set; }

        /// <summary>
        /// If set, the current number of hrs we will wait before next attempt
        /// </summary>
        public float HoldHrs { get; set; }

        public RenewalDueInfo(string reason, bool isRenewalDue, DateTime? renewalAttemptDate, TimeSpan? certLifetime, bool isRenewalOnHold = false, float holdHrs = 0)
        {
            Reason = reason;
            IsRenewalDue = isRenewalDue;
            DateNextRenewalAttempt = renewalAttemptDate;
            CertLifetime = certLifetime;
            IsRenewalOnHold = isRenewalOnHold;
            HoldHrs = holdHrs;
        }
    }

    public class ManagedCertificate : BindableBase
    {
        public ManagedCertificate()
        {
            Name = "New Managed Certificate";
            IncludeInAutoRenew = true;

            DomainOptions = new ObservableCollection<DomainOption>();
            RequestConfig = new CertRequestConfig();

            IncludeInAutoRenew = true;

#if DEBUG
            UseStagingMode = true;
#else
            UseStagingMode = false;
#endif
        }

        /// <summary>
        /// If set, managed item is from an external source
        /// </summary>
        public string? SourceId { get; set; }
        public string? SourceName { get; set; }

        /// <summary>
        /// Default CA to use for this request
        /// </summary>
        public string? CertificateAuthorityId { get; set; }

        /// <summary>
        /// If true, the staging (test) API and account key will be used for orders
        /// </summary>
        public bool UseStagingMode { get; set; }

        /// <summary>
        /// If true, the auto renewal process will include this item in attempted renewal operations
        /// if applicable
        /// </summary>
        public bool IncludeInAutoRenew { get; set; }

        /// <summary>
        /// List of configured domains this managed site will include (primary subject or SAN)
        /// </summary>
        public ObservableCollection<DomainOption> DomainOptions { get; set; }

        /// <summary>
        /// Configuration options for this request
        /// </summary>
        public CertRequestConfig RequestConfig { get; set; }

        /// <summary>
        /// Optional list of tasks (scripts, webhooks etc) to perform after request/renewal or on demand
        /// </summary>
        public ObservableCollection<DeploymentTaskConfig>? PreRequestTasks { get; set; }

        /// <summary>
        /// Optional list of deployment tasks to perform after request/renewal or on demand
        /// </summary>
        public ObservableCollection<DeploymentTaskConfig>? PostRequestTasks { get; set; }

        /// <summary>
        /// Unique ID for this managed item
        /// </summary>
        public string? Id { get; set; }
        public long Version { get; set; }

        /// <summary>
        /// Deprecated, use Server Site Id
        /// </summary>

        public string? GroupId { get; set; }

        /// <summary>
        /// Id of specific matching site on server (replaces GroupId)
        /// </summary>
        public string? ServerSiteId { get => GroupId; set => GroupId = value; }

        /// <summary>
        /// If set, this is an identifier for the host to group multiple sets of managed sites across servers
        /// </summary>
        public string? InstanceId { get; set; }

        /// <summary>
        /// Display name for this item, for easier reference
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Optional user notes regarding this item
        /// </summary>
        public string? Comments { get; set; }

        /// <summary>
        /// Specific type of item we are managing, affects the renewal/request operations required
        /// </summary>
        public ManagedCertificateType ItemType { get; set; }

        public DateTime? DateStart { get; set; }
        public DateTime? DateExpiry { get; set; }
        public DateTime? DateRenewed { get; set; }

        /// <summary>
        /// Date we last check the OCSP status for this cert
        /// </summary>
        public DateTime? DateLastOcspCheck { get; set; }

        /// <summary>
        /// Date we last checked the CA renewal info (ARI), if available
        /// </summary>
        public DateTime? DateLastRenewalInfoCheck { get; set; }

        /// <summary>
        /// If set, date we should next attempt renewal. This is normally not set but may be for items affected by ARI renewal windows etc
        /// </summary>
        public DateTime? DateNextScheduledRenewalAttempt { get; set; }

        /// <summary>
        /// Date we last attempted renewal
        /// </summary>
        public DateTime? DateLastRenewalAttempt { get; set; }

        /// <summary>
        /// Status of most recent renewal attempt
        /// </summary>
        public RequestState? LastRenewalStatus { get; set; }

        /// <summary>
        /// ID of last attempted CA, used to decide if we should attempt failover to another CA
        /// </summary>
        public string? LastAttemptedCA { get; set; }

        /// <summary>
        /// Count of renewal failures since last success
        /// </summary>
        public int RenewalFailureCount { get; set; }

        /// <summary>
        /// Message from last failed renewal attempt
        /// </summary>
        public string? RenewalFailureMessage { get; set; }

        /// <summary>
        /// The Base64 encoded Certificate Id (OCSP, ACME ARI etc) for the current certificate
        /// </summary>
        public string? CertificateId { get; set; }
        public string? CertificatePath { get; set; }
        public string? CertificateFriendlyName { get; set; }
        public string? CertificateThumbprintHash { get; set; }
        public string? CertificatePreviousThumbprintHash { get; set; }
        public bool CertificateRevoked { get; set; }

        /// <summary>
        /// Optional stored credential ID for preferred PFX password (pwd is blank otherwise)
        /// </summary>
        public string? CertificatePasswordCredentialId { get; set; }

        public string? CurrentOrderUri { get; set; }

        /// <summary>
        /// If true, pre/post request tasks will run for renewal but the certificate order won't be performed (used for testing).
        /// </summary>
        public bool? SkipCertificateRequest { get; set; } = false;

        /// <summary>
        /// If specified this is the days or percentage target for renewal, depending on the renewal mode
        /// </summary>
        public float? CustomRenewalTarget { get; set; }

        /// <summary>
        /// If specified, custom renewal interval mode (DaysBeforeExpiry, DaysAfterRenewal, PercentageLifetime)
        /// </summary>
        public string? CustomRenewalIntervalMode { get; set; }

        public override string ToString() => $"[{Id ?? "null"}]: \"{Name}\"";

        [JsonIgnore]
        public bool Deleted { get; set; } // do not serialize to settings

        /// <summary>
        /// Get the percentage of the certificate lifetime elapsed, if known
        /// </summary>
        /// <param name="testDateTime"></param>
        /// <returns></returns>
        public int? GetPercentageLifetimeElapsed(DateTime testDateTime)
        {
            if (DateStart == null || DateExpiry == null)
            {
                return null;
            }

            var certLifetime = (DateTime)DateExpiry - (DateTime)DateStart;

            if (certLifetime.TotalMinutes <= 0)
            {
                return 100;
            }

            var certElapsed = testDateTime - (DateTime)DateStart;
            var elapsedMinutes = certLifetime.TotalMinutes - (certLifetime.TotalMinutes - certElapsed.TotalMinutes);

            if (elapsedMinutes > 0)
            {
                if (elapsedMinutes >= certLifetime.TotalMinutes)
                {
                    return 100;
                }
                else
                {
                    return (int)(elapsedMinutes / certLifetime.TotalMinutes * 100);
                }
            }
            else
            {
                return 0;
            }
        }

        [JsonIgnore]
        public ManagedCertificateHealth Health
        {
            get
            {
                var percentageElapsed = GetPercentageLifetimeElapsed(DateTime.Now);

                if (LastRenewalStatus == RequestState.Error)
                {
                    if (RenewalFailureCount > 3 || percentageElapsed > 90)
                    {
                        return ManagedCertificateHealth.Error;
                    }
                    else
                    {
                        return ManagedCertificateHealth.Warning;
                    }
                }
                else
                {
                    if (LastRenewalStatus != null)
                    {
                        if (LastRenewalStatus.Value == RequestState.Paused)
                        {
                            return ManagedCertificateHealth.AwaitingUser;
                        }
                        else
                        {
                            if (CertificateRevoked)
                            {
                                return ManagedCertificateHealth.Error;
                            }
                            else
                            {
                                // if cert is otherwise OK but is expiring soon, report health as warning or error (expired)
                                if (percentageElapsed > 95)
                                {
                                    return ManagedCertificateHealth.Error;
                                }
                                else if (percentageElapsed > 75)
                                {
                                    return ManagedCertificateHealth.Warning;
                                }
                                else
                                {
                                    if (LastRenewalStatus == RequestState.Warning)
                                    {
                                        return ManagedCertificateHealth.Warning;
                                    }
                                    else
                                    {
                                        return ManagedCertificateHealth.OK;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return ManagedCertificateHealth.Unknown;
                    }
                }
            }
        }

        /// <summary>
        /// get distrinct list of certificate identifiers for this managed cert
        /// </summary>
        /// <returns></returns>
        public List<CertIdentifierItem> GetCertificateIdentifiers()
        {
            return RequestConfig.GetCertificateIdentifiers();
        }

        /// <summary>
        /// Get distinct list of certificate domains/hostnames for this managed cert
        /// </summary>
        /// <returns></returns>
        public List<string> GetCertificateDomains()
        {
            if (RequestConfig == null)
            {
                return new List<string>();
            }
            else
            {
                return RequestConfig.GetCertificateDomains();
            }
        }

        /// <summary>
        /// For the given challenge config and list of identifiers, return subset of identifiers which will
        /// be matched against the config (considering all other configs)
        /// </summary>
        /// <param name="config">  </param>
        /// <param name="identifiers">  </param>
        /// <returns>  </returns>
        public List<CertIdentifierItem> GetChallengeConfigDomainMatches(CertRequestChallengeConfig config, IEnumerable<CertIdentifierItem> domains)
        {
            var matches = new List<CertIdentifierItem>();
            foreach (var d in domains)
            {
                var matchedConfig = GetChallengeConfig(d);
                if (matchedConfig == config)
                {
                    matches.Add(d);
                }
            }

            return matches;
        }

        /// <summary>
        /// For the given identifier, get the matching challenge config (DNS provider variant etc)
        /// </summary>
        /// <param name="managedCertificate">  </param>
        /// <param name="identifier">  </param>
        /// <returns>  </returns>
        public CertRequestChallengeConfig GetChallengeConfig(CertIdentifierItem identifier)
        {

            if (RequestConfig.Challenges == null || RequestConfig.Challenges.Count == 0)
            {
                // there are no challenge configs defined return a default based on the parent
                return new CertRequestChallengeConfig
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    ChallengeType = RequestConfig.ChallengeType
#pragma warning restore CS0618 // Type or member is obsolete
                };
            }
            else
            {
                //identify matching challenge config based on identifier etc
                if (RequestConfig.Challenges.Count == 1)
                {
                    return RequestConfig.Challenges[0];
                }
                else
                {
                    // start by matching first config with no specific identifier
                    var matchedConfig = RequestConfig.Challenges.FirstOrDefault(c => string.IsNullOrEmpty(c.DomainMatch));

                    if (identifier != null && !string.IsNullOrEmpty(identifier?.Value))
                    {
                        // expand configs into per identifier list
                        var configsPerDomain = new Dictionary<string, CertRequestChallengeConfig>();
                        foreach (var c in RequestConfig.Challenges.Where(config => !string.IsNullOrEmpty(config.DomainMatch)))
                        {
                            if (c != null)
                            {
                                if (c.DomainMatch != null && !string.IsNullOrEmpty(c.DomainMatch))
                                {
                                    c.DomainMatch = c.DomainMatch.Replace(",", ";"); // if user has entered comma seperators instead of semicolons, convert now.

                                    if (!c.DomainMatch.Contains(";"))
                                    {
                                        var domainMatchKey = c.DomainMatch.Trim();

                                        // if identifier key is test.com for example we only support one matching config
                                        if (!configsPerDomain.ContainsKey(domainMatchKey))
                                        {
                                            configsPerDomain.Add(domainMatchKey, c);
                                        }
                                    }
                                    else
                                    {
                                        var domains = c.DomainMatch.Split(';');
                                        foreach (var d in domains)
                                        {
                                            if (!string.IsNullOrWhiteSpace(d))
                                            {
                                                var domainMatchKey = d.Trim().ToLowerInvariant();
                                                if (!configsPerDomain.ContainsKey(domainMatchKey))
                                                {
                                                    configsPerDomain.Add(domainMatchKey, c);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // if exact match exists, use that
                        var identifierKey = identifier?.Value.ToLowerInvariant() ?? "";
                        if (configsPerDomain.TryGetValue(identifierKey, out var value))
                        {
                            return value;
                        }

                        // if explicit wildcard match exists, use that
                        if (configsPerDomain.TryGetValue("*." + identifierKey, out var wildValue))
                        {
                            return wildValue;
                        }

                        //if a more specific config matches the identifier, use that, in order of longest identifier name match first
                        var allMatchingConfigKeys = configsPerDomain.Keys.OrderByDescending(l => l.Length);

                        foreach (var wildcard in allMatchingConfigKeys.Where(k => k.StartsWith("*.", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            if (ManagedCertificate.IsDomainOrWildcardMatch(new List<string> { wildcard }, identifier?.Value))
                            {
                                return configsPerDomain[wildcard];
                            }
                        }

                        foreach (var configDomain in allMatchingConfigKeys)
                        {
                            if (configDomain.EndsWith(identifier?.Value.ToLowerInvariant(), StringComparison.CurrentCultureIgnoreCase))
                            {
                                // use longest matching identifier (so subdomain.test.com takes priority
                                // over test.com, )
                                return configsPerDomain[configDomain];
                            }
                        }
                    }

                    // no other matches, just use first
                    if (matchedConfig != null)
                    {
                        return matchedConfig;
                    }
                    else
                    {
                        // no match, return default
                        return new CertRequestChallengeConfig
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            ChallengeType = RequestConfig.ChallengeType
#pragma warning restore CS0618 // Type or member is obsolete
                        };
                    }
                }
            }
        }

        public ManagedCertificate CopyAsTemplate(bool preserveAttributes = false)
        {

            // clone current object
            var managedCert = JsonConvert.DeserializeObject<ManagedCertificate>(JsonConvert.SerializeObject(this));

            if (managedCert == null)
            {
                return new ManagedCertificate();
            }

            // reset fields we don't want to re-use from the original
            managedCert.Id = Guid.NewGuid().ToString();

            managedCert.DateLastRenewalAttempt = null;
            managedCert.DateStart = null;
            managedCert.DateRenewed = null;
            managedCert.DateExpiry = null;
            managedCert.CertificateThumbprintHash = null;
            managedCert.CertificatePreviousThumbprintHash = null;
            managedCert.CurrentOrderUri = null;
            managedCert.LastAttemptedCA = null;
            managedCert.SourceId = null;
            managedCert.SourceName = null;
            managedCert.RenewalFailureCount = 0;
            managedCert.RenewalFailureMessage = null;

            managedCert.LastRenewalStatus = null;
            managedCert.CurrentOrderUri = null;
            managedCert.CertificatePath = null;
            managedCert.CertificateId = null;
            managedCert.CertificateFriendlyName = null;
            managedCert.ItemType = ManagedCertificateType.SSL_ACME;

            if (!preserveAttributes)
            {
                managedCert.RequestConfig.SubjectAlternativeNames = Array.Empty<string>();
                managedCert.RequestConfig.SubjectIPAddresses = Array.Empty<string>();
                managedCert.RequestConfig.PrimaryDomain = string.Empty;
                managedCert.DomainOptions = new System.Collections.ObjectModel.ObservableCollection<DomainOption>();
                managedCert.Name = string.Empty;
            }
            else
            {
                managedCert.Name = $"{managedCert.Name.WithDefault("")} (Copy)";
            }

            if (managedCert.PreRequestTasks != null)
            {
                foreach (var t in managedCert.PreRequestTasks)
                {
                    t.Id = Guid.NewGuid().ToString();
                }
            }

            if (managedCert.PostRequestTasks != null)
            {
                foreach (var t in managedCert.PostRequestTasks)
                {
                    t.Id = Guid.NewGuid().ToString();
                }
            }

            return managedCert;
        }

        /// <summary>
        /// </summary>
        /// <param name="dnsNames">  </param>
        /// <param name="hostname">  </param>
        /// <param name="matchWildcardsToRootDomain">
        /// if true, *.test.com would match test.com (as well as www.test.com)
        /// </param>
        /// <returns>  </returns>
        public static bool IsDomainOrWildcardMatch(List<string> dnsNames, string? hostname, bool matchWildcardsToRootDomain = false)
        {
            var isMatch = false;

            if (!string.IsNullOrEmpty(hostname))
            {
                hostname = hostname?.ToLowerInvariant();

                // list of dns anmes has an exact match
                if (dnsNames.Contains(hostname))
                {
                    isMatch = true;
                }
                else
                {
                    //if any of our dnsHosts are a wildcard, check for a match
                    var wildcards = dnsNames.Where(d => d.StartsWith("*.", StringComparison.CurrentCultureIgnoreCase));
                    foreach (var w in wildcards)
                    {
                        if (string.Equals(w, hostname, StringComparison.OrdinalIgnoreCase))
                        {
                            isMatch = true;
                        }
                        else
                        {
                            var domain = w.Replace("*.", "");

                            // if match wildcards to root is enabled and is a root identifier match
                            if (string.Equals(domain, hostname, StringComparison.OrdinalIgnoreCase) && matchWildcardsToRootDomain)
                            {
                                isMatch = true;
                            }
                            else
                            {
                                //if hostname ends with our identifier and is only 1 label longer then it's a match
                                if (hostname.EndsWith("." + domain, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (hostname.Count(c => c == '.') == domain.Count(c => c == '.') + 1)
                                    {
                                        isMatch = true;
                                    }
                                }
                            }
                        }

                        if (isMatch)
                        {
                            return isMatch;
                        }
                    }
                }
            }

            return isMatch;
        }

        /// <summary>
        /// Given a CertificateRequestResult or ManagedCertificate, return the managed certiicate
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static ManagedCertificate? GetManagedCertificate(object subject)
        {
            if (subject == null)
            {
                return null;
            }
            else
            {

                if (subject is CertificateRequestResult)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    return (subject as CertificateRequestResult).ManagedItem;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
                else if (subject is ManagedCertificate)
                {
                    return (subject as ManagedCertificate);
                }
                else
                {

                    return default;
                }
            }
        }

        public static RenewalDueInfo? CalculateNextRenewalAttempt(ManagedCertificate s, float renewalInterval, string renewalIntervalMode, bool checkFailureStatus = false, DateTime? testDateTime = null)
        {

            if (s == null)
            {
                return null;
            }

            var nextRenewalAttemptDate = s.DateExpiry ?? DateTime.Now;

            var checkDate = DateTime.Now;

            if (testDateTime != null)
            {
                checkDate = testDateTime.Value;
            }

            var isRenewalRequired = false;
            var renewalStatusReason = " Item not due for renewal";
            TimeSpan? certLifetime = null;

            // use default renewal interval and mode, or prefer custom if specified
            var selectedRenewalIntervalMode = s.CustomRenewalIntervalMode ?? renewalIntervalMode;
            var selectedRenewalInterval = s.CustomRenewalTarget ?? (float)renewalInterval;

            // if cert has previously been renewed, calculate next renewal, otherwise renewal will be immediately due unless renewal has been failing
            if (s.DateRenewed.HasValue)
            {
                var timeSinceLastRenewal = checkDate - s.DateRenewed.Value;

                var expiryDate = s.DateExpiry ?? checkDate;
                var timeToExpiry = expiryDate - checkDate;
                certLifetime = s.DateExpiry - s.DateStart;

                if (s.DateNextScheduledRenewalAttempt != null && s.DateNextScheduledRenewalAttempt <= checkDate)
                {
                    return new RenewalDueInfo("Certificate scheduled renewal is now due.", true, checkDate, certLifetime: certLifetime);
                }

                // strategy if cert lifetime is less than the standard renewal interval allows or the renewal mode is based on percentage lifetime
                if (certLifetime.HasValue && (certLifetime.Value.TotalDays < renewalInterval || selectedRenewalIntervalMode == RenewalIntervalModes.PercentageLifetime))
                {
                    // cert has a shorter lifetime than the renewal interval. Switch to a percentage based renewal 
                    float targetRenewalPercentage = 75;

                    if (selectedRenewalIntervalMode == RenewalIntervalModes.PercentageLifetime && selectedRenewalInterval > 0)
                    {
                        targetRenewalPercentage = selectedRenewalInterval;

                        if (targetRenewalPercentage > 100) { targetRenewalPercentage = 100; }
                    }

                    var targetRenewalMinutesAfterCertStart = certLifetime.Value.TotalMinutes * (targetRenewalPercentage / 100);
                    var targetRenewalDate = s.DateStart.Value.AddMinutes(targetRenewalMinutesAfterCertStart);
                    nextRenewalAttemptDate = targetRenewalDate;

                    if (targetRenewalDate <= checkDate)
                    {
                        isRenewalRequired = true;
                        renewalStatusReason = $"Certificate has exceeded {targetRenewalPercentage}% of its lifetime.";
                    }
                    else
                    {
                        isRenewalRequired = false;
                        renewalStatusReason = $"Certificate has not yet exceeded {targetRenewalPercentage}% of its lifetime.";
                    }
                }
                else
                {
                    // calculate renewal for non-percentage based strategies

                    if (renewalIntervalMode == RenewalIntervalModes.DaysBeforeExpiry)
                    {
                        var renewalDiffDays = timeToExpiry.TotalDays - renewalInterval;

                        // is item expiring within N days
                        if (timeToExpiry.TotalDays <= renewalInterval)
                        {

                            isRenewalRequired = true;
                            nextRenewalAttemptDate = checkDate;
                            renewalStatusReason = "Certificate is due to expire within the default renewal interval.";
                        }
                        else
                        {
                            isRenewalRequired = false;
                            nextRenewalAttemptDate = checkDate.AddDays(renewalDiffDays);
                            renewalStatusReason = $"Certificate has {renewalDiffDays} remaining days before the default renewal interval occurs.";
                        }
                    }
                    else
                    {
                        // was item renewed more than N days ago
                        var daysSinceLastRenewal = timeSinceLastRenewal.TotalDays;
                        var renewalDiffDays = timeSinceLastRenewal.TotalDays - renewalInterval;

                        if (daysSinceLastRenewal >= renewalInterval)
                        {
                            isRenewalRequired = true;
                            nextRenewalAttemptDate = checkDate;
                            renewalStatusReason = "Certificate is due for renewal, based on the default renewal settings.";
                        }
                        else
                        {
                            isRenewalRequired = false;
                            nextRenewalAttemptDate = checkDate.AddDays(-renewalDiffDays);
                            renewalStatusReason = "Certificate does not yet require renewal, based on the default renewal settings.";
                        }
                    }
                }
            }

            // if we have never achieved renewal, renew now
            if (!isRenewalRequired && s.DateRenewed == null)
            {
                isRenewalRequired = true;
                renewalStatusReason = "Certificate has not yet been successfully requested, so a renewal attempt is required.";
            }

            // if renewal is required but we have previously failed, scale the frequency of renewal
            // attempts to a minimum of once per 24hrs.
            if (isRenewalRequired && (s.LastRenewalStatus == RequestState.Error || s.LastRenewalStatus == RequestState.Warning || s.RenewalFailureCount > 0))
            {
                // our last attempt failed, check how many failures we've had to decide whether
                // we should attempt now or scale wait time based on how many attempts we've made.
                // Max 48hrs between attempts or 90% of lifetime (if known)

                if (s.RenewalFailureCount < 4)
                {
                    return new RenewalDueInfo(
                                reason: $"Renewal attempt is due, item has failed {s.RenewalFailureCount} times and is not yet subject to limited attempts.",
                                isRenewalDue: true,
                                checkDate,
                                certLifetime,
                                isRenewalOnHold: false
                                );
                }
                else
                {

                    if (s.DateLastRenewalAttempt != null)
                    {
                        var maxWaitHrsLimit = 48f; // absolute max wait time if cert lifetime not known
                        var maxWaitHrs = maxWaitHrsLimit;

                        // prefer max hold wait of 10% of lifetime, particularly useful for short lifetime certs
                        if (s.RequestConfig.PreferredExpiryDays != null)
                        {
                            maxWaitHrs = ((float)s.RequestConfig.PreferredExpiryDays * 24) * 0.1f;
                        }
                        else if (s.DateExpiry != null && s.DateStart != null)
                        {
                            var lifetime = s.DateExpiry - s.DateStart;
                            maxWaitHrs = (float)lifetime.Value.TotalHours * 0.1f;
                        }

                        // set ceiling for max hold wait time
                        maxWaitHrs = Math.Min(maxWaitHrs, maxWaitHrsLimit);

                        // calculate exponential back off, increasing 10% with retries to a max wait based on lifetime
                        var factor = 1 + (maxWaitHrs / 10);
                        var minWaitHrs = 1;

                        var calcWaitHrs = (float)Math.Min(minWaitHrs * (factor * s.RenewalFailureCount), maxWaitHrs);
                        var nextAttemptByDate = s.DateLastRenewalAttempt.Value.AddHours(calcWaitHrs);

                        if (DateTime.Now < nextAttemptByDate)
                        {
                            return new RenewalDueInfo(
                                    reason: $"Renewal attempt is on hold for {calcWaitHrs}hrs, item has failed multiple times and attempts are subject to periodic limits.",
                                    isRenewalDue: true,
                                    nextAttemptByDate, certLifetime,
                                    isRenewalOnHold: true,
                                    holdHrs: calcWaitHrs
                                    );
                        }
                        else
                        {
                            return new RenewalDueInfo(
                                   reason: $"Renewal attempt is due, item has failed multiple times and renewal will be periodically attempted. Hold skipped as attempt is already due.",
                                   isRenewalDue: true,
                                   nextAttemptByDate, certLifetime,
                                   isRenewalOnHold: false
                                   );
                        }
                    }
                    else
                    {
                        // never attempted, can't be put on hold
                        return new RenewalDueInfo(
                                  reason: $"Renewal attempt is due, item has not yet been attempted.",
                                  isRenewalDue: true,
                                  checkDate,
                                  certLifetime,
                                  isRenewalOnHold: false
                                  );
                    }
                }
            }

            if (!isRenewalRequired && s.DateNextScheduledRenewalAttempt.HasValue && s.DateNextScheduledRenewalAttempt < nextRenewalAttemptDate)
            {
                renewalStatusReason = "Certificate renewal is not yet required but has been scheduled ahead of normal renewal.";
                nextRenewalAttemptDate = s.DateNextScheduledRenewalAttempt.Value;
            }

            return new RenewalDueInfo(renewalStatusReason, isRenewalRequired, nextRenewalAttemptDate, certLifetime);
        }
    }
}

