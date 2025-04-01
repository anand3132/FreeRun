using RedGaint.Network.Runtime;

namespace RedGaint.Network.Tests.Runtime
{
    internal class ExtraTestsController : Controller<TestsApplication>
    {
        void OnDestroy()
        {
            RemoveListeners();
        }
        internal override void RemoveListeners() { }
    }
}
