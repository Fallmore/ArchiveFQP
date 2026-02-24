using Archive.Models.Database;

namespace Archive.Interfaces
{
	public interface IApplicationService<TValue>
	{
		public void SetObject(TValue obj);
		public TValue? GetObject();
	}
}
