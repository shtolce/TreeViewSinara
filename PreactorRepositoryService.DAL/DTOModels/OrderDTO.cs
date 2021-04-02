using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    [DebuggerDisplay("OrderNo:{OrderNo},OpNo:{OpNo},ВРЦ{ResourceGroup},Партно{Product} ранги  {TableAttribute1Rank} , {TableAttribute2Rank} }")]
    public class OrderDTO
    {
        public int BelongsToOrderNo { get; set; }
        public int Id { get; set;}
        public string OrderNo { get; set; }
        public int OpNo { get; set; }
        public string OpName { get; set; }
        public double Quantity { get; set; }
        public int Priority { get; set; }
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
        public int ResourceId { get; set; }
        public string PartNo { get; set; }
        public List<OperationDTO> Operations { get; set; }
        public DateTime SetupStart { get; set; }
        public int SetupTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime EarliestStartDate { get; set; }
        public DateTime DueDate { get; set; }
        //-доп аттрибуты
        public double NumericalAttribute1 { get; set; }
        public double NumericalAttribute2 { get; set; }
        public double NumericalAttribute3 { get; set; }
        public double NumericalAttribute4 { get; set; }
        public double NumericalAttribute5 { get; set; }
        public double NumericalAttribute15 { get; set; }
        public string TableAttribute1 { get; set; }
        public string TableAttribute2 { get; set; }
        public string TableAttribute3 { get; set; }
        public int TableAttribute1Rank { get; set; }
        public int TableAttribute2Rank { get; set; }
        public int TableAttribute3Rank { get; set; }
        public string StringAttribute1 { get; set; }
        public string StringAttribute2 { get; set; }
        public string StringAttribute3 { get; set; }
        public string ResSpecType { get; set; }



        public bool Hold { get; set; }
        public bool Scheduled { get; set; }
        public int LotNumber { get; set; }


    }
}
