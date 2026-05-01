namespace HackerNewsGateway.Domain.Interfaces
{
    public interface IUseCase<TInput, TOutput>
    {
        Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct = default);
    }
}
