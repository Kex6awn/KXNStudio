namespace KxnPhotoStudio.Services
{
    public interface IRazorViewRenderService
    {
        Task<string> RenderViewToStringAsync<TModel>(
            string viewName,
            TModel model);
    }
}