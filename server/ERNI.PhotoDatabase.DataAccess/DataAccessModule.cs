using System.Reflection;
using Autofac;
using ERNI.PhotoDatabase.DataAccess.Images;
using ERNI.PhotoDatabase.DataAccess.Repository;
using Module = Autofac.Module;

namespace ERNI.PhotoDatabase.DataAccess
{
    public class DataAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<PhotoRepository>().As<IPhotoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TagRepository>().As<ITagRepository>().InstancePerLifetimeScope();

            builder.RegisterType<UnitOfWork.UnitOfWork>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ImageStoreConfiguration>().AsSelf().SingleInstance();
            builder.RegisterType<ImageStore>().AsSelf().InstancePerLifetimeScope();
        }
    }
}
