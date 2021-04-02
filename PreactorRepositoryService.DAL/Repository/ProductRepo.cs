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
    public class ProductRepo:IRepository<ProductDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private ResourceGroupRepo resGrpRepo;
        private ResourceRepo resRepo;
        private string _entityName = "Products";
        public ProductRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса ProductRepo");
            this._preactor = preactor;
            resGrpRepo = new ResourceGroupRepo(_preactor);
            resRepo = new ResourceRepo(_preactor);
        }
        /// <summary>
        /// Создает элемент Product в преакторе
        /// </summary>
        /// <param name="obj">Передайте объект типа ProductDTO, подчиненые строки поместите в Operations</param>
        /// <returns></returns>
        public bool Create(ProductDTO obj)
        {

            if (GetByNo(obj.Product) != null)
            {
                logger.Trace($"Экземпляр продукта уже существует {obj.Product}");
                return false;
            }

            try
            {
                if (obj.OpCount == 0)
                {
                    int productRecNo = _preactor.CreateRecord(_entityName);

                    //_preactor.WriteField(_entityName, "Number", productRecNo, obj.Id);
                    _preactor.WriteField(_entityName, "Op. No.", productRecNo, obj.OpNo);
                    _preactor.WriteField(_entityName, "Product", productRecNo, obj.Product);
                    _preactor.WriteField(_entityName, "Operation Name", productRecNo, obj.OpName);
                    //_preactor.WriteField(_entityName, "Resource", productRecNo, obj.Resource);
                    _preactor.WriteField(_entityName, "Resource Group", productRecNo, obj.ResourceGroup);
                    _preactor.WriteField(_entityName, "Part No.", productRecNo, obj.PartNo);
                }
                else
                {
                    foreach (OperationDTO objOp in obj.Operations)
                    {
                        int productRecNo = _preactor.CreateRecord(_entityName);

                        //_preactor.WriteField(_entityName, "Number", productRecNo, obj.Id);
                        _preactor.WriteField(_entityName, "Op. No.", productRecNo, objOp.OpNo);
                        _preactor.WriteField(_entityName, "Product", productRecNo, obj.Product);
                        _preactor.WriteField(_entityName, "Operation Name", productRecNo, objOp.OpName);
                        _preactor.WriteField(_entityName, "Part No.", productRecNo, objOp.OrderPartNo);
                        //_preactor.WriteField(_entityName, "Resource", productRecNo, objOp.Resource);
                        _preactor.WriteField(_entityName, "Resource Group", productRecNo, obj.ResourceGroup);
                        _preactor.WriteField(_entityName, "Parent Part", productRecNo, objOp.ParentPart==null?"Родитель": objOp.ParentPart);
                    }
                }
                logger.Trace($"Создан экземпляр продукта {obj.Product}");
                return true;
            }
            catch(Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра продукта {ex.Message} {obj.Product}");
            }
            return false;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName, new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр Product успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра Product {ex.Message} {id}");
            }
        }
        /// <summary>
        /// Получает список всех продуктов в преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ProductDTO> GetAll()
        {
            logger.Trace($"Получение списка продуктов начато");
            int recCount = _preactor.RecordCount(_entityName);
            string prdNo = "-1";
            string prdNoOldVal = "";
            List<ProductDTO> res = new List<ProductDTO>();
            ProductDTO foundProduct = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    prdNo = _preactor.ReadFieldString(_entityName, "Product", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение продуктов по всем записям) " + ex.TargetSite.Name);
                }
                if (prdNo != prdNoOldVal)
                {
                    foundProduct = GetByNo(prdNo);
                    res.Add(foundProduct);
                }
                prdNoOldVal = prdNo;
            }
            logger.Trace($"Получение списка продуктов закончено");
            return res;
        }
        /// <summary>
        /// Получает Product по наименованию продукта в преакторе, поле  Product
        /// </summary>
        /// <param name="productNo">Поле Product в преакторе</param>
        /// <returns></returns>
        public ProductDTO GetByNo(string productNo)
        {
            logger.Trace($"Получение продукта по номеру начато {productNo}");
            string requestStr = $"~{{$Part No.}}~==~{productNo}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId;
            int resourceId;

            List<OperationDTO> opList = new List<OperationDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", firstRecNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", firstRecNo);
                    OperationDTO op = new OperationDTO
                    {
                        Id = _preactor.ReadFieldInt(_entityName, "Number", recordNo),
                        OpNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo),
                        OpName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo),
                        PartNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo),
                        ParentPart = _preactor.ReadFieldString(_entityName, "Parent Part", firstRecNo),
                        //ResourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", firstRecNo)
                        ResourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name,
                        Resource = resRepo.GetById(resourceId)?.Name

                };
                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (product operation) " + ex.TargetSite.Name + " Номер продукта" + productNo);
                }
                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }


            int id = 0;
            int opNo = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string resource = "";
            string resourceGroup = "";
            string parentPart = "";
            double numericalAttribute1 = 0;
            double numericalAttribute2 = 0;
            double numericalAttribute3 = 0;

            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", firstRecNo);
                partNo = _preactor.ReadFieldString(_entityName, "Part No.", firstRecNo);
                product = _preactor.ReadFieldString(_entityName, "Product", firstRecNo);
                opName = _preactor.ReadFieldString(_entityName, "Operation Name", firstRecNo);
                resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", firstRecNo);
                resource = resRepo.GetById(resourceId)?.Name;
                resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", firstRecNo);
                resourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name;

                numericalAttribute1 = _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 1", firstRecNo);
                numericalAttribute2 = _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 2", firstRecNo);
                numericalAttribute3 = _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 3", firstRecNo);
                //resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", firstRecNo);
                parentPart = _preactor.ReadFieldString(_entityName, "Parent Part", firstRecNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Продукт" + productNo);
            }

            ProductDTO result = new ProductDTO()
            {
                Id = id,
                OpNo = opNo,
                Product = product,
                OpName = opName,
                PartNo = partNo,
                Resource=resource,
                ResourceGroup = resourceGroup,
                Operations = opList,
                NumericalAttribute1 = numericalAttribute1,
                NumericalAttribute2 = numericalAttribute2,
                NumericalAttribute3 = numericalAttribute3

            };
            logger.Trace($"Получение продукта по номеру завершено {productNo}");
            return result;
        }

        public ProductDTO GetById(int productNo)
        {
            logger.Trace($"Получение продукта по номеру начато {productNo}");
            string requestStr = $"~{{#Number}}~==~{productNo}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            int resourceGroupId;
            int resourceId;

            List<OperationDTO> opList = new List<OperationDTO>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", firstRecNo);
                    resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", firstRecNo);
                    OperationDTO op = new OperationDTO
                    {
                        Id = _preactor.ReadFieldInt(_entityName, "Number", recordNo),
                        OpNo = _preactor.ReadFieldInt(_entityName, "Op. No.", recordNo),
                        OpName = _preactor.ReadFieldString(_entityName, "Operation Name", recordNo),
                        PartNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo),
                        ParentPart = _preactor.ReadFieldString(_entityName, "Parent Part", firstRecNo),
                        //ResourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", firstRecNo)
                        ResourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name,
                        Resource = resRepo.GetById(resourceId)?.Name

                    };
                    opList.Add(op);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (product operation) " + ex.TargetSite.Name + " Номер продукта" + productNo);
                }
                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }


            int id = 0;
            int opNo = 0;
            string product = "";
            string opName = "";
            string partNo = "";
            string resource = "";
            string resourceGroup = "";
            string parentPart = "";
            double numericalAttribute1 = 0;
            double numericalAttribute2 = 0;
            double numericalAttribute3 = 0;

            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                opNo = _preactor.ReadFieldInt(_entityName, "Op. No.", firstRecNo);
                partNo = _preactor.ReadFieldString(_entityName, "Part No.", firstRecNo);
                product = _preactor.ReadFieldString(_entityName, "Product", firstRecNo);
                opName = _preactor.ReadFieldString(_entityName, "Operation Name", firstRecNo);
                resourceId = _preactor.ReadFieldInt(_entityName, "Required Resource", firstRecNo);
                resource = resRepo.GetById(resourceId)?.Name;
                resourceGroupId = _preactor.ReadFieldInt(_entityName, "Resource Group", firstRecNo);
                resourceGroup = resGrpRepo.GetById(resourceGroupId)?.Name;

                numericalAttribute1 = _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 1", firstRecNo);
                numericalAttribute2 = _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 2", firstRecNo);
                numericalAttribute3 = _preactor.ReadFieldDouble(_entityName, "Numerical Attribute 3", firstRecNo);
                //resourceGroup = _preactor.ReadFieldString(_entityName, "Resource Group", firstRecNo);
                parentPart = _preactor.ReadFieldString(_entityName, "Parent Part", firstRecNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Продукт" + productNo);
            }

            ProductDTO result = new ProductDTO()
            {
                Id = id,
                OpNo = opNo,
                Product = product,
                OpName = opName,
                PartNo = partNo,
                Resource = resource,
                ResourceGroup = resourceGroup,
                Operations = opList,
                NumericalAttribute1 = numericalAttribute1,
                NumericalAttribute2 = numericalAttribute2,
                NumericalAttribute3 = numericalAttribute3

            };
            logger.Trace($"Получение продукта по номеру завершено {productNo}");
            return result;
        }


        public void Update(ProductDTO obj)
        {
            ProductDTO foundPi = GetByNo(obj.PartNo);
            if (foundPi == null)
            {
                logger.Trace($"Экземпляр Product не найден, нечего обновлять {obj.PartNo}");
                return;
            }
            int resourceRecNo = foundPi.Id;
            try
            {
                foreach(OperationDTO op in foundPi.Operations)
                {
                    Delete(op.Id);
                    if (Create(obj) == true)
                        logger.Trace($"Обновлен экземпляр Product {obj.PartNo}");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра Product {ex.Message} {obj.PartNo}");
            }
            return;
        }
        ~ProductRepo()
        {
            LogManager.Flush();
        }

    }
}
