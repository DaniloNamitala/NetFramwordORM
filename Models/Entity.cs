using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataSource.Models
{
    public class Entity
    {
        public bool Save()
        {
            MethodInfo method = typeof(DatabaseConnection).GetMethod("Update");
            method = method.MakeGenericMethod(this.GetType());
            return (bool)method.Invoke(DatabaseConnection.Instance, new[] { this });
        }
    }
}
