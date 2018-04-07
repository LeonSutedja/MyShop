using Shop.Infrastructure.Customer;
using Shop.Infrastructure.Interfaces;
using Shop.Infrastructure.Product;
using Shop.Infrastructure.Repository;
using Shop.Infrastructure.TableCreator;
using Shop.Order;
using Shop.Shared.Models.CommandHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using Unity.Lifetime;
using Unity.RegistrationByConvention;

namespace Shop
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container

        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;

        #endregion Unity Container

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();

            // NOTE: To load from web.config uncomment the line below. Make sure to add a Microsoft.Practices.Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            /// Currently, we use singleton for repository. This is to handle function where object not being persisted into the database.
            /// The following line is to map between generic interface of IRepository to all of their implementations.
            container.RegisterTypes(AllClasses.FromLoadedAssemblies()
                    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                                                           i.GetGenericTypeDefinition() == typeof(IRepository<>))),
                WithMappings.FromAllInterfaces,
                WithName.Default, WithLifetime.ContainerControlled);

            // External services mapping
            // Map shop.infrastructure.service
            container.RegisterType<IProductService, ProductService>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICustomerService, CustomerService>(new ContainerControlledLifetimeManager());

            // Map Shop.Order
            container.RegisterType<IOrderService, OrderService>(new ContainerControlledLifetimeManager());

            _registerCommandHandlers(container);
            _registerTableCreator(container);
        }

        private static void _registerTableCreator(IUnityContainer container)
        {
            // TableCreator
            container.RegisterTypes(AllClasses.FromLoadedAssemblies()
                    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                                                           i.GetGenericTypeDefinition() == typeof(ITableColumnRepository<>))),
                WithMappings.FromAllInterfaces,
                WithName.Default, WithLifetime.PerThread);

            var tColumnTypes = AllClasses.FromLoadedAssemblies()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                                                       i.GetGenericTypeDefinition() == typeof(ITableColumn<>))).ToList();

            container.RegisterTypes(tColumnTypes,
                WithMappings.FromAllInterfaces,
                WithName.TypeName, WithLifetime.PerThread);

            var ienumerable = typeof(IEnumerable<>);
            var itableColumnType = typeof(ITableColumn<Product>);
            Type[] typeArgs = { itableColumnType };
            ienumerable.MakeGenericType(typeArgs);

            container.RegisterType(typeof(IEnumerable<ITableColumn>), typeof(ITableColumn[]));

            container.RegisterTypes(AllClasses.FromLoadedAssemblies()
                    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                                                           i.GetGenericTypeDefinition() == typeof(ITableBuilder<>))),
                WithMappings.FromAllInterfaces,
                WithName.Default, WithLifetime.PerThread);
        }

        private static void _registerCommandHandlers(IUnityContainer container)
        {
            // Handlers
            container.RegisterType<ICommandHandlerFactory, CommandHandlerFactory>(new ContainerControlledLifetimeManager());
            container.RegisterTypes(AllClasses.FromLoadedAssemblies()
                    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                                                           i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))), WithMappings.FromAllInterfaces,
                WithName.Default, WithLifetime.ContainerControlled);
        }
    }
}