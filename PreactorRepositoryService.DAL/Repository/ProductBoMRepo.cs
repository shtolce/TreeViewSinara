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
    public class ProductBoMRepo:IRepository<ProductBoMDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Product Bill of Materials";
        public ProductBoMRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса ProductBoMRepo");
            this._preactor = preactor;
        }
        /// <summary>
        /// Создает  спецификацию продукции в преакторе по ProductBoMDTO, подчиненные объекты указывать в Operations
        /// </summary>
        /// <param name="obj">Создайте экземпляр класса ProductBoMDTO для передачи</param>
        /// <returns></returns>
        public bool Create(ProductBoMDTO obj)
        {
            if (GetByNo(obj.PartNo) != null)
            {
                logger.Trace($"Экземпляр спецификации продукта уже существует {obj.PartNo}");
                return false;
            }

            if (obj.OpCount == 0)
            {
                try
                {
                    int productRecNo = _preactor.CreateRecord(_entityName);
                    //_preactor.WriteField(_entityName, "Number", productRecNo, obj.Id);
                    _preactor.WriteField(_entityName, "Op. No.", productRecNo, obj.OpNo);
                    _preactor.WriteField(_entityName, "Part No.", productRecNo, obj.PartNo);
                    _preactor.WriteField(_entityName, "Product", productRecNo, obj.PartName);
                    _preactor.WriteField(_entityName, "Belongs to BOM", productRecNo, obj.BelongsToBOM==null?"Родитель": obj.BelongsToBOM);
                    _preactor.WriteField(_entityName, "Required Part No.", productRecNo, obj.RequiredPartNo);
                    _preactor.WriteField(_entityName, "Required Product", productRecNo, obj.RequiredPartName);
                    _preactor.WriteField(_entityName, "Required Quantity", productRecNo, obj.RequiredQuantity);
                    _preactor.WriteField(_entityName, "Multiply by order quantity", productRecNo, obj.MultiplyByOrderQuantity);
                    _preactor.WriteField(_entityName, "Multiple Quantity", productRecNo, obj.MultipleQuantity);
                    _preactor.WriteField(_entityName, "Operation Name", productRecNo, obj.OpName);



                }
                catch
                {
                    logger.Trace($"Ошибка создания экземпляра спецификации продукта  {obj.PartNo}");
                    return false;
                }
            }
            else
            {

                foreach (OperationDTO objOp in obj.Operations)
                {
                    try
                    {

                        int productRecNo = _preactor.CreateRecord(_entityName);
                        //_preactor.WriteField(_entityName, "Number", productRecNo, obj.Id);
                        _preactor.WriteField(_entityName, "Belongs to BOM", productRecNo, objOp.BelongsToBOM == null ? "Родитель" : objOp.BelongsToBOM);
                        _preactor.WriteField(_entityName, "Part No.", productRecNo, objOp.PartNo);
                        _preactor.WriteField(_entityName, "Product", productRecNo, obj.PartName);
                        _preactor.WriteField(_entityName, "Required Part No.", productRecNo, objOp.RequiredPartNo);
                        _preactor.WriteField(_entityName, "Required Product", productRecNo, obj.RequiredPartName);
                        _preactor.WriteField(_entityName, "Op. No.", productRecNo, objOp.OpNo);
                        _preactor.WriteField(_entityName, "Operation Name", productRecNo, objOp.OpName);
                        _preactor.WriteField(_entityName, "Required Quantity", productRecNo, objOp.RequiredQuantity);
                        _preactor.WriteField(_entityName, "Multiply by order quantity", productRecNo, objOp.MultiplyByOrderQuantity);
                        _preactor.WriteField(_entityName, "Multiple Quantity", productRecNo, objOp.MultipleQuantity);

                    }
                    catch
                    {
                        logger.Trace($"Ошибка создания экземпляра спецификации продукта  {obj.PartNo}");
                        return false;

                    }
                }

            }
            logger.Trace($"Создан экземпляр спецификации продукта {obj.PartNo}");
            return true;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName, new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр ProductBoM успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра ProductBoM {ex.Message} {id}");

            }
        }

        /// <summary>
        /// Получает список всех спецификаций продукции в преакторе
        /// метод не оптимален. перебирает весь ProductBoM, а нужно только тот у которого родитель -1
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ProductBoMDTO> GetAll()
        {
            logger.Trace($"Получение списка спецификаций продуктов начато");
            int recCount = _preactor.RecordCount(_entityName);
            string prdNo = "-1";
            string prdNoOldVal = "";
            List<ProductBoMDTO> res = new List<ProductBoMDTO>();
            ProductBoMDTO foundProduct = null;

            string requestStr = $"~{{$Belongs to BOM}}~==~Родитель~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;

                try
                {
                    prdNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение спецификаций продуктов по всем записям) " + ex.TargetSite.Name);
                }
                foundProduct = GetByNo(prdNo);
                res.Add(foundProduct);
                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }

            /*
            for (int i = 1; i <= recCount; i++)
            {

                try
                {
                    prdNo = _preactor.ReadFieldString("Products", "Part No.", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение спецификаций продуктов по всем записям) " + ex.TargetSite.Name);
                }
                if (prdNo != prdNoOldVal)
                {
                    foundProduct = GetByNo(prdNo);
                    res.Add(foundProduct);
                }
                prdNoOldVal = prdNo;
            }
            */
            logger.Trace($"Получение списка продуктов закончено");
            return res;
        }

        
        /// <summary>
        /// Получает спецификацию продукции в преакторе по полю  Part No.
        /// подчиненные строки помещает в Operations
        /// </summary>
        /// <param name="productPartNo">Наименование продукции , поле Part No. в преакторе </param>
        /// <returns></returns>
        public ProductBoMDTO GetByNo(string productPartNo)
        {
            logger.Trace($"Получение спецификации продукта по номеру начато {productPartNo}");
            string requestStr = $"~{{$Part No.}}~==~{productPartNo}~";
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
                        PartName = _preactor.ReadFieldString(_entityName, "Product", recordNo),
                        RequiredPartName = _preactor.ReadFieldString(_entityName, "Required Product", recordNo),

                    };
                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (product BoM operation) " + ex.TargetSite.Name + " Номер продукта" + productPartNo);
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

            string partName = "";
            string requiredPartName = "";


            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", firstRecNo);
                partNo = _preactor.ReadFieldString(_entityName, "Part No.", firstRecNo);
                opName = _preactor.ReadFieldString(_entityName, "Operation Name", firstRecNo);
                belongsToBOM = _preactor.ReadFieldString(_entityName, "Belongs to BOM", firstRecNo);
                multipleQuantity = _preactor.ReadFieldDouble(_entityName, "Multiple Quantity", firstRecNo);
                multiplyByOrderQuantity = _preactor.ReadFieldBool(_entityName, "Multiply by order quantity", firstRecNo);
                requiredPartNo = _preactor.ReadFieldString(_entityName, "Required Part No.", firstRecNo);
                requiredQuantity = _preactor.ReadFieldDouble(_entityName, "Required Quantity", firstRecNo);
                partName = _preactor.ReadFieldString(_entityName, "Product", firstRecNo);
                requiredPartName = _preactor.ReadFieldString(_entityName, "Required Product", firstRecNo);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Продукт" + productPartNo);
            }
            ProductBoMDTO result = new ProductBoMDTO()
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
                PartName = partName,
                RequiredPartName = requiredPartName

            };
            logger.Trace($"Получение спецификации продукта по номеру завершено {productPartNo}");
            return result;
        }
        public void Update(ProductBoMDTO obj)
        {
            ProductBoMDTO foundPi = GetByNo(obj.PartNo);
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
        ~ProductBoMRepo()
        {
            LogManager.Flush();
        }

    }
}
