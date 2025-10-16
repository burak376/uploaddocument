namespace DocumentTasking.Api.Infrastructure.Email;

using RazorLight;

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync<TModel>(string templateKey, TModel model);
}

public class RazorEmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly RazorLightEngine _engine;

    public RazorEmailTemplateRenderer()
    {
        var templatesPath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Email", "Templates");
        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatesPath)
            .UseMemoryCachingProvider()
            .Build();
    }

    public Task<string> RenderAsync<TModel>(string templateKey, TModel model)
    {
        return _engine.CompileRenderAsync(templateKey, model);
    }
}
