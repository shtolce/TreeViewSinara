using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{

    /// <summary>
    /// Метод расширения для вспомогательной функции (ResourceDTO) getDescription
    /// </summary>
    public static class ExtensionMethods
    {
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }

    public enum ResourceBehaviourType
    {
        [Description("Бесконечность")]
        Infinite = 0,
        [Description("Конечный")]
        Finite=1,
        [Description("Неограниченный с шаблонами смещения")]
        InfiniteWithStoryboardTemplate=2,

    }

    public class ResourceDTO
    {
        /// <summary>
        /// Получает значение перечисления по его описанию в аннотации Description
        /// </summary>
        /// <param name="enumDescStr"></param>
        /// <returns></returns>
        public static ResourceBehaviourType getEnumValueByDescription(string enumDescStr)
        {
            return Enum.GetValues(typeof(ResourceBehaviourType))
                .Cast<ResourceBehaviourType>()
                .FirstOrDefault(v => v.GetDescription() == enumDescStr);
        }
        /// <summary>
        /// /Получает аннотацию Descrtiption поля типа перечисления по его значению
        /// </summary>
        /// <param name="enumElement">Значение поля типа перечисления </param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum enumElement)
        {
            Type type = enumElement.GetType();

            MemberInfo[] memInfo = type.GetMember(enumElement.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return enumElement.ToString();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResourceGroupName { get; set; }
        public string ChangeoverGroup { get; set; }
        public int FiniteOrInfinite { get; set; }
        public ResourceBehaviourType FiniteModeBehavior { get; set; }
        public string Attribute1 { get; set; }
        public float Attribute2 { get; set; }
        public DateTime Attribute3 { get; set; }

        


        public string FiniteModeBehaviorDesc
        {
            get
            {
                return GetEnumDescription(FiniteModeBehavior);
            }
        }
        public ResourceBehaviourType InfiniteModeBehavior { get; set; }
        public string InfiniteModeBehaviorDesc
        {
            get
            {
                return GetEnumDescription(InfiniteModeBehavior);
            }
        }
        public List<string> SecondaryConstraints { get; set;}
        public List<SecondaryConstraintDTO> SecConstraints { get; set; }
    }
}
