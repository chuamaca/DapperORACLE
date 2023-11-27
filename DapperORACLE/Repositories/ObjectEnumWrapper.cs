using System.Linq.Expressions;
using System;
using System.Linq.Expressions;

namespace DapperORACLE.Repositories
{
    internal class ObjectEnumWrapper<TObject, TEnumType> : ObjectWrapper<TObject, TEnumType>
    {
        private readonly string _enumType;

        private readonly string _propertyName;

        private readonly Type _objectType;

        public ObjectEnumWrapper(string enumType, string propertyName, Type objectType)
            : base(propertyName, objectType)
        {
            _enumType = enumType;
            _propertyName = propertyName;
            _objectType = objectType;
        }

        protected override Func<TObject, TEnumType> CreateGetter()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TObject));
            return Expression.Lambda<Func<TObject, TEnumType>>(Expression.Convert(Expression.Convert(Expression.Property(Expression.Convert(parameterExpression, _objectType), _propertyName), typeof(int)), typeof(TEnumType)), new ParameterExpression[1] { parameterExpression }).Compile();
        }

        protected override Action<TObject, TEnumType> CreateSetter()
        {
            Type type = _objectType.Assembly.GetType(_objectType.Namespace + "." + _enumType);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TObject));
            ParameterExpression parameterExpression2 = Expression.Parameter(typeof(TEnumType));
            UnaryExpression right = Expression.Convert(Expression.Convert(parameterExpression2, typeof(int)), type);
            return Expression.Lambda<Action<TObject, TEnumType>>(Expression.Assign(Expression.PropertyOrField(Expression.Convert(parameterExpression, _objectType), _propertyName), right), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
        }
    }
}
