﻿using Ninject;

namespace SkyKick.NinjectWorkshop.WordCounting.UI
{
    public class Startup
    {
        public IKernel BuildKernel()
        {
            return new StandardKernel(
                new SkyKick.Bcl.Logging.ConsoleTestLogger.NinjectModule(),
                new SkyKick.NinjectWorkshop.WordCounting.NinjectModule(),
                new SkyKick.NinjectWorkshop.WordCounting.UI.NinjectModule());
        }
    }
}
