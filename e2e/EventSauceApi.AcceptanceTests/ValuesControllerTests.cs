using Xunit;
using FluentAssertions;

namespace EventSauceApi.AcceptanceTests
{
    public class ValuesControllerTests
    {
        [Fact]
        public void TrueIsTrue()
        {
            true.Should().BeTrue();
        }
    }
}
