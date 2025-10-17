using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Psychology.Infrastructure.Email
{
    public interface ITemplateRenderer
    {
        /// <summary>
        /// Renders a Razor view (.cshtml) file to an HTML string.
        /// </summary>
        Task<string> RenderAsync(string viewPath, object model);
    }

    public sealed class TemplateRenderer(
    IRazorViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider) : ITemplateRenderer
    {
        public async Task<string> RenderAsync(string viewPath, object model)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext { RequestServices = serviceProvider },
                new RouteData(),
                new ActionDescriptor());

            var viewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: false);
            if (!viewResult.Success)
                throw new InvalidOperationException($"View '{viewPath}' not found.");

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            { Model = model };

            await using var sw = new StringWriter();
            var viewCtx = new ViewContext(actionContext, viewResult.View, viewDictionary,
                                          new TempDataDictionary(actionContext.HttpContext, tempDataProvider), sw, new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewCtx);
            return sw.ToString();
        }
    }
}
