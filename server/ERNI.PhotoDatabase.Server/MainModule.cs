using Autofac;
using ERNI.PhotoDatabase.DataAccess;

namespace ERNI.PhotoDatabase.Server
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<DataAccessModule>();
        }
    }
}
