using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class TrafficCase
    {
        public string Id { get; set; }

        [Display(Name = "Date of Infraction")]
        [DataType(DataType.Date)]
        [Required]
        public DateTime? Date { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Violation { get; set; }

        [Required]
        public string License { get; set; }

        [Required]
        public string Status { get; set; }
       
    }
}
