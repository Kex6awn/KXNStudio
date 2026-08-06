using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using System.IO;
using KxnPhotoStudio.Services.Interfaces;

namespace KxnPhotoStudio.Services.Implementations
{
    public class RazorViewRenderService : IRazorViewRenderService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewRenderService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(
    string viewName,
    TModel model)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());

            var viewResult = _viewEngine.FindView(
                actionContext,
                viewName,
                isMainPage: false);

            if (!viewResult.Success)
            {
                var searchedLocations = string.Join(
                    Environment.NewLine,
                    viewResult.SearchedLocations);

                throw new InvalidOperationException(
                    $"Could not find Razor view '{viewName}'." +
                    $"{Environment.NewLine}Searched locations:" +
                    $"{Environment.NewLine}{searchedLocations}");
            }

            await using var writer = new StringWriter();

            var viewData = new ViewDataDictionary<TModel>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(
                httpContext,
                _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                tempData,
                writer,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return writer.ToString();
        }
    }
}