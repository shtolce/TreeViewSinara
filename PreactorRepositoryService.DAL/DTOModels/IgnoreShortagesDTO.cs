using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    public class IgnoreShortagesDTOItem
    {
        public int Id { get; set;}
        public int IgnoreShortages { get; set; }
        public int ExternalDemandOrder { get; set; }
        public int InternalDemandOrder { get; set; }
        public string PartNo { get; set; }
        public string DatasetId { get; set; }
        public string __seq__Shortages { get; set; }
    }

    public class IgnoreShortagesDTO
    {
        public string PartNo { get; set; }
        public int ExternalDemandOrder { get; set; }
        public int InternalDemandOrder { get; set; }
        public List<IgnoreShortagesDTOItem> items { get; set; }

    }

}
