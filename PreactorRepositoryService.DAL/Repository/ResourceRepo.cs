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
    public class ResourceRepo:IRepository<ResourceDTO>
    {
        private static Logger logger;
        private IPreactor _preactor;
        private string _entityName = "Resources";
        public ResourceRepo(IPreactor preactor)
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Создан экземпляр сервиса ResourceRepo");
            this._preactor = preactor;
        }
        /// <summary>
        /// Создает ресурс в преакторе, с помощью заполненого ResourceDTO
        /// для заполнения Constrains нужно заполнить соответствующий список в ResourceDTO
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Create(ResourceDTO obj)
        {
            if (GetByNo(obj.Name) != null)
            {
                logger.Trace($"Экземпляр Resource уже существует {obj.Name}");
                return false;
            }

            try
            {
                int resourceRecNo = _preactor.CreateRecord(_entityName);
                //_preactor.WriteField(_entityName, "Number", resourceRecNo, obj.Id);
                _preactor.WriteField(_entityName, "Name", resourceRecNo, obj.Name);
                _preactor.WriteField(_entityName, "Changeover Group", resourceRecNo, obj.ChangeoverGroup);
                _preactor.WriteField(_entityName, "Finite or Infinite", resourceRecNo, obj.FiniteModeBehaviorDesc);
                _preactor.WriteField(_entityName, "Infinite Mode Behavior", resourceRecNo, obj.InfiniteModeBehaviorDesc);
                int idxConstr =0;
                _preactor.SetAutoListSize(_entityName, "Secondary Constraints", resourceRecNo, obj.SecondaryConstraints.Count);
                foreach (string el in obj.SecondaryConstraints)
                {
                    idxConstr++;
                    _preactor.WriteListField(_entityName, "Secondary Constraints", resourceRecNo, el, idxConstr);
                }

                logger.Trace($"Создан экземпляр Resource {obj.Name}");
                return true;
            }
            catch
            {
                logger.Trace($"Ошибка создания экземпляра Resource  {obj.Name}");
            }
            return false;
        }

        public void Delete(int id)
        {
            try
            {
                int recordNumber = _preactor.GetRecordNumber(_entityName,new PrimaryKey(id));
                _preactor.DeleteRecord(_entityName, recordNumber);
                logger.Trace($"Экземпляр Resource успешно удален, номер записи {id}");
            }
            catch (Exception ex)
            {
                logger.Trace($"Ошибка удаления экземпляра Resource {ex.Message} {id}");

            }
        }
        /// <summary>
        /// Получает список всех ресурсов в преакторе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ResourceDTO> GetAll()
        {
            logger.Trace($"Получение списка ресурсов начато");
            int recCount = _preactor.RecordCount(_entityName);
            string resName = "-1";
            List<ResourceDTO> res = new List<ResourceDTO>();
            ResourceDTO foundResource = null;
            for (int i = 1; i <= recCount; i++)
            {
                try
                {
                    resName = _preactor.ReadFieldString(_entityName, "Name", i);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (получение ресурсов по всем записям) " + ex.TargetSite.Name);
                }
                foundResource = GetByNo(resName);
                res.Add(foundResource);
            }
            logger.Trace($"Получение списка ресурсов закончено");
            return res;
        }

        /// <summary>
        /// Получает ресурс по его имени в Preactor поле Name
        /// </summary>
        /// <param name="ResName">Имя ресурса в преакторе поле Name</param>
        /// <returns></returns>
        public ResourceDTO GetByNo(string ResName)
        {
            logger.Trace($"Получение ресурса по номеру начато {ResName}");
            string requestStr = $"~{{$Name}}~==~{ResName}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            List<string> secondaryConstraintList = new List<string>();
            if (recordNo < 0)
                return null;
            int constraintCount = _preactor.MatrixFieldSize(_entityName, "Secondary Constraints", recordNo).X;
            for (int i=1; i<=constraintCount; i++)
            {
                try
                {
                    string secConstr = _preactor.ReadFieldString(_entityName, "Secondary Constraints", recordNo, i);
                    secondaryConstraintList.Add(secConstr);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (resource secondary constraint) " + ex.TargetSite.Name + " resource name" + ResName);
                }
            }

            int id = 0;
            double attr2 = 0.0;
            string resName = "";
            string finiteModeName = "";
            string infiniteModeName = "";
            string changeoverGroup = "";
            ResourceBehaviourType FiniteModeBehavior = default(ResourceBehaviourType);
            ResourceBehaviourType InfiniteModeBehavior = default(ResourceBehaviourType);
            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                resName = _preactor.ReadFieldString(_entityName, "Name", firstRecNo);
                finiteModeName = _preactor.ReadFieldString(_entityName, "Finite or Infinite", firstRecNo);
                infiniteModeName = _preactor.ReadFieldString(_entityName, "Infinite Mode Behavior", firstRecNo);
                FiniteModeBehavior = ResourceDTO.getEnumValueByDescription(finiteModeName);
                InfiniteModeBehavior = ResourceDTO.getEnumValueByDescription(infiniteModeName);
                changeoverGroup = _preactor.ReadFieldString(_entityName, "Changeover Group", firstRecNo);
                attr2 = _preactor.ReadFieldDouble(_entityName, "Attribute 2", firstRecNo);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Resource" + ResName);
            }
            ResourceDTO result = new ResourceDTO()
            {
                Id = id,
                Name = resName,
                SecondaryConstraints = secondaryConstraintList,
                FiniteModeBehavior = FiniteModeBehavior,
                InfiniteModeBehavior = InfiniteModeBehavior,
                ChangeoverGroup = changeoverGroup,
                Attribute2 = (float)attr2

            };
            logger.Trace($"Получение Resource по имени завершено {ResName}");
            return result;
        }

        /// <summary>
        /// Получает ресурс по его Id в Preactor поле Name
        /// </summary>
        /// <param name="ResId">Id ресурса в преакторе поле Name</param>
        /// <returns></returns>
        public ResourceDTO GetById(int ResId)
        {
            logger.Trace($"Получение ресурса по номеру начато {ResId}");
            string requestStr = $"~{{#Number}}~==~{ResId}~";
            int recordNo = _preactor.FindMatchingRecord(_entityName, 0, requestStr);
            int firstRecNo = recordNo;
            List<string> secondaryConstraintList = new List<string>();
            if (recordNo < 0)
                return null;
            int constraintCount = _preactor.MatrixFieldSize(_entityName, "Secondary Constraints", recordNo).X;
            for (int i = 1; i <= constraintCount; i++)
            {
                try
                {
                    string secConstr = _preactor.ReadFieldString(_entityName, "Secondary Constraints", recordNo, i);
                    secondaryConstraintList.Add(secConstr);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message + " (resource secondary constraint) " + ex.TargetSite.Name + " resource name" + ResId);
                }
            }

            int id = 0;
            string resName = "";
            string finiteModeName = "";
            string infiniteModeName = "";
            string changeoverGroup = "";
            double attr2 = 0;
            ResourceBehaviourType FiniteModeBehavior = default(ResourceBehaviourType);
            ResourceBehaviourType InfiniteModeBehavior = default(ResourceBehaviourType);
            try
            {
                id = _preactor.ReadFieldInt(_entityName, "Number", firstRecNo);
                attr2 = _preactor.ReadFieldDouble(_entityName, "Attribute 2", firstRecNo);
                resName = _preactor.ReadFieldString(_entityName, "Operation Name", firstRecNo);
                finiteModeName = _preactor.ReadFieldString(_entityName, "Finite or Infinite", firstRecNo);
                infiniteModeName = _preactor.ReadFieldString(_entityName, "Infinite Mode Behavior", firstRecNo);
                FiniteModeBehavior = ResourceDTO.getEnumValueByDescription(finiteModeName);
                InfiniteModeBehavior = ResourceDTO.getEnumValueByDescription(infiniteModeName);
                changeoverGroup = _preactor.ReadFieldString(_entityName, "Changeover Group", firstRecNo);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + " (header)" + ex.TargetSite.Name + " Resource" + ResId);
            }
            ResourceDTO result = new ResourceDTO()
            {
                Id = id,
                Name = resName,
                SecondaryConstraints = secondaryConstraintList,
                FiniteModeBehavior = FiniteModeBehavior,
                InfiniteModeBehavior = InfiniteModeBehavior,
                ChangeoverGroup = changeoverGroup,
                Attribute2=(float)attr2
            };
            logger.Trace($"Получение Resource по Id завершено {ResId}");
            return result;
        }

        public void Update(ResourceDTO obj)
        {

            ResourceDTO foundRes = GetByNo(obj.Name);
            if (foundRes == null)
            {
                logger.Trace($"Экземпляр Resource не найден, нечего обновлять {obj.Name}");
                return;
            }
            int resourceRecNo = foundRes.Id;
            try
            {
                Delete(foundRes.Id);
                if (Create(obj)==true)
                    logger.Trace($"Обновлен экземпляр Resource {obj.Name}");
                return;
            }
            catch(Exception ex)
            {
                logger.Trace($"Ошибка создания экземпляра Resource {ex.Message} {obj.Name}");
            }
            return;

        }
        ~ResourceRepo()
        {
            LogManager.Flush();
        }




    }
}
