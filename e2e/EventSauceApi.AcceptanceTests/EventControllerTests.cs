using Xunit;
using FluentAssertions;

namespace EventSauceApi.AcceptanceTests
{
    public class EventControllerTests
    {
        [Fact]
        public void TrueIsTrue()
        {
            true.Should().BeTrue();
        }
    }
}
