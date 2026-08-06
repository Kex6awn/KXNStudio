namespace KxnPhotoStudio.Services.Interfaces
{
    public interface IRazorViewRenderService
    {
        Task<string> RenderViewToStringAsync<TModel>(
            string viewName,
            TModel model);
    }
}