namespace BIE.Core.BaseRepository
{
    public interface IRepositoryFactory
    {
        IInfrastructureRepository InfrastructureRepository { get; }

    }
}
