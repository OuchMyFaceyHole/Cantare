//Lightly modified from https://github.com/referbruv/razorviewenginesample/blob/master/Providers/TemplateHelper.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Cantare.Helpers
{
    public class HTMLGenerator
    {
        private IRazorViewEngine razorViewEngine;
        private ITempDataProvider tempDataProvider;

        public HTMLGenerator(IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider)
        {
            this.razorViewEngine = razorViewEngine;
            this.tempDataProvider = tempDataProvider;
        }

        public async Task<string> GenerateHTML<T>(HttpContext httpContext, string viewPath, T model)
        {
            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());

            using (StringWriter sw = new StringWriter())
            {
                var viewResult = razorViewEngine.GetView(null, viewPath, true);

                if (viewResult.View == null)
                {
                    throw new ArgumentException();
                }

                var viewDataDictionary = new ViewDataDictionary(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary()
                )
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDataDictionary,
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                var data = sw.ToString();
                data = data.Remove(0, sw.ToString().LastIndexOf("class=\"pb-3\">") + "class=\"pb-3\">".Length);
                var lastIndex = data.IndexOf("</main>");
                data = data.Remove(data.IndexOf("</main>"), data.Length - lastIndex);

                return data;
            }
        }
    }
}
