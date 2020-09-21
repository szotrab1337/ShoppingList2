using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestApi.Model
{
    public class SqlShop
    {
        [Key]
        public int ShopID { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
