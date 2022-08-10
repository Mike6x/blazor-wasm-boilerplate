namespace FSH.BlazorWebAssembly.Client.Components.EntityTable;

public interface IImportModal<TRequest>
{
    TRequest RequestModel { get; }
    void ForceRender();
}