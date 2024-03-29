//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EMarketingApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public partial class tbl_product
    {
        public int pro_id { get; set; }
        [DisplayName("Product Name")]
        public string pro_name { get; set; }
        [DisplayName("Product Image")]
        public string pro_image { get; set; }
        [DisplayName("Product Description")]
        public string pro_des { get; set; }
        [DisplayName("Product Price")]
        public Nullable<int> pro_price { get; set; }
        public Nullable<int> pro_fk_cat { get; set; }
        public Nullable<int> pro_fk_user { get; set; }
        [DisplayName("Created On")]
        public Nullable<System.DateTime> pro_created { get; set; }
        [DisplayName("Updated On")]
        public Nullable<System.DateTime> pro_updated { get; set; }
        [DisplayName("Product Category")]
        public virtual tbl_category tbl_category { get; set; }
        [DisplayName("Product User")]
        public virtual tbl_user tbl_user { get; set; }
    }
}
