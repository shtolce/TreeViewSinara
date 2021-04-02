using PreactorRepositoryService.DAL.DTOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.NetGraphModels
{
    public class OperationResourceEdge
    {
        OperationDTO Operation { get; set; }
        OrderDTO Order { get; set; }
        public List<OperationDTO> Predecessors { get; set; }
        public List<OperationDTO> Successors { get; set; }
        ResourceGroupDTO ResourceGroup { get; set; }
        ResourceDTO Resource { get; set; }
        double ResourceUsageCount { get; set; }

    }
}
