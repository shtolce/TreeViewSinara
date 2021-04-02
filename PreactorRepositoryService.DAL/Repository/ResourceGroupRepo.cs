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
    public class ResourceGroupRepo:IRepository<ResourceGroupDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Resource Groups";
        private ResourceRepo resRepo;
        public ResourceGroupRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса ResourceGroupRepo");
            this._preactor = preactor;
            resRepo = new ResourceRepo(_preactor);
    }
    /// <summary>
    /// Создает группу ресурса с вложениями  в преакторе, с помощью заполненого ResourceGroupDTO
    /// </summary>
    /// <param name="obj">Заполните параметр  типа ResourceGroupDTO</param>
    /// <returns></returns>
    public bool Create(ResourceGroupDTO obj)
        {
            if (GetByNo(obj.Name) != null)
            {
                logger.Trace($"Экземпляр ResourceGroup уже существует {obj.Name}");
                return false;
            }

            try
            {
                int ResourceGroupRecNo = _preactor.CreateRecord(_entityName);
                //_preactor.WriteField(_entityName, "Number", ResourceGroupRecNo, obj.Id);
                _preactor.WriteField(_entityName, "Name", ResourceGroupRecNo, obj.Name);
                int idxRes =0;
                _preactor.SetAutoListSize(_entityName, "Resources", ResourceGroupRecNo, obj.Resources.Count);
                foreach (ResourceDTO el in obj.Resources)
                {
                    idxRes++;
                    _preactor.WriteListField(_entityName, "Resources", ResourceGroupRecNo, el.Name, idxRes);
                }

                logger.Trace($"Создан экземпляр ResourceGroup {obj.Name}");
                return true;
            }
            catch
            {
                logger.Trace($"Ошибка создания экземпляра ResourceGroup  {obj.Name}");
            }
            return false;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName, new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр Resource Group успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра Resource Group {ex.Message} {id}");

            }
        }

        /// <summary>
        /// Получает список групп ресурсов в Преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ResourceGroupDTO> GetAll()
        {
            logger.Trace($"Получение списка групп ресурсов начато");
            int recCount = _preactor.RecordCount(_entityName);
            string resName = "-1";
            List<ResourceGroupDTO> res = new List<ResourceGroupDTO>();
            ResourceGroupDTO foundResourceGroup = null;

            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    resName = _preactor.ReadFieldString(_entityName, "Name", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение групп ресурсов по всем записям) " + ex.TargetSite.Name);
                }
                foundResourceGroup = GetByNo(resName);
                res.Add(foundResourceGroup);
            }
            logger.Trace($"Получение списка групп ресурсов закончено");
            return res;
        }

        /// <summary>
        /// Получает группу ресурса по ее имени в Preactor поле Name
        /// </summary>
        /// <param name="ResName">Имя группы ресурса в преакторе поле Name</param>
        /// <returns></returns>
        public ResourceGroupDTO GetByNo(string ResName)
        {
            logger.Trace($"Получение группы ресурса по номеру начато {ResName}");
            string requestStr = $"~{{$Name}}~==~{ResName}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            ResourceDTO resFoundElement;
            List<ResourceDTO> resourcesList = new List<ResourceDTO>();
            if (recordNo < 0)
                return null;
            int constraintCount = _preactor.MatrixFieldSize(_entityName, "Resources", recordNo).X;
            for (int i=1; i<=constraintCount; i++)
            {
                try
                {
                    string resNameStr = _preactor.ReadFieldString(_entityName, "Resources", recordNo, i);
                    resFoundElement = resRepo.GetByNo(resNameStr);
                    if (resFoundElement!=null)
                        resourcesList.Add(resFoundElement);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (ResourceGroup Resources) " + ex.TargetSite.Name + " ResourceGroup name" + ResName);
                }
            }

            int id = 0;
            string resGrpName = "";
            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                resGrpName = _preactor.ReadFieldString(_entityName, "Name", firstRecNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " ResourceGroup" + ResName);
            }
            ResourceGroupDTO result = new ResourceGroupDTO()
            {
                Id = id,
                Name = resGrpName,
                Resources = resourcesList
            };
            logger.Trace($"Получение ResourceGroup по имени завершено {ResName}");
            return result;
        }

        /// <summary>
        /// Получает группу ресурса по ее Id в Preactor поле Name
        /// </summary>
        /// <param name="ResId">Id группы ресурса в преакторе поле Name</param>
        /// <returns></returns>
        public ResourceGroupDTO GetById(int ResId)
        {
            logger.Trace($"Получение группы ресурса по Id начато {ResId}");
            string requestStr = $"~{{#Number}}~==~{ResId}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            ResourceDTO resFoundElement;
            List<ResourceDTO> resourcesList = new List<ResourceDTO>();
            if (recordNo < 0)
                return null;
            int constraintCount = _preactor.MatrixFieldSize(_entityName, "Resources", recordNo).X;
            for (int i = 1; i <= constraintCount; i++)
            {
                try
                {
                    string resNameStr = _preactor.ReadFieldString(_entityName, "Resources", recordNo, i);
                    resFoundElement = resRepo.GetByNo(resNameStr);
                    if (resFoundElement != null)
                        resourcesList.Add(resFoundElement);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (ResourceGroup Resources) " + ex.TargetSite.Name + " ResourceGroup Id" + ResId);
                }
            }

            int id = 0;
            string resGrpName = "";
            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                resGrpName = _preactor.ReadFieldString(_entityName, "Name", firstRecNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " ResourceGroup" + ResId);
            }
            ResourceGroupDTO result = new ResourceGroupDTO()
            {
                Id = id,
                Name = resGrpName,
                Resources = resourcesList
            };
            logger.Trace($"Получение ResourceGroup по Id завершено {ResId}");
            return result;
        }


        public void Update(ResourceGroupDTO obj)
        {
            ResourceGroupDTO foundRes = GetByNo(obj.Name);
            if (foundRes == null)
            {
                logger.Trace($"Экземпляр Resource Group не найден, нечего обновлять {obj.Name}");
                return;
            }
            int resourceRecNo = foundRes.Id;
            try
            {
                Delete(foundRes.Id);
                if (Create(obj) == true)
                    logger.Trace($"Обновлен экземпляр Resource Group {obj.Name}");
                return;
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра Resource Group {ex.Message} {obj.Name}");
            }
            return;
        }
        ~ResourceGroupRepo()
        {
            LogManager.Flush();
        }

    }
}
