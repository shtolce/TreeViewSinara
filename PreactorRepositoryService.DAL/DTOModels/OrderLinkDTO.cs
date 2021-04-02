using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{

    public class OrderLinkDTO
    {
        public int Id { get; set; }
        public int FromExternalSupplyOrder { get; set; }
        public int FromInternalSupplyOrder { get; set; }
        public int ToExternalDemandOrder { get; set; }
        public int ToInternalDemandOrder { get; set; }
        public string PartNo { get; set; }
        public double Quantity { get; set; }
        public string PeggingRuleUsed { get; set; }
        public bool Locked { get; set; }
    }
}
