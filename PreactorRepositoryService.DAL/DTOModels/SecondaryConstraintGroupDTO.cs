using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{

    public class SecondaryConstraintGroupDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SecondaryConstraintDTO> SecondaryConstraints{ get; set; }
    }
}
