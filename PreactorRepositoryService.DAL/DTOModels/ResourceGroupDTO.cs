using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    public class ResourceGroupDTO
    {
        public int Id { get; set;}
        public string Name { get; set; }
        public List<ResourceDTO> Resources { get; set; }

    }
}
