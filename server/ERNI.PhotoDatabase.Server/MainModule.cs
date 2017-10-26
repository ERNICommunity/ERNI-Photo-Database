using Autofac;
using ERNI.PhotoDatabase.DataAccess;
using ERNI.PhotoDatabase.Server.Utils.Image;

namespace ERNI.PhotoDatabase.Server
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<DataAccessModule>();

            builder.RegisterType<ImageManipulation>().AsImplementedInterfaces();
        }
    }
}
