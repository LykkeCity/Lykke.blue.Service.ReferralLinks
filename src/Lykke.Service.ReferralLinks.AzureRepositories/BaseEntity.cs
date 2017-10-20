using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Reflection;
namespace Lykke.Service.ReferralLinks.AzureRepositories
{
    public class BaseEntity : TableEntity
    {
        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            foreach (var p in GetType().GetProperties().Where(x =>
                (x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)) && properties.ContainsKey(x.Name)))
            {
                var value = properties[p.Name].StringValue;
                p.SetValue(this, value != null ? Convert.ToDecimal(value, CultureInfo.InvariantCulture) : (decimal?)null);
            }

            foreach (var p in GetType().GetProperties().Where(x => x.PropertyType.GetTypeInfo().IsEnum && properties.ContainsKey(x.Name)))
                p.SetValue(this, Enum.ToObject(p.PropertyType, properties[p.Name].Int32Value));

        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = base.WriteEntity(operationContext);

            foreach (var p in GetType().GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                properties.Add(p.Name, new EntityProperty(p.GetValue(this)?.ToString()));

            foreach (var p in GetType().GetProperties().Where(x => x.PropertyType.GetTypeInfo().IsEnum))
                properties.Add(p.Name, new EntityProperty((int)p.GetValue(this)));

            return properties;
        }
    }
}
