using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    public class BoMDTO
    {
        public int Id { get; set;}
        public string OrderNo { get; set; }
        public int OpNo { get; set; }
        public string OpName { get; set; }
        public double Quantity { get; set; }
        public int OpCount
        {
            get
            {
                if (Operations == null) Operations = new List<OperationDTO>();
                return (Operations == null)?0:Operations.Count;
            }
        }
        public string Product {get;set;}
        public string ResourceGroup { get; set; }
        public string Resource { get; set; }
        public string PartNo { get; set; }
        public double RequiredQuantity { get; set; }
        public string RequiredPartNo { get; set; }
        public bool MultiplyByOrderQuantity { get; set; }
        public double MultipleQuantity { get; set; }
        public string BelongsToBOM { get; set; }
        public string OrderPartNo { get; set; }
        public bool IgnoreShortage { get; set; }
        public List<OperationDTO> Operations { get; set; }
    }
}
