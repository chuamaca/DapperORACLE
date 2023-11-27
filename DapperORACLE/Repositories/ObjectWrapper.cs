using System.Linq.Expressions;

namespace DapperORACLE.Repositories
{
    public class ObjectWrapper<TObject, TValue>
    {
        private readonly string _propertyName;

        private readonly Type _objectType;

        private Action<TObject, TValue> _setter;

        private Func<TObject, TValue> _getter;

        public ObjectWrapper(string propertyName, Type objectType)
        {
            _propertyName = propertyName;
            _objectType = objectType;
        }

        public void SetValue(TObject command, TValue value)
        {
            if (_setter == null)
            {
                _setter = CreateSetter();
            }

            _setter(command, value);
        }

        public TValue GetValue(TObject obj)
        {
            if (_getter == null)
            {
                _getter = CreateGetter();
            }

            return _getter(obj);
        }

        protected virtual Func<TObject, TValue> CreateGetter()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TObject));
            if (typeof(TObject) != _objectType)
            {
                return Expression.Lambda<Func<TObject, TValue>>(Expression.Property(Expression.Convert(parameterExpression, _objectType), _objectType.GetProperty(_propertyName)), new ParameterExpression[1] { parameterExpression }).Compile();
            }

            return Expression.Lambda<Func<TObject, TValue>>(Expression.Property(parameterExpression, _objectType.GetProperty(_propertyName)), new ParameterExpression[1] { parameterExpression }).Compile();
        }

        protected virtual Action<TObject, TValue> CreateSetter()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TObject));
            ParameterExpression parameterExpression2 = Expression.Parameter(typeof(TValue));
            return Expression.Lambda<Action<TObject, TValue>>(Expression.Assign(Expression.PropertyOrField(Expression.Convert(parameterExpression, _objectType), _propertyName), parameterExpression2), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
        }
    }
}
