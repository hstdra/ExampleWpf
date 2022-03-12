using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace WpfApp.Helpers;

public static class ObjectHelper
{
    internal static void SetDeepValue<TObject, T>(this TObject? target, Expression<Func<TObject, T>> propertyToSet,
        T valueToSet)
    {
        List<MemberInfo> members = new List<MemberInfo>();

        Expression? exp = propertyToSet.Body;

        // There is a chain of getters in propertyToSet, with at the 
        // beginning a ParameterExpression. We put the MemberInfo of
        // these getters in members. We don't really need the 
        // ParameterExpression

        while (exp != null)
        {
            if (exp is MemberExpression mi)
            {
                members.Add(mi.Member);
                exp = mi.Expression;
            }
            else
            {
                if (exp is not ParameterExpression)
                {
                    throw new NotSupportedException();
                }

                break;
            }
        }

        if (members.Count == 0)
        {
            // We need at least a getter
            throw new NotSupportedException();
        }

        // Now we must walk the getters (excluding the last).
        object? targetObject = target;

        // We have to walk the getters from last (most inner) to second
        // (the first one is the one we have to use as a setter)
        for (int i = members.Count - 1; i >= 1; i--)
        {
            var pi = members[i] as PropertyInfo;

            if (pi != null)
            {
                targetObject = pi.GetValue(targetObject);
            }
            else
            {
                FieldInfo fi = (FieldInfo) members[i];
                targetObject = fi.GetValue(targetObject);
            }
        }

        // The first one is the getter we treat as a setter
        {
            var pi = members[0] as PropertyInfo;
            if (pi != null)
            {
                pi.SetValue(targetObject, valueToSet);
            }
            else
            {
                FieldInfo fi = (FieldInfo) members[0];
                fi.SetValue(targetObject, valueToSet);
            }
        }
    }
}