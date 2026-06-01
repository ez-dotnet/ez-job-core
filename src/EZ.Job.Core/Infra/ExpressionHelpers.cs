using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace EZ.Job.Core;

public static class ExpressionHelpers
{
    private static readonly ConcurrentDictionary<MethodInfo, Func<object, object?[], object?>> _invokers = new(MethodInfoEqualityComparer.Instance);

    public static (MethodInfo Method, object?[] Arguments) Extract<T>(Expression<Action<T>> expression)
    {
        var call = GetMethodCall(expression);
        return (call.Method, ExtractArguments(call));
    }

    public static (MethodInfo Method, object?[] Arguments) Extract<T>(Expression<Func<T, Task>> expression)
    {
        var call = GetMethodCall(expression);
        return (call.Method, ExtractArguments(call));
    }

    public static Func<object, object?[], object?> GetOrCreateInvoker(MethodInfo method)
    {
        return _invokers.GetOrAdd(method, CreateInvoker);
    }

    private static MethodCallExpression GetMethodCall(LambdaExpression expression)
    {
        if (expression.Body is MethodCallExpression call)
            return call;

        if (expression.Body is UnaryExpression unary && unary.Operand is MethodCallExpression call2)
            return call2;

        throw new ArgumentException("Expression must be a method call.", nameof(expression));
    }

    private static object?[] ExtractArguments(MethodCallExpression call)
    {
        var args = new object?[call.Arguments.Count];
        for (var i = 0; i < call.Arguments.Count; i++)
        {
            args[i] = EvaluateExpression(call.Arguments[i]);
        }
        return args;
    }

    private static object? EvaluateExpression(Expression expr)
    {
        switch (expr)
        {
            case ConstantExpression constant:
                return constant.Value;

            case MemberExpression member when member.Expression is ConstantExpression closure:
                return member.Member switch
                {
                    FieldInfo field => field.GetValue(closure.Value),
                    PropertyInfo prop => prop.GetValue(closure.Value),
                    _ => CompileAndInvoke(expr)
                };

            case MemberExpression nested when nested.Expression is MemberExpression inner:
            {
                var obj = EvaluateExpression(inner);
                if (obj is null) return null;
                return nested.Member switch
                {
                    FieldInfo field => field.GetValue(obj),
                    PropertyInfo prop => prop.GetValue(obj),
                    _ => CompileAndInvoke(expr)
                };
            }

            case UnaryExpression unary when unary.NodeType == ExpressionType.Convert
                && unary.Operand is MemberExpression convMember
                && convMember.Expression is ConstantExpression convClosure:
                return convMember.Member switch
                {
                    FieldInfo field => field.GetValue(convClosure.Value),
                    PropertyInfo prop => prop.GetValue(convClosure.Value),
                    _ => CompileAndInvoke(expr)
                };

            default:
                return CompileAndInvoke(expr);
        }
    }

    private static object? CompileAndInvoke(Expression expr)
    {
        return Expression.Lambda<Func<object?>>(
            Expression.Convert(expr, typeof(object)))
            .Compile()();
    }

    private static Func<object, object?[], object?> CreateInvoker(MethodInfo method)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var args = Expression.Parameter(typeof(object?[]), "args");

        var typedInstance = Expression.Convert(instance, method.DeclaringType!);

        var parameters = method.GetParameters();
        var castArgs = new Expression[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var index = Expression.Constant(i);
            var arg = Expression.ArrayIndex(args, index);
            castArgs[i] = Expression.Convert(arg, parameters[i].ParameterType);
        }

        var call = Expression.Call(typedInstance, method, castArgs);

        if (method.ReturnType == typeof(void))
        {
            var body = Expression.Block(call, Expression.Default(typeof(object)));
            return Expression.Lambda<Func<object, object?[], object?>>(body, instance, args).Compile();
        }

        if (method.ReturnType == typeof(Task))
        {
            return Expression.Lambda<Func<object, object?[], object?>>(call, instance, args).Compile();
        }

        return Expression.Lambda<Func<object, object?[], object?>>(
            Expression.Convert(call, typeof(object)), instance, args).Compile();
    }

    private sealed class MethodInfoEqualityComparer : IEqualityComparer<MethodInfo>
    {
        public static readonly MethodInfoEqualityComparer Instance = new();

        public bool Equals(MethodInfo? x, MethodInfo? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.MetadataToken == y.MetadataToken && x.Module == y.Module;
        }

        public int GetHashCode(MethodInfo obj)
        {
            return HashCode.Combine(obj.MetadataToken, obj.Module);
        }
    }
}
