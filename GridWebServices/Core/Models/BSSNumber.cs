using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public  class BSSNumbers
    {
        public BSSNumbers()
        {
            PremiumNumbers = new List<PremiumNumbers>();
        }
        public List<FreeNumber> FreeNumbers { get; set; }

        public List<PremiumNumbers> PremiumNumbers { get; set; }
    }

    public class FreeNumber
    {
        public string MobileNumber { get; set; }
        public string ServiceCode { get; set; }
    }

    public class PremiumNumbers
    {
        public string MobileNumber { get; set; }

        public double Price { get; set; }

        public string PortalServiceName { get; set; }

        public int ServiceCode { get; set; }

    }

    public class ServiceFees
    {
        public string PortalServiceName { get; set; }
        public int ServiceCode { get; set; }
        public double ServiceFee { get; set; }

    }

    public class NewNumber
    {
        [MaxLength(8, ErrorMessage = "Maximum 8 characters allowed")]
        [Required(ErrorMessage = "New MobileNumber is required")]
        public string MobileNumber { get; set; }

        public int ServiceCode { get; set; }
    }
}
