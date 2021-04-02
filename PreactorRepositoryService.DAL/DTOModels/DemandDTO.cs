using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    public class DemandDTO
    {
        public string BelongsToOrderNo { get; set; }
        public int Id { get; set; }
        public string OrderNo { get; set; }
        public int OrderLine { get; set; }
        public double Quantity { get; set; }
        public string PartNo { get; set; }
        public string Owner { get; set; }
        public string OwnerNo { get; set; }
        public DateTime DemandDate { get; set; }
        public double Priority { get; set; }
        public string StringAttribute1 { get; set; }
        public string StringAttribute2 { get; set; }
        public string StringAttribute3 { get; set; }
        public string StringAttribute4 { get; set; }
        public int MultipleQuantity { get; set; }
        public int DatasetId { get; set; }
        public int __seq__Demand { get; set; }
        public string OrderType { get; set; }
        public int Davalec { get; set; }
        public string Description { get; set; }
        public DateTime DateMonthRO2 { get; set; }
        public string DemandState { get; set; }
        public string OrderNo1c { get; set; }
        public string StringNo1c { get; set; }
        public string PartNoOld { get; set; }
    }
}
