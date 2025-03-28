using RedGaint.Network.Runtime;

namespace RedGaint.Network.Tests.Runtime
{
    class TestsController : Controller<TestsApplication>
    {
        void OnDestroy()
        {
            RemoveListeners();
        }
        internal override void RemoveListeners() { }
    }
}
