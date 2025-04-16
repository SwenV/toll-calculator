using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollFeeCalculator
{
    public class Car : IVehicle
    {
        private readonly CarType type;

        public Car(CarType type = CarType.Standard) => 
            this.type = type;

        bool IVehicle.IsTollFree() => type != CarType.Standard;

        public enum CarType
        {
            Standard,
            Tractor,
            Emergency,
            Diplomat,
            Foreign,
            Military
        }
    }
}