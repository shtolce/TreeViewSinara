using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.NetGraphModels
{
    public class EventVertex
    {
        public double Duration { get; set; }
        public double Optimistic { get; set; }
        public double Pessimistic { get; set; }
        public double Mostlikely { get; set; }
        public string Name { get; }
        public List<OperationResourceEdge> Edges { get; }

        public void AddEdge(OperationResourceEdge newEdge)
        {
            Edges.Add(newEdge);
        }

        public override string ToString() => Name;
    }





}
