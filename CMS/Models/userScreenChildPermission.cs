//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CMS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class userScreenChildPermission
    {
        public int id { get; set; }
        public Nullable<int> screen_id { get; set; }
        public Nullable<bool> add { get; set; }
        public Nullable<bool> edit { get; set; }
        public Nullable<bool> delete { get; set; }
    }
}
