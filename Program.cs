using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using System.Reflection;

namespace CarSample
{
    public interface ICarContract
    {
        string DoSomething();
    }

    public interface ICarMetadata
    {
        [DefaultValue("NoName")]
        string Name { get; }

        [DefaultValue((uint)0)]
        uint Price { get; }
    }

    public class CarBMW : ICarContract
    {
        public CarBMW()
        {
            Console.WriteLine("Constructor CarBMW");
        }
        public string DoSomething()
        {
            return "CarBMW";
        }
    }

    public class CarMercedes : ICarContract
    {
        public CarMercedes()
        {
            Console.WriteLine("Constructor CarMercedes");
        }
        public string DoSomething()
        {
            return "CarMercedes";
        }
    }

    public class CarHost
    {
        private Lazy<ICarContract, ICarMetadata>[] carPartsA = null;
        private Lazy<ICarContract, ICarMetadata>[] carPartsB = null;

        public CarHost(Lazy<ICarContract, ICarMetadata>[] carPartsA, Lazy<ICarContract, ICarMetadata>[] carPartsB)
        {
            Console.WriteLine("Constructor CarHost");
            this.carPartsA = carPartsA;
            this.carPartsB = carPartsB;
        }

        public Lazy<ICarContract, ICarMetadata>[] GetCarPartsA()
        {
            return carPartsA;
        }

        public Lazy<ICarContract, ICarMetadata>[] GetCarPartsB()
        {
            return carPartsB;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }
        void Run()
        {
            RegistrationBuilder builder = new RegistrationBuilder();

            builder.ForTypesDerivedFrom<ICarContract>()
                .Export<ICarContract>(eb =>
                {
                    eb.AddMetadata("Name", t => { return (t == typeof(CarBMW)) ? "BMW" : "Mercedes"; });
                    eb.AddMetadata("Price", t => { return (t == typeof(CarBMW)) ? (uint)51000 : (uint)48000; });
                })
                .SetCreationPolicy(CreationPolicy.NonShared);

            builder.ForType<CarHost>()
                .Export()
                .SelectConstructor(ctors => ctors.First(ci => ci.GetParameters().Length == 2), (pi, ib) => ib.AsMany(true))
                .SetCreationPolicy(CreationPolicy.NonShared);

            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly(), builder);
            var container = new CompositionContainer(catalog);

            var carHost = container.GetExportedValue<CarHost>();

            Lazy<ICarContract, ICarMetadata>[] carPartsA = carHost.GetCarPartsA();
            Lazy<ICarContract, ICarMetadata>[] carPartsB = carHost.GetCarPartsB();

            Console.WriteLine("CarPart A:");
            foreach (Lazy<ICarContract, ICarMetadata> carPart in carPartsA)
                Console.WriteLine(carPart.Value.DoSomething());
            Console.WriteLine("");

            Console.WriteLine("CarPart B:");
            foreach (Lazy<ICarContract, ICarMetadata> carPart in carPartsB)
                Console.WriteLine(carPart.Value.DoSomething());
            Console.WriteLine("");

            Console.WriteLine("Metadata CarPart A: ");
            foreach (Lazy<ICarContract, ICarMetadata> carPart in carPartsA)
            {
                Console.WriteLine(carPart.Metadata.Name);
                Console.WriteLine(carPart.Metadata.Price);
            }
            Console.WriteLine("");

            Console.WriteLine("Metadata CarPart B: ");
            foreach (Lazy<ICarContract, ICarMetadata> carPart in carPartsB)
            {
                Console.WriteLine(carPart.Metadata.Name);
                Console.WriteLine(carPart.Metadata.Price);
            }

            Console.ReadLine();
        }
    }
}
