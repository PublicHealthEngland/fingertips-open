//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PholioVisualisation.UserData
{
    using System;
    using System.Collections.Generic;
    
    public partial class AreaListAreaCode
    {
        public int Id { get; set; }
        public int AreaListId { get; set; }
        public string AreaCode { get; set; }
    
        public virtual AreaList AreaList { get; set; }
    }
}
