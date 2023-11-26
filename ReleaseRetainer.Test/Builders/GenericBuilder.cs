using System.Linq.Expressions;

namespace ReleaseRetainer.Test.Builders;

public abstract class GenericBuilder<T> where T : new()
{
    protected T Instance = new();

    public abstract GenericBuilder<T> CreateRandom();

    public GenericBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty value)
    {
        if (propertyPicker.Body is not MemberExpression memberExpression)
        {
            return this;
        }

        var propertyInfo = memberExpression.Member as System.Reflection.PropertyInfo;
        propertyInfo?.SetValue(Instance, value);

        return this;
    }

    public T Build()
    {
        var builtInstance = Instance;
        Instance = new T();
        return builtInstance;
    }
}