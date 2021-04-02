using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;

namespace PreactorRepositoryService.DAL.DTOModels
{

    public class SecondaryConstraintDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool UseAsAConstraint { get; set; }
        public string CalendarEffect { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public DateTime ReferenceDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double Dim { get; set; }
        
    }
}
