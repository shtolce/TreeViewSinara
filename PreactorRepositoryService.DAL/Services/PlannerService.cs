using Preactor;
using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.Services
{
    public class PlannerService
    {
        private IPreactor _preactor;
        OrderRepo ordRepo;
        ResourceRepo resRepo;
        private IPlanningBoard _planningBoard;


        /// <summary>
        /// Проверяет, была ли операция выделена и подсвечена в редакторе
        /// </summary>
        /// <param name="resId"></param>
        /// <returns></returns>
        public bool IsHighlighted(int opId)
        {
            int recNum = _preactor.GetRecordNumber("Resources", new PrimaryKey(opId));
            return _planningBoard.GetOperationLocateState(recNum);
        }

        public PlannerService(IPreactor preactor)
        {
            this._preactor = preactor;
            _planningBoard = _preactor.PlanningBoard;
            ordRepo = new OrderRepo(_preactor);
            resRepo = new ResourceRepo(_preactor);
        }
        /// <summary>
        /// Тестирует и определяет операцию на свободный допустимый рессурс
        /// прямым и обратным методом, т.е. прямым - по заданой дате начала, или обратным - по дате окончания
        /// </summary>
        /// <param name="orderNo">Номер заказа</param>
        /// <param name="knownDate">дата начала или окончания(например начало след. операции) в зависимости от метода</param>
        /// <param name="method">Перечисление направления поиска</param>
        /// <returns></returns>
        public bool ScheduleByOrderNo(string orderNo, ref DateTime knownDate, SearchDirection method)
        {
            OrderDTO orderList = ordRepo.GetByNo(orderNo);
            _preactor.RunEventScript("SMC");
            int? resId = resRepo.GetByNo(orderList.Resource)?.Id;
            foreach (OperationDTO ord in orderList.Operations)
            {
                resId = _preactor.PlanningBoard.FindResources(ord.Id).ElementAt(0);

                OperationTimes? times;
                if (method == SearchDirection.Backwards)
                    times = _planningBoard.BackTestOpOnResource(ord.Id, (int)resId, knownDate);
                else
                    times = _planningBoard.TestOperationOnResource(ord.Id, (int)resId, knownDate);

                if (!times.HasValue)
                    return false;

                //knownDate = times.Value.ChangeStart;
                _planningBoard.PutOperationOnResource(ord.Id, (int)resId, knownDate);
                knownDate = _planningBoard.GetOperationTimes(ord.Id).Value.OperationTimes.ProcessEnd;
            }
            return true;
        }

        /// <summary>
        /// Находит время , доступное для размещения операции на ресурсе, исходя из SetupStart,
        /// в качестве верхнего порога даты.(Прямой метод поиска)
        /// </summary>
        /// <param name="operation">Объект OperationDTO</param>
        /// <param name="resource">Объект ResourceDTO</param>
        /// <returns>Найденное время на ресурсе или null, если такого места нет</returns>
        public OperationTimes? FindFreeStartTimeOnResourceForward(OperationDTO operation,ResourceDTO resource)
        {
            DateTime startTimeTreshold = operation.SetupStart;
            OperationTimes? testTime = _preactor.PlanningBoard.TestOperationOnResource(operation.Id, resource.Id, startTimeTreshold);
            if (!testTime.HasValue)
                return null;
            return testTime;
        }



        //--------------------------------------------------------------------------------------
        //Ниже идут функции в который OrderDTO не использует коллекцию Operations, один обьект одна операция
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Тестирует и определяет операцию на свободный допустимый рессурс
        /// прямым и обратным методом, т.е. прямым - по заданой дате начала, или обратным - по дате окончания
        /// </summary>
        /// <param name="orderId">Id заказа</param>
        /// <param name="knownDate">дата начала или окончания(например начало след. операции) в зависимости от метода</param>
        /// <param name="resId">Resource Id</param>
        /// <param name="method">Перечисление направления поиска</param>
        /// <returns></returns>
        public bool ScheduleByOrderId(int orderId,int resId ,ref DateTime knownDate, SearchDirection method)
        {

            var res = _planningBoard.PutOperationOnResource(orderId, resId, knownDate);
            return res;
        }


        /// <summary>
        /// Удаляет операции с указанного рессурса
        /// </summary>
        /// <param name="resId"></param>
        /// <param name="fromTt">Учитывать вермя терминаторной линии или нет</param>
        /// <param name="startDate">Если не указана , то берется терминаторная линиия</param>
        /// <param name="endDate">Если не указаны все даты, то удаляет все операции с ресурса</param>
        public void UnallocateOpOnResources(int resId, bool fromTt = false, DateTime startDate=default(DateTime), DateTime endDate = default(DateTime))
        {
            int resNumber = _preactor.GetRecordNumber("Resources", new PrimaryKey(resId));

            DateTime tTime = _preactor.PlanningBoard.TerminatorTime;
            if (fromTt==true)
            {
                startDate = tTime;
                _preactor.PlanningBoard.UnallocateResource(resNumber, startDate, endDate, OperationReferencePoint.IgnoreTime);
            }

            if (startDate == default(DateTime) && endDate != default(DateTime))
            {
                startDate = tTime;
                _preactor.PlanningBoard.UnallocateResource(resNumber, startDate, endDate, OperationReferencePoint.AnyPart);
            }
            else if (startDate == default(DateTime) && endDate == default(DateTime))
            {
                _preactor.PlanningBoard.UnallocateResource(resNumber, startDate, endDate, OperationReferencePoint.IgnoreTime);
            }
        }

    }
}
