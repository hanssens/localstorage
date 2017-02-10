using System;
using System.Collections.Generic;
using System.Text;

namespace LocalStorage.Tests.Stubs
{
    public class CarFactory
    {
        public static IEnumerable<Car> Create()
        {
            var cars = new List<Car>();

            cars.Add(new Car() { Brand = "BMW", Model = "3-Series", Year = 2012 });
            cars.Add(new Car(){ Brand = "BMW", Model = "5-Series", Year = 2017 });
            cars.Add(new Car() { Brand = "Mercedes-Benz", Model = "CLA 63", Year = 2016 });

            return cars;
        }
    }
}
