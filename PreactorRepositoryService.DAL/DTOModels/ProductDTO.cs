using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    public class ProductDTO
    {
        public int Id { get; set;}
        public int OpNo { get; set; }
        public string OpName { get; set; }
        public string Product {get;set;}
        public string ResourceGroup { get; set; }
        public string Resource { get; set; }
        public string PartNo { get; set; }
        public double NumericalAttribute1 { get; set; }
        public double NumericalAttribute2 { get; set; }
        public double NumericalAttribute3 { get; set; }

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
