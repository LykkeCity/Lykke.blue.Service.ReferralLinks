using System.Collections;
using System.Collections.Generic;

namespace Lykke.blue.Service.ReferralLinks.Core.Domain.Health
{
    public class HealthIssuesCollection : IReadOnlyCollection<HealthIssue>
    {
        public int Count => _list.Count;

        private readonly List<HealthIssue> _list;

        public HealthIssuesCollection()
        {
            _list = new List<HealthIssue>();
        }

        public IEnumerator<HealthIssue> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
