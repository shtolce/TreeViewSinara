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
    public class BoMRepo:IRepository<BoMDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Bill of Materials";
        public BoMRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса BoMRepo");
            this._preactor = preactor;
        }
        /// <summary>
        /// Создает  спецификацию заказа в преакторе по BoMDTO, подчиненные объекты указывать в Operations
        /// </summary>
        /// <param name="obj">Создайте экземпляр класса BoMDTO для передачи</param>
        /// <returns></returns>
        public bool Create(BoMDTO obj)
        {
            if (GetByNo(obj.PartNo) != null)
            {
                logger.Trace($"Экземпляр спецификации заказа уже существует {obj.PartNo}");
                return false;
            }

            try
            {
                if (obj.OpCount == 0)
                {
                    int productRecNo = _preactor.CreateRecord(_entityName);
                    //_preactor.WriteField(_entityName, "Number", productRecNo, obj.Id);
                    _preactor.WriteField(_entityName, "Op. No.", productRecNo, obj.OpNo);
                    _preactor.WriteField(_entityName, "Operation Name", productRecNo, obj.OpName);
                    _preactor.WriteField(_entityName, "Part No.", productRecNo, obj.PartNo);
                    _preactor.WriteField(_entityName, "Belongs to BOM", productRecNo, obj.BelongsToBOM==""?"Родитель": obj.BelongsToBOM);
                    _preactor.WriteField(_entityName, "Required Part No.", productRecNo, obj.RequiredPartNo);
                    _preactor.WriteField(_entityName, "Required Quantity", productRecNo, obj.RequiredQuantity);
                    _preactor.WriteField(_entityName, "Multiply by order quantity", productRecNo, obj.MultiplyByOrderQuantity);
                    _preactor.WriteField(_entityName, "Multiple Quantity", productRecNo, obj.MultipleQuantity);
                    _preactor.WriteField(_entityName, "Order Part No.", productRecNo, obj.OrderPartNo);
                    _preactor.WriteField(_entityName, "Ignore Shortages", productRecNo, obj.IgnoreShortage);
                    _preactor.WriteField(_entityName, "Order No.", productRecNo, obj.OrderNo);

                }
                else
                {
                    foreach (OperationDTO objOp in obj.Operations)
                    {
                        int productRecNo = _preactor.CreateRecord(_entityName);
                        //_preactor.WriteField(_entityName, "Number", productRecNo, obj.Id);
                        _preactor.WriteField(_entityName, "Op. No.", productRecNo, obj.OpNo);
                        _preactor.WriteField(_entityName, "Operation Name", productRecNo, objOp.OpName);
                        _preactor.WriteField(_entityName, "Part No.", productRecNo, objOp.PartNo);
                        _preactor.WriteField(_entityName, "Belongs to BOM", productRecNo, obj.BelongsToBOM == "" ? "Родитель" : obj.BelongsToBOM);
                        _preactor.WriteField(_entityName, "Required Part No.", productRecNo, obj.RequiredPartNo);
                        _preactor.WriteField(_entityName, "Required Quantity", productRecNo, obj.RequiredQuantity);
                        _preactor.WriteField(_entityName, "Multiply by order quantity", productRecNo, obj.MultiplyByOrderQuantity);
                        _preactor.WriteField(_entityName, "Multiple Quantity", productRecNo, obj.MultipleQuantity);
                        _preactor.WriteField(_entityName, "Order Part No.", productRecNo, obj.OrderPartNo);
                        _preactor.WriteField(_entityName, "Ignore Shortages", productRecNo, obj.IgnoreShortage);
                        _preactor.WriteField(_entityName, "Order No.", productRecNo, obj.OrderNo);
                    }
                }
                logger.Trace($"Создан экземпляр спецификации продукта {obj.PartNo}");
                return true;
            }
            catch
            {
                logger.Trace($"Ошибка создания экземпляра спецификации продукта  {obj.PartNo}");
            }
            return false;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName, new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр BoM успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра BoM {ex.Message} {id}");
            }
        }

        /// <summary>
        /// Получает список всех спецификаций заказа в преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BoMDTO> GetAll()
        {
            logger.Trace($"Получение списка спецификаций заказов начато");
            int recCount = _preactor.RecordCount(_entityName);
            string ordNo = "-1";
            string ordNoOldVal = "";
            List<BoMDTO> res = new List<BoMDTO>();
            BoMDTO foundOrder = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    ordNo = _preactor.ReadFieldString(_entityName, "Order No.", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение спецификаций заказов по всем записям) " + ex.TargetSite.Name);
                }
                if (ordNo != ordNoOldVal)
                {
                    foundOrder = GetByNo(ordNo);
                    res.Add(foundOrder);
                }
                ordNoOldVal = ordNo;
            }
            logger.Trace($"Получение списка спецификаций заказов закончено");
            return res;
        }
        /// <summary>
        /// Получает спецификацию заказа в преакторе по полю  Part No.
        /// подчиненные строки помещает в Operations
        /// </summary>
        /// <param name="OrderNo">Номер заказа , поле Part No. в преакторе </param>
        /// <returns></returns>
        public BoMDTO GetByNo(string OrderNo)
        {
            logger.Trace($"Получение спецификации заказа по номеру начато {OrderNo}");
            string requestStr = $"~{{$Order No.}}~==~{OrderNo}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            List<OperationDTO> opList = new List<OperationDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    OperationDTO op = new OperationDTO
                    {
                        Id = _preactor.ReadFieldInt(_entityName, "Number", recordNo),
                        OpNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo),
                        OpName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo),
                        PartNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo),
                        BelongsToBOM = _preactor.ReadFieldString(_entityName, "Belongs to BOM", recordNo),
                        RequiredPartNo = _preactor.ReadFieldString(_entityName, "Required Part No.", recordNo),
                        RequiredQuantity = _preactor.ReadFieldDouble(_entityName, "Required Quantity", recordNo),
                        MultiplyByOrderQuantity = _preactor.ReadFieldBool(_entityName, "Multiply by order quantity", recordNo),
                        MultipleQuantity = _preactor.ReadFieldDouble(_entityName, "Multiple Quantity", recordNo),
                        OrderPartNo = _preactor.ReadFieldString(_entityName, "Order Part No.", recordNo),
                        IgnoreShortage = _preactor.ReadFieldBool(_entityName, "Ignore Shortages", recordNo),
                        OrderNo = _preactor.ReadFieldString(_entityName, "Order No.", recordNo),
                    };
                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (BoM operation) " + ex.TargetSite.Name + " Номер заказа" + OrderNo);
                }
                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }

            int id = 0;
            int opNo = 0;
            string opName = "";
            string partNo = "";
            string belongsToBOM = "";
            double multipleQuantity = 1;
            bool multiplyByOrderQuantity = true;
            string requiredPartNo = "";
            double requiredQuantity = 0; ;
            bool ignoreShortage = default(bool);
            string orderPartNo = "";
            string orderNo = "";
            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", firstRecNo);
                partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                opName = _preactor.ReadFieldString(_entityName, "Operation Name", firstRecNo);
                belongsToBOM = _preactor.ReadFieldString(_entityName, "Belongs to BOM", firstRecNo);
                multipleQuantity = _preactor.ReadFieldDouble(_entityName, "Multiple Quantity", firstRecNo);
                multiplyByOrderQuantity = _preactor.ReadFieldBool(_entityName, "Multiply by order quantity", firstRecNo);
                requiredPartNo = _preactor.ReadFieldString(_entityName, "Required Part No.", firstRecNo);
                requiredQuantity = _preactor.ReadFieldDouble(_entityName, "Required Quantity", firstRecNo);
                orderPartNo = _preactor.ReadFieldString(_entityName, "Order Part No.", firstRecNo);
                ignoreShortage = _preactor.ReadFieldBool(_entityName, "Ignore Shortages", firstRecNo);
                orderNo = _preactor.ReadFieldString(_entityName, "Order No.", firstRecNo);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " заказ " + OrderNo);
            }
            BoMDTO result = new BoMDTO()
            {
                Id = id,
                OpNo = opNo,
                OpName = opName,
                PartNo = partNo,
                Operations = opList,
                BelongsToBOM = belongsToBOM,
                MultipleQuantity = multipleQuantity,
                MultiplyByOrderQuantity = multiplyByOrderQuantity,
                RequiredPartNo = requiredPartNo,
                RequiredQuantity = requiredQuantity,
                OrderNo = orderNo,
                OrderPartNo = orderPartNo,
                IgnoreShortage = ignoreShortage
            };
            logger.Trace($"Получение спецификации заказа по номеру завершено {OrderNo}");
            return result;
        }
        public void Update(BoMDTO obj)
        {
            BoMDTO foundPi = GetByNo(obj.PartNo);
            if (foundPi == null)
            {
                logger.Trace($"Экземпляр BoM не найден, нечего обновлять {obj.PartNo}");
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
                    logger.Trace($"Обновлен экземпляр BoM {obj.PartNo}");
                return;
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра BoM {ex.Message} {obj.PartNo}");
            }
            return;
        }
        ~BoMRepo()
        {
            LogManager.Flush();
        }

    }
}
