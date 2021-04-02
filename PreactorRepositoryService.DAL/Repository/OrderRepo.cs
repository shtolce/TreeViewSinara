using NLog;
using Preactor;
using PreactorRepositoryService.DAL.DTOModels;
using PreactorRepositoryService.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//конфигурационный файл , должен быть  в папке преактора exe
namespace PreactorRepositoryService.DAL.Repository
{
    public class OrderComparer : IEqualityComparer<OrderDTO>
    {
        bool IEqualityComparer<OrderDTO>.Equals(OrderDTO x, OrderDTO y)
        {
            return (x.OrderNo == y.OrderNo);
        }

        int IEqualityComparer<OrderDTO>.GetHashCode(OrderDTO obj)
        {
            if (Object.ReferenceEquals(obj, null))
                return 0;

            return obj.OrderNo.GetHashCode();
        }
    }


    public class OrderRepo : IRepository<OrderDTO>
    {
        private static Logger logger;
        private ResourceGroupRepo resGrpRepo;
        private ResourceRepo resRepo;
        private IPreactor _preactor;
        private string _entityName = "Orders";
        public OrderRepo(IPreactor preactor)
        {

            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса OrderRepo");
            this._preactor = preactor;
            resGrpRepo = new ResourceGroupRepo(_preactor);
            resRepo = new ResourceRepo(_preactor);
        }
        /// <summary>
        /// Создает ордер в Преакторе со всеми подчиненными строками указаными в Operations
        /// </summary>
        /// <param name="obj">Создайте объект класса OrderDTO</param>
        /// <returns></returns>
        public bool Create(OrderDTO obj)
        {
            if (GetByNo(obj.OrderNo)!=null)
            {
                logger.Trace($"Экземпляр заказа Order уже существует {obj.OrderNo}");
                return false;
            }

            if (obj.OpCount == 0)
            {
                try
                {
                    int orderRecNo = _preactor.CreateRecord(_entityName);
                    _preactor.WriteField(_entityName, "Quantity", orderRecNo, obj.Quantity);
                    _preactor.WriteField(_entityName, "Order No.", orderRecNo, obj.OrderNo);
                    //_preactor.WriteField(_entityName, "Number", orderRecNo, obj.Id);
                    if (obj.OpNo >0)
                        _preactor.WriteField(_entityName, "Op. No.", orderRecNo, obj.OpNo);
                    _preactor.WriteField(_entityName, "Quantity", orderRecNo, obj.Quantity);
                    _preactor.WriteField(_entityName, "Part No.", orderRecNo, obj.PartNo);
                    _preactor.WriteField(_entityName, "Op. No.", orderRecNo, obj.OpNo);
                    _preactor.WriteField(_entityName, "Product", orderRecNo, obj.Product);
                    _preactor.WriteField(_entityName, "Operation Name", orderRecNo, obj.OpName);
                    if (obj.Resource!=null)
                        _preactor.WriteField(_entityName, "Required Resource", orderRecNo, obj.Resource);
                    if (obj.ResourceGroup != null)
                        _preactor.WriteField(_entityName, "Resource Group", orderRecNo, obj.ResourceGroup);
                    _preactor.WriteField(_entityName, "Setup Start", orderRecNo, obj.SetupStart);
                    _preactor.WriteField(_entityName, "Start Time", orderRecNo, obj.StartTime);
                    _preactor.WriteField(_entityName, "End Time", orderRecNo, obj.EndTime);
                    _preactor.WriteField(_entityName, "Earliest Start Date", orderRecNo, obj.EarliestStartDate);
                    _preactor.WriteField(_entityName, "Due Date", orderRecNo, obj.DueDate);
//                    System.Threading.Thread.Sleep(1);
                    _preactor.UpdateRecord("Orders", orderRecNo);
                    _preactor.Commit("Orders");
                    _preactor.ExpandJob(_entityName, orderRecNo);
                    logger.Trace($"Создан экземпляр заказа Order {obj.OrderNo}");
                    return true;

                }
                catch
                {
                    logger.Trace($"Ошибка создания экземпляра заказа(ver1) Order {obj.OrderNo}");
                }

            }
            else
            {
                foreach (OperationDTO objOp in obj.Operations)
                {
                    try
                    {
                        int orderRecNo = _preactor.CreateRecord(_entityName);
                        _preactor.WriteField(_entityName, "Belongs To Order", orderRecNo, objOp.BelongsToOrder==null?"Родитель": objOp.BelongsToOrder);
                        _preactor.WriteField(_entityName, "Quantity", orderRecNo, objOp.Quantity);
                        _preactor.WriteField(_entityName, "Order No.", orderRecNo, objOp.OrderNo);
                        //_preactor.WriteField(_entityName, "Number", orderRecNo, obj.Id);
                        if (obj.OpNo > 0)
                            _preactor.WriteField(_entityName, "Op. No.", orderRecNo, objOp.OpNo);
                        _preactor.WriteField(_entityName, "Part No.", orderRecNo, objOp.OrderPartNo);
                        if (obj.Product != null)
                            _preactor.WriteField(_entityName, "Product", orderRecNo, obj.Product);
                        _preactor.WriteField(_entityName, "Operation Name", orderRecNo, objOp.OpName);
                        if (objOp.Resource != null)
                            _preactor.WriteField(_entityName, "Required Resource", orderRecNo, objOp.Resource);
                        if (obj.ResourceGroup != null)
                            _preactor.WriteField(_entityName, "Resource Group", orderRecNo, objOp.ResourceGroup);
                        _preactor.WriteField(_entityName, "Setup Start", orderRecNo, objOp.SetupStart);
                        _preactor.WriteField(_entityName, "Start Time", orderRecNo, objOp.StartTime);
                        _preactor.WriteField(_entityName, "End Time", orderRecNo, objOp.EndTime);
                        _preactor.WriteField(_entityName, "Earliest Start Date", orderRecNo, objOp.EarliestStartDate);
                        _preactor.WriteField(_entityName, "Due Date", orderRecNo, objOp.DueDate);
                        _preactor.ExpandJob(_entityName, orderRecNo);
                        logger.Trace($"Создан экземпляр заказа Order {objOp.OrderNo}");
//                        System.Threading.Thread.Sleep(1);
                        _preactor.Commit("Orders");
                        _preactor.UpdateRecord("Orders", orderRecNo);
                        _preactor.ExpandJob(_entityName, orderRecNo);

                    }
                    catch
                    {
                        logger.Trace($"Ошибка создания экземпляра заказа(ver2) Order {obj.OrderNo}");
                        return false;
                    }
                }

            }
            return true;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName, new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр Order успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра Order {ex.Message} {id}");
            }
        }

