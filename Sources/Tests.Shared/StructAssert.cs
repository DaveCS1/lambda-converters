﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Shared
{
    [ExcludeFromCodeCoverage]
    internal static class StructAssert
    {
        static void AssertOverridesMethod(
            [JetBrains.Annotations.NotNull] this Type type,
            [JetBrains.Annotations.NotNull] string name,
            [JetBrains.Annotations.NotNull] [ItemNotNull] Type[] parameterTypes,
            Type returnType)
        {
            var method = type.GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly,
                null,
                CallingConventions.Any,
                parameterTypes,
                null);

            Assert.IsNotNull(method);
            Assert.AreEqual(returnType, method.ReturnType);
            Assert.IsTrue(method.IsVirtual);
        }

        static void AssertHasStaticMethod(
            [JetBrains.Annotations.NotNull] this Type type,
            [JetBrains.Annotations.NotNull] string name,
            [JetBrains.Annotations.NotNull] [ItemNotNull] Type[] parameterTypes,
            Type returnType,
            bool isOperator)
        {
            var method = type.GetMethod(
                name,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,
                null,
                CallingConventions.Any,
                parameterTypes,
                null);

            Assert.IsNotNull(method);
            Assert.AreEqual(returnType, method.ReturnType);
            Assert.AreEqual(isOperator, method.IsSpecialName);
        }

        static void AssertDoesNotHaveStaticMethod(
            [JetBrains.Annotations.NotNull] this Type type,
            [JetBrains.Annotations.NotNull] string name,
            [JetBrains.Annotations.NotNull][ItemNotNull]
            Type[] parameterTypes)
        {
            var method = type.GetMethod(
                name,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,
                null,
                CallingConventions.Any,
                parameterTypes,
                null);

            Assert.IsNull(method);
        }

        public static void IsCorrect<T>(bool mustBeComparable = false) where T : struct
        {
            var value = default(T);

            Assert.IsInstanceOfType(value, typeof(IEquatable<T>));

            var type = typeof(T);

            type.AssertOverridesMethod(nameof(object.Equals), new[] { typeof(object) }, typeof(bool));
            type.AssertOverridesMethod(nameof(GetHashCode), ArrayUtils.GetEmpty<Type>(), typeof(int));

            type.AssertHasStaticMethod("op_Equality", new[] { type, type }, typeof(bool), true);
            type.AssertHasStaticMethod("op_Inequality", new[] { type, type }, typeof(bool), true);

            Assert.IsFalse(value.Equals(null));

            if (mustBeComparable)
            {
                Assert.IsInstanceOfType(value, typeof(IComparable<T>));

                type.AssertHasStaticMethod("Compare", new[] { type, type }, typeof(int), false);
                type.AssertHasStaticMethod("op_GreaterThan", new[] { type, type }, typeof(bool), true);
                type.AssertHasStaticMethod("op_GreaterThanOrEqual", new[] { type, type }, typeof(bool), true);
                type.AssertHasStaticMethod("op_LessThan", new[] { type, type }, typeof(bool), true);
                type.AssertHasStaticMethod("op_LessThanOrEqual", new[] { type, type }, typeof(bool), true);
            }
            else
            {
                Assert.IsNotInstanceOfType(value, typeof(IComparable<T>));

                type.AssertDoesNotHaveStaticMethod("Compare", new[] { type, type });
                type.AssertDoesNotHaveStaticMethod("op_GreaterThan", new[] { type, type });
                type.AssertDoesNotHaveStaticMethod("op_GreaterThanOrEqual", new[] { type, type });
                type.AssertDoesNotHaveStaticMethod("op_LessThan", new[] { type, type });
                type.AssertDoesNotHaveStaticMethod("op_LessThanOrEqual", new[] { type, type });
            }
        }

        public static void AreEqual<T>(
            T value,
            [JetBrains.Annotations.NotNull] Func<T, T, bool> equalityOperator,
            [JetBrains.Annotations.NotNull] Func<T, T, bool> inequalityOperator) where T : struct, IEquatable<T>
        {
            Assert.IsTrue(value.Equals(value));
            Assert.IsTrue(value.Equals(value as object));
            Assert.IsTrue(equalityOperator(value, value)); // value == value
            Assert.IsFalse(inequalityOperator(value, value)); // value != value
        }

        public static void AreNotEqual<T>(
            T x,
            T y,
            [JetBrains.Annotations.NotNull] Func<T, T, bool> equalityOperator,
            [JetBrains.Annotations.NotNull] Func<T, T, bool> inequalityOperator) where T : struct, IEquatable<T>
        {
            Assert.IsFalse(x.Equals(y));
            Assert.IsFalse(y.Equals(x));

            Assert.IsFalse(x.Equals(y as object));
            Assert.IsFalse(y.Equals(x as object));

            Assert.IsFalse(equalityOperator(x, y)); // x == y
            Assert.IsFalse(equalityOperator(y, x)); // y == x

            Assert.IsTrue(inequalityOperator(x, y)); // x != y
            Assert.IsTrue(inequalityOperator(y, x)); // y != x
        }
    }
}