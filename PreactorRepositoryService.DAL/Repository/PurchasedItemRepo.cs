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
    public class PurchasedItemRepo:IRepository<PurchasedItemDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Purchased Items";
        public PurchasedItemRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса PurchasedItemRepo");
            this._preactor = preactor;
        }
        /// <summary>
        /// Создает покупные элементы в преакторе, с помощью заполненого PurchasedItemDTO
        /// </summary>
        /// <param name="obj">параметр типа PurchasedItemDTO</param>
        /// <returns></returns>
        public bool Create(PurchasedItemDTO obj)
        {
            if (GetByNo(obj.PartNo) != null)
            {
                logger.Trace($"Экземпляр PurchasedItem уже существует {obj.PartNo}");
                return false;
            }

            try
            {
                int PurchasedItemRecNo = _preactor.CreateRecord(_entityName);
                //_preactor.WriteField(_entityName, "Number", PurchasedItemRecNo, obj.Id);
                _preactor.WriteField(_entityName, "Part No.", PurchasedItemRecNo, obj.PartNo);
                _preactor.WriteField(_entityName, "Description", PurchasedItemRecNo, obj.Description);
                logger.Trace($"Создан экземпляр PurchasedItem {obj.PartNo}");
                return true;
            }
            catch(Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра PurchasedItem {ex.Message} {obj.PartNo}");
            }
            return false;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName, new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр PurchasedItem успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра PurchasedItem {ex.Message} {id}");

            }
        }
        /// <summary>
        /// Получает список всех ресурсов в преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PurchasedItemDTO> GetAll()
        {
            logger.Trace($"Получение списка PurchasedItem начато");
            int recCount = _preactor.RecordCount(_entityName);
            string piName = "-1";
            List<PurchasedItemDTO> res = new List<PurchasedItemDTO>();
            PurchasedItemDTO foundPurchasedItem = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    piName = _preactor.ReadFieldString(_entityName, "Part No.", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение PurchasedItem по всем записям) " + ex.TargetSite.Name);
                }
                foundPurchasedItem = GetByNo(piName);
                res.Add(foundPurchasedItem);
            }
            logger.Trace($"Получение списка PurchasedItem закончено");
            return res;
        }

        /// <summary>
        /// Получает Покупной элемент по его имени в Preactor поле Name
        /// </summary>
        /// <param name="partNo">Имя PurchasedItem в преакторе поле Name</param>
        /// <returns></returns>
        public PurchasedItemDTO GetByNo(string partNo)
        {
            logger.Trace($"Получение PurchasedItem по номеру начато {partNo}");
            string requestStr = $"~{{$Part No.}}~==~{partNo}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            if (recordNo < 0)
                return null;

            int _id = 0;
            string _partNo = "";
            string _description = "";
            try
            {
                _id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                _partNo = _preactor.ReadFieldString(_entityName, "Part No.", firstRecNo);
                _description = _preactor.ReadFieldString(_entityName, "Description", firstRecNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " PurchasedItem" + partNo);
            }
            PurchasedItemDTO result = new PurchasedItemDTO()
            {
                Id = _id,
                PartNo = _partNo,
                Description = _description
                
            };
            logger.Trace($"Получение PurchasedItem по имени завершено {partNo}");
            return result;
        }
        public void Update(PurchasedItemDTO obj)
        {
            PurchasedItemDTO foundPi = GetByNo(obj.PartNo);
            if (foundPi == null)
            {
                logger.Trace($"Экземпляр PurchasedItem не найден, нечего обновлять {obj.PartNo}");
                return;
            }
            int resourceRecNo = foundPi.Id;
            try
            {
                Delete(foundPi.Id);
                if (Create(obj) == true)
                    logger.Trace($"Обновлен экземпляр PurchasedItem {obj.PartNo}");
                return;
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра PurchasedItem {ex.Message} {obj.PartNo}");
            }
            return;
        }
        ~PurchasedItemRepo()
        {
            LogManager.Flush();
        }




    }
}
