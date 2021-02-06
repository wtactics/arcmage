using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Arcmage.Game.Api.Utils
{
    public static class QueryHelper
    {
        public static bool PropertyExists<T>(string propertyName)
        {
            return GetPropertyType<T>(propertyName) != null;
        }

        public static Type GetPropertyType<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;
            var subproperties = propertyName.Split('.');
            var type = typeof (T);
            foreach (var subproperty in subproperties)
            {
                var propertyType = type.GetProperty(subproperty,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyType == null) return null;
                type = propertyType.PropertyType;
            }
            return type;
        }


        public static Expression<Func<T, RT>> GetPropertyExpression<T, RT>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;

            var paramterExpression = Expression.Parameter(typeof (T), "x");
            Expression body = paramterExpression;

            var subproperties = propertyName.Split('.');
            foreach (var subproperty in subproperties)
            {
                body = Expression.PropertyOrField(body, subproperty);

            }
            return (Expression<Func<T, RT>>) Expression.Lambda(body, paramterExpression);
        }
    }

}
