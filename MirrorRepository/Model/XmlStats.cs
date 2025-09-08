using System.Collections.Generic;
using System.Xml.Serialization;

namespace MirrorRepository.Model
{
	[XmlRoot(ElementName = "xmlstats", Namespace = "")]
	public class XmlStats
	{

		[XmlElement(ElementName = "scheduler.queue.age", Namespace = "")]
		public SchedulerQueueAge SchedulerQueueAge { get; set; }

		[XmlElement(ElementName = "scheduler.queue.overdue_age", Namespace = "")]
		public SchedulerQueueOverdueAge SchedulerQueueOverdueAge { get; set; }

		[XmlElement(ElementName = "scheduler.worker.count", Namespace = "")]
		public int SchedulerWorkerCount { get; set; }

		[XmlElement(ElementName = "scheduler.queue.length", Namespace = "")]
		public int SchedulerQueueLength { get; set; }

		[XmlElement(ElementName = "scheduler.mean.queue.age", Namespace = "")]
		public double SchedulerMeanQueueAge { get; set; }

		[XmlElement(ElementName = "scheduler.system_id", Namespace = "")]
		public string SchedulerSystemId { get; set; }

		[XmlElement(ElementName = "scheduler.total.jobs", Namespace = "")]
		public int SchedulerTotalJobs { get; set; }

		[XmlElement(ElementName = "scheduler.total.claimed_jobs", Namespace = "")]
		public int SchedulerTotalClaimedJobs { get; set; }

		[XmlElement(ElementName = "scheduler.total.released_jobs", Namespace = "")]
		public int SchedulerTotalReleasedJobs { get; set; }

		[XmlElement(ElementName = "scheduler.total.burst.workers", Namespace = "")]
		public int SchedulerTotalBurstWorkers { get; set; }

		[XmlElement(ElementName = "scheduler.is.running", Namespace = "")]
		public bool SchedulerIsRunning { get; set; }

		[XmlElement(ElementName = "scheduler.workers", Namespace = "")]
		public SchedulerWorkers SchedulerWorkers { get; set; }

		[XmlElement(ElementName = "scheduler.worker.running_count", Namespace = "")]
		public int SchedulerWorkerRunningCount { get; set; }

		[XmlElement(ElementName = "scheduler.queue", Namespace = "")]
		public object SchedulerQueue { get; set; }

		[XmlElement(ElementName = "sessionsummary", Namespace = "")]
		public Sessionsummary Sessionsummary { get; set; }

		[XmlElement(ElementName = "db.prefix", Namespace = "")]
		public string DbPrefix { get; set; }

		[XmlElement(ElementName = "db.name", Namespace = "")]
		public string DbName { get; set; }

		[XmlElement(ElementName = "db.url", Namespace = "")]
		public string DbUrl { get; set; }

		[XmlElement(ElementName = "db.rdbms", Namespace = "")]
		public string DbRdbms { get; set; }

		[XmlElement(ElementName = "db.product_name", Namespace = "")]
		public string DbProductName { get; set; }

		[XmlElement(ElementName = "db.product_version", Namespace = "")]
		public string DbProductVersion { get; set; }

		[XmlElement(ElementName = "db.driver_name", Namespace = "")]
		public string DbDriverName { get; set; }

		[XmlElement(ElementName = "db.driver_version", Namespace = "")]
		public string DbDriverVersion { get; set; }

		[XmlElement(ElementName = "db.jdbc_major_version", Namespace = "")]
		public int DbJdbcMajorVersion { get; set; }

		[XmlElement(ElementName = "db.jdbc_minor_version", Namespace = "")]
		public int DbJdbcMinorVersion { get; set; }

		[XmlElement(ElementName = "db.user_name", Namespace = "")]
		public string DbUserName { get; set; }

		[XmlElement(ElementName = "db.server", Namespace = "")]
		public DbServer DbServer { get; set; }

		[XmlElement(ElementName = "db.candidates", Namespace = "")]
		public DbCandidates DbCandidates { get; set; }

		[XmlElement(ElementName = "db.pools", Namespace = "")]
		public DbPools DbPools { get; set; }

		[XmlElement(ElementName = "system.memory.max", Namespace = "")]
		public List<double> SystemMemoryMax { get; set; }

		[XmlElement(ElementName = "system.memory.total", Namespace = "")]
		public double SystemMemoryTotal { get; set; }

		[XmlElement(ElementName = "system.memory.in.use", Namespace = "")]
		public double SystemMemoryInUse { get; set; }

		[XmlElement(ElementName = "system.memory.pct.free", Namespace = "")]
		public double SystemMemoryPctFree { get; set; }

		[XmlElement(ElementName = "system.memory.buffer_pools", Namespace = "")]
		public SystemMemoryBufferPools SystemMemoryBufferPools { get; set; }

		[XmlElement(ElementName = "system.memory_codecache.max_in_mb", Namespace = "")]
		public int SystemMemoryCodecacheMaxInMb { get; set; }

		[XmlElement(ElementName = "system.memory_codecache.total_in_mb", Namespace = "")]
		public int SystemMemoryCodecacheTotalInMb { get; set; }

		[XmlElement(ElementName = "system.memory_codecache.in.use_in_mb", Namespace = "")]
		public int SystemMemoryCodecacheInUseInMb { get; set; }

		[XmlElement(ElementName = "instance_id", Namespace = "")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "instance_name", Namespace = "")]
		public string InstanceName { get; set; }

		[XmlElement(ElementName = "instance_type", Namespace = "")]
		public string InstanceType { get; set; }

		[XmlElement(ElementName = "instance_build_type", Namespace = "")]
		public string InstanceBuildType { get; set; }

		[XmlElement(ElementName = "instance_assigned_version", Namespace = "")]
		public string InstanceAssignedVersion { get; set; }

		[XmlElement(ElementName = "instance_current_version", Namespace = "")]
		public string InstanceCurrentVersion { get; set; }

		[XmlElement(ElementName = "mids", Namespace = "")]
		public Mids Mids { get; set; }

		[XmlElement(ElementName = "diagnostic.events", Namespace = "")]
		public DiagnosticEvents DiagnosticEvents { get; set; }

		[XmlElement(ElementName = "servlet.started", Namespace = "")]
		public string ServletStarted { get; set; }

		[XmlElement(ElementName = "servlet.cache.built", Namespace = "")]
		public string ServletCacheBuilt { get; set; }

		[XmlElement(ElementName = "servlet.cache.flushes", Namespace = "")]
		public int ServletCacheFlushes { get; set; }

		[XmlElement(ElementName = "servlet.uptime", Namespace = "")]
		public int ServletUptime { get; set; }

		[XmlElement(ElementName = "servlet.transactions", Namespace = "")]
		public int ServletTransactions { get; set; }

		[XmlElement(ElementName = "servlet.errors.handled", Namespace = "")]
		public int ServletErrorsHandled { get; set; }

		[XmlElement(ElementName = "servlet.processor.transactions", Namespace = "")]
		public int ServletProcessorTransactions { get; set; }

		[XmlElement(ElementName = "servlet.cancelled.transactions", Namespace = "")]
		public int ServletCancelledTransactions { get; set; }

		[XmlElement(ElementName = "servlet.active.sessions", Namespace = "")]
		public int ServletActiveSessions { get; set; }

		[XmlElement(ElementName = "servlet.info", Namespace = "")]
		public string ServletInfo { get; set; }

		[XmlElement(ElementName = "servlet.node_id", Namespace = "")]
		public string ServletNodeId { get; set; }

		[XmlElement(ElementName = "servlet.node_allows_inbound", Namespace = "")]
		public bool ServletNodeAllowsInbound { get; set; }

		[XmlElement(ElementName = "servlet.hostname", Namespace = "")]
		public string ServletHostname { get; set; }

		[XmlElement(ElementName = "servlet.port", Namespace = "")]
		public int ServletPort { get; set; }

		[XmlElement(ElementName = "servlet.metrics", Namespace = "")]
		public ServletMetrics ServletMetrics { get; set; }

		[XmlElement(ElementName = "lazywriter.queue.length", Namespace = "")]
		public int LazywriterQueueLength { get; set; }

		[XmlElement(ElementName = "lazywriter.total.operations", Namespace = "")]
		public int LazywriterTotalOperations { get; set; }

		[XmlElement(ElementName = "lazywriter.queue.peak.length", Namespace = "")]
		public int LazywriterQueuePeakLength { get; set; }

		[XmlElement(ElementName = "lazywriter.current.waiters", Namespace = "")]
		public int LazywriterCurrentWaiters { get; set; }

		[XmlElement(ElementName = "lazywriter.current.delay", Namespace = "")]
		public int LazywriterCurrentDelay { get; set; }

		[XmlElement(ElementName = "lazywriter.estimated.memory", Namespace = "")]
		public int LazywriterEstimatedMemory { get; set; }

		[XmlElement(ElementName = "semaphores", Namespace = "")]
		public List<Semaphores> Semaphores { get; set; }

