using System.ComponentModel.Composition.Hosting;

namespace Easy.Common.NetCore.Startup
{
    public static class EasyMefContainer
    {
        public static CompositionContainer Container { get; private set; }

        public static void InitMefContainer(CompositionContainer container)
        {
            Container = container;
        }
    }
}