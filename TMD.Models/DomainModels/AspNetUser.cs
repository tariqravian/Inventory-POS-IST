﻿using System;
using System.Collections.Generic;

namespace TMD.Models.DomainModels
{
    public partial class AspNetUser
    {
        public AspNetUser()
        {
            AspNetUserClaims = new HashSet<AspNetUserClaim>();
            AspNetUserLogins = new HashSet<AspNetUserLogin>();
            AspNetRoles = new HashSet<AspNetRole>();
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }
        public string ImageName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Qualification { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }

        public string RegisterPayPalTxnID { get; set; }
        public DateTime ? RegisterPayPalDate { get; set; }
        public double ? PayPalAmount { get; set; }
        public double ? PayPalAmountAfterDeduct { get; set; }
        public string PayPalMisc { get; set; }
        
        public int ? Package { get; set; }
        
            
        public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
         public virtual ICollection<AspNetRole> AspNetRoles { get; set; }
         
        public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual ICollection<UserPrefrence> UserPrefrences { get; set; }

        public virtual ICollection<StagingEbayBatchImport> StagingEbayBatchesImportCreatedBy { get; set; }
        public virtual ICollection<StagingEbayBatchImport> StagingEbayBatchesImportDeletedBy { get; set; }

        public virtual ICollection<StagingEbayItem> StagingEbayItemsCreatedBy { get; set; }
        public virtual ICollection<StagingEbayItem> StagingEbayItemsDeletedBy { get; set; }
        public virtual ICollection<StagingEbayItem> StagingEbayItemsModifiedBy { get; set; }

    }
}