		[XmlElement(ElementName = "glide.build.name", Namespace = "")]
		public string GlideBuildName { get; set; }

		[XmlElement(ElementName = "glide.build.date", Namespace = "")]
		public string GlideBuildDate { get; set; }

		[XmlElement(ElementName = "glide.build.tag", Namespace = "")]
		public string GlideBuildTag { get; set; }

		[XmlElement(ElementName = "com.glide.sys.WorkerThreadManager", Namespace = "")]
		public ComGlideSysWorkerThreadManager ComGlideSysWorkerThreadManager { get; set; }

		[XmlElement(ElementName = "com.glide.ui.ServletStatus", Namespace = "")]
		public ComGlideUiServletStatus ComGlideUiServletStatus { get; set; }

		[XmlElement(ElementName = "encryption_wrapper_listener_storage", Namespace = "")]
		public EncryptionWrapperListenerStorage EncryptionWrapperListenerStorage { get; set; }

		[XmlElement(ElementName = "glide.ais.ha.failover_in_progress", Namespace = "")]
		public GlideAisHaFailoverInProgress GlideAisHaFailoverInProgress { get; set; }

		[XmlElement(ElementName = "glide.db.archiver", Namespace = "")]
		public GlideDbArchiver GlideDbArchiver { get; set; }

		[XmlElement(ElementName = "glide.db.name", Namespace = "")]
		public GlideDbName GlideDbName { get; set; }

		[XmlElement(ElementName = "glide.db.pooler.connections.max", Namespace = "")]
		public GlideDbPoolerConnectionsMax GlideDbPoolerConnectionsMax { get; set; }

		[XmlElement(ElementName = "glide.event_heartbeat.current_age", Namespace = "")]
		public GlideEventHeartbeatCurrentAge GlideEventHeartbeatCurrentAge { get; set; }

		[XmlElement(ElementName = "glide.event_heartbeat.last_beat", Namespace = "")]
		public GlideEventHeartbeatLastBeat GlideEventHeartbeatLastBeat { get; set; }

		[XmlElement(ElementName = "glide.event_heartbeat.last_delay", Namespace = "")]
		public GlideEventHeartbeatLastDelay GlideEventHeartbeatLastDelay { get; set; }

		[XmlElement(ElementName = "glide.event_heartbeat.last_id", Namespace = "")]
		public GlideEventHeartbeatLastId GlideEventHeartbeatLastId { get; set; }

