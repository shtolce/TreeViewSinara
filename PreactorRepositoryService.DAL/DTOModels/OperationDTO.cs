using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    public class OperationDTO
    {
        public string ParentPart { get; set; }
        public string BelongsToOrder { get; set; }
        public int Id { get; set;}
        public string OrderNo { get; set; }
        public int OpNo { get; set; }
        public string OpName { get; set; }
        public double Quantity { get; set; }
        public double RequiredQuantity { get; set; }
        public string PartNo { get; set; }
        public string RequiredPartNo { get; set; }
        public bool MultiplyByOrderQuantity { get; set; }
        public double MultipleQuantity { get; set; }
        public string BelongsToBOM { get; set; }
        public string OrderPartNo { get; set; }
        public bool IgnoreShortage { get; set; }
        public string ResourceGroup { get; set; }
        public string Resource { get; set; }
        public DateTime SetupStart { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime EarliestStartDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool ToggleAttribute1 { get; set; }
        public string PartName { get; set; }
        public string RequiredPartName { get; set; }

    }
}