        /// <summary>
        /// Получает из Преактора все ордера заказы в класс OrderDTO
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> GetAll()
        {
            logger.Trace($"Получение списка ордеров начато");
            int recCount = _preactor.RecordCount(_entityName);
            string ordNo="-1";
            string ordNoOldVal = "";
            List<OrderDTO> res = new List<OrderDTO>();
            OrderDTO foundOrder = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    ordNo = _preactor.ReadFieldString(_entityName, "Order No.", i);
                }
                catch(Exception ex)
                {
                    logger.Error(ex.Message + " (получение OrderNo по всем записям) " + ex.TargetSite.Name );
                }
                if (ordNo!=ordNoOldVal)
                {
                    foundOrder =GetByNo(ordNo);
                    res.Add(foundOrder);
                }
                ordNoOldVal = ordNo;
            }
            logger.Trace($"Получение списка ордеров закончено");
            return res;
        }


        public IEnumerable<OrderDTO> GetAllPartNo_Op(IPreactor preactor, string partNo, int opNo)
        {
            logger.Trace($"Получение ордера по номеру начато {partNo} {opNo}");

            string requestStr = $"(~{{$Part No.}}~==~{partNo}~)&&({{#Op. No.}}=={opNo})";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;

            int id = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            double totalCapacityVal = 0;
            int rank1 = 0;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);

            string belongsToOrder = "";
            List<OrderDTO> opList = new List<OrderDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Resource", recordNo);
                    resource = _preactor.ReadFieldString(_entityName, "Resource", recordNo);
                    resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    //totalCapacityVal = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    //rank1 = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);

                    OrderDTO op = new OrderDTO()
                    {
                        OrderNo = orderNo,
                        Id = id,
                        OpNo = opNo,
                        Quantity = quantity,
                        Product = product,
                        OpName = opName,
                        Resource = resource,
                        ResourceGroup = resourceGroup,
                        ResourceId = resourceId,
                        PartNo = partNo,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        TableAttribute1Rank = rank1,
                        NumericalAttribute15 = totalCapacityVal
                    };

                        opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);

            }
            return opList;
        }


        public IEnumerable<OrderDTO> GetAllPartNo_Op_Name(IPreactor preactor, string partNo, int opNo,string opName2)
        {
            logger.Trace($"Получение ордера по номеру начато {partNo} {opNo}");

            string requestStr = $"(~{{$Part No.}}~==~{partNo}~)&&({{#Op. No.}}=={opNo})&&(~{{$Operation Name}}~==~{opName2}~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;
            int lotNumber = 0;

            int id = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            double totalCapacityVal = 0;
            int rank1 = 0;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);

            string belongsToOrder = "";
            List<OrderDTO> opList = new List<OrderDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Resource", recordNo);
                    resource = _preactor.ReadFieldString(_entityName, "Resource", recordNo);
                    resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);

                    try
                    {
                        lotNumber = _preactor.ReadFieldInt(_entityName, "Lot Number", recordNo);
                    }
                    catch
                    {
                        lotNumber = 1;
                    }


                    //totalCapacityVal = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    //rank1 = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);

                    OrderDTO op = new OrderDTO()
                    {
                        OrderNo = orderNo,
                        Id = id,
                        OpNo = opNo,
                        Quantity = quantity,
                        Product = product,
                        OpName = opName,
                        Resource = resource,
                        ResourceGroup = resourceGroup,
                        ResourceId = resourceId,
                        PartNo = partNo,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        TableAttribute1Rank = rank1,
                        NumericalAttribute15 = totalCapacityVal,
                        LotNumber = lotNumber
                    };

                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);

            }
            return opList;
        }



        /// <summary>
        /// Получает ордер заказ из преактора по его номеру OrderNo, полчиненные объекты ордера помещаются в Operations
        /// </summary>
        /// <param name="orderNo">Укажите в строке номер заказа из преактора</param>
        /// <returns></returns>
        public OrderDTO GetByNo(string orderNo)
        {
            logger.Trace($"Получение ордера по номеру начато {orderNo}");
            string requestStr = $"~{{$Order No.}}~==~{orderNo}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId=0;
            int resourceId=0;

            string orderN = "";
            int id = 0;
            int opNo = 0;
            double quantity = 0;
            int numerical2 = 0;
            int numerical5 = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string resource = "";
            string resourceGroup = "";
            bool toggleAttribute1=false;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);
            string belongsToOrder = "";
            List<OperationDTO> opList = new List<OperationDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    resourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name;
                    resource = resRepo.GetById(resourceId)?.Name;
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    toggleAttribute1 = _preactor.ReadFieldBool(_entityName, "Toggle Attribute 1", recordNo);
                    OperationDTO op = new OperationDTO
                    {
                        Id = id,
                        BelongsToOrder = belongsToOrder,
                        OrderNo = orderNo,
                        OpNo = opNo,
                        Quantity = quantity,
                        OpName = opName,
                        PartNo = partNo,
                        ResourceGroup = resourceGroup,
                        Resource = resource,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        ToggleAttribute1 = toggleAttribute1

                    };
                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (order operation) " + ex.TargetSite.Name + " Номер заказа" + orderNo);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }

            resourceGroupId=0;
            resourceId=0;

            try
            {
                orderN = _preactor.ReadFieldString(_entityName, "Order No.", firstRecNo);
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", firstRecNo);
                partNo = _preactor.ReadFieldString(_entityName, "Part No.", firstRecNo);
                quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", firstRecNo);
                product = _preactor.ReadFieldString(_entityName, "Product", firstRecNo);
                opName = _preactor.ReadFieldString(_entityName, "Operation Name", firstRecNo);
                resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", firstRecNo);
                resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", firstRecNo);
                resourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name;
                resource = resRepo.GetById(resourceId)?.Name;
                setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", firstRecNo);
                startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", firstRecNo);
                endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", firstRecNo);
                earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", firstRecNo);
                dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", firstRecNo);
                resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", firstRecNo);
                resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", firstRecNo);
                numerical2 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", firstRecNo);
                numerical5 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", firstRecNo);

                resource = resRepo.GetById(resourceId)?.Name;
                resourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name;
                OrderDTO result = new OrderDTO()
                {
                    OrderNo = orderN,
                    Id = id,
                    OpNo = opNo,
                    Quantity = quantity,
                    Operations = opList,
                    Product = product,
                    OpName = opName,
                    Resource = resource,
                    ResourceGroup = resourceGroup,
                    PartNo = partNo,
                    SetupStart = setupStart,
                    StartTime = startTime,
                    EndTime = endTime,
                    EarliestStartDate = earliestStartDate,
                    DueDate = dueDate,
                    NumericalAttribute5 = numerical5,
                    NumericalAttribute2 = numerical2


                };
                logger.Trace($"Получение ордера по номеру завершено {orderNo}");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message+" (order header)"+ ex.TargetSite.Name+ " Номер заказа"+ orderNo);
                return null;
            }
        }





        /// <summary>
        /// Получает списко ордеров, без реквизитов, только номера заказов, для ускорения доступа
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> GetAllWithOrderNoOnly()
        {
            logger.Trace($"Получение списка ордеров начато");
            int recCount = _preactor.RecordCount(_entityName);
            string ordNo = "-1";
            string ordNoOldVal = "";
            double numerical2=0;
            double numerical5 = 0;
            DateTime setupStart = default(DateTime);
            int id=0;
            List<OrderDTO> res = new List<OrderDTO>();
            OrderDTO foundOrder = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    ordNo = _preactor.ReadFieldString(_entityName, "Order No.", i);
                    id = _preactor.ReadFieldInt(_entityName, "Number", i);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", i);
                    numerical2 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", i);
                    numerical5 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", i);

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение OrderNo по всем записям) " + ex.TargetSite.Name);
                }
                if (ordNo != ordNoOldVal) 
                {

                    foundOrder = new OrderDTO()
                    {
                        OrderNo = ordNo,
                        Id = id,
                        NumericalAttribute2 = numerical2,
                        NumericalAttribute5 = numerical5

                    };

                    res.Add(foundOrder);
                }
                ordNoOldVal = ordNo;
            }
            logger.Trace($"Получение списка ордеров закончено");
            return res;
        }

        public IEnumerable<OrderDTO> GetAllWithOrderNoOpNoOnly()
        {
            logger.Trace($"Получение списка ордеров начато");
            int recCount = _preactor.RecordCount(_entityName);
            string ordNo = "-1";
            string ordNoOldVal = "";
            string resName = "-1";
            double numerical2 = 0;
            double numerical5 = 0;
            int opNo =0;
            DateTime setupStart = default(DateTime);
            int id = 0;
            List<OrderDTO> res = new List<OrderDTO>();
            OrderDTO foundOrder = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", i);
                    ordNo = _preactor.ReadFieldString(_entityName, "Order No.", i);
                    resName = _preactor.ReadFieldString(_entityName, "Resource", i);
                    id = _preactor.ReadFieldInt(_entityName, "Number", i);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", i);
                    numerical2 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", i);
                    numerical5 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", i);


                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение OrderNo по всем записям) " + ex.TargetSite.Name);
                }


                foundOrder = new OrderDTO()
                    {
                        OrderNo = ordNo,
                        Id = id,
                        OpNo = opNo,
                        NumericalAttribute2 = numerical2,
                        NumericalAttribute5 = numerical5,
                        Resource = resName

                    };

                    res.Add(foundOrder);
            }
            logger.Trace($"Получение списка ордеров закончено");
            return res;
        }




        /// <summary>
        /// Получает из Преактора все ордера заказы не спланированные
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> GetAllUnScheduled(IPreactor preactor)
        {
            string requestStr = $"(~{{$Resource}}~==~Не задано~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;

            int id = 0;
            int opNo = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            double totalCapacityVal = 0;
            double Numerical2 = 0;
            double Numerical5 = 0;
            int rank1 = 0;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);

            int belongsToOrder = 0;
            List<OrderDTO> opList = new List<OrderDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Resource", recordNo);
                    resource = _preactor.ReadFieldString(_entityName, "Resource", recordNo);
                    resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", recordNo);

                    belongsToOrder = _preactor.ReadFieldInt(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    totalCapacityVal = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    Numerical2 = preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", recordNo);
                    Numerical5 = preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1 = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);

                    OrderDTO op = new OrderDTO()
                    {
                        OrderNo = orderNo,
                        Id = id,
                        OpNo = opNo,
                        Quantity = quantity,
                        Product = product,
                        OpName = opName,
                        Resource = resource,
                        ResourceGroup = resourceGroup,
                        ResourceId = resourceId,
                        PartNo = partNo,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        TableAttribute1Rank = rank1,
                        NumericalAttribute15 = totalCapacityVal,
                        NumericalAttribute5 = Numerical5,
                        NumericalAttribute2 = Numerical2,
                        BelongsToOrderNo = belongsToOrder


                    };

                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (order operation) " + ex.TargetSite.Name);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);

            }
            return opList;



        }


        /// <summary>
        /// Получает из Преактора все ордера заказы не спланированные
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> GetAllUnScheduledMarked(IPreactor preactor)
        {
            string requestStr = $"(~{{$Resource}}~==~Не задано~)&&({{#Numerical Attribute 5}}!=-1)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;

            int id = 0;
            int opNo = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            double totalCapacityVal = 0;
            double Numerical2 = 0;
            double Numerical5 = 0;
            int rank1 = 0;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);

            string belongsToOrder = "";
            List<OrderDTO> opList = new List<OrderDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Resource", recordNo);
                    resource = _preactor.ReadFieldString(_entityName, "Resource", recordNo);
                    resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    totalCapacityVal = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    Numerical2 = preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", recordNo);
                    Numerical5 = preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1 = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);

                    OrderDTO op = new OrderDTO()
                    {
                        OrderNo = orderNo,
                        Id = id,
                        OpNo = opNo,
                        Quantity = quantity,
                        Product = product,
                        OpName = opName,
                        Resource = resource,
                        ResourceGroup = resourceGroup,
                        ResourceId = resourceId,
                        PartNo = partNo,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        TableAttribute1Rank = rank1,
                        NumericalAttribute15 = totalCapacityVal,
                        NumericalAttribute5 = Numerical5,
                        NumericalAttribute2 = Numerical2

                    };

                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (order operation) " + ex.TargetSite.Name);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);

            }
            return opList;



        }







        /// <summary>
        /// Получает из Преактора все ордера заказы предшествующие указанной на ресурсе в класс OrderDTO
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDTO> GetAllScheduledPrevOnResource2(IPreactor preactor,int resRecordNo,int ordRecordNo)
        {
            logger.Trace($"Получение списка ордеров начато");
            int recCount = _preactor.RecordCount(_entityName);
            string ordNoCur = "-1";
            string resName = "";
            string resRecNameCur = "";
            string resRecName = "";
            int ordRecordNoCur;
            DateTime opTime = default(DateTime);
            DateTime opTimeCur = default(DateTime);
            List<OrderDTO> resList = new List<OrderDTO>();
            OrderDTO foundOrder = null;
            for (int i = 1; i <= recCount; i++)
            {
                ordRecordNoCur = i;
                try
                {
                    opTime = preactor.PlanningBoard.GetOperationTimes(ordRecordNo).Value.OperationTimes.ChangeStart;
                    opTimeCur = preactor.PlanningBoard.GetOperationTimes(ordRecordNoCur).Value.OperationTimes.ProcessEnd;
                    ordNoCur = _preactor.ReadFieldString(_entityName, "Order No.", ordRecordNoCur);
                    resRecNameCur = preactor.ReadFieldString(_entityName, "Resource", ordRecordNoCur);
                    resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение OrderNo по всем записям) " + ex.TargetSite.Name);
                }

                if (ordRecordNoCur != ordRecordNo && resRecNameCur == resRecName && opTimeCur<= opTime && resRecNameCur!="")
                {
                    foundOrder = GetById(ordRecordNoCur);
                    resList.Add(foundOrder);
                }
            }
            logger.Trace($"Получение списка ордеров закончено");
            return resList.Distinct();
        }

        public IEnumerable<OrderDTO> GetAllScheduledPrevOnResource(IPreactor preactor, int resRecordNo,int ordRecordNo,DateTime BestChangeStart=default(DateTime))
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            logger.Trace($"Получение ордера по номеру начато {ordRecordNo}");
            int recN = _preactor.ReadFieldInt(_entityName, "Number", ordRecordNo);

            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)&&({{#Number}}!={recN})";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;

            int id = 0;
            int opNo = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            double totalCapacityVal = 0;
            double Numerical2 = 0;
            double Numerical5 = 0;
            int rank1 = 0;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);
            DateTime opTime = BestChangeStart;
            //opTime = preactor.PlanningBoard.GetOperationTimes(ordRecordNo).Value.OperationTimes.ChangeStart;

            string belongsToOrder = "";
            List<OrderDTO> opList = new List<OrderDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Resource", recordNo);
                    resource = _preactor.ReadFieldString(_entityName, "Resource", recordNo);
                    resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    totalCapacityVal = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    Numerical2 = preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", recordNo);
                    Numerical5 = preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1 = preactor.ReadFieldInt(_entityName,"Table Attribute 1 Rank", recordNo);

                    OrderDTO op = new OrderDTO()
                    {
                        OrderNo = orderNo,
                        Id = id,
                        OpNo = opNo,
                        Quantity = quantity,
                        Product = product,
                        OpName = opName,
                        Resource = resource,
                        ResourceGroup = resourceGroup,
                        ResourceId = resourceId,
                        PartNo = partNo,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        TableAttribute1Rank = rank1,
                        NumericalAttribute15 = totalCapacityVal,
                        NumericalAttribute5 = Numerical5,
                        NumericalAttribute2 = Numerical2

                    };

                    if (id != ordRecordNo && opTime>op.SetupStart)
                        opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (order operation) " + ex.TargetSite.Name + " id заказа" + ordRecordNo);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);

            }
            return opList;
        }
        public IEnumerable<OrderDTO> GetAllScheduledPrevOnResource2(IPreactor preactor, int resRecordNo, int ordRecordNo, DateTime BestChangeStart = default(DateTime))
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            logger.Trace($"Получение ордера по номеру начато {ordRecordNo}");
            int recN = _preactor.ReadFieldInt(_entityName, "Number", ordRecordNo);

            //string requestStr = $"(~{{$Resource}}~==~{resRecName}~)&&({{#Number}}!={recN})";
            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;

            int id = 0;
            int opNo = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            double totalCapacityVal = 0;
            double Numerical2 = 0;
            double Numerical5 = 0;
            int rank1 = 0;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);
            DateTime opTime = BestChangeStart;
            //opTime = preactor.PlanningBoard.GetOperationTimes(ordRecordNo).Value.OperationTimes.ChangeStart;

            string belongsToOrder = "";
            List<OrderDTO> opList = new List<OrderDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Resource", recordNo);
                    resource = _preactor.ReadFieldString(_entityName, "Resource", recordNo);
                    resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    totalCapacityVal = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    Numerical2 = preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", recordNo);
                    Numerical5 = preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1 = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);

                    OrderDTO op = new OrderDTO()
                    {
                        OrderNo = orderNo,
                        Id = id,
                        OpNo = opNo,
                        Quantity = quantity,
                        Product = product,
                        OpName = opName,
                        Resource = resource,
                        ResourceGroup = resourceGroup,
                        ResourceId = resourceId,
                        PartNo = partNo,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        TableAttribute1Rank = rank1,
                        NumericalAttribute15 = totalCapacityVal,
                        NumericalAttribute5 = Numerical5,
                        NumericalAttribute2 = Numerical2

                    };

                    //if (id != ordRecordNo && opTime == op.SetupStart)
                    if (opTime == op.SetupStart)
                        opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (order operation) " + ex.TargetSite.Name + " id заказа" + ordRecordNo);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);

            }
            return opList;
        }

        public DateTime GetMaxOpDateOnResourceRank(IPreactor preactor, int resRecordNo,DateTime BestChangeStart, int rank1,int rank2=0)
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);

            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            DateTime setupStart = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime maxDate = BestChangeStart;
            int rank1Current = 0;
            int rank2Current = 0;
            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    rank1Current = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);
                    rank2Current = preactor.ReadFieldInt(_entityName, "Table Attribute 2 Rank", recordNo);
                    if (rank1 == rank1Current && endTime >= maxDate)
                    {
                        maxDate = endTime;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }
            return maxDate<= BestChangeStart ? BestChangeStart : maxDate;
        }

        private bool checkOperationMinRes(IPreactor preactor, int resRecordNo, DateTime setupStartCurrent, int currentRec)
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            int recN = _preactor.ReadFieldInt(_entityName, "Number", currentRec);
            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)&&({{#Number}}!={recN})";
            int recordNo = preactor.FindMatchingRecord(_entityName, 0, requestStr);
            double totalCapacityValCurrent = 0;
            totalCapacityValCurrent = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", currentRec);
            double totalCapacityVal = 0;
            DateTime setupStart;
            var Table = preactor.GetFormatNumber("Resources");
            var resMinCapacity = preactor.ReadFieldDouble(new FormatFieldPair(Table, preactor.GetFieldNumber(Table, "Attribute 2")), resRecordNo);


            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                if (recordNo != currentRec)
                {
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);

                    if (setupStart == setupStartCurrent)
                        totalCapacityVal += _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                }

                recordNo = preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }//while
            if (totalCapacityVal < resMinCapacity)
                return false;
            else
                return true;
        }




        private bool checkOperationMaxRes(IPreactor preactor, int resRecordNo, DateTime setupStartCurrent,int currentRec,double? resMaxResVal1= 0)
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            int recN = _preactor.ReadFieldInt(_entityName, "Number", currentRec);
            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)&&({{#Number}}!={recN})";
            int recordNo = preactor.FindMatchingRecord(_entityName, 0, requestStr);
            //int rank1Current = 0;
            //int rank2Current = 0;
            double totalCapacityValCurrent = 0;
            totalCapacityValCurrent = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", currentRec);
            double totalCapacityVal = 0;
            DateTime setupStart;


            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                if (recordNo != currentRec)
                {
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);

                    if (setupStart == setupStartCurrent)
                        totalCapacityVal += _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    //на ранги не нужно проверять они и так равны по условию

                }

                recordNo = preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }//while
            if (totalCapacityVal + totalCapacityValCurrent > resMaxResVal1)
                return false;
            else
                return true;
        }


        private void  markOperationMaxRes(IPreactor preactor, int resRecordNo, DateTime setupStartCurrent, int currentRec, double? resMaxResVal1 = 0)
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            int recN = _preactor.ReadFieldInt(_entityName, "Number", currentRec);
            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)&&({{#Number}}!={recN})";
            int recordNo = preactor.FindMatchingRecord(_entityName, 0, requestStr);
            double totalCapacityValCurrent = 0;
            totalCapacityValCurrent = preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", currentRec);
            double totalCapacityVal = 0;
            DateTime setupStart;
            var TableOrders = preactor.GetFormatNumber("Orders");
            var checkedField = new FormatFieldPair(TableOrders, preactor.GetFieldNumber(TableOrders, "Numerical Attribute 5"));

            if (!checkOperationMinRes(preactor, resRecordNo, setupStartCurrent, currentRec))
            {
                return;
            }

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                if (recordNo != currentRec)
                {
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);

                    if (setupStart == setupStartCurrent)
                    {
                        preactor.WriteField(checkedField,  recordNo, -1);

                    }
                    //на ранги не нужно проверять они и так равны по условию

                }

                recordNo = preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }//while
        }



        public double GetOperationResTotalCapacity(IPreactor preactor, int resRecordNo, DateTime setupStartCurrent)
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)";
            int recordNo = preactor.FindMatchingRecord(_entityName, 0, requestStr);
            double totalCapacityVal = 0;
            DateTime setupStart;
            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                if (setupStart == setupStartCurrent)
                    totalCapacityVal += _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    //на ранги не нужно проверять они и так равны по условию
                recordNo = preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }//while
            return totalCapacityVal;
        }




        public DateTime GetMaxOpDateOnResource(IPreactor preactor, int resRecordNo, DateTime BestChangeStart ,int currentRec,int rank1=0, int rank2=0,double? resMaxResVal1=0)
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            int rank1Current = 0;
            int rank2Current = 0;

            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            DateTime setupStart = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime maxDate = BestChangeStart;
            int checkedField;



            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;

                    
                try
                {
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    checkedField = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1Current = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);
                    rank2Current = preactor.ReadFieldInt(_entityName, "Table Attribute 2 Rank", recordNo);


                    if ((endTime > maxDate && checkedField == -1 && rank1Current == rank1) || (endTime > maxDate && rank1Current != rank1))
                    {
                        maxDate = endTime;

                    }
                    if (endTime > maxDate && checkedField != -1 && rank1Current == rank1)
                    {
                        //Перебрать на этом ресурсе по этому setupstart все операции , если превышелет макс, сменить дату, если нет равной setup
                        //---------------


                        if (checkOperationMaxRes(preactor, resRecordNo, setupStart, currentRec, resMaxResVal1))
                        {
                            maxDate = setupStart;
                        }
                        else
                        {
                            maxDate = endTime;
                        }
                        //--------------
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }
            return maxDate <= BestChangeStart ? BestChangeStart : maxDate;
            //return BestChangeStart;
        }

        public DateTime GetMaxFreeOpDateOnResource_Old(IPreactor preactor, int resRecordNo, DateTime BestChangeStart, DateTime BestEndTime, int currentRec, int rank1 = 0, int rank2 = 0, double? resMaxResVal1 = 0)
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            int rank1Current = 0;
            int rank2Current = 0;

            //string requestStr = $"(~{{$Resource}}~==~{resRecName}~) &&({{#Setup Start}}>={BestChangeStart})  ";
            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            DateTime setupStart = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime maxDate = BestChangeStart;
            int checkedField;



            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;


                try
                {
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    checkedField = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1Current = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);
                    rank2Current = preactor.ReadFieldInt(_entityName, "Table Attribute 2 Rank", recordNo);
                    if (setupStart >= maxDate)
                    {

                        if ((endTime > maxDate && BestEndTime > setupStart && checkedField == -1 && rank1Current == rank1) || (endTime > maxDate && rank1Current != rank1 && BestEndTime > setupStart))
                        {
                            maxDate = endTime;

                        }
                        if (endTime > maxDate && BestEndTime > setupStart && checkedField != -1 && rank1Current == rank1)
                        {
                            //Перебрать на этом ресурсе по этому setupstart все операции , если превышелет макс, сменить дату, если нет равной setup
                            //---------------


                            if (checkOperationMaxRes(preactor, resRecordNo, setupStart, currentRec, resMaxResVal1) && maxDate < setupStart)
                            {
                                maxDate = setupStart;
                                return maxDate;
                            }
                            else
                            {
                                maxDate = endTime;
                            }
                            //--------------
                        }






                    }//if (setupStart >= maxDate)


                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }
            return maxDate;// <= BestChangeStart ? BestChangeStart : maxDate;
            //return BestChangeStart;
        }


        private struct OrderItem
        {
            internal int rank1Current;
            internal int rank2Current;
            internal DateTime setupStart;
            internal DateTime endTime;
            internal int checkedField;

        }

        private OrderItem[] getOrdersArray(IPreactor preactor,int resRecordNo)
        {
            string resRecName = preactor.ReadFieldString("Resources", "Name", resRecordNo);
            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int rank1Current = 0;
            int rank2Current = 0;
            DateTime setupStart = default(DateTime);
            DateTime endTime = default(DateTime);
            int checkedField;
            OrderItem[] ordersArrp = new OrderItem[_preactor.RecordCount(_entityName)];

            int incCount = 0;
            while (recordNo >= 0)
            {
                try
                {
                    if (recordNo < 0)
                        break;
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    checkedField = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1Current = preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);
                    rank2Current = preactor.ReadFieldInt(_entityName, "Table Attribute 2 Rank", recordNo);
                    ordersArrp[incCount].setupStart = setupStart;
                    ordersArrp[incCount].endTime = endTime;
                    ordersArrp[incCount].checkedField = checkedField;
                    ordersArrp[incCount].rank1Current = rank1Current;
                    ordersArrp[incCount].rank2Current = rank2Current;
                    incCount++;

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }//while
            return ordersArrp.OrderBy(x => x.setupStart).ToArray();


        }




        public DateTime GetMaxFreeOpDateOnResource(IPreactor preactor, int resRecordNo, DateTime BestChangeStart, DateTime BestEndTime, int currentRec, int rank1 = 0, int rank2 = 0, double? resMaxResVal1 = 0)
        {
            DateTime maxDate = BestChangeStart;
            DateTime bestEndTime = preactor.PlanningBoard.TestOperationOnResource(currentRec, resRecordNo, BestChangeStart).Value.ProcessEnd;

            var arr = getOrdersArray(preactor, resRecordNo);
            foreach (OrderItem item in arr)
            {

                if (item.endTime >= maxDate)
                {

                    bestEndTime = preactor.PlanningBoard.TestOperationOnResource(currentRec, resRecordNo, maxDate).Value.ProcessEnd;

                    
                    if ((bestEndTime>=item.endTime  && maxDate < item.setupStart))
                    {
                        maxDate = item.endTime;

                    }
                    if ((bestEndTime>item.endTime   && maxDate <= item.setupStart))
                    {
                        maxDate = item.endTime;

                    }

                    if ((bestEndTime<=item.endTime  && maxDate > item.setupStart))
                    {
                        maxDate = item.endTime;

                    }
                    if ((bestEndTime<item.endTime   && maxDate >= item.setupStart))
                    {
                        maxDate = item.endTime;

                    }

                    if ((maxDate < item.endTime && maxDate > item.setupStart) && (item.checkedField == -1 || item.rank1Current != rank1))
                    {
                        maxDate = item.endTime;

                    }
                    else if ((maxDate < item.endTime && maxDate > item.setupStart) && !(item.checkedField == -1 || item.rank1Current != rank1))
                    {
                        if (!checkOperationMaxRes(preactor, resRecordNo, item.setupStart, currentRec, resMaxResVal1))
                        {
                            markOperationMaxRes(preactor, resRecordNo, item.setupStart, currentRec, resMaxResVal1);
                            maxDate = item.endTime;
                        }

                    }

                    if (bestEndTime > item.setupStart && bestEndTime <= item.endTime  && maxDate <= item.setupStart && item.checkedField != -1 && item.rank1Current == rank1)
                    {
                        if (checkOperationMaxRes(preactor, resRecordNo, item.setupStart, currentRec, resMaxResVal1))
                        {
                            maxDate = item.setupStart;
                            return maxDate;
                        }
                        else
                        {
                            maxDate = item.endTime;
                        }
                    }
                    if (bestEndTime > item.setupStart && bestEndTime <= item.endTime && maxDate <= item.setupStart && (item.checkedField == -1 || item.rank1Current != rank1))
                    {
                        maxDate = item.endTime;

                    }


                }//if (setupStart >= maxDate)


            }
            return maxDate;// <= BestChangeStart ? BestChangeStart : maxDate;
            //return BestChangeStart;
        }



        public void Update(OrderDTO obj)
        {
            OrderDTO foundPi = GetByNo(obj.PartNo);
            if (foundPi == null)
            {
                logger.Trace($"Экземпляр Product не найден, нечего обновлять {obj.PartNo}");
                return;
            }
            int resourceRecNo = foundPi.Id;
            try
            {
                foreach (OperationDTO op in foundPi.Operations)
                {
                    Delete(op.Id);
                }
                if (Create(obj) == true)
                    logger.Trace($"Обновлен экземпляр Product {obj.PartNo}");
                return;
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра Product {ex.Message} {obj.PartNo}");
            }
            return;
        }

        /// <summary>
        /// Получает ордер заказ из преактора по его id, полчиненные объекты ордера помещаются в Operations
        /// </summary>
        /// <param name="orderNo">Укажите в строке номер заказа из преактора</param>
        /// <returns></returns>
        public OrderDTO GetById(int _id)
        {
            if (_id < 0)
                return null;
            logger.Trace($"Получение ордера по номеру начато {_id}");
            string requestStr = $"~{{#Number}}~==~{_id}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;

            string orderN = "";
            int id = 0;
            int opNo = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);
            string belongsToOrder = "";
            List<OperationDTO> opList = new List<OperationDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    resourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name;
                    resource = resRepo.GetById(resourceId)?.Name;
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);

                    OperationDTO op = new OperationDTO
                    {
                        Id = id,
                        BelongsToOrder = belongsToOrder,
                        OrderNo = orderNo,
                        OpNo = opNo,
                        Quantity = quantity,
                        OpName = opName,
                        PartNo = partNo,
                        ResourceGroup = resourceGroup,
                        Resource = resource,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate
                    };
                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (order operation) " + ex.TargetSite.Name + " id заказа" + _id);
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }

            resourceGroupId = 0;
            resourceId = 0;

            try
            {
                orderN = _preactor.ReadFieldString(_entityName, "Order No.", firstRecNo);
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", firstRecNo);
                partNo = _preactor.ReadFieldString(_entityName, "Part No.", firstRecNo);
                quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", firstRecNo);
                product = _preactor.ReadFieldString(_entityName, "Product", firstRecNo);
                opName = _preactor.ReadFieldString(_entityName, "Operation Name", firstRecNo);
                resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", firstRecNo);
                resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", firstRecNo);
                resourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name;
                resource = resRepo.GetById(resourceId)?.Name;
                setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", firstRecNo);
                startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", firstRecNo);
                endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", firstRecNo);
                earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", firstRecNo);
                dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", firstRecNo);
                resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", firstRecNo);
                resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", firstRecNo);
                resource = resRepo.GetById(resourceId)?.Name;
                resourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name;
                OrderDTO result = new OrderDTO()
                {
                    OrderNo = orderN,
                    Id = id,
                    OpNo = opNo,
                    Quantity = quantity,
                    Operations = opList,
                    Product = product,
                    OpName = opName,
                    Resource = resource,
                    ResourceGroup = resourceGroup,
                    PartNo = partNo,
                    SetupStart = setupStart,
                    StartTime = startTime,
                    EndTime = endTime,
                    EarliestStartDate = earliestStartDate,
                    DueDate = dueDate


                };
                logger.Trace($"Получение ордера по id завершено {_id}");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (order header)" + ex.TargetSite.Name + " Номер заказа" + _id);
                return null;
            }
        }


        public IEnumerable<OrderDTO> GetAllScheduledOnResource(int resRecordNo)
        {
            string resRecName = _preactor.ReadFieldString("Resources", "Name", resRecordNo);

            string requestStr = $"(~{{$Resource}}~==~{resRecName}~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;

            int id = 0;
            int opNo = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            double totalCapacityVal = 0;
            double Numerical2 = 0;
            double Numerical5 = 0;
            int rank1 = 0;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);

            string belongsToOrder = "";
            List<OrderDTO> opList = new List<OrderDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Resource", recordNo);
                    resource = _preactor.ReadFieldString(_entityName, "Resource", recordNo);
                    resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    totalCapacityVal = _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    Numerical2 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", recordNo);
                    Numerical5 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1 = _preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);

                    OrderDTO op = new OrderDTO()
                    {
                        OrderNo = orderNo,
                        Id = id,
                        OpNo = opNo,
                        Quantity = quantity,
                        Product = product,
                        OpName = opName,
                        Resource = resource,
                        ResourceGroup = resourceGroup,
                        ResourceId = resourceId,
                        PartNo = partNo,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        TableAttribute1Rank = rank1,
                        NumericalAttribute15 = totalCapacityVal,
                        NumericalAttribute5 = Numerical5,
                        NumericalAttribute2 = Numerical2

                    };

                        opList.Add(op);
                }
                catch (Exception ex)
                {
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);

            }
            return opList;
        }


        /// <summary>
        /// Выбирает ордера у которых заданная ресурсная группа, бесконечные со сменной моделью, только спланированые на ресурс
        /// </summary>
        /// <param name="resRecordNo"></param>
        /// <returns></returns>
        public IEnumerable<OrderDTO> GetAllScheduledOnResourceGroupInfinite(int resRecordNo,DateTime afterTime)
        {

            //*10000000 такты
            
            resRecordNo = _preactor.GetRecordNumber("Resource Groups", new PrimaryKey(resRecordNo));
            string resRecName = _preactor.ReadFieldString("Resource Groups", "Name", resRecordNo);

            string requestStr = $"(~{{$Resource Group}}~==~{resRecName}~)";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId = 0;
            int resourceId = 0;

            int id = 0;
            int opNo = 0;
            double quantity = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string orderNo = "";
            string resource = "";
            string resourceGroup = "";
            double totalCapacityVal = 0;
            double Numerical2 = 0;
            double Numerical5 = 0;
            int rank1 = 0;
            DateTime setupStart = default(DateTime);
            DateTime startTime = default(DateTime);
            DateTime endTime = default(DateTime);
            DateTime earliestStartDate = default(DateTime);
            DateTime dueDate = default(DateTime);

            string belongsToOrder = "";
            List<OrderDTO> opList = new List<OrderDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {

                    resourceId = _preactor.ReadFieldInt(_entityName, "Resource", recordNo);
                    if (resourceId > 0)
                    {
                        //resRecordNo = _preactor.GetRecordNumber("Resources", new PrimaryKey(resRecordNo));
                        string finiteModeName = _preactor.ReadFieldString("Resources", "Finite or Infinite", resourceId);
                        string infiniteModeName = _preactor.ReadFieldString("Resources", "Infinite Mode Behavior", resourceId);
                        ResourceBehaviourType FiniteModeBehavior = ResourceDTO.getEnumValueByDescription(finiteModeName);
                        if (FiniteModeBehavior != ResourceBehaviourType.InfiniteWithStoryboardTemplate)
                        {
                            recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
                            continue;
                        }
                    }
                    else
                    {
                        recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
                        continue;
                    }



                    setupStart = _preactor.ReadFieldDateTime(_entityName, "Setup Start", recordNo);
                    if(setupStart<afterTime)
                    {
                        recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
                        continue;
                    }


                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", recordNo);
                    resource = _preactor.ReadFieldString(_entityName, "Resource", recordNo);
                    resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", recordNo);

                    belongsToOrder = _preactor.ReadFieldString(_entityName, "Belongs to Order No.", recordNo);
                    orderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo);
                    opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo);
                    quantity = _preactor.ReadFieldDouble(_entityName, "Quantity", recordNo);
                    opName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo);
                    partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    startTime = _preactor.ReadFieldDateTime(_entityName, "Start Time", recordNo);
                    endTime = _preactor.ReadFieldDateTime(_entityName, "End Time", recordNo);
                    earliestStartDate = _preactor.ReadFieldDateTime(_entityName, "Earliest Start Date", recordNo);
                    dueDate = _preactor.ReadFieldDateTime(_entityName, "Due Date", recordNo);
                    id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    totalCapacityVal = _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 15", recordNo);
                    Numerical2 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 2", recordNo);
                    Numerical5 = _preactor.ReadFieldInt(_entityName, "Numerical Attribute 5", recordNo);
                    rank1 = _preactor.ReadFieldInt(_entityName, "Table Attribute 1 Rank", recordNo);

                    OrderDTO op = new OrderDTO()
                    {
                        OrderNo = orderNo,
                        Id = id,
                        OpNo = opNo,
                        Quantity = quantity,
                        Product = product,
                        OpName = opName,
                        Resource = resource,
                        ResourceGroup = resourceGroup,
                        ResourceId = resourceId,
                        PartNo = partNo,
                        SetupStart = setupStart,
                        StartTime = startTime,
                        EndTime = endTime,
                        EarliestStartDate = earliestStartDate,
                        DueDate = dueDate,
                        TableAttribute1Rank = rank1,
                        NumericalAttribute15 = totalCapacityVal,
                        NumericalAttribute5 = Numerical5,
                        NumericalAttribute2 = Numerical2

                    };

                    opList.Add(op);

                }
                catch (Exception ex)
                {
                }

                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);

            }
            return opList;
        }


        ~OrderRepo()
        {
            LogManager.Flush();
        }

    }
}
