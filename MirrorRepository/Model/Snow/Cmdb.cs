using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MirrorRepository.Model.Snow
{
    //[Table("cmdb")]
    public partial class Cmdb
    {
        [Key]
        [Column(name:"sys_id")]
        public string SysId { get; set; }
        public string Asset { get; set; }
        public string AssetTag { get; set; }
        public DateTime? Assigned { get; set; }
        public string AssignedTo { get; set; }
        public string AssignmentGroup { get; set; }
        public DateTime? CheckedIn { get; set; }
        public DateTime? CheckedOut { get; set; }
        public string Company { get; set; }
        public float? Cost { get; set; }
        public string CostCc { get; set; }
        public string CostCenter { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Department { get; set; }
        public DateTime? Due { get; set; }
        public string DueIn { get; set; }
        public string GlAccount { get; set; }
        public DateTime? InstallDate { get; set; }
        public long? InstallStatus { get; set; }
        public string InvoiceNumber { get; set; }
        public string Justification { get; set; }
        public string LeaseId { get; set; }
        public string Location { get; set; }
        public string ManagedBy { get; set; }
        public string Manufacturer { get; set; }
        public string ModelId { get; set; }
        public string Name { get; set; }
        public DateTime? OrderDate { get; set; }
        public string OwnedBy { get; set; }
        public string PoNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string SerialNumber { get; set; }
        public bool? SkipSync { get; set; }
        public string SupportedBy { get; set; }
        public string SupportGroup { get; set; }
        [Column(name: "sys_class_name")]
        public string SysClassName { get; set; }
        [Column(name: "sys_class_path")]
        public string SysClassPath { get; set; }
        [Column(name: "sys_created_by")]
        public string SysCreatedBy { get; set; }
        [Column(name: "sys_created_on")]
        public DateTime? SysCreatedOn { get; set; }
        [Column(name: "sys_domain")]
        public string SysDomain { get; set; }
        [Column(name: "sys_domain_path")]
        public string SysDomainPath { get; set; }
        [Column(name: "sys_mod_count")]
        public long? SysModCount { get; set; }
        [Column(name: "sys_updated_by")]
        public string SysUpdatedBy { get; set; }
        [Column(name: "sys_updated_on")]
        public DateTime? SysUpdatedOn { get; set; }
        public bool? Unverified { get; set; }
        public string Vendor { get; set; }
        public DateTime? WarrantyExpiration { get; set; }
        public DateTime? SnowdbsyncCreated { get; set; }
        public DateTime? SnowdbsyncUpdated { get; set; }
    }
}
