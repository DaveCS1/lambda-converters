﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Shared
{
    [ExcludeFromCodeCoverage]
    [DebuggerStepThrough]
    [SuppressMessage("ReSharper", "UncatchableException")]
    internal static class ExceptionAssert
    {
        static E Handle<E>(E exception, string? expectedParamName = null) where E : Exception
        {
            if (exception is ArgumentException argumentException)
            {
                if (string.Equals(expectedParamName ?? "", argumentException.ParamName ?? "", StringComparison.Ordinal))
                {
                    return exception;
                }

                throw new AssertInconclusiveException(
                    string.Format(
                        "{0}<{1}> inconclusive. Expected '{2}': <{3}>. Actual: <{4}>",
                        nameof(Throws),
                        typeof(E).Name,
                        nameof(ArgumentException.ParamName),
                        expectedParamName ?? "(null)",
                        argumentException.ParamName ?? "(null)"));
            }

            if (expectedParamName != null)
            {
                throw new AssertInconclusiveException(
                    string.Format("{0}<{1}> inconclusive. '{2}' is not used.", nameof(Throws), typeof(E).Name, nameof(expectedParamName)));
            }

            return exception;
        }

        public static E Throws<E>([InstantHandle] Action action, string? expectedParamName = null) where E : Exception
        {
            try
            {
                action();
            }
            catch (E e)
            {
                return Handle(e, expectedParamName);
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AssertFailedException(
                    string.Format(
                        "{0}<{1}> failed. Expected exception: '{1}'. Actual: '{2}' - {3}.",
                        nameof(Throws),
                        typeof(E).Name,
                        e.GetType().Name,
                        e));
            }

            throw new AssertFailedException(string.Format("{0}<{1}> failed. Exception '{1}' not thrown.", nameof(Throws), typeof(E).Name));
        }
    }
}