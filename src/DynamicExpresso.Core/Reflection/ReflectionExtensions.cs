﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DynamicExpresso.Reflection
{
	internal static class ReflectionExtensions
	{
		public static DelegateInfo GetDelegateInfo(Type delegateType, string[] parametersNames)
		{
			MethodInfo method = delegateType.GetMethod("Invoke");
			if (method == null)
				throw new ArgumentException("The specified type is not a delegate");

			var delegateParameters = method.GetParameters();
			var parameters = new ParameterExpression[delegateParameters.Length];

			bool useCustomNames = parametersNames != null && parametersNames.Length > 0;

			if (useCustomNames && parametersNames.Length != parameters.Length)
				throw new ArgumentException(string.Format("Provided parameters names doesn't match delegate parameters, {0} parameters expected.", parameters.Length));

			for (int i = 0; i < parameters.Length; i++)
			{
				var paramName = useCustomNames ? parametersNames[i] : delegateParameters[i].Name;
				var paramType = delegateParameters[i].ParameterType;

				parameters[i] = Expression.Parameter(paramType, paramName);
			}

			return new DelegateInfo()
			{
				Parameters = parameters,
				ReturnType = method.ReturnType
			};
		}

		public static IEnumerable<MethodInfo> GetExtensionMethods(Type type)
		{
			if (type.IsSealed && !type.IsGenericType && !type.IsNested)
			{
				var query = from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
										where method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
										select method;

				return query;
			}

			return new MethodInfo[0];
		}

		public class DelegateInfo
		{
			public Type ReturnType { get; set; }
			public ParameterExpression[] Parameters { get; set; }
		}
	}
}
