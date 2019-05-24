//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace TestAOI
//{
//    public class Host
//    {
//        public static void Main()
//        {
//            IServiceCollection serviceCollection = new ServiceCollection();
//            ConfigureServices(serviceCollection);
//            Application application = new Application(serviceCollection);
//            // Run
//            // ...
//        }
//        static private void ConfigureServices(IServiceCollection serviceCollection)
//        {
//            ILoggerFactory loggerFactory = new Logging.LoggerFactory();
//            serviceCollection.AddInstance<ILoggerFactory>(loggerFactory);
//        }
//    }
//    public class Application
//    {
//        public IServiceProvider Services { get; set; }
//        public ILogger Logger { get; set; }
//        public Application(IServiceCollection serviceCollection)
//        {
//            ConfigureServices(serviceCollection);
//            Services = serviceCollection.BuildServiceProvider();
//            Logger = Services.GetRequiredService<ILoggerFactory>()
//                    .CreateLogger<Application>();
//            Logger.LogInformation("Application created successfully.");
//        }
//        public void MakePayment(PaymentDetails paymentDetails)
//        {
//            Logger.LogInformation(
//              $"Begin making a payment { paymentDetails }");
//            IPaymentService paymentService =
//              Services.GetRequiredService<IPaymentService>();
//            // ...
//        }
//        private void ConfigureServices(IServiceCollection serviceCollection)
//        {
//            serviceCollection.AddSingleton<IPaymentService, PaymentService>();
//        }
//    }
//    public class PaymentService : IPaymentService
//    {
//        public ILogger Logger { get; }
//        public PaymentService(ILoggerFactory loggerFactory)
//        {
//            Logger = loggerFactory?.CreateLogger<PaymentService>();
//            if (Logger == null)
//            {
//                throw new ArgumentNullException(nameof(loggerFactory));
//            }
//            Logger.LogInformation("PaymentService created");
//        }
//    }
//}
