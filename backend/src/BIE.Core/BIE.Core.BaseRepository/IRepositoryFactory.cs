namespace BIE.Core.BaseRepository
{
    public interface IRepositoryFactory
    {
        IInfrastructureRepository BatchRepository { get; }

    }
}