		[XmlElement(ElementName = "glide.event_heartbeat.version", Namespace = "")]
		public GlideEventHeartbeatVersion GlideEventHeartbeatVersion { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a", Namespace = "")]
		public GlideLdapListener1f82d2b31b90095040be9753b24bcb3a GlideLdapListener1f82d2b31b90095040be9753b24bcb3a { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.active", Namespace = "")]
		public GlideLdapListener1f82d2b31b90095040be9753b24bcb3aActive GlideLdapListener1f82d2b31b90095040be9753b24bcb3aActive { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.last_change", Namespace = "")]
		public GlideLdapListener1f82d2b31b90095040be9753b24bcb3aLastChange GlideLdapListener1f82d2b31b90095040be9753b24bcb3aLastChange { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.last_error", Namespace = "")]
		public GlideLdapListener1f82d2b31b90095040be9753b24bcb3aLastError GlideLdapListener1f82d2b31b90095040be9753b24bcb3aLastError { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.queue_size", Namespace = "")]
		public GlideLdapListener1f82d2b31b90095040be9753b24bcb3aQueueSize GlideLdapListener1f82d2b31b90095040be9753b24bcb3aQueueSize { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.shutdown_pending", Namespace = "")]
		public GlideLdapListener1f82d2b31b90095040be9753b24bcb3aShutdownPending GlideLdapListener1f82d2b31b90095040be9753b24bcb3aShutdownPending { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0", Namespace = "")]
		public GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0 GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0 { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.active", Namespace = "")]
		public GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0Active GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0Active { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.last_change", Namespace = "")]
		public GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0LastChange GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0LastChange { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.last_error", Namespace = "")]
		public GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0LastError GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0LastError { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.queue_size", Namespace = "")]
		public GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0QueueSize GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0QueueSize { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.shutdown_pending", Namespace = "")]
		public GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0ShutdownPending GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0ShutdownPending { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4", Namespace = "")]
		public GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4 GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4 { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.active", Namespace = "")]
		public GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4Active GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4Active { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.last_change", Namespace = "")]
		public GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4LastChange GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4LastChange { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.last_error", Namespace = "")]
		public GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4LastError GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4LastError { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.queue_size", Namespace = "")]
		public GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4QueueSize GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4QueueSize { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.shutdown_pending", Namespace = "")]
		public GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4ShutdownPending GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4ShutdownPending { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88", Namespace = "")]
		public GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88 GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88 { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.active", Namespace = "")]
		public GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88Active GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88Active { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.last_change", Namespace = "")]
		public GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88LastChange GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88LastChange { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.last_error", Namespace = "")]
		public GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88LastError GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88LastError { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.queue_size", Namespace = "")]
		public GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88QueueSize GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88QueueSize { get; set; }

		[XmlElement(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.shutdown_pending", Namespace = "")]
		public GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88ShutdownPending GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88ShutdownPending { get; set; }

		[XmlElement(ElementName = "glide.policy.eventdelegator", Namespace = "")]
		public GlidePolicyEventdelegator GlidePolicyEventdelegator { get; set; }

		[XmlElement(ElementName = "glide.pop3.status", Namespace = "")]
		public GlidePop3Status GlidePop3Status { get; set; }

		[XmlElement(ElementName = "glide.service.modeling.lastCheckpointsCleanup", Namespace = "")]
		public GlideServiceModelingLastCheckpointsCleanup GlideServiceModelingLastCheckpointsCleanup { get; set; }

		[XmlElement(ElementName = "glide.service.modeling.lastSweep", Namespace = "")]
		public GlideServiceModelingLastSweep GlideServiceModelingLastSweep { get; set; }

		[XmlElement(ElementName = "glide.service.modeling.sweepDuration", Namespace = "")]
		public GlideServiceModelingSweepDuration GlideServiceModelingSweepDuration { get; set; }

		[XmlElement(ElementName = "glide.servlet.port", Namespace = "")]
		public GlideServletPort GlideServletPort { get; set; }

		[XmlElement(ElementName = "glide.smtp.status", Namespace = "")]
		public GlideSmtpStatus GlideSmtpStatus { get; set; }

		[XmlElement(ElementName = "glide.ui.max.transactions", Namespace = "")]
		public GlideUiMaxTransactions GlideUiMaxTransactions { get; set; }

		[XmlElement(ElementName = "glide.update_operation.queue.status", Namespace = "")]
		public GlideUpdateOperationQueueStatus GlideUpdateOperationQueueStatus { get; set; }

		[XmlElement(ElementName = "glide.update_operation.queue.upgrade_check", Namespace = "")]
		public GlideUpdateOperationQueueUpgradeCheck GlideUpdateOperationQueueUpgradeCheck { get; set; }

		[XmlElement(ElementName = "instance_registration", Namespace = "")]
		public InstanceRegistration InstanceRegistration { get; set; }

		[XmlElement(ElementName = "license_mutex_status_message", Namespace = "")]
		public LicenseMutexStatusMessage LicenseMutexStatusMessage { get; set; }

		[XmlElement(ElementName = "mid.monitor.heartbeat_sent", Namespace = "")]
		public MidMonitorHeartbeatSent MidMonitorHeartbeatSent { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.ais_index_queue_stats", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeAisIndexQueueStats NoOptimizeWriteAuditNewValueNoOptimizeAisIndexQueueStats { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.ais_partition_health", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeAisPartitionHealth NoOptimizeWriteAuditNewValueNoOptimizeAisPartitionHealth { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.ais_partition_health_response", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeAisPartitionHealthResponse NoOptimizeWriteAuditNewValueNoOptimizeAisPartitionHealthResponse { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.ais_updatable_field_event", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeAisUpdatableFieldEvent NoOptimizeWriteAuditNewValueNoOptimizeAisUpdatableFieldEvent { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.cds_client_staging", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeCdsClientStaging NoOptimizeWriteAuditNewValueNoOptimizeCdsClientStaging { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.cmdb_ire_partial_payloads", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeCmdbIrePartialPayloads NoOptimizeWriteAuditNewValueNoOptimizeCmdbIrePartialPayloads { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.diagnostic_event", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeDiagnosticEvent NoOptimizeWriteAuditNewValueNoOptimizeDiagnosticEvent { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.discovery_device_duplicate_ips", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeDiscoveryDeviceDuplicateIps NoOptimizeWriteAuditNewValueNoOptimizeDiscoveryDeviceDuplicateIps { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_alert", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeEmAlert NoOptimizeWriteAuditNewValueNoOptimizeEmAlert { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_alert_insight_state", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeEmAlertInsightState NoOptimizeWriteAuditNewValueNoOptimizeEmAlertInsightState { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_alert_trigger_queue", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeEmAlertTriggerQueue NoOptimizeWriteAuditNewValueNoOptimizeEmAlertTriggerQueue { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_ci_graph_reuse", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeEmCiGraphReuse NoOptimizeWriteAuditNewValueNoOptimizeEmCiGraphReuse { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_connected_services", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeEmConnectedServices NoOptimizeWriteAuditNewValueNoOptimizeEmConnectedServices { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_extra_data_json", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeEmExtraDataJson NoOptimizeWriteAuditNewValueNoOptimizeEmExtraDataJson { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_impacted_ci", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeEmImpactedCi NoOptimizeWriteAuditNewValueNoOptimizeEmImpactedCi { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_impact_graph", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeEmImpactGraph NoOptimizeWriteAuditNewValueNoOptimizeEmImpactGraph { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.jrobin_database", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeJrobinDatabase NoOptimizeWriteAuditNewValueNoOptimizeJrobinDatabase { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.ml_update_set", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeMlUpdateSet NoOptimizeWriteAuditNewValueNoOptimizeMlUpdateSet { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.multisso_request_parameter", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeMultissoRequestParameter NoOptimizeWriteAuditNewValueNoOptimizeMultissoRequestParameter { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.pa_rw_aggregate", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizePaRwAggregate NoOptimizeWriteAuditNewValueNoOptimizePaRwAggregate { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.qb_query_status", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeQbQueryStatus NoOptimizeWriteAuditNewValueNoOptimizeQbQueryStatus { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sa_agg_pattern_alert", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSaAggPatternAlert NoOptimizeWriteAuditNewValueNoOptimizeSaAggPatternAlert { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sa_debug_session", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSaDebugSession NoOptimizeWriteAuditNewValueNoOptimizeSaDebugSession { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sa_debug_session_status", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSaDebugSessionStatus NoOptimizeWriteAuditNewValueNoOptimizeSaDebugSessionStatus { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sa_paged_payload", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSaPagedPayload NoOptimizeWriteAuditNewValueNoOptimizeSaPagedPayload { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sm_ci_field_data", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSmCiFieldData NoOptimizeWriteAuditNewValueNoOptimizeSmCiFieldData { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sm_flapper_strategy_data", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSmFlapperStrategyData NoOptimizeWriteAuditNewValueNoOptimizeSmFlapperStrategyData { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sn_cmp_cloud_event", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSnCmpCloudEvent NoOptimizeWriteAuditNewValueNoOptimizeSnCmpCloudEvent { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.svc_changes", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSvcChanges NoOptimizeWriteAuditNewValueNoOptimizeSvcChanges { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_atf_test_result", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysAtfTestResult NoOptimizeWriteAuditNewValueNoOptimizeSysAtfTestResult { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_atf_test_suite_result", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysAtfTestSuiteResult NoOptimizeWriteAuditNewValueNoOptimizeSysAtfTestSuiteResult { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_ci_analytics", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysCiAnalytics NoOptimizeWriteAuditNewValueNoOptimizeSysCiAnalytics { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_cluster_message", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysClusterMessage NoOptimizeWriteAuditNewValueNoOptimizeSysClusterMessage { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_coalesce_strategy_deferred", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysCoalesceStrategyDeferred NoOptimizeWriteAuditNewValueNoOptimizeSysCoalesceStrategyDeferred { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_cs_ca_message", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysCsCaMessage NoOptimizeWriteAuditNewValueNoOptimizeSysCsCaMessage { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_cs_consumer", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysCsConsumer NoOptimizeWriteAuditNewValueNoOptimizeSysCsConsumer { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_execution_tracker", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysExecutionTracker NoOptimizeWriteAuditNewValueNoOptimizeSysExecutionTracker { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_flow_context", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysFlowContext NoOptimizeWriteAuditNewValueNoOptimizeSysFlowContext { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_geocoding_request", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysGeocodingRequest NoOptimizeWriteAuditNewValueNoOptimizeSysGeocodingRequest { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_hub_popular_artifacts", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysHubPopularArtifacts NoOptimizeWriteAuditNewValueNoOptimizeSysHubPopularArtifacts { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_import_set_row_error", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysImportSetRowError NoOptimizeWriteAuditNewValueNoOptimizeSysImportSetRowError { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_object_source", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysObjectSource NoOptimizeWriteAuditNewValueNoOptimizeSysObjectSource { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_poll", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysPoll NoOptimizeWriteAuditNewValueNoOptimizeSysPoll { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_rate_limit_count", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysRateLimitCount NoOptimizeWriteAuditNewValueNoOptimizeSysRateLimitCount { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_script_execution_history", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysScriptExecutionHistory NoOptimizeWriteAuditNewValueNoOptimizeSysScriptExecutionHistory { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_transform_target_row", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeSysTransformTargetRow NoOptimizeWriteAuditNewValueNoOptimizeSysTransformTargetRow { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.ui_notification_inbox", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeUiNotificationInbox NoOptimizeWriteAuditNewValueNoOptimizeUiNotificationInbox { get; set; }

		[XmlElement(ElementName = "no_optimize_write_audit.new_value.no_optimize.wf_workflow_version", Namespace = "")]
		public NoOptimizeWriteAuditNewValueNoOptimizeWfWorkflowVersion NoOptimizeWriteAuditNewValueNoOptimizeWfWorkflowVersion { get; set; }

		[XmlElement(ElementName = "system.build.date", Namespace = "")]
		public SystemBuildDate SystemBuildDate { get; set; }

		[XmlElement(ElementName = "system.hostname", Namespace = "")]
		public SystemHostname SystemHostname { get; set; }

		[XmlElement(ElementName = "system.java.home", Namespace = "")]
		public SystemJavaHome SystemJavaHome { get; set; }

		[XmlElement(ElementName = "system.java.version", Namespace = "")]
		public SystemJavaVersion SystemJavaVersion { get; set; }

		[XmlElement(ElementName = "system.mysqld.version", Namespace = "")]
		public SystemMysqldVersion SystemMysqldVersion { get; set; }

		[XmlElement(ElementName = "system.os.arch", Namespace = "")]
		public SystemOsArch SystemOsArch { get; set; }

		[XmlElement(ElementName = "system.os.name", Namespace = "")]
		public SystemOsName SystemOsName { get; set; }

		[XmlElement(ElementName = "system.os.version", Namespace = "")]
		public SystemOsVersion SystemOsVersion { get; set; }

		[XmlElement(ElementName = "system.session.timeout", Namespace = "")]
		public SystemSessionTimeout SystemSessionTimeout { get; set; }

		[XmlElement(ElementName = "system.startup", Namespace = "")]
		public SystemStartup SystemStartup { get; set; }

		[XmlElement(ElementName = "system.status", Namespace = "")]
		public SystemStatus SystemStatus { get; set; }

		[XmlElement(ElementName = "update_mutex_status_message", Namespace = "")]
		public UpdateMutexStatusMessage UpdateMutexStatusMessage { get; set; }

		[XmlElement(ElementName = "system.cluster.node_id", Namespace = "")]
		public string SystemClusterNodeId { get; set; }

		[XmlElement(ElementName = "glide.offering", Namespace = "")]
		public string GlideOffering { get; set; }

		[XmlElement(ElementName = "system.memory_metaspace.max", Namespace = "")]
		public double SystemMemoryMetaspaceMax { get; set; }

		[XmlElement(ElementName = "system.memory_metaspace.total", Namespace = "")]
		public double SystemMemoryMetaspaceTotal { get; set; }

		[XmlElement(ElementName = "system.memory_metaspace.in.use", Namespace = "")]
		public double SystemMemoryMetaspaceInUse { get; set; }

		[XmlElement(ElementName = "jvm.version", Namespace = "")]
		public string JvmVersion { get; set; }

		[XmlElement(ElementName = "jvm.time", Namespace = "")]
		public double JvmTime { get; set; }

		[XmlElement(ElementName = "jvm.time_friendly", Namespace = "")]
		public string JvmTimeFriendly { get; set; }

		[XmlElement(ElementName = "jvm.java_opts", Namespace = "")]
		public string JvmJavaOpts { get; set; }

		[XmlElement(ElementName = "jvm.cpu.time", Namespace = "")]
		public double JvmCpuTime { get; set; }

		[XmlElement(ElementName = "jvm.cpu.time_friendly", Namespace = "")]
		public string JvmCpuTimeFriendly { get; set; }

		[XmlElement(ElementName = "jvm.cpu.count", Namespace = "")]
		public double JvmCpuCount { get; set; }

		[XmlElement(ElementName = "jvm.classes.loaded", Namespace = "")]
		public double JvmClassesLoaded { get; set; }

		[XmlElement(ElementName = "jvm.classes.unloaded", Namespace = "")]
		public double JvmClassesUnloaded { get; set; }

		[XmlElement(ElementName = "jvm.classes.verbose", Namespace = "")]
		public bool JvmClassesVerbose { get; set; }

		[XmlElement(ElementName = "jvm.gc", Namespace = "")]
		public JvmGc JvmGc { get; set; }

		[XmlAttribute(AttributeName = "created", Namespace = "")]
		public string Created { get; set; }

		[XmlAttribute(AttributeName = "includes", Namespace = "")]
		public string Includes { get; set; }

		[XmlAttribute(AttributeName = "version", Namespace = "")]
		public int Version { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "scheduler.queue.age", Namespace = "")]
	public class SchedulerQueueAge
	{

		[XmlAttribute(AttributeName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlAttribute(AttributeName = "max", Namespace = "")]
		public int Max { get; set; }

		[XmlAttribute(AttributeName = "mean", Namespace = "")]
		public double Mean { get; set; }

		[XmlAttribute(AttributeName = "median", Namespace = "")]
		public double Median { get; set; }

		[XmlAttribute(AttributeName = "min", Namespace = "")]
		public int Min { get; set; }
	}

	[XmlRoot(ElementName = "scheduler.queue.overdue_age", Namespace = "")]
	public class SchedulerQueueOverdueAge
	{

		[XmlAttribute(AttributeName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlAttribute(AttributeName = "max", Namespace = "")]
		public int Max { get; set; }

		[XmlAttribute(AttributeName = "mean", Namespace = "")]
		public double Mean { get; set; }

		[XmlAttribute(AttributeName = "median", Namespace = "")]
		public double Median { get; set; }

		[XmlAttribute(AttributeName = "min", Namespace = "")]
		public int Min { get; set; }
	}

	[XmlRoot(ElementName = "glide.scheduler.worker.7", Namespace = "")]
	public class GlideSchedulerWorker7
	{

		[XmlElement(ElementName = "current.job", Namespace = "")]
		public string CurrentJob { get; set; }

		[XmlElement(ElementName = "total.jobs", Namespace = "")]
		public int TotalJobs { get; set; }

		[XmlElement(ElementName = "mean.duration", Namespace = "")]
		public int MeanDuration { get; set; }
	}

	[XmlRoot(ElementName = "glide.scheduler.worker.6", Namespace = "")]
	public class GlideSchedulerWorker6
	{

		[XmlElement(ElementName = "current.job", Namespace = "")]
		public string CurrentJob { get; set; }

		[XmlElement(ElementName = "total.jobs", Namespace = "")]
		public int TotalJobs { get; set; }

		[XmlElement(ElementName = "mean.duration", Namespace = "")]
		public int MeanDuration { get; set; }
	}

	[XmlRoot(ElementName = "glide.scheduler.worker.5", Namespace = "")]
	public class GlideSchedulerWorker5
	{

		[XmlElement(ElementName = "current.job", Namespace = "")]
		public string CurrentJob { get; set; }

		[XmlElement(ElementName = "total.jobs", Namespace = "")]
		public int TotalJobs { get; set; }

		[XmlElement(ElementName = "mean.duration", Namespace = "")]
		public int MeanDuration { get; set; }
	}

	[XmlRoot(ElementName = "glide.scheduler.worker.4", Namespace = "")]
	public class GlideSchedulerWorker4
	{

		[XmlElement(ElementName = "current.job", Namespace = "")]
		public string CurrentJob { get; set; }

		[XmlElement(ElementName = "total.jobs", Namespace = "")]
		public int TotalJobs { get; set; }

		[XmlElement(ElementName = "mean.duration", Namespace = "")]
		public int MeanDuration { get; set; }
	}

	[XmlRoot(ElementName = "glide.scheduler.worker.3", Namespace = "")]
	public class GlideSchedulerWorker3
	{

		[XmlElement(ElementName = "current.job", Namespace = "")]
		public string CurrentJob { get; set; }

		[XmlElement(ElementName = "total.jobs", Namespace = "")]
		public int TotalJobs { get; set; }

		[XmlElement(ElementName = "mean.duration", Namespace = "")]
		public int MeanDuration { get; set; }
	}

	[XmlRoot(ElementName = "glide.scheduler.worker.2", Namespace = "")]
	public class GlideSchedulerWorker2
	{

		[XmlElement(ElementName = "current.job", Namespace = "")]
		public string CurrentJob { get; set; }

		[XmlElement(ElementName = "total.jobs", Namespace = "")]
		public int TotalJobs { get; set; }

		[XmlElement(ElementName = "mean.duration", Namespace = "")]
		public int MeanDuration { get; set; }
	}

	[XmlRoot(ElementName = "glide.scheduler.worker.1", Namespace = "")]
	public class GlideSchedulerWorker1
	{

		[XmlElement(ElementName = "current.job", Namespace = "")]
		public string CurrentJob { get; set; }

		[XmlElement(ElementName = "total.jobs", Namespace = "")]
		public int TotalJobs { get; set; }

		[XmlElement(ElementName = "mean.duration", Namespace = "")]
		public int MeanDuration { get; set; }
	}

	[XmlRoot(ElementName = "glide.scheduler.worker.0", Namespace = "")]
	public class GlideSchedulerWorker0
	{

		[XmlElement(ElementName = "current.job", Namespace = "")]
		public string CurrentJob { get; set; }

		[XmlElement(ElementName = "total.jobs", Namespace = "")]
		public int TotalJobs { get; set; }

		[XmlElement(ElementName = "mean.duration", Namespace = "")]
		public int MeanDuration { get; set; }
	}

	[XmlRoot(ElementName = "scheduler.workers", Namespace = "")]
	public class SchedulerWorkers
	{

		[XmlElement(ElementName = "glide.scheduler.worker.7", Namespace = "")]
		public GlideSchedulerWorker7 GlideSchedulerWorker7 { get; set; }

		[XmlElement(ElementName = "glide.scheduler.worker.6", Namespace = "")]
		public GlideSchedulerWorker6 GlideSchedulerWorker6 { get; set; }

		[XmlElement(ElementName = "glide.scheduler.worker.5", Namespace = "")]
		public GlideSchedulerWorker5 GlideSchedulerWorker5 { get; set; }

		[XmlElement(ElementName = "glide.scheduler.worker.4", Namespace = "")]
		public GlideSchedulerWorker4 GlideSchedulerWorker4 { get; set; }

		[XmlElement(ElementName = "glide.scheduler.worker.3", Namespace = "")]
		public GlideSchedulerWorker3 GlideSchedulerWorker3 { get; set; }

		[XmlElement(ElementName = "glide.scheduler.worker.2", Namespace = "")]
		public GlideSchedulerWorker2 GlideSchedulerWorker2 { get; set; }

		[XmlElement(ElementName = "glide.scheduler.worker.1", Namespace = "")]
		public GlideSchedulerWorker1 GlideSchedulerWorker1 { get; set; }

		[XmlElement(ElementName = "glide.scheduler.worker.0", Namespace = "")]
		public GlideSchedulerWorker0 GlideSchedulerWorker0 { get; set; }
	}

	[XmlRoot(ElementName = "sessionsummary", Namespace = "")]
	public class Sessionsummary
	{

		[XmlAttribute(AttributeName = "end_user", Namespace = "")]
		public int EndUser { get; set; }

		[XmlAttribute(AttributeName = "logged_in", Namespace = "")]
		public int LoggedIn { get; set; }

		[XmlAttribute(AttributeName = "total", Namespace = "")]
		public int Total { get; set; }
	}

	[XmlRoot(ElementName = "db.server", Namespace = "")]
	public class DbServer
	{

		[XmlElement(ElementName = "build", Namespace = "")]
		public string Build { get; set; }

		[XmlElement(ElementName = "engine_version", Namespace = "")]
		public string EngineVersion { get; set; }
	}

	[XmlRoot(ElementName = "last_check", Namespace = "")]
	public class LastCheck
	{

		[XmlElement(ElementName = "last_check_succeeded", Namespace = "")]
		public bool LastCheckSucceeded { get; set; }

		[XmlElement(ElementName = "last_check_timestamp", Namespace = "")]
		public string LastCheckTimestamp { get; set; }

		[XmlElement(ElementName = "last_check_age", Namespace = "")]
		public int LastCheckAge { get; set; }

		[XmlElement(ElementName = "instance_id", Namespace = "")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "valid_for_switchover", Namespace = "")]
		public bool ValidForSwitchover { get; set; }
	}

	[XmlRoot(ElementName = "candidate", Namespace = "")]
	public class Candidate
	{

		[XmlElement(ElementName = "writable", Namespace = "")]
		public bool Writable { get; set; }

		[XmlElement(ElementName = "readable", Namespace = "")]
		public bool Readable { get; set; }

		[XmlElement(ElementName = "failures", Namespace = "")]
		public int Failures { get; set; }

		[XmlElement(ElementName = "last_good_timestamp", Namespace = "")]
		public string LastGoodTimestamp { get; set; }

		[XmlElement(ElementName = "last_good_age", Namespace = "")]
		public int LastGoodAge { get; set; }

		[XmlElement(ElementName = "last_check", Namespace = "")]
		public LastCheck LastCheck { get; set; }

		[XmlAttribute(AttributeName = "url", Namespace = "")]
		public string Url { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "db.candidates", Namespace = "")]
	public class DbCandidates
	{

		[XmlElement(ElementName = "candidate", Namespace = "")]
		public Candidate Candidate { get; set; }
	}

	[XmlRoot(ElementName = "pool", Namespace = "")]
	public class Pool
	{

		[XmlElement(ElementName = "url", Namespace = "")]
		public string Url { get; set; }

		[XmlElement(ElementName = "status", Namespace = "")]
		public string Status { get; set; }

		[XmlElement(ElementName = "busy", Namespace = "")]
		public int Busy { get; set; }

		[XmlElement(ElementName = "available", Namespace = "")]
		public int Available { get; set; }

		[XmlElement(ElementName = "total", Namespace = "")]
		public int Total { get; set; }

		[XmlElement(ElementName = "max", Namespace = "")]
		public int Max { get; set; }

		[XmlElement(ElementName = "replication_lag", Namespace = "")]
		public string ReplicationLag { get; set; }

		[XmlElement(ElementName = "age", Namespace = "")]
		public int Age { get; set; }

		[XmlAttribute(AttributeName = "name", Namespace = "")]
		public string Name { get; set; }

		[XmlText]
		public string Text { get; set; }

		[XmlAttribute(AttributeName = "primary", Namespace = "")]
		public bool Primary { get; set; }
	}

	[XmlRoot(ElementName = "db.pools", Namespace = "")]
	public class DbPools
	{

		[XmlElement(ElementName = "pool", Namespace = "")]
		public List<Pool> Pool { get; set; }
	}

	[XmlRoot(ElementName = "mapped", Namespace = "")]
	public class Mapped
	{

		[XmlElement(ElementName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlElement(ElementName = "total_capacity_in_mb", Namespace = "")]
		public int TotalCapacityInMb { get; set; }

		[XmlElement(ElementName = "memory_used_in_mb", Namespace = "")]
		public int MemoryUsedInMb { get; set; }
	}

	[XmlRoot(ElementName = "direct", Namespace = "")]
	public class Direct
	{

		[XmlElement(ElementName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlElement(ElementName = "total_capacity_in_mb", Namespace = "")]
		public int TotalCapacityInMb { get; set; }

		[XmlElement(ElementName = "memory_used_in_mb", Namespace = "")]
		public int MemoryUsedInMb { get; set; }
	}

	[XmlRoot(ElementName = "system.memory.buffer_pools", Namespace = "")]
	public class SystemMemoryBufferPools
	{

		[XmlElement(ElementName = "mapped", Namespace = "")]
		public Mapped Mapped { get; set; }

		[XmlElement(ElementName = "direct", Namespace = "")]
		public Direct Direct { get; set; }
	}

	[XmlRoot(ElementName = "mid_server", Namespace = "")]
	public class MidServer
	{

		[XmlElement(ElementName = "name", Namespace = "")]
		public string Name { get; set; }

		[XmlElement(ElementName = "version", Namespace = "")]
		public string Version { get; set; }

		[XmlElement(ElementName = "jvm_version", Namespace = "")]
		public string JvmVersion { get; set; }

		[XmlElement(ElementName = "ip_address", Namespace = "")]
		public string IpAddress { get; set; }

		[XmlElement(ElementName = "host_name", Namespace = "")]
		public string HostName { get; set; }

		[XmlElement(ElementName = "host_type", Namespace = "")]
		public string HostType { get; set; }

		[XmlElement(ElementName = "started", Namespace = "")]
		public string Started { get; set; }

		[XmlElement(ElementName = "stopped", Namespace = "")]
		public string Stopped { get; set; }

		[XmlElement(ElementName = "last_refreshed", Namespace = "")]
		public string LastRefreshed { get; set; }

		[XmlElement(ElementName = "status", Namespace = "")]
		public string Status { get; set; }
	}

	[XmlRoot(ElementName = "mids", Namespace = "")]
	public class Mids
	{

		[XmlElement(ElementName = "mid_server", Namespace = "")]
		public List<MidServer> MidServer { get; set; }
	}

	[XmlRoot(ElementName = "event", Namespace = "")]
	public class Event
	{

		[XmlElement(ElementName = "id", Namespace = "")]
		public string Id { get; set; }

		[XmlElement(ElementName = "name", Namespace = "")]
		public string Name { get; set; }

		[XmlElement(ElementName = "detail", Namespace = "")]
		public string Detail { get; set; }

		[XmlElement(ElementName = "severity", Namespace = "")]
		public string Severity { get; set; }

		[XmlElement(ElementName = "status", Namespace = "")]
		public string Status { get; set; }

		[XmlElement(ElementName = "reported_on", Namespace = "")]
		public string ReportedOn { get; set; }

		[XmlElement(ElementName = "system_id", Namespace = "")]
		public string SystemId { get; set; }

		[XmlElement(ElementName = "recorded_at", Namespace = "")]
		public string RecordedAt { get; set; }
	}

	[XmlRoot(ElementName = "diagnostic.events", Namespace = "")]
	public class DiagnosticEvents
	{

		[XmlElement(ElementName = "event", Namespace = "")]
		public List<Event> Event { get; set; }
	}

	[XmlRoot(ElementName = "one", Namespace = "")]
	public class One
	{

		[XmlAttribute(AttributeName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlAttribute(AttributeName = "max", Namespace = "")]
		public int Max { get; set; }

		[XmlAttribute(AttributeName = "mean", Namespace = "")]
		public double Mean { get; set; }

		[XmlAttribute(AttributeName = "median", Namespace = "")]
		public double Median { get; set; }

		[XmlAttribute(AttributeName = "min", Namespace = "")]
		public int Min { get; set; }

		[XmlAttribute(AttributeName = "ninetypercent", Namespace = "")]
		public int Ninetypercent { get; set; }

		[XmlAttribute(AttributeName = "ninetypercentTrimmedMean", Namespace = "")]
		public string NinetypercentTrimmedMean { get; set; }
	}

	[XmlRoot(ElementName = "five", Namespace = "")]
	public class Five
	{

		[XmlAttribute(AttributeName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlAttribute(AttributeName = "max", Namespace = "")]
		public int Max { get; set; }

		[XmlAttribute(AttributeName = "mean", Namespace = "")]
		public double Mean { get; set; }

		[XmlAttribute(AttributeName = "median", Namespace = "")]
		public double Median { get; set; }

		[XmlAttribute(AttributeName = "min", Namespace = "")]
		public int Min { get; set; }

		[XmlAttribute(AttributeName = "ninetypercent", Namespace = "")]
		public int Ninetypercent { get; set; }

		[XmlAttribute(AttributeName = "ninetypercentTrimmedMean", Namespace = "")]
		public string NinetypercentTrimmedMean { get; set; }
	}

	[XmlRoot(ElementName = "fifteen", Namespace = "")]
	public class Fifteen
	{

		[XmlAttribute(AttributeName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlAttribute(AttributeName = "max", Namespace = "")]
		public int Max { get; set; }

		[XmlAttribute(AttributeName = "mean", Namespace = "")]
		public double Mean { get; set; }

		[XmlAttribute(AttributeName = "median", Namespace = "")]
		public double Median { get; set; }

		[XmlAttribute(AttributeName = "min", Namespace = "")]
		public int Min { get; set; }

		[XmlAttribute(AttributeName = "ninetypercent", Namespace = "")]
		public int Ninetypercent { get; set; }

		[XmlAttribute(AttributeName = "ninetypercentTrimmedMean", Namespace = "")]
		public string NinetypercentTrimmedMean { get; set; }
	}

	[XmlRoot(ElementName = "hour", Namespace = "")]
	public class Hour
	{

		[XmlAttribute(AttributeName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlAttribute(AttributeName = "max", Namespace = "")]
		public int Max { get; set; }

		[XmlAttribute(AttributeName = "mean", Namespace = "")]
		public double Mean { get; set; }

		[XmlAttribute(AttributeName = "median", Namespace = "")]
		public double Median { get; set; }

		[XmlAttribute(AttributeName = "min", Namespace = "")]
		public int Min { get; set; }

		[XmlAttribute(AttributeName = "ninetypercent", Namespace = "")]
		public int Ninetypercent { get; set; }

		[XmlAttribute(AttributeName = "ninetypercentTrimmedMean", Namespace = "")]
		public string NinetypercentTrimmedMean { get; set; }
	}

	[XmlRoot(ElementName = "daily", Namespace = "")]
	public class Daily
	{

		[XmlAttribute(AttributeName = "count", Namespace = "")]
		public int Count { get; set; }

		[XmlAttribute(AttributeName = "max", Namespace = "")]
		public int Max { get; set; }

		[XmlAttribute(AttributeName = "mean", Namespace = "")]
		public double Mean { get; set; }

		[XmlAttribute(AttributeName = "median", Namespace = "")]
		public double Median { get; set; }

		[XmlAttribute(AttributeName = "min", Namespace = "")]
		public int Min { get; set; }

		[XmlAttribute(AttributeName = "ninetypercent", Namespace = "")]
		public int Ninetypercent { get; set; }

		[XmlAttribute(AttributeName = "ninetypercentTrimmedMean", Namespace = "")]
		public string NinetypercentTrimmedMean { get; set; }
	}

	[XmlRoot(ElementName = "transactions", Namespace = "")]
	public class Transactions
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "client_transactions", Namespace = "")]
	public class ClientTransactions
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "processed_transactions", Namespace = "")]
	public class ProcessedTransactions
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "db_connections", Namespace = "")]
	public class DbConnections
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "semaphores", Namespace = "")]
	public class Semaphores
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlAttribute(AttributeName = "available", Namespace = "")]
		public int Available { get; set; }

		[XmlAttribute(AttributeName = "max_queue_depth", Namespace = "")]
		public int MaxQueueDepth { get; set; }

		[XmlAttribute(AttributeName = "maximum_concurrency", Namespace = "")]
		public int MaximumConcurrency { get; set; }

		[XmlAttribute(AttributeName = "name", Namespace = "")]
		public string Name { get; set; }

		[XmlAttribute(AttributeName = "queue_age", Namespace = "")]
		public int QueueAge { get; set; }

		[XmlAttribute(AttributeName = "queue_depth", Namespace = "")]
		public int QueueDepth { get; set; }

		[XmlAttribute(AttributeName = "queue_depth_limit", Namespace = "")]
		public int QueueDepthLimit { get; set; }

		[XmlAttribute(AttributeName = "rejected_executions", Namespace = "")]
		public int RejectedExecutions { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_waiters", Namespace = "")]
	public class SemaphoreWaiters
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "session_waiters", Namespace = "")]
	public class SessionWaiters
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "uri_transactions", Namespace = "")]
	public class UriTransactions
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_amb_receive_rejected", Namespace = "")]
	public class SemaphoreAmbReceiveRejected
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_amb_send_rejected", Namespace = "")]
	public class SemaphoreAmbSendRejected
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_api_int_rejected", Namespace = "")]
	public class SemaphoreApiIntRejected
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_debug_rejected", Namespace = "")]
	public class SemaphoreDebugRejected
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_default_rejected", Namespace = "")]
	public class SemaphoreDefaultRejected
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_presence_rejected", Namespace = "")]
	public class SemaphorePresenceRejected
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_amb_receive_response_time", Namespace = "")]
	public class SemaphoreAmbReceiveResponseTime
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_amb_send_response_time", Namespace = "")]
	public class SemaphoreAmbSendResponseTime
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_api_int_response_time", Namespace = "")]
	public class SemaphoreApiIntResponseTime
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_default_response_time", Namespace = "")]
	public class SemaphoreDefaultResponseTime
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_debug_response_time", Namespace = "")]
	public class SemaphoreDebugResponseTime
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "semaphore_presence_response_time", Namespace = "")]
	public class SemaphorePresenceResponseTime
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "event_logs", Namespace = "")]
	public class EventLogs
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "events_processed", Namespace = "")]
	public class EventsProcessed
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "logs", Namespace = "")]
	public class Logs
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "errors", Namespace = "")]
	public class Errors
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "sys_load", Namespace = "")]
	public class SysLoad
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "threads_business_rules", Namespace = "")]
	public class ThreadsBusinessRules
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "threads_cpu", Namespace = "")]
	public class ThreadsCpu
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "threads_db", Namespace = "")]
	public class ThreadsDb
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "threads_network", Namespace = "")]
	public class ThreadsNetwork
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "threads_concurrency", Namespace = "")]
	public class ThreadsConcurrency
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "memory", Namespace = "")]
	public class Memory
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "memory_total", Namespace = "")]
	public class MemoryTotal
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "memory_max", Namespace = "")]
	public class MemoryMax
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "metaspace_max", Namespace = "")]
	public class MetaspaceMax
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "metaspace_total", Namespace = "")]
	public class MetaspaceTotal
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "metaspace_used", Namespace = "")]
	public class MetaspaceUsed
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "garbage_collection", Namespace = "")]
	public class GarbageCollection
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "ParNew_collector_count", Namespace = "")]
	public class ParNewCollectorCount
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "ConcurrentMarkSweep_collector_count", Namespace = "")]
	public class ConcurrentMarkSweepCollectorCount
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "memory_active", Namespace = "")]
	public class MemoryActive
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "memory_cache", Namespace = "")]
	public class MemoryCache
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "memory_swap", Namespace = "")]
	public class MemorySwap
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "sql_response", Namespace = "")]
	public class SqlResponse
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "sql_inserts", Namespace = "")]
	public class SqlInserts
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "sql_updates", Namespace = "")]
	public class SqlUpdates
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "sql_deletes", Namespace = "")]
	public class SqlDeletes
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "sql_selects", Namespace = "")]
	public class SqlSelects
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "replication_lag", Namespace = "")]
	public class ReplicationLag
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "job_queue", Namespace = "")]
	public class JobQueue
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "processed_jobs", Namespace = "")]
	public class ProcessedJobs
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "job_times", Namespace = "")]
	public class JobTimes
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "sessions", Namespace = "")]
	public class Sessions
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "transpilation_time", Namespace = "")]
	public class TranspilationTime
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "transpilation_memory_peak", Namespace = "")]
	public class TranspilationMemoryPeak
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "amb_stats", Namespace = "")]
	public class AmbStats
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "kmf_metrics", Namespace = "")]
	public class KmfMetrics
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "sql_stats", Namespace = "")]
	public class SqlStats
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "amb_transactions", Namespace = "")]
	public class AmbTransactions
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "replication", Namespace = "")]
	public class Replication
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "client_network_times", Namespace = "")]
	public class ClientNetworkTimes
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "client_browser_times", Namespace = "")]
	public class ClientBrowserTimes
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "dscy_probe_run", Namespace = "")]
	public class DscyProbeRun
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "dscy_sensor_que", Namespace = "")]
	public class DscySensorQue
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "dscy_sensor_run", Namespace = "")]
	public class DscySensorRun
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "Workload_Recalculation_Monitor", Namespace = "")]
	public class WorkloadRecalculationMonitor
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }

		[XmlElement(ElementName = "hour", Namespace = "")]
		public Hour Hour { get; set; }

		[XmlElement(ElementName = "daily", Namespace = "")]
		public Daily Daily { get; set; }
	}

	[XmlRoot(ElementName = "client-interaction-dynamic", Namespace = "")]
	public class Clientinteractiondynamic
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "time-to-interaction", Namespace = "")]
	public class Timetointeraction
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "cs_stats", Namespace = "")]
	public class CsStats
	{

		[XmlElement(ElementName = "one", Namespace = "")]
		public One One { get; set; }

		[XmlElement(ElementName = "five", Namespace = "")]
		public Five Five { get; set; }

		[XmlElement(ElementName = "fifteen", Namespace = "")]
		public Fifteen Fifteen { get; set; }
	}

	[XmlRoot(ElementName = "servlet.metrics", Namespace = "")]
	public class ServletMetrics
	{

		[XmlElement(ElementName = "transactions", Namespace = "")]
		public Transactions Transactions { get; set; }

		[XmlElement(ElementName = "client_transactions", Namespace = "")]
		public ClientTransactions ClientTransactions { get; set; }

		[XmlElement(ElementName = "processed_transactions", Namespace = "")]
		public ProcessedTransactions ProcessedTransactions { get; set; }

		[XmlElement(ElementName = "db_connections", Namespace = "")]
		public DbConnections DbConnections { get; set; }

		[XmlElement(ElementName = "semaphores", Namespace = "")]
		public Semaphores Semaphores { get; set; }

		[XmlElement(ElementName = "semaphore_waiters", Namespace = "")]
		public SemaphoreWaiters SemaphoreWaiters { get; set; }

		[XmlElement(ElementName = "session_waiters", Namespace = "")]
		public SessionWaiters SessionWaiters { get; set; }

		[XmlElement(ElementName = "uri_transactions", Namespace = "")]
		public UriTransactions UriTransactions { get; set; }

		[XmlElement(ElementName = "semaphore_amb_receive_rejected", Namespace = "")]
		public SemaphoreAmbReceiveRejected SemaphoreAmbReceiveRejected { get; set; }

		[XmlElement(ElementName = "semaphore_amb_send_rejected", Namespace = "")]
		public SemaphoreAmbSendRejected SemaphoreAmbSendRejected { get; set; }

		[XmlElement(ElementName = "semaphore_api_int_rejected", Namespace = "")]
		public SemaphoreApiIntRejected SemaphoreApiIntRejected { get; set; }

		[XmlElement(ElementName = "semaphore_debug_rejected", Namespace = "")]
		public SemaphoreDebugRejected SemaphoreDebugRejected { get; set; }

		[XmlElement(ElementName = "semaphore_default_rejected", Namespace = "")]
		public SemaphoreDefaultRejected SemaphoreDefaultRejected { get; set; }

		[XmlElement(ElementName = "semaphore_presence_rejected", Namespace = "")]
		public SemaphorePresenceRejected SemaphorePresenceRejected { get; set; }

		[XmlElement(ElementName = "semaphore_amb_receive_response_time", Namespace = "")]
		public SemaphoreAmbReceiveResponseTime SemaphoreAmbReceiveResponseTime { get; set; }

		[XmlElement(ElementName = "semaphore_amb_send_response_time", Namespace = "")]
		public SemaphoreAmbSendResponseTime SemaphoreAmbSendResponseTime { get; set; }

		[XmlElement(ElementName = "semaphore_api_int_response_time", Namespace = "")]
		public SemaphoreApiIntResponseTime SemaphoreApiIntResponseTime { get; set; }

		[XmlElement(ElementName = "semaphore_default_response_time", Namespace = "")]
		public SemaphoreDefaultResponseTime SemaphoreDefaultResponseTime { get; set; }

		[XmlElement(ElementName = "semaphore_debug_response_time", Namespace = "")]
		public SemaphoreDebugResponseTime SemaphoreDebugResponseTime { get; set; }

		[XmlElement(ElementName = "semaphore_presence_response_time", Namespace = "")]
		public SemaphorePresenceResponseTime SemaphorePresenceResponseTime { get; set; }

		[XmlElement(ElementName = "event_logs", Namespace = "")]
		public EventLogs EventLogs { get; set; }

		[XmlElement(ElementName = "events_processed", Namespace = "")]
		public EventsProcessed EventsProcessed { get; set; }

		[XmlElement(ElementName = "logs", Namespace = "")]
		public Logs Logs { get; set; }

		[XmlElement(ElementName = "errors", Namespace = "")]
		public Errors Errors { get; set; }

		[XmlElement(ElementName = "sys_load", Namespace = "")]
		public SysLoad SysLoad { get; set; }

		[XmlElement(ElementName = "threads_business_rules", Namespace = "")]
		public ThreadsBusinessRules ThreadsBusinessRules { get; set; }

		[XmlElement(ElementName = "threads_cpu", Namespace = "")]
		public ThreadsCpu ThreadsCpu { get; set; }

		[XmlElement(ElementName = "threads_db", Namespace = "")]
		public ThreadsDb ThreadsDb { get; set; }

		[XmlElement(ElementName = "threads_network", Namespace = "")]
		public ThreadsNetwork ThreadsNetwork { get; set; }

		[XmlElement(ElementName = "threads_concurrency", Namespace = "")]
		public ThreadsConcurrency ThreadsConcurrency { get; set; }

		[XmlElement(ElementName = "memory", Namespace = "")]
		public Memory Memory { get; set; }

		[XmlElement(ElementName = "memory_total", Namespace = "")]
		public List<MemoryTotal> MemoryTotal { get; set; }

		[XmlElement(ElementName = "memory_max", Namespace = "")]
		public MemoryMax MemoryMax { get; set; }

		[XmlElement(ElementName = "metaspace_max", Namespace = "")]
		public MetaspaceMax MetaspaceMax { get; set; }

		[XmlElement(ElementName = "metaspace_total", Namespace = "")]
		public MetaspaceTotal MetaspaceTotal { get; set; }

		[XmlElement(ElementName = "metaspace_used", Namespace = "")]
		public MetaspaceUsed MetaspaceUsed { get; set; }

		[XmlElement(ElementName = "garbage_collection", Namespace = "")]
		public GarbageCollection GarbageCollection { get; set; }

		[XmlElement(ElementName = "ParNew_collector_count", Namespace = "")]
		public ParNewCollectorCount ParNewCollectorCount { get; set; }

		[XmlElement(ElementName = "ConcurrentMarkSweep_collector_count", Namespace = "")]
		public ConcurrentMarkSweepCollectorCount ConcurrentMarkSweepCollectorCount { get; set; }

		[XmlElement(ElementName = "memory_active", Namespace = "")]
		public MemoryActive MemoryActive { get; set; }

		[XmlElement(ElementName = "memory_cache", Namespace = "")]
		public MemoryCache MemoryCache { get; set; }

		[XmlElement(ElementName = "memory_swap", Namespace = "")]
		public MemorySwap MemorySwap { get; set; }

		[XmlElement(ElementName = "sql_response", Namespace = "")]
		public SqlResponse SqlResponse { get; set; }

		[XmlElement(ElementName = "sql_inserts", Namespace = "")]
		public SqlInserts SqlInserts { get; set; }

		[XmlElement(ElementName = "sql_updates", Namespace = "")]
		public SqlUpdates SqlUpdates { get; set; }

		[XmlElement(ElementName = "sql_deletes", Namespace = "")]
		public SqlDeletes SqlDeletes { get; set; }

		[XmlElement(ElementName = "sql_selects", Namespace = "")]
		public SqlSelects SqlSelects { get; set; }

		[XmlElement(ElementName = "replication_lag", Namespace = "")]
		public ReplicationLag ReplicationLag { get; set; }

		[XmlElement(ElementName = "job_queue", Namespace = "")]
		public JobQueue JobQueue { get; set; }

		[XmlElement(ElementName = "processed_jobs", Namespace = "")]
		public ProcessedJobs ProcessedJobs { get; set; }

		[XmlElement(ElementName = "job_times", Namespace = "")]
		public JobTimes JobTimes { get; set; }

		[XmlElement(ElementName = "sessions", Namespace = "")]
		public Sessions Sessions { get; set; }

		[XmlElement(ElementName = "transpilation_time", Namespace = "")]
		public TranspilationTime TranspilationTime { get; set; }

		[XmlElement(ElementName = "transpilation_memory_peak", Namespace = "")]
		public TranspilationMemoryPeak TranspilationMemoryPeak { get; set; }

		[XmlElement(ElementName = "amb_stats", Namespace = "")]
		public AmbStats AmbStats { get; set; }

		[XmlElement(ElementName = "kmf_metrics", Namespace = "")]
		public KmfMetrics KmfMetrics { get; set; }

		[XmlElement(ElementName = "sql_stats", Namespace = "")]
		public SqlStats SqlStats { get; set; }

		[XmlElement(ElementName = "amb_transactions", Namespace = "")]
		public AmbTransactions AmbTransactions { get; set; }

		[XmlElement(ElementName = "replication", Namespace = "")]
		public Replication Replication { get; set; }

		[XmlElement(ElementName = "client_network_times", Namespace = "")]
		public ClientNetworkTimes ClientNetworkTimes { get; set; }

		[XmlElement(ElementName = "client_browser_times", Namespace = "")]
		public ClientBrowserTimes ClientBrowserTimes { get; set; }

		[XmlElement(ElementName = "dscy_probe_run", Namespace = "")]
		public DscyProbeRun DscyProbeRun { get; set; }

		[XmlElement(ElementName = "dscy_sensor_que", Namespace = "")]
		public DscySensorQue DscySensorQue { get; set; }

		[XmlElement(ElementName = "dscy_sensor_run", Namespace = "")]
		public DscySensorRun DscySensorRun { get; set; }

		[XmlElement(ElementName = "Workload_Recalculation_Monitor", Namespace = "")]
		public WorkloadRecalculationMonitor WorkloadRecalculationMonitor { get; set; }

		[XmlElement(ElementName = "client-interaction-dynamic", Namespace = "")]
		public Clientinteractiondynamic Clientinteractiondynamic { get; set; }

		[XmlElement(ElementName = "time-to-interaction", Namespace = "")]
		public Timetointeraction Timetointeraction { get; set; }

		[XmlElement(ElementName = "cs_stats", Namespace = "")]
		public CsStats CsStats { get; set; }
	}

	[XmlRoot(ElementName = "com.glide.sys.WorkerThreadManager", Namespace = "")]
	public class ComGlideSysWorkerThreadManager
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "com.glide.ui.ServletStatus", Namespace = "")]
	public class ComGlideUiServletStatus
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "encryption_wrapper_listener_storage", Namespace = "")]
	public class EncryptionWrapperListenerStorage
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ais.ha.failover_in_progress", Namespace = "")]
	public class GlideAisHaFailoverInProgress
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.db.archiver", Namespace = "")]
	public class GlideDbArchiver
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.db.name", Namespace = "")]
	public class GlideDbName
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.db.pooler.connections.max", Namespace = "")]
	public class GlideDbPoolerConnectionsMax
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.event_heartbeat.current_age", Namespace = "")]
	public class GlideEventHeartbeatCurrentAge
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public double Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.event_heartbeat.last_beat", Namespace = "")]
	public class GlideEventHeartbeatLastBeat
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.event_heartbeat.last_delay", Namespace = "")]
	public class GlideEventHeartbeatLastDelay
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public double Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.event_heartbeat.last_id", Namespace = "")]
	public class GlideEventHeartbeatLastId
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.event_heartbeat.version", Namespace = "")]
	public class GlideEventHeartbeatVersion
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public double Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a", Namespace = "")]
	public class GlideLdapListener1f82d2b31b90095040be9753b24bcb3a
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.active", Namespace = "")]
	public class GlideLdapListener1f82d2b31b90095040be9753b24bcb3aActive
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.last_change", Namespace = "")]
	public class GlideLdapListener1f82d2b31b90095040be9753b24bcb3aLastChange
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.last_error", Namespace = "")]
	public class GlideLdapListener1f82d2b31b90095040be9753b24bcb3aLastError
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.queue_size", Namespace = "")]
	public class GlideLdapListener1f82d2b31b90095040be9753b24bcb3aQueueSize
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-1f82d2b31b90095040be9753b24bcb3a.shutdown_pending", Namespace = "")]
	public class GlideLdapListener1f82d2b31b90095040be9753b24bcb3aShutdownPending
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0", Namespace = "")]
	public class GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.active", Namespace = "")]
	public class GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0Active
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.last_change", Namespace = "")]
	public class GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0LastChange
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.last_error", Namespace = "")]
	public class GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0LastError
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.queue_size", Namespace = "")]
	public class GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0QueueSize
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-36b8fee11bf4b4d0369a98271d4bcbc0.shutdown_pending", Namespace = "")]
	public class GlideLdapListener36b8fee11bf4b4d0369a98271d4bcbc0ShutdownPending
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4", Namespace = "")]
	public class GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.active", Namespace = "")]
	public class GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4Active
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.last_change", Namespace = "")]
	public class GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4LastChange
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.last_error", Namespace = "")]
	public class GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4LastError
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.queue_size", Namespace = "")]
	public class GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4QueueSize
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-4b4d855a8754c510202f0fe60cbb35e4.shutdown_pending", Namespace = "")]
	public class GlideLdapListener4b4d855a8754c510202f0fe60cbb35e4ShutdownPending
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88", Namespace = "")]
	public class GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.active", Namespace = "")]
	public class GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88Active
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.last_change", Namespace = "")]
	public class GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88LastChange
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.last_error", Namespace = "")]
	public class GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88LastError
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.queue_size", Namespace = "")]
	public class GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88QueueSize
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ldap.listener-e92e64281bf2fc14b2b11f41b24bcb88.shutdown_pending", Namespace = "")]
	public class GlideLdapListenere92e64281bf2fc14b2b11f41b24bcb88ShutdownPending
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.policy.eventdelegator", Namespace = "")]
	public class GlidePolicyEventdelegator
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.pop3.status", Namespace = "")]
	public class GlidePop3Status
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.service.modeling.lastCheckpointsCleanup", Namespace = "")]
	public class GlideServiceModelingLastCheckpointsCleanup
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.service.modeling.lastSweep", Namespace = "")]
	public class GlideServiceModelingLastSweep
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.service.modeling.sweepDuration", Namespace = "")]
	public class GlideServiceModelingSweepDuration
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.servlet.port", Namespace = "")]
	public class GlideServletPort
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.smtp.status", Namespace = "")]
	public class GlideSmtpStatus
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.ui.max.transactions", Namespace = "")]
	public class GlideUiMaxTransactions
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.update_operation.queue.status", Namespace = "")]
	public class GlideUpdateOperationQueueStatus
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "glide.update_operation.queue.upgrade_check", Namespace = "")]
	public class GlideUpdateOperationQueueUpgradeCheck
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "instance_registration", Namespace = "")]
	public class InstanceRegistration
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "license_mutex_status_message", Namespace = "")]
	public class LicenseMutexStatusMessage
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "mid.monitor.heartbeat_sent", Namespace = "")]
	public class MidMonitorHeartbeatSent
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.ais_index_queue_stats", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeAisIndexQueueStats
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.ais_partition_health", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeAisPartitionHealth
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.ais_partition_health_response", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeAisPartitionHealthResponse
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.ais_updatable_field_event", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeAisUpdatableFieldEvent
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.cds_client_staging", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeCdsClientStaging
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.cmdb_ire_partial_payloads", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeCmdbIrePartialPayloads
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.diagnostic_event", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeDiagnosticEvent
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.discovery_device_duplicate_ips", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeDiscoveryDeviceDuplicateIps
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_alert", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeEmAlert
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_alert_insight_state", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeEmAlertInsightState
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_alert_trigger_queue", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeEmAlertTriggerQueue
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_ci_graph_reuse", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeEmCiGraphReuse
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_connected_services", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeEmConnectedServices
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_extra_data_json", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeEmExtraDataJson
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_impacted_ci", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeEmImpactedCi
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.em_impact_graph", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeEmImpactGraph
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.jrobin_database", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeJrobinDatabase
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.ml_update_set", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeMlUpdateSet
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.multisso_request_parameter", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeMultissoRequestParameter
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.pa_rw_aggregate", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizePaRwAggregate
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.qb_query_status", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeQbQueryStatus
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sa_agg_pattern_alert", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSaAggPatternAlert
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sa_debug_session", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSaDebugSession
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sa_debug_session_status", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSaDebugSessionStatus
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sa_paged_payload", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSaPagedPayload
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sm_ci_field_data", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSmCiFieldData
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sm_flapper_strategy_data", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSmFlapperStrategyData
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sn_cmp_cloud_event", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSnCmpCloudEvent
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.svc_changes", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSvcChanges
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_atf_test_result", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysAtfTestResult
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_atf_test_suite_result", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysAtfTestSuiteResult
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_ci_analytics", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysCiAnalytics
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_cluster_message", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysClusterMessage
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_coalesce_strategy_deferred", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysCoalesceStrategyDeferred
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_cs_ca_message", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysCsCaMessage
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_cs_consumer", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysCsConsumer
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_execution_tracker", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysExecutionTracker
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_flow_context", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysFlowContext
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_geocoding_request", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysGeocodingRequest
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_hub_popular_artifacts", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysHubPopularArtifacts
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_import_set_row_error", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysImportSetRowError
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_object_source", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysObjectSource
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_poll", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysPoll
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_rate_limit_count", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysRateLimitCount
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_script_execution_history", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysScriptExecutionHistory
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.sys_transform_target_row", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeSysTransformTargetRow
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.ui_notification_inbox", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeUiNotificationInbox
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "no_optimize_write_audit.new_value.no_optimize.wf_workflow_version", Namespace = "")]
	public class NoOptimizeWriteAuditNewValueNoOptimizeWfWorkflowVersion
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public bool Text { get; set; }
	}

	[XmlRoot(ElementName = "system.build.date", Namespace = "")]
	public class SystemBuildDate
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.hostname", Namespace = "")]
	public class SystemHostname
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.java.home", Namespace = "")]
	public class SystemJavaHome
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.java.version", Namespace = "")]
	public class SystemJavaVersion
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.memory.max", Namespace = "")]
	public class SystemMemoryMax
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public double Text { get; set; }
	}

	[XmlRoot(ElementName = "system.mysqld.version", Namespace = "")]
	public class SystemMysqldVersion
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.os.arch", Namespace = "")]
	public class SystemOsArch
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.os.name", Namespace = "")]
	public class SystemOsName
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.os.version", Namespace = "")]
	public class SystemOsVersion
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.session.timeout", Namespace = "")]
	public class SystemSessionTimeout
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "system.startup", Namespace = "")]
	public class SystemStartup
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "system.status", Namespace = "")]
	public class SystemStatus
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "update_mutex_status_message", Namespace = "")]
	public class UpdateMutexStatusMessage
	{

		[XmlAttribute(AttributeName = "type", Namespace = "")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "gc", Namespace = "")]
	public class Gc
	{

		[XmlElement(ElementName = "name", Namespace = "")]
		public string Name { get; set; }

		[XmlElement(ElementName = "run_count", Namespace = "")]
		public int RunCount { get; set; }

		[XmlElement(ElementName = "run_count_per_fivemin", Namespace = "")]
		public string RunCountPerFivemin { get; set; }

		[XmlElement(ElementName = "run_time", Namespace = "")]
		public int RunTime { get; set; }

		[XmlElement(ElementName = "run_time_readable", Namespace = "")]
		public string RunTimeReadable { get; set; }

		[XmlElement(ElementName = "avg_run_time", Namespace = "")]
		public double AvgRunTime { get; set; }
	}

	[XmlRoot(ElementName = "jvm.gc", Namespace = "")]
	public class JvmGc
	{

		[XmlElement(ElementName = "gc", Namespace = "")]
		public List<Gc> Gc { get; set; }
	}

}
