using NLog;
using Preactor;
using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.Repository
{
    public class OrderLinkRepo:IRepository<OrderLinkDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Order Links";
        public OrderLinkRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса OrderLinkRepo");
            this._preactor = preactor;
        }
        /// <summary>
        /// Создает элемент OrderLink в преакторе
        /// </summary>
        /// <param name="obj">Передайте объект типа OrderLinkDTO, подчиненые строки поместите в Operations</param>
        /// <returns></returns>
        public bool Create(OrderLinkDTO obj)
        {

            int selectedParameters = 1;
            OrderLinkDTO foundOl=null;
            if (obj.FromExternalSupplyOrder > 0 & obj.ToInternalDemandOrder> 0)
                selectedParameters = 2;
            if (selectedParameters == 1)
                foundOl = GetByNo(obj.FromInternalSupplyOrder, obj.ToExternalDemandOrder);
            else
                foundOl = GetByNo(0, 0, obj.FromExternalSupplyOrder, obj.ToInternalDemandOrder);


            if (foundOl != null)
            {
                logger.Trace($"Экземпляр Order Links уже существует {obj.Id}");
                return false;
            }

            try
            {
                int orderLinkRecNo = _preactor.CreateRecord(_entityName);
                //_preactor.WriteField(_entityName, "Number", orderLinkRecNo, obj.Id);
                _preactor.WriteField(_entityName, "Part No.", orderLinkRecNo, obj.PartNo);
                _preactor.WriteField(_entityName, "From External Supply Order", orderLinkRecNo, obj.FromExternalSupplyOrder);
                _preactor.WriteField(_entityName, "From Internal Supply Order", orderLinkRecNo, obj.FromInternalSupplyOrder);
                _preactor.WriteField(_entityName, "To External Demand Order", orderLinkRecNo, obj.ToExternalDemandOrder);
                _preactor.WriteField(_entityName, "To Internal Demand Order", orderLinkRecNo, obj.ToInternalDemandOrder);
                _preactor.WriteField(_entityName, "Quantity", orderLinkRecNo, obj.Quantity);
                _preactor.WriteField(_entityName, "Pegging Rule Used", orderLinkRecNo, obj.PeggingRuleUsed);
                _preactor.WriteField(_entityName, "Locked", orderLinkRecNo, obj.Locked);
                logger.Trace($"Создан экземпляр продукта {obj.Id}");
                return true;
            }
            catch
            {
                logger.Trace($"Ошибка создания экземпляра продукта  {obj.Id}");
            }
            return false;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName, new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр OrderLinks успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра OrderLinks {ex.Message} {id}");

            }
        }
        /// <summary>
        /// Получает список всех продуктов в преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderLinkDTO> GetAll()
        {
            logger.Trace($"Получение списка orderlinks начато");
            int recCount = _preactor.RecordCount(_entityName);
            string ordLinksId = "-1";
            List<OrderLinkDTO> res = new List<OrderLinkDTO>();
            OrderLinkDTO foundOrderLink = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    ordLinksId = _preactor.ReadFieldString(_entityName, "Number", i);
                    foundOrderLink = GetByNo(ordLinksId);
                    res.Add(foundOrderLink);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение order links по всем записям) " + ex.TargetSite.Name);
                }
            }
            logger.Trace($"Получение списка продуктов закончено");
            return res;
        }
        /// <summary>
        /// Получает OrderLink по Id в преакторе, поле  OrderLink
        /// </summary>
        /// <param name="OrderLinkId">Поле Id OrderLinks в преакторе</param>
        /// <returns></returns>
        public OrderLinkDTO GetByNo(string orderLinkId)
        {
            logger.Trace($"Получение продукта по номеру начато {orderLinkId}");
            string requestStr = $"~{{$Number}}~==~{orderLinkId}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            if (recordNo < 0)
                return null;

            int id = 0;
            string partNo = "";
            int fromExternalSupplyOrder = 0;
            int fromInternalSupplyOrder = 0;
            int toExternalDemandOrder = 0;
            int toInternalDemandOrder = 0;
            double quantity = 0;
            string peggingRuleUsed = "";
            bool   locked = default(bool);


            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                fromExternalSupplyOrder = _preactor.ReadFieldInt(_entityName, "From External Supply Order", recordNo);
                fromInternalSupplyOrder = _preactor.ReadFieldInt(_entityName, "From Internal Supply Order", recordNo);
                toExternalDemandOrder = _preactor.ReadFieldInt(_entityName, "To External Demand Order", recordNo);
                toInternalDemandOrder = _preactor.ReadFieldInt(_entityName, "To Internal Demand Order", recordNo);
                quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                peggingRuleUsed = _preactor.ReadFieldString(_entityName, "Pegging Rule Used", recordNo);
                locked = _preactor.ReadFieldBool(_entityName, "Locked", recordNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Order Links" + orderLinkId);
            }
            OrderLinkDTO result = new OrderLinkDTO()
            {
                Id = id,
                PartNo = partNo,
                FromExternalSupplyOrder = fromExternalSupplyOrder,
                FromInternalSupplyOrder = fromInternalSupplyOrder,
                ToExternalDemandOrder = toExternalDemandOrder,
                ToInternalDemandOrder = toInternalDemandOrder,
                PeggingRuleUsed = peggingRuleUsed,
                Quantity = quantity,
                Locked = locked
            };
            logger.Trace($"Получение OrderLinks по номеру завершено {orderLinkId}");
            return result;
        }
        /// <summary>
        /// Получает OrdeLinks  по параметрам 
        /// </summary>
        /// <param name="fromIntSupOrd"></param>
        /// <param name="toExtDemOrder"></param>
        /// <param name="fromExtSupOrder"></param>
        /// <param name="toIntDemOrder"></param>
        /// <returns></returns>
        public OrderLinkDTO GetByNo(int fromIntSupOrd=0,int toExtDemOrder=0,int fromExtSupOrder=0, int toIntDemOrder=0)
        {
            string requestStr1 = $"({{#From Internal Supply Order}}=={fromIntSupOrd}) && ({{#To External Demand Order}}=={toExtDemOrder})";
            string requestStr2 = $"({{#From External Supply Order}}=={fromExtSupOrder}) && ({{#To Internal Demand Order}}=={toIntDemOrder})";
            int selectedParameters = 1;
            if (fromExtSupOrder>0 & toIntDemOrder>0)
                selectedParameters = 2;
            if (selectedParameters == 1)
                logger.Trace($"Получение продукта по параметрам начато from int sup. order {fromIntSupOrd} to ext. demand order {toExtDemOrder}");
            else
                logger.Trace($"Получение продукта по параметрам начато from ext sup. order {fromExtSupOrder} to int. demand order {toIntDemOrder}");


            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, selectedParameters==1?requestStr1: requestStr2);
            if (recordNo < 0)
                return null;

            int id = 0;
            string partNo = "";
            int fromExternalSupplyOrder = 0;
            int fromInternalSupplyOrder = 0;
            int toExternalDemandOrder = 0;
            int toInternalDemandOrder = 0;
            double quantity = 0;
            string peggingRuleUsed = "";
            bool locked = default(bool);
            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                fromExternalSupplyOrder = _preactor.ReadFieldInt(_entityName, "From External Supply Order", recordNo);
                fromInternalSupplyOrder = _preactor.ReadFieldInt(_entityName, "From Internal Supply Order", recordNo);
                toExternalDemandOrder = _preactor.ReadFieldInt(_entityName, "To External Demand Order", recordNo);
                toInternalDemandOrder = _preactor.ReadFieldInt(_entityName, "To Internal Demand Order", recordNo);
                quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                peggingRuleUsed = _preactor.ReadFieldString(_entityName, "Pegging Rule Used", recordNo);
                locked = _preactor.ReadFieldBool(_entityName, "Locked", recordNo);
            }
            catch (Exception ex)
            {
                if (selectedParameters == 1)
                    logger.Trace($"{ex.Message} по параметрам начато from int sup. order {fromIntSupOrd} to ext. demand order {toExtDemOrder}");
                else
                    logger.Trace($"{ex.Message} по параметрам начато from ext sup. order {fromExtSupOrder} to int. demand order {toIntDemOrder}");
            }
            OrderLinkDTO result = new OrderLinkDTO()
            {
                Id = id,
                PartNo = partNo,
                FromExternalSupplyOrder = fromExternalSupplyOrder,
                FromInternalSupplyOrder = fromInternalSupplyOrder,
                ToExternalDemandOrder = toExternalDemandOrder,
                ToInternalDemandOrder = toInternalDemandOrder,
                PeggingRuleUsed = peggingRuleUsed,
                Quantity = quantity,
                Locked = locked
            };
            logger.Trace($"Получение OrderLinks по номеру завершено");
            return result;
        }

        public void Update(OrderLinkDTO obj)
        {
            OrderLinkDTO foundRes = GetByNo(obj.FromInternalSupplyOrder,obj.ToExternalDemandOrder,obj.FromExternalSupplyOrder,obj.ToInternalDemandOrder);
            if (foundRes == null)
            {
                logger.Trace($@"Экземпляр Resource не найден, нечего обновлять fiso{obj.FromInternalSupplyOrder} 
                             tedo{obj.ToExternalDemandOrder} feso{obj.FromExternalSupplyOrder} tido{ obj.ToInternalDemandOrder} ");
                return;
            }
            int resourceRecNo = foundRes.Id;
            try
            {
                Delete(foundRes.Id);
                if (Create(obj) == true)
                    logger.Trace($"Обновлен экземпляр Resource {obj.Id}");
                return;
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра Resource {ex.Message} {obj.Id}");
            }
            return;
        }
        ~OrderLinkRepo()
        {
            LogManager.Flush();
        }

    }
}
