using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace EventSauceApi.Extensions
{
    internal static class MvcOptionsExtensions
    {
        public static void UseGeneralRoutePrefix(this MvcOptions opts, string prefix) =>
            opts.Conventions.Add(new RoutePrefixConvention(new RouteAttribute(prefix)));

        private class RoutePrefixConvention : IApplicationModelConvention
        {
            private readonly AttributeRouteModel _routePrefix;

            public RoutePrefixConvention(IRouteTemplateProvider route) =>
                _routePrefix = new AttributeRouteModel(route);

            public void Apply(ApplicationModel application)
            {
                foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
                {
                    selector.AttributeRouteModel = selector.AttributeRouteModel != null
                        ? AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, selector.AttributeRouteModel)
                        : _routePrefix;
                }
            }
        }
    }
}
