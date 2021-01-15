using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AEther
{
    public static class GenericOperator<T, S>
    {

        public delegate Expression UnaryOperator(Expression a);
        public delegate S Operator(T x);

        static Operator Compile(UnaryOperator body)
        {
            var a = Expression.Parameter(typeof(T), "a");
            return Expression.Lambda<Operator>(body(a), a).Compile();
        }

        public static readonly Operator Minus = Compile(a => Expression.Negate(a));

    }

    public static class GenericOperator<T, S, R>
    {

        public delegate Expression BinaryOperator(Expression a, Expression b);
        public delegate R Operator(T left, S right);
        
        static Operator Compile(BinaryOperator body)
        {
            var a = Expression.Parameter(typeof(T), "a");
            var b = Expression.Parameter(typeof(S), "b");
            return Expression.Lambda<Operator>(body(a, b), a, b).Compile();
        }

        public static readonly Operator Add = Compile((a, b) => Expression.Add(a, b));
        public static readonly Operator Subtract = Compile((a, b) => Expression.Subtract(a, b));
        public static readonly Operator Multiply = Compile((a, b) => Expression.Multiply(a, b));
        public static readonly Operator Divide = Compile((a, b) => Expression.Divide(a, b));
        public static readonly Operator LessThan = Compile((a, b) => Expression.LessThan(a, b));
        public static readonly Operator Equal = Compile((a, b) => Expression.Equal(a, b));

    }

}
