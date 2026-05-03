using ITW.Application.Organisation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Authorization.Modules;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireModuleAttribute : TypeFilterAttribute
{
    public RequireModuleAttribute(ModulCode modul)
        : base(typeof(ModuleAccessFilter))
    {
        Arguments = new object[] { modul };
        Order = int.MinValue + 100;
    }
}