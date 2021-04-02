using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    public class ShortagesDTOItem
    {
        public int Id { get; set;}
        public int ExternalDemandOrder { get; set; }
        public int InternalDemandOrder { get; set; }
        public string PartNo { get; set; }
        public double ShortageQuantity { get; set; }
        public string DatasetId { get; set; }
        public string __seq__Shortages { get; set; }
    }

    public class ShortagesDTO
    {
        public string PartNo { get; set; }
        public int ExternalDemandOrder { get; set; }
        public int InternalDemandOrder { get; set; }
        public double ShortageQuantity { get; set; }
        public List<ShortagesDTOItem> items { get; set; }
    }


}
