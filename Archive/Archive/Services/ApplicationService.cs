using Archive.Interfaces;
using Archive.Models.Database;

namespace Archive.Services
{
	public class ApplicationService<TValue> : IApplicationService<TValue>
	{
		private TValue? obj;
		public TValue? Obj { get => GetObject(); set => SetObject(value); }

		public TValue? GetObject()
		{
			return obj;
		}

		public void SetObject(TValue? obj)
		{
			this.obj = obj;
		}
	}
}
