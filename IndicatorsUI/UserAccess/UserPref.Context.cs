﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IndicatorsUI.UserAccess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class FINGERTIPS_USERSEntities : DbContext
    {
        public FINGERTIPS_USERSEntities()
            : base("name=FINGERTIPS_USERSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<IndicatorList> IndicatorLists { get; set; }
        public virtual DbSet<IndicatorListItem> IndicatorListItems { get; set; }
    }
}