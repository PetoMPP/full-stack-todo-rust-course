using System.Collections;

namespace TodoAPI_MVC.Authentication
{
    public interface IRevokedTokens : IReadOnlyCollection<Guid>
    {
        void Add(Guid guid, DateTime expirationTime);
    }

    public class RevokedTokens : IRevokedTokens
    {
        private readonly List<Guid> _tokens;

        public int Count => _tokens.Count;

        private delegate void DeletionHandler(Guid guid, TimeSpan delay);
        private event DeletionHandler? DeletionRequired;

        public RevokedTokens()
        {
            _tokens = new List<Guid>();
            DeletionRequired += InvalidatedTokens_DeletionRequired;
        }

        public void Add(Guid guid, DateTime expirationTime)
        {
            _tokens.Add(guid);
            DeletionRequired!.Invoke(guid, expirationTime - DateTime.UtcNow);
        }

        private async void InvalidatedTokens_DeletionRequired(Guid guid, TimeSpan delay)
        {
            delay = delay >= TimeSpan.Zero ? delay : TimeSpan.Zero;
            await Task.Delay(delay);
            _tokens.Remove(guid);
        }

        public IEnumerator<Guid> GetEnumerator() => _tokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _tokens.GetEnumerator();
    }
}
