using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.Controllers;

namespace EHC.API.Tests.ControllersTest
{
    [ExcludeFromCodeCoverage]
    [UnitTestCategory]
    [TestClass]
   public class EquipmentControllerTest
   { 

        [TestMethod]
        public void Test_Class_Should_BeDecorated()
        {
            ApiBaseTest.ValidateClassAttributes<EquipmentController>();
        }
     
   }

}
