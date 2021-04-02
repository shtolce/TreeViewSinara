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
    public class IgnoreShortagesRepo:IRepository<IgnoreShortagesDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Ignore Shortages";
        public IgnoreShortagesRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса IgnoreShortagesRepo");
            this._preactor = preactor;
        }
        /// <summary>
        /// Создает покупные элементы в преакторе, с помощью заполненого IgnoreShortagesDTO
        /// </summary>
        /// <param name="obj">параметр типа IgnoreShortagesDTO</param>
        /// <returns></returns>
        public bool Create(IgnoreShortagesDTO obj)
        {
            if (GetByNo(obj.PartNo) != null)
            {
                logger.Trace($"Экземпляр IgnoreShortages уже существует {obj.PartNo} IntDemOrd{obj.InternalDemandOrder} ExtDemOrd{obj.ExternalDemandOrder}");
                return false;
            }

            foreach (IgnoreShortagesDTOItem shortageItem in obj.items)
            {
                try
                {
                    int IgnoreShortagesRecNo = _preactor.CreateRecord(_entityName);
                    //_preactor.WriteField(_entityName, "Number", IgnoreShortagesRecNo, obj.Id);
                    _preactor.WriteField(_entityName, "Part No.", IgnoreShortagesRecNo, shortageItem.PartNo);
                    _preactor.WriteField(_entityName, "Ignore Shortages", IgnoreShortagesRecNo, shortageItem.IgnoreShortages);
                    _preactor.WriteField(_entityName, "Internal Demand Order", IgnoreShortagesRecNo, shortageItem.InternalDemandOrder);
                    _preactor.WriteField(_entityName, "External Demand Order", IgnoreShortagesRecNo, shortageItem.ExternalDemandOrder);
                    logger.Trace($@"Создан экземпляр IgnoreShortages {shortageItem.PartNo} IntDemOrd{shortageItem.InternalDemandOrder} 
                                        ExtDemOrd{shortageItem.ExternalDemandOrder}");
                }
                catch (Exception ex)
                {
                    logger.Trace($@"Ошибка создания экземпляра IgnoreShortages {ex.Message} IntDemOrd{shortageItem.InternalDemandOrder}
                                    ExtDemOrd{shortageItem.ExternalDemandOrder}");
                    return false;
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
                logger.Trace($"Экземпляр IgnoreShortages успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра IgnoreShortages {ex.Message} {id}");

            }
        }
        /// <summary>
        /// Получает список всех ресурсов в преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IgnoreShortagesDTO> GetAll()
        {
            logger.Trace($"Получение списка IgnoreShortages начато");
            int recCount = _preactor.RecordCount("Products");
            string piName = "-1";
            List<IgnoreShortagesDTO> res = new List<IgnoreShortagesDTO>();
            IgnoreShortagesDTO foundIgnoreShortages = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    piName = _preactor.ReadFieldString("Products", "Part No.", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение IgnoreShortages по всем записям) " + ex.TargetSite.Name);
                }
                foundIgnoreShortages = GetByNo(piName);
                if (foundIgnoreShortages != null)
                    res.Add(foundIgnoreShortages);
            }
            logger.Trace($"Получение списка IgnoreShortages закончено");
            return res;
        }

        /// <summary>
        /// Получает дефициты по номеру продукта
        /// </summary>
        /// <param name="partNo">PartNo</param>
        /// <returns></returns>
        public IgnoreShortagesDTO GetByNo(string partNo)
        {
            logger.Trace($"Получение IgnoreShortages по номеру начато {partNo}");
            string requestStr = $"~{{$Part No.}}~==~{partNo}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            if (recordNo < 0)
                return null;

            int _id = 0;
            string _partNo = "";
            int _shortageIgnore = 0;
            int _internalDemandOrder = 0;
            int _externalDemandOrder = 0;
            List<IgnoreShortagesDTOItem> itemList = new List<IgnoreShortagesDTOItem>();
            if (recordNo < 0)
                return null;

            while (recordNo >= 0)
            {
                if (recordNo < 0)
                    break;
                try
                {
                    _id = _preactor.ReadFieldInt(_entityName, "Number", recordNo);
                    _partNo = _preactor.ReadFieldString(_entityName, "Part No.", recordNo);
                    _internalDemandOrder = _preactor.ReadFieldInt(_entityName, "Internal Demand Order", recordNo);
                    _externalDemandOrder = _preactor.ReadFieldInt(_entityName, "External Demand Order", recordNo);
                    _shortageIgnore = _preactor.ReadFieldInt(_entityName, "Ignore Shortages", recordNo);
                    IgnoreShortagesDTOItem item = new IgnoreShortagesDTOItem()
                    {
                        Id = _id,
                        PartNo = _partNo,
                        InternalDemandOrder = _internalDemandOrder,
                        ExternalDemandOrder = _externalDemandOrder,
                        IgnoreShortages = _shortageIgnore
                    };
                    itemList.Add(item);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " IgnoreShortages" + partNo);
                }
                recordNo = _preactor.FindMatchingRecord(_entityName, recordNo, requestStr);
            }

            try
            {
                _id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                _partNo = _preactor.ReadFieldString(_entityName, "Part No.", firstRecNo);
                _internalDemandOrder = _preactor.ReadFieldInt(_entityName, "Internal Demand Order", firstRecNo);
                _externalDemandOrder = _preactor.ReadFieldInt(_entityName, "External Demand Order", firstRecNo);
                _shortageIgnore = _preactor.ReadFieldInt(_entityName, "Ignore Shortages", firstRecNo);
                IgnoreShortagesDTO result = new IgnoreShortagesDTO()
                {
                    PartNo = _partNo,
                    InternalDemandOrder = _internalDemandOrder,
                    ExternalDemandOrder = _externalDemandOrder,
                    items = itemList
                };
                logger.Trace($"Получение IgnoreShortages по PartNo завершено {partNo}");
                return result;

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " IgnoreShortages" + partNo);
                return null;
            }
        }
        public void Update(IgnoreShortagesDTO obj)
        {
            IgnoreShortagesDTO foundPi = GetByNo(obj.PartNo);
            if (foundPi == null)
            {
                logger.Trace($"Экземпляр IgnoreShortages не найден, нечего обновлять {obj.PartNo}");
                return;
            }
            try
            {
                foreach (IgnoreShortagesDTOItem item in foundPi.items)
                {
                    Delete(item.Id);
                }
                if (Create(obj) == true)
                    logger.Trace($"Обновлен экземпляр IgnoreShortages {obj.PartNo}");
                return;
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра IgnoreShortages {ex.Message} {obj.PartNo}");
            }
            return;
        }
        ~IgnoreShortagesRepo()
        {
            LogManager.Flush();
        }




    }
}
