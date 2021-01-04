using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationDemosDAL.Utilities
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription<TEnum>(this TEnum item)
        => item.GetType()
               .GetField(item.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false)
               .Cast<DescriptionAttribute>()
               .FirstOrDefault()?.Description ?? string.Empty;

        public static void SeedEnumValues<T, TEnum>(this DbSet<T> dbSet, Func<TEnum, T> converter, Expression<Func<T, object>> idExpression, DbContext context)
            where T : class
        {

            var enumValues = Enum.GetValues(typeof(TEnum))
             .Cast<object>()
             .Select(value => converter((TEnum)value))
             .ToList();

            using (var transaction = context.Database.BeginTransaction())
            {
                var tableName = typeof(T).Name;
                context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[{tableName}s] ON");

                var dbSetE = context.Set<T>();
                enumValues.ForEach(value => dbSetE.AddOrUpdate(ref value, idExpression));
                context.SaveChanges();

                context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[{tableName}s] OFF");
                transaction.Commit();
            }        
        }
    }
}
