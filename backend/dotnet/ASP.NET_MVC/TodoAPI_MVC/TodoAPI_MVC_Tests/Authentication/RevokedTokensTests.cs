using System.Collections;
using TodoAPI_MVC.Authentication;

namespace TodoAPI_MVC_Tests.Authentication
{
    public class RevokedTokensTests
    {
        private const int AssertionDelayMs = 10; // assertion is faster that event invokation

        [TestCase(100)]
        [TestCase(-100)]
        [TestCase(0)]
        public async Task Add_ShouldAddAndRemoveGuid(int delayMs)
        {
            var tokens = new RevokedTokens();
            var guid = Guid.NewGuid();
            tokens.Add(guid, DateTime.UtcNow.AddMilliseconds(delayMs));
            if (delayMs > 0)
            {
                delayMs += AssertionDelayMs;
                tokens.Should().OnlyContain(g => g == guid);
                tokens.Count.Should().Be(1);
            }
            else
            {
                delayMs = AssertionDelayMs;
            }

            await Task.Delay(delayMs);
            tokens.Should().BeEmpty();
            tokens.Count.Should().Be(0);
        }

        [Test]
        public void IEnumerableGetEnumerator_ShouldReturnEnumerator()
        {
            var tokens = new RevokedTokens();
            var guid = Guid.NewGuid();
            tokens.Add(guid, DateTime.UtcNow.AddMilliseconds(10));
            tokens.Add(guid, DateTime.UtcNow.AddMilliseconds(10));
            IEnumerable enumerable = tokens;

            var actual = enumerable.GetEnumerator();

            actual.MoveNext().Should().BeTrue();
            actual.MoveNext().Should().BeTrue();
            actual.MoveNext().Should().BeFalse();
        }
    }
}
