﻿using System.ComponentModel.DataAnnotations;

namespace TMD.Web.Models
{
    public class ProductCategoryModel
    {
        public long CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime RecCreatedDate { get; set; }
        public System.DateTime RecLastUpdatedDate { get; set; }
        public string RecCreatedBy { get; set; }
        public string RecLastUpdatedBy { get; set; }
    }
}