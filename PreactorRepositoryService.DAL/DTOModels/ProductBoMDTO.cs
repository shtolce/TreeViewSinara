using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
//Product Bill of Materials
//Belongs to BOM  (Родитель или номер детали)
//Part No.
//Operation Name
//Op.No.
//Required Part No.
//Required Quantity
//Multiply by order quantity(bool)
//Multiple Quantity
    public class ProductBoMDTO
    {
        public int Id { get; set;}
        public int OpNo { get; set; }
        public string OpName { get; set; }
        public string PartNo { get; set; }
        public double RequiredQuantity { get; set; }
        public string RequiredPartNo { get; set; }
        public bool MultiplyByOrderQuantity { get; set; }
        public double MultipleQuantity { get; set; }
        public string BelongsToBOM { get; set; }
        public string PartName { get; set; }
        public string RequiredPartName { get; set; }

        public int OpCount
        {
            get
            {
                if (Operations == null) Operations = new List<OperationDTO>();
                return (Operations == null) ? 0 : Operations.Count;
            }
        }

        public List<OperationDTO> Operations { get; set; }

    }
}
